using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Axle.IO.Extensions.Stream;
using Axle.Text.Documents;
using Axle.Web.AspNetCore.Mvc.ModelBinding;

namespace Forest.Web.AspNetCore.Mvc
{
    // TODO: make parameter parsing as router path detection
    internal sealed class ForestMessageTextDocumentAdapter : ITextDocumentAdapter
    {
        public ForestMessageTextDocumentAdapter(string key, string value, params ITextDocumentAdapter[] children)
        {
            Key = key;
            Value = value;
            Children = children;
        }

        public string Key { get; }
        public string Value { get; }
        public IEnumerable<ITextDocumentAdapter> Children { get; }
    }
    internal sealed class MessageReader : AbstractTextDocumentReader
    {
        public MessageReader(StringComparer comparer) : base(comparer)
        {
        }

        protected override ITextDocumentAdapter CreateAdapter(Stream stream, Encoding encoding)
        {
            return CreateAdapter(encoding.GetString(stream.ToByteArray()));
        }

        protected override ITextDocumentAdapter CreateAdapter(string data)
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

        public async Task<object> Resolve(IMvcMetadata metadata, ModelResolutionContext next)
        {
            var template = metadata.RouteData.TryGetValue(ForestController.Template, out var tpl) ? tpl : null;
            var path = metadata.RouteData.TryGetValue(ForestController.Message, out var msg) ? msg : null;
            return null;
        }
    }
}