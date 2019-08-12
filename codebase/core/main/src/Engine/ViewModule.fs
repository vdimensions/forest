namespace Forest

open System
open System.Reflection
open Axle
open Axle.Verification
open Forest
open Forest.ComponentModel


[<RequireQualifiedAccessAttribute>]
[<CompiledName("View")>]
module View =
    type [<Struct;NoComparison>] Error =
        | ViewAttributeMissing of nonAnnotatedViewType : Type
        | ViewTypeIsAbstract of abstractViewType : Type
        | NonGenericView of nonGenericViewType : Type
        | InstantiationError of viewHandle : ViewHandle * cause : exn

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

    let getModelType (NotNull "viewType" viewType : Type) = Result.some (NonGenericView viewType) (_tryGetViewModelType viewType)

    let resolveError = function
        | ViewAttributeMissing t -> upcast ViewAttributeMissingException(t) : exn
        | ViewTypeIsAbstract t -> upcast ViewTypeIsAbstractException(t) : exn
        | NonGenericView t -> upcast ArgumentException("t", String.Format("The type `{0}` does not implement the {1} interface. ", t.FullName, typedefof<IView<_>>.FullName)) : exn
        | InstantiationError (h, e) -> 
            match h with
            | ByType t -> upcast ArgumentException("h", String.Format("Failed to instantiate view type `{0}` See inner exception for more details. ", t.FullName), e) : exn
            | ByName n -> upcast ArgumentException("h", String.Format("Failed to instantiate view `{0}` See inner exception for more details. ", n, typedefof<IView<_>>.FullName), e) : exn
    let handleError(e : Error) = e |> resolveError |> raise

    type [<Sealed;NoComparison>] Factory() = 
        member __.Resolve (NotNull "descriptor" descriptor : IViewDescriptor) : IView = 
            let flags = BindingFlags.Public|||BindingFlags.Instance
            let constructors = 
                descriptor.ViewType.GetConstructors(flags) 
                |> Array.toList
                |> List.filter (fun c -> c.GetParameters().Length = 0) 
            match constructors with
            | [] -> raise (InvalidOperationException(String.Format("View `{0}` does not have suitable constructor", descriptor.ViewType.FullName)))
            | head::_ -> downcast head.Invoke([||]) : IView

        member __.Resolve (NotNull "descriptor" descriptor : IViewDescriptor, NotNull "model" model : obj) : IView = 
            let flags = BindingFlags.Public|||BindingFlags.Instance
            let constructors = 
                descriptor.ViewType.GetConstructors(flags) 
                |> Array.toList
                |> List.filter (fun c -> c.GetParameters().Length = 1 && c.GetParameters().[0].ParameterType.GetTypeInfo().IsAssignableFrom(model.GetType().GetTypeInfo())) 
            match constructors with
            | [] -> raise (InvalidOperationException(String.Format("View `{0}` does not have suitable constructor", descriptor.ViewType.FullName)))
            | head::_ -> downcast head.Invoke([|model|]) : IView
        
        interface IViewFactory with 
            member this.Resolve d = this.Resolve d
            member this.Resolve (d, m) = this.Resolve(d, m)
