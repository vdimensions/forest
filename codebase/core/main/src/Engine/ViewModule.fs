namespace Forest

open System
open System.Reflection
open Axle
open Axle.Verification
open Forest
open Forest.ComponentModel
open Forest.Engine
open Forest.Engine.Instructions


[<RequireQualifiedAccessAttribute>]
[<CompiledName("View")>]
module View =
    type [<NoComparison>] Error =
        | ViewAttributeMissing of nonAnnotatedViewType : Type
        | ViewTypeIsAbstract of instruction : InstantiateViewInstruction
        | NonGenericView of instruction : InstantiateViewInstruction
        | InstantiationError of instruction : InstantiateViewInstruction * cause : exn

    #if NETSTANDARD
    let inline private _selectViewModelTypes (tt : TypeInfo) =
    #else
    let inline private _selectViewModelTypes (tt : Type) =
    #endif
        let isGenericView = tt.IsGenericType && (tt.GetGenericTypeDefinition() = typedefof<IView<_>>)
        match isGenericView with
        | true -> Some (tt.GetGenericArguments().[0])
        | false -> None

    let inline private _tryGetViewModelType (t : Type) = 
        t.GetInterfaces()
        #if NETSTANDARD
        |> Seq.map (fun t -> t.GetTypeInfo())
        #endif
        |> Seq.choose _selectViewModelTypes
        |> Seq.tryHead

    //let getModelType (NotNull "viewType" viewType : Type) = Result.some (NonGenericView viewType) (_tryGetViewModelType viewType)

    let resolveError = function
        | ViewAttributeMissing t -> upcast ViewAttributeMissingException(t) : exn
        | ViewTypeIsAbstract t -> upcast ViewTypeIsAbstractException(t) : exn
        | NonGenericView t -> upcast ViewTypeIsNotGenericException(t) : exn
        | InstantiationError (h, e) -> upcast ViewInstantiationException(h, e)
    let handleError(e : Error) = e |> resolveError |> raise
