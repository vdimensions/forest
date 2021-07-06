using System;

namespace Forest.UI.Dialogs
{
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [Serializable]
    #endif
    [Flags]
    public enum DialogOptions : byte
    {
        Default = 0,
        FitToContents = (1 << 0),
        AllowMove = (1 << 1),
        AllowResize = (1 << 2),
    }
}