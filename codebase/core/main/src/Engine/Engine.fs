namespace Forest

open System
open Axle
open Axle.Verification
open Forest.UI
open Forest.Templates.Raw


type [<Sealed;NoComparison>] ForestResult internal (state : State, changeList : ChangeList, ctx : IForestContext) = 
    do
        ignore <| (|NotNull|) "state" state
        ignore <| (|NotNull|) "changeList" changeList

    member __.Render ([<ParamArray>] renderers : IDomProcessor array) =
        state |> State.traverse (ForestDomRenderer(renderers |> Seq.ofArray, ctx))

    override __.ToString() = state.Tree.ToString()

    member internal __.State with get() = state
    member __.ChangeList with get() = changeList

type [<Sealed;NoComparison;NoEquality>] internal ForestRuntimeEngine(runtime : ForestRuntime) =

    member __.ExecuteCommand command target message = 
        Runtime.Operation.InvokeCommand(command, target, message) 
        |> runtime.Do 
        |> Runtime.resolve ignore

    [<Obsolete>]
    member __.RegisterSystemView<'sv when 'sv :> ISystemView> () = 
        let descriptor = 
            match typeof<'sv> |> runtime.Context.ViewRegistry.GetDescriptor |> null2vopt with
            | ValueNone -> 
                runtime.Context.ViewRegistry
                |> ViewRegistry.registerViewType typeof<'sv> 
                |> ViewRegistry.getDescriptorByType typeof<'sv> 
            | ValueSome d -> d
        let key = TreeNode.shell |> TreeNode.newKey TreeNode.shell.Region descriptor.Name
        runtime.GetOrActivateView<'sv> key

    member __.SendMessage<'msg> (message : 'msg) = 
        Runtime.Operation.DispatchMessage(message, [||])
        |> runtime.Do
        |> Runtime.resolve ignore

    member __.LoadTree (name) =
        name 
        |> Raw.loadTemplate runtime.Context.TemplateProvider
        |> Templates.TemplateCompiler.compileOps
        |> Runtime.Operation.Multiple
        |> runtime.Do
        |> Runtime.resolve ignore

    member __.LoadTree (name, message) =
        name 
        |> Raw.loadTemplate runtime.Context.TemplateProvider
        |> Templates.TemplateCompiler.compileOps
        |> List.append [Runtime.Operation.DispatchMessage(message, [||])]
        |> Runtime.Operation.Multiple
        |> runtime.Do   
        |> Runtime.resolve ignore

    member internal __.Runtime with get() = runtime

    interface IForestEngine with
        member this.RegisterSystemView<'sv when 'sv :> ISystemView>() = this.RegisterSystemView<'sv>()
    interface IMessageDispatcher with
        member this.SendMessage<'msg> msg = this.SendMessage<'msg> msg
    interface ICommandDispatcher with
        member this.ExecuteCommand c t m = this.ExecuteCommand c t m
    interface ITreeNavigator with
        member this.LoadTree name = this.LoadTree name
        member this.LoadTree (name, msg) = this.LoadTree (name, msg)

module ForestEngineStateDecorator =
    type private T (engine) =
        inherit ForestEngineDecorator(engine)

    let Decorate engine = upcast T (engine) : IForestEngine
        

type [<Sealed;NoComparison>] ForestStateManager private (ctx : IForestContext, state : State, syncRoot : obj) =
    [<DefaultValue>]
    val mutable private _rt : ForestRuntime voption
    let mutable st : State = state
    new (ctx : IForestContext) = ForestStateManager(ctx, State.initial, obj())

    static member inline private toResult (rt : ForestRuntime) (fuid : Fuid option) (state : State) =
        match rt.Deconstruct() with 
        | (a, b, c, cl) -> 
        let newState = 
            match fuid with
            | Some f -> State.createWithFuid(a, b, c, f)
            | None -> State.create(a, b, c)
        ForestResult(newState, ChangeList(state.Hash, cl, newState.Fuid), rt.Context)

    member inline private this.WrapAction (s : State option) (action : ForestRuntime -> Fuid option) =
        let syncAction rt =
            lock syncRoot (fun () -> action rt)

        let actionInitialState = match s with Some x -> x | None -> st
        let result =
            try match this._rt with
                | ValueNone -> 
                    use rt = ForestRuntime.Create(actionInitialState.Tree, actionInitialState.Models, actionInitialState.ViewStates, ctx)
                    this._rt <- ValueSome rt
                    let result = actionInitialState |> ForestStateManager.toResult rt (syncAction rt)
                    this._rt <- ValueNone
                    result
                | ValueSome rt -> actionInitialState |> ForestStateManager.toResult rt (syncAction rt)
            with :? ForestException as e -> 
                let resetState = State.initial
                use rt = ForestRuntime.Create(resetState.Tree, resetState.Models, resetState.ViewStates, ctx)
                //let errorView = Error.Show rt
                // TODO: set error data
                resetState |> ForestStateManager.toResult rt (Some st.Fuid)
        st <- result.State
        result

    member internal this.SwapState (initialState : State option) (operation : Runtime.Operation) : ForestResult =
        (fun (rt : ForestRuntime) ->
            operation |> rt.Do |> Runtime.resolve ignore
            None
        ) |> this.WrapAction initialState

    member internal this.Update (operation : System.Action<IForestEngine>) : ForestResult = 
        (fun rt ->
            ForestRuntimeEngine(rt) |> operation.Invoke
            None
        ) |> this.WrapAction None

    member internal this.Sync (changes : ChangeList) : ForestResult =
        let rec _applyChangelog (rt : ForestRuntime) (cl : StateChange List) =
            match cl with
            | [] -> None
            | head::tail -> 
                match head |> rt.Apply with
                | Some e -> Some e
                | None -> _applyChangelog rt tail
        (fun rt ->
            match _applyChangelog rt (changes.ToList()) with
            | None -> Some changes.Fuid
            | Some e -> failwith "error" //TODO
        ) |> this.WrapAction None

    member internal __.Context with get() = ctx
    member internal __.InitialResult with get() = ForestResult(State.initial, ChangeList(State.initial.Hash, List.empty, State.initial.Fuid), ctx)
