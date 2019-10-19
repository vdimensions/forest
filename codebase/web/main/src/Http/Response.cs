using System;
using System.Diagnostics;

using Forest.Dom;


namespace Axle.Forest.Web.Api.Http
{
    [Serializable]
    public sealed class Response
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly ResponseHeader header;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IViewNode body;

        internal Response(ResponseHeader header, IViewNode body)
        {
            this.header = header;
            this.body = body;
        }

        public ResponseHeader Header { get { return header; } }
        public IViewNode Body { get { return body; } }
    }
}