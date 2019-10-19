using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Http.Metadata;
using System.Web.SessionState;

using Axle.Conversion.ComponentModel;
using Axle.Extensions.String;
using Axle.Extensions.Type;
using Axle.Forest.ComponentModel;
using Axle.Forest.Web.ComponentModel;
using Axle.Reflection;
using Axle.Web.Http;
using Axle.Web.Http.Controllers;

using Forest;
using Forest.Engine;


namespace Axle.Forest.Web.Api.Http
{
    /// <summary>
    /// An attribute that captures the entire content body and stores it
    /// into the parameter of type that is determined by the forest model at runtime
    /// </summary>
    /// <remarks>
    /// The parameter marked up with this attribute should be the only parameter as it reads the
    /// entire request body and assigns it to that parameter.    
    /// </remarks>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false, Inherited = false)]
    internal sealed class ForestCommandArgumentAttribute : DelegatedParameterBindingAttribute
    {
        private const string ForestLayoutSessionKey = "E55E8CCE-B4D5-4285-917D-E42C30A3DF3B";
        
        private readonly ModelSubstituteRegistryComponent registry;
        private readonly IForestEngine forestEngine;

        public ForestCommandArgumentAttribute()
        {
            this.registry = new ModelSubstituteRegistryComponent();
            this.forestEngine = new ForestEngineComponent();
        }

        public override Task ExecuteBindingAsync(
            IDelegatedParameterBinding parameterBinding, 
            ModelMetadataProvider metadataProvider, 
            HttpActionContext actionContext, 
            CancellationToken cancellationToken)
        {
            var parameterBindings = actionContext.ActionDescriptor.ActionBinding.ParameterBindings;
            var templateNameBidning = parameterBindings.Where(pb => !pb.WillReadBody && pb.Descriptor.GetCustomAttributes<ForestTemplateNameAttribute>().Any()).SingleOrDefault();
            var commandInvokerBidning = parameterBindings.Where(pb => !pb.WillReadBody && pb.Descriptor.GetCustomAttributes<ForestEncodedCommandInvokerAttribute>().Any()).SingleOrDefault();
            var commandNameBidning = parameterBindings.Where(pb => !pb.WillReadBody && pb.Descriptor.GetCustomAttributes<ForestCommandNameAttribute>().Any()).SingleOrDefault();

            var content = actionContext.Request.Content;
            Func<Type, Task<object>> resolveFunc;
            if (ResolveFromBody)
            {
                var dataParameterBinding = parameterBindings.Where(pb => pb.WillReadBody).ToArray();
                if ((commandNameBidning == null) || (commandInvokerBidning == null) || (dataParameterBinding.Length > 1) || (actionContext.Request.Method == HttpMethod.Get))
                {
                    return content.ReadAsVoidAsync();
                }

                resolveFunc = type => ReadBodyAsync(content, type ?? typeof (void));
            }
            else
            {
                var commandArgBinding = parameterBindings.Where(pb => !pb.WillReadBody && pb.Descriptor.GetCustomAttributes<ForestCommandArgAttribute>().Any()).SingleOrDefault();
                var commandArgRawValue = (string) actionContext.ModelState[commandArgBinding.Descriptor.ParameterName].Value.RawValue;

                resolveFunc = type => ReadRequestAsync(content, type ?? typeof (void), commandArgRawValue);
            }

            var templateName = (string)actionContext.ModelState[templateNameBidning.Descriptor.ParameterName].Value.RawValue;
            var path = ForestDecodedCommandInvokerAttribute.DecodeCommandPath((string) actionContext.ModelState[commandInvokerBidning.Descriptor.ParameterName].Value.RawValue);
            var commandName = (string) actionContext.ModelState[commandNameBidning.Descriptor.ParameterName].Value.RawValue;

            var state = Session[ForestLayoutSessionKey] as ApplicationState;
            if (state == null)
            {
                Session[ForestLayoutSessionKey] = state = this.forestEngine.CreateState().NavigateTo(templateName);
            }
            var commandParameter = state.GetCommandParameter(path, commandName);
            if (commandParameter != null)
            {
                var type = commandParameter.Type;
                var argType = registry.Find(type) ?? type;
                var task = resolveFunc(argType);
                task.Wait();
                var arg = task.Result;
                ICastOperator castOp;
                if ((type != argType) && !type.IsAssignableFrom(argType) && (castOp = CastOperator.For(argType, type)).IsDefined)
                {
                    arg = castOp.Invoke(arg);
                }
                parameterBinding.SetValue(actionContext, arg);
            }
            else
            {
                parameterBinding.SetValue(actionContext, null);
            }
            return content.ReadAsVoidAsync();
        }


