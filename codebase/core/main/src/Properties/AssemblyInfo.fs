﻿module Forest.Core.AssemblyInfo

open System.Runtime.CompilerServices
open System.Runtime.InteropServices

#if NETSTANDARD1_1_OR_NEWER || NETFRAMEWORK
[<assembly: Guid("7CBDBFBD-F8FD-4C48-AF8E-0D77923DDF42")>]
#endif

[<assembly: InternalsVisibleTo("Forest.Core.Tests")>]

do ()