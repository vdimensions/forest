﻿namespace Forest.Templates

open Forest
open Forest.NullHandling
open Forest.Templates.Raw

open System.Runtime.CompilerServices

[<Extension>]
type TemplateExtensions =
    [<Extension>]
    static member internal LoadTemplate(engine : ForestEngine, NotNull "name" name : string) =
        name 
        |> Raw.loadTemplate engine.Context.TemplateProvider
        |> TemplateCompiler.compile
        |> engine.SwapState None

