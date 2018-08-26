namespace Forest

open Forest.NullHandling

open System.Runtime.CompilerServices


type [<Interface>] IForestEngine =
    abstract member ActivateView: name:string -> 'a when 'a:>IView
    abstract member GetOrActivateView: name:string -> 'a when 'a:>IView
    abstract member Publish: message:'M -> unit
    abstract member ExecuteCommand: target:HierarchyKey -> command:string -> arg:'M -> unit

module internal MessagingRemoteControl =
    [<Literal>]
    let Name = "MessagingRemoteControl"
    let private Key = HierarchyKey.shell |> HierarchyKey.newKey HierarchyKey.shell.Region Name
    type [<Sealed>] ViewModel() = override __.Equals o = true
    [<View(Name)>]
    type [<Sealed>] View() =
        inherit AbstractView<ViewModel>() with
        override __.Load() = ()
    let Reg (ctx:IForestContext) = 
        match null2opt <| ctx.ViewRegistry.GetDescriptor Name with
        | Some _ -> ()
        | None -> ctx.ViewRegistry.Register typeof<View> |> ignore
    let Get (ms:MutableScope) : View = Key |> ms.GetOrActivateView<View>

module internal Error =
    [<Literal>]
    let Name = "Error"
    let private Key = HierarchyKey.shell |> HierarchyKey.newKey HierarchyKey.shell.Region Name
    type ViewModel() = class end
    [<View(Name)>]
    type View() =
        inherit AbstractView<ViewModel>() with
        override __.Load() = ()
    let Reg (ctx:IForestContext) = 
        match null2opt <| ctx.ViewRegistry.GetDescriptor Name with
        | Some _ -> ()
        | None -> ctx.ViewRegistry.Register typeof<View> |> ignore
    let Show (ms:MutableScope) : View = 
        Key |> ms.GetOrActivateView

type private ForestEngineAdapter(scope: MutableScope) =
    let mutable _messagingRC = nil<MessagingRemoteControl.View>
    do
        MessagingRemoteControl.Reg scope.Context
        _messagingRC <- scope |> MessagingRemoteControl.Get
        ()
    interface IForestEngine with
        member __.ActivateView (name) : 'a when 'a:>IView = 
            let result = HierarchyKey.shell |> HierarchyKey.newKey HierarchyKey.shell.Region name |> scope.ActivateView
            downcast result:'a
        member __.GetOrActivateView name = 
            HierarchyKey.shell |> HierarchyKey.newKey HierarchyKey.shell.Region name |> scope.GetOrActivateView
        member __.ExecuteCommand target command message = 
            ForestOperation.InvokeCommand(target, command, message) |> scope.Update
        member __.Publish message = 
            _messagingRC.Publish(message, System.String.Empty)

[<Extension>]
[<AutoOpen>]
type StateExtensions =
    static member inline private toResult (scope:MutableScope) (fuid:Fuid option) (state:State) =
        match scope.Deconstruct() with 
        | (a, b, c, cl) -> 
        let newState = 
            match fuid with
            | Some f -> State.createWithFuid(a, b, c, f)
            | None -> State.create(a, b, c)
        EngineResult(newState, ChangeList(state.Hash, cl, newState.Fuid))

    [<Extension>]
    static member Update (state:State, ctx:IForestContext, operation:System.Action<IForestEngine>):EngineResult =
        try 
            use mutationScope = MutableScope.Create(state.Hierarchy, state.ViewModels, state.ViewStates, ctx)
            let adapter = new ForestEngineAdapter(mutationScope)
            operation.Invoke adapter
            state |> StateExtensions.toResult mutationScope None
        with
        | e -> 
            let se = State.empty
            use mutationScope = MutableScope.Create(se.Hierarchy, se.ViewModels, se.ViewStates, ctx)
            let errorView = Error.Show mutationScope
            // TODO: set error data
            se |> StateExtensions.toResult mutationScope None
    [<Extension>]
    static member Sync (state:State, ctx:IForestContext, changes:ChangeList):EngineResult =
        try 
            use mutationScope = MutableScope.Create(state.Hierarchy, state.ViewModels, state.ViewStates, ctx)
            let rec _applyChangelog (ms: MutableScope) (cl: StateChange List) =
                match cl with
                | [] -> None
                | head::tail -> 
                    match head |> ms.Apply with
                    | Some e -> Some e
                    | None -> _applyChangelog ms tail
            match _applyChangelog mutationScope (changes.ToList()) with
            | None -> state |> StateExtensions.toResult mutationScope (Some changes.Fuid)
            | Some e -> failwith "error" //TODO
        with
        | e -> 
            let se = State.empty
            use mutationScope = MutableScope.Create(se.Hierarchy, se.ViewModels, se.ViewStates, ctx)
            let errorView = Error.Show mutationScope
            // TODO: set error data
            se |> StateExtensions.toResult mutationScope None
