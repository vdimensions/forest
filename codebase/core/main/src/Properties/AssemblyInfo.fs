module Forest.Core.AssemblyInfo

open System.Reflection
open System.Runtime.CompilerServices
open System.Runtime.InteropServices

[<assembly: AssemblyTitle("Forest.Core")>]
[<assembly: AssemblyProduct("Forest.Core")>]
[<assembly: AssemblyDescription("Forest (Front-end over REST) is a UI abstraction framework for creating fully-functional applications while being agnostic of the presentation layer")>]

[<assembly: AssemblyCompany("Virtual Dimensions")>]
[<assembly: AssemblyCopyright("Copyright © Virtual Dimensions 2013-2018")>]
[<assembly: AssemblyTrademark("")>]

[<assembly: AssemblyConfiguration("")>]
[<assembly: AssemblyCulture("")>]

[<assembly: ComVisible(false)>]
#if NETSTANDARD1_1_OR_NEWER || NETFRAMEWORK
[<assembly: Guid("7CBDBFBD-F8FD-4C48-AF8E-0D77923DDF42")>]
#endif

[<assembly: AssemblyVersion("2.0.0.146")>]
[<assembly: AssemblyFileVersion("2.0.0.146")>]
[<assembly: AssemblyInformationalVersion("2.0.0.146")>]

[<assembly: InternalsVisibleTo("Forest.Core.Tests")>]

do ()