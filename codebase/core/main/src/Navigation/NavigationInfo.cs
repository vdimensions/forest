#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
using System.Runtime.Serialization;
#endif

namespace Forest.Navigation
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [System.Serializable]
    #endif
    public sealed class NavigationInfo
    {
        public static readonly NavigationInfo Empty = new NavigationInfo();
        
        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [DataMember]
        #endif
        private readonly string _template;
        
        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [DataMember]
        #endif
        private readonly object _message;

        internal NavigationInfo(string template, object message)
        {
            _template = template;
            _message = message;
        }
        internal NavigationInfo(string template) : this(template, null) { }
        private NavigationInfo() : this(string.Empty) { }

        public string Template => _template;
        public object Message => _message;
    }
}