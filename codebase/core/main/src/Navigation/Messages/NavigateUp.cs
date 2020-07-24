using System;

namespace Forest.Navigation.Messages
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    internal struct NavigateUp
    {
        private readonly int? _offset;

        public NavigateUp(int offset) : this()
        {
            _offset = offset;
        }
        
        public int Offset => _offset.GetValueOrDefault(1);
    }
}