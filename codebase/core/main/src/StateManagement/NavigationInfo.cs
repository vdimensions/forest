#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
using System.Runtime.Serialization;
#endif

namespace Forest.StateManagement
{
    public sealed class NavigationInfo
    {
        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [DataMember]
        #endif
        private readonly string _template;
        
        #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
        [DataMember]
        #endif
        private readonly object _parameter;

        internal NavigationInfo(string template, object parameter)
        {
            _template = template;
            _parameter = parameter;
        }
        public NavigationInfo() : this(string.Empty, null) { }

        public string Template => _template;
        public object Parameter => _parameter;
    }
}