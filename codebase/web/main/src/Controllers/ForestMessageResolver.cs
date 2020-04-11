using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Axle.IO.Extensions.Stream;
using Axle.Text.Data;
using Axle.Web.AspNetCore.Mvc.ModelBinding;

namespace Forest.Web.AspNetCore.Controllers
{
    // TODO: make parameter parsing as router path detection
    internal sealed class ForestMessageTextDataAdapter : ITextDataAdapter
    {
        public ForestMessageTextDataAdapter(string key, string value, params ITextDataAdapter[] children)
        {
            Key = key;
            Value = value;
            Children = children;
        }

        public string Key { get; }
        public string Value { get; }
        public IEnumerable<ITextDataAdapter> Children { get; }
    }
    internal sealed class MessageReader : AbstractTextDataReader
    {
        public MessageReader(StringComparer comparer) : base(comparer)
        {
        }

        protected override ITextDataAdapter CreateAdapter(Stream stream, Encoding encoding)
        {
            return CreateAdapter(encoding.GetString(stream.ToByteArray()));
        }

        protected override ITextDataAdapter CreateAdapter(string data)
        {
            throw new NotImplementedException();
        }
    }
    
    internal sealed class ForestMessageResolver : IModelResolver
    {
        private readonly IClientViewsHelper _clientViewsHelper;

        public ForestMessageResolver(IClientViewsHelper clientViewsHelper)
        {
            _clientViewsHelper = clientViewsHelper;
        }

        public async Task<object> Resolve(IReadOnlyDictionary<string, object> routeData, ModelResolutionContext next)
        {
            var template = routeData.TryGetValue(ForestController.Template, out var tpl) ? tpl : null;
            var path = routeData.TryGetValue(ForestController.Message, out var msg) ? msg : null;
            return null;
        }
    }
}