        private Task<object> ReadBodyAsync(HttpContent content, Type type)
        {
            if (content.IsMimeMultipartContent())
            {
                var root = HttpContext.Current.Server.MapPath("~/App_Data");
                var p = new MultipartFormDataStreamProvider(root);
                var t = content.ReadAsMultipartAsync(p);
                t.Wait();
                //return content.ReadAsMultipartAsync(p).ContinueWith(
                    //t =>
                    //{
                        var provider = t.Result;
                        var dict = new Dictionary<string, object>();
                        foreach (var httpContent in provider.FormData.AllKeys)
                        {
                            dict[httpContent] = provider.FormData[httpContent];
                        }
                        foreach (var fileContent in provider.FileData)
                        {
                            dict[fileContent.Headers.ContentDisposition.Name] = fileContent.LocalFileName;
                        }
                    //    return forestEngine.MapObject(dict, type);
                    //},
                    //TaskContinuationOptions.ExecuteSynchronously);
                return null;
            }
            if (type == typeof(string))
            {
                return content.ReadAsStringAsync().ContinueWith<object>(previousTask => previousTask.Result, TaskContinuationOptions.ExecuteSynchronously);
            }
            if (type == typeof(byte[]))
            {
                return content.ReadAsByteArrayAsync().ContinueWith<object>(previousTask => previousTask.Result, TaskContinuationOptions.ExecuteSynchronously);
            }
            if (type == typeof(void))
            {
                return content.ReadAsVoidAsync().ContinueWith<object>(previousTask => null);
            }
            if (type.ExtendsOrImplements<IEnumerable<HttpContent>>())
            {
                return content.ReadAsMultipartAsync().ContinueWith<object>(previousTask => previousTask.Result.Contents, TaskContinuationOptions.ExecuteSynchronously);
            }
            if (type == typeof(Stream))
            {
                return content.ReadAsStreamAsync().ContinueWith<object>(previousTask => previousTask.Result, TaskContinuationOptions.ExecuteSynchronously);
            }
            return content.ReadAsAsync(type);
        }

        private static Task<object> ReadRequestAsync(HttpContent content, Type type, string value)
        {
            var startPoint = content.ReadAsVoidAsync();
            Exception cause = null;
            if ((type == typeof(void)) || (value == null))
            {
                return startPoint.ContinueWith<object>(previousTask => null, TaskContinuationOptions.ExecuteSynchronously);
            }
            if (type == typeof(string))
            {
                return startPoint.ContinueWith<object>(previousTask => value, TaskContinuationOptions.ExecuteSynchronously);
            }
            try
            {
                var result = Parser.ForType(type).Parse(value);
                return startPoint.ContinueWith(previousTask => result, TaskContinuationOptions.ExecuteSynchronously);
            } 
            catch (Exception e)
            {
                cause = e;
            }
            throw new InvalidOperationException("Could not parse command argument '{0} to type {1}".FormatFor(value, type), cause);
        }

        public override bool WillReadBody { get { return ResolveFromBody; } }

        private bool ResolveFromBody { get { return Source == ForestCommandArgumentSource.Body; } }
        
        public ForestCommandArgumentSource Source { get; set; }

        private HttpSessionState Session { get { return HttpContext.Current.Session; } }
    }
}