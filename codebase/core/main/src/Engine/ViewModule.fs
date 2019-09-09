namespace Forest

open System
open Forest
open Forest.Engine.Instructions


[<RequireQualifiedAccessAttribute>]
[<CompiledName("View")>]
module View =
    type [<NoComparison>] Error =
        | ViewAttributeMissing of nonAnnotatedViewType : Type
        | ViewTypeIsAbstract of instruction : InstantiateViewInstruction
        | NonGenericView of instruction : InstantiateViewInstruction
        | InstantiationError of instruction : InstantiateViewInstruction * cause : exn

    let resolveError = function
        | ViewAttributeMissing t -> upcast ViewAttributeMissingException(t) : exn
        | ViewTypeIsAbstract t -> upcast ViewTypeIsAbstractException(t) : exn
        | NonGenericView t -> upcast ViewTypeIsNotGenericException(t) : exn
        | InstantiationError (h, e) -> upcast ViewInstantiationException(h, e)
    let handleError(e : Error) = e |> resolveError |> raise
