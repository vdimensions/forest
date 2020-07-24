using System;

namespace Forest.Navigation.Messages
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    internal struct NavigateBack
    {
        private readonly int? _offset;

        public NavigateBack(int offset) : this()
        {
            _offset = offset;
        }
        
        public int Offset => _offset.GetValueOrDefault(1);
    }
}