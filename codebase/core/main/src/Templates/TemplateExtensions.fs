namespace Forest.Templates

open Forest
open Forest.NullHandling
open Forest.Templates.Raw

open System.Runtime.CompilerServices

[<Extension>]
type TemplateExtensions =
    [<Extension>]
    static member LoadTemplate(engine:Engine, NotNull "name" name:string) =
        let provider = engine.Context.TemplateProvider
        engine.Update(
            fun e -> 
                name 
                |> Raw.loadTemplate provider
                |> TemplateCompiler.compile
                |> (downcast e:ForestEngineAdapter).Runtime.Update)

