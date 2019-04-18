namespace Forest.Templates
open System.Runtime.CompilerServices
open Axle.Verification
open Forest
open Forest.Templates.Raw

[<Extension;System.Obsolete>]
type TemplateExtensions =
    [<Extension;System.Obsolete>]
    static member internal LoadTree(engine : ForestStateManager, NotNull "name" name : string) =
        name 
        |> Raw.loadTemplate engine.Context.TemplateProvider
        |> TemplateCompiler.compile
        |> engine.SwapState None

