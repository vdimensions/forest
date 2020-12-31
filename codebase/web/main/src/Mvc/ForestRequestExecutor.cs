using System;
using System.Linq;
using System.Net;
using Forest.Engine;
using Forest.Navigation;
using Forest.Web.AspNetCore.Dom;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Forest.Web.AspNetCore.Mvc
{
    /// <summary>
    /// A class that is used to delegate web request to the forest engine, and convert
    /// the result to a format suitable for a REST-ful communication.
    /// </summary>
    public sealed class ForestRequestExecutor
    {
        [Serializable]
        private sealed class ForestResponse
        {
            public ForestResponse(string path, ViewNode[] views)
            {
                Path = path;
                Views = views;
            }

            public string Path { get; }
            public ViewNode[] Views { get; }
        }
        
        public sealed class ForestResult : ObjectResult, IClientErrorActionResult
        {
            private ForestResult(ForestResponse response, HttpStatusCode statusCode) : base(response)
            {
                StatusCode = (int) statusCode;
            }

            internal ForestResult(string path, ViewNode[] views, HttpStatusCode statusCode) : this(new ForestResponse(path, views), statusCode) { }
        }
        
        private readonly IForestEngine _forest;
        private readonly IClientViewsHelper _clientViewsHelper;
        private readonly ForestMessageConverter _messageConverter;

        internal ForestRequestExecutor(
            IForestEngine forest, 
            IClientViewsHelper clientViewsHelper, 
            ForestMessageConverter messageConverter)
        {
            _forest = forest;
            _clientViewsHelper = clientViewsHelper;
            _messageConverter = messageConverter;
        }
        
        private string GetPath(Location location)
        {
            if (location.Value != null)
            {
                var messageStr = _messageConverter.ConvertMessage(location.Value);
                return $"{location.Path}/{messageStr}";
            }
            return location.Path;
        }

        public ForestResult ExecuteCommand(string instanceId, string command, object arg)
        {
            _forest.ExecuteCommand(command, instanceId, arg);
            return new ForestResult(GetPath(_clientViewsHelper.Location), _clientViewsHelper.UpdatedViews.Values.ToArray(), HttpStatusCode.PartialContent);
        }
        
        public ActionResult Navigate(string template, string customPath = null)
        {
            _forest.Navigate(template);
            return new ForestResult(
                string.IsNullOrEmpty(customPath) ? GetPath(_clientViewsHelper.Location) : customPath, 
                _clientViewsHelper.AllViews.Values.ToArray(), 
                HttpStatusCode.OK);
        }
        
        public ActionResult Navigate(string template, object message, string customPath = null)
        {
            _forest.Navigate(template, message);
            return new ForestResult(
                string.IsNullOrEmpty(customPath) ? GetPath(_clientViewsHelper.Location) : customPath, 
                _clientViewsHelper.AllViews.Values.ToArray(), 
                HttpStatusCode.OK);
        }
        
        public ActionResult GetPartial(string instanceId)
        {
            if (_clientViewsHelper.AllViews.TryGetValue(instanceId, out var view))
            {
                return new ForestResult(GetPath(_clientViewsHelper.Location), new []{ view }, HttpStatusCode.PartialContent);
            }
            return new NotFoundResult();
        }
    }
}