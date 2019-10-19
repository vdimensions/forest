using System;
using System.Diagnostics;

using Forest;


namespace Axle.Forest.Web.Api.Http
{
    [Serializable]
    public sealed class ResponseHeader
    {
        internal static readonly Version ForestVersion = typeof (ApplicationState).Assembly.GetName().Version;

        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string protocolVersion;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string format;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string template;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly string master;

        internal ResponseHeader(string template, string master, ResponseFormat format)
        {
            this.template = template;
            this.master = master;
            this.format = format.ToString();
            this.protocolVersion = ForestVersion.ToString();
        }

        public string Template { get { return template; } }
        public string Master { get { return master; } }
        public string Format { get { return format; } }
        public string ProtocolVersion { get { return protocolVersion; } }
    }
}