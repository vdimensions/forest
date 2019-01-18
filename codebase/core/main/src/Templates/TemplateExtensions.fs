namespace Forest.Templates
open System.Runtime.CompilerServices
open Axle.Verification
open Forest
open Forest.Templates.Raw

[<Extension>]
type TemplateExtensions =
    [<Extension>]
    static member internal LoadTree(engine : ForestEngine, NotNull "name" name : string) =
        name 
        |> Raw.loadTemplate engine.Context.TemplateProvider
        |> TemplateCompiler.compile
        |> engine.SwapState None

