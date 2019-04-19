namespace Forest

open System
open Axle
open Axle.Verification
open Forest.UI
open Forest.Templates.Raw


module ForestExecutionEngine =
    /// The lowest forest engine tier. It performs all the forest operations directly against the forest execution context.
    type [<Sealed;NoComparison;NoEquality>] internal T() =
        [<DefaultValue>]
        val mutable private _executionContext : ForestExecutionContext voption

        member private this.ExecutionContext 
            with get() = 
                match this._executionContext with
                | ValueSome ec -> ec
                | ValueNone -> invalidOp "No execution context is available."
            and set value = this._executionContext <- value |> ValueSome

        member this.ExecutionContextMaybe 
            with get() = this._executionContext
            and set value = this._executionContext <- value

        member this.ExecuteCommand (NotNull "command" command) target message = 
            ExecutionContext.Operation.InvokeCommand(command, target, message) 
            |> this.ExecutionContext.Do 
            |> ExecutionContext.resolve ignore

        [<Obsolete>]
        member this.RegisterSystemView<'sv when 'sv :> ISystemView> () = 
            let descriptor = 
                match typeof<'sv> |> this.ExecutionContext.Context.ViewRegistry.GetDescriptor |> null2vopt with
                | ValueNone -> 
                    this.ExecutionContext.Context.ViewRegistry
                    |> ViewRegistry.registerViewType typeof<'sv> 
                    |> ViewRegistry.getDescriptorByType typeof<'sv> 
                | ValueSome d -> d
            let key = TreeNode.shell |> TreeNode.newKey TreeNode.shell.Region descriptor.Name
            this.ExecutionContext.GetOrActivateView<'sv> key

        member this.SendMessage<'msg> (message : 'msg) = 
            ExecutionContext.Operation.DispatchMessage(message, [||])
            |> this.ExecutionContext.Do
            |> ExecutionContext.resolve ignore

        member this.LoadTree (NotNullOrEmpty "name" name) =
            name 
            |> Raw.loadTemplate this.ExecutionContext.Context.TemplateProvider
            |> Templates.TemplateCompiler.compileOps
            |> ExecutionContext.Operation.Multiple
            |> this.ExecutionContext.Do
            |> ExecutionContext.resolve ignore

        member this.LoadTree (NotNullOrEmpty "name" name, message) =
            name 
            |> Raw.loadTemplate this.ExecutionContext.Context.TemplateProvider
            |> Templates.TemplateCompiler.compileOps
            |> List.append [ExecutionContext.Operation.DispatchMessage(message, [||])]
            |> ExecutionContext.Operation.Multiple
            |> this.ExecutionContext.Do   
            |> ExecutionContext.resolve ignore

        interface IForestEngine with
            member this.RegisterSystemView<'sv when 'sv :> ISystemView>() = this.RegisterSystemView<'sv>()
        interface IMessageDispatcher with
            member this.SendMessage<'msg> msg = this.SendMessage<'msg> msg
        interface ICommandDispatcher with
            member this.ExecuteCommand c t m = this.ExecuteCommand c t m
        interface ITreeNavigator with
            member this.LoadTree name = this.LoadTree name
            member this.LoadTree (name, msg) = this.LoadTree (name, msg)

module ForestStateEngine =
    type private T private (engine : ForestExecutionEngine.T, ctx : IForestContext, sp : ForestStateManager) =
        inherit ForestEngineDecorator<ForestExecutionEngine.T>(engine)
        new (engine, ctx, renderer : IPhysicalViewRenderer, sp : IForestStateProvider) = T (engine, ctx, ForestStateManager(renderer, sp))

        member inline private this.WrapAction (s : State option) (action : ForestExecutionEngine.T -> 'a) (engine : ForestExecutionEngine.T) : 'a =
            match engine.ExecutionContextMaybe with
            | ValueNone ->
                let scope = sp.BeginStateScope(ctx, this)
                try 
                    engine.ExecutionContextMaybe <- ValueSome scope.ExecutionContext
                    let actionResult = action engine
                    engine.ExecutionContextMaybe <- ValueNone

                    actionResult
                finally
                    sp.EndStateScope(scope)
            | ValueSome _ -> 
                //let actionResult = action engine
                //let result = initialState |> toResult ec
                action engine

        override this.ExecuteCommand engine command target arg =
            this.WrapAction None (fun e -> e.ExecuteCommand command target arg) engine

        override this.SendMessage engine msg =
            this.WrapAction None (fun e -> e.SendMessage msg) engine

        override this.LoadTree (engine, name) =
            this.WrapAction None (fun e -> e.LoadTree name) engine

        override this.LoadTree (engine, name, msg) =
            this.WrapAction None (fun e -> e.LoadTree (name, msg)) engine

        [<Obsolete>]
        override this.RegisterSystemView<'sv when 'sv :> ISystemView> engine =
            this.WrapAction None (fun e -> e.RegisterSystemView<'sv> ()) engine

    let internal Create ctx renderer sp engine = upcast T(engine, ctx, renderer, sp) : IForestEngine   

//type [<Sealed;NoComparison>] ForestStateManager private (ctx : IForestContext, state : State, syncRoot : obj) =
//    [<DefaultValue>]
//    val mutable private _rt : ForestExecutionContext voption
//    let mutable st : State = state
//    new (ctx : IForestContext) = ForestStateManager(ctx, State.initial, obj())
//
//    static member inline private toResult (rt : ForestExecutionContext) (fuid : Fuid option) (state : State) =
//        match rt.Deconstruct() with 
//        | (a, b, c, cl) -> 
//        let newState = 
//            match fuid with
//            | Some f -> State.createWithFuid(a, b, c, f)
//            | None -> State.create(a, b, c)
//        ForestResult(newState, ChangeList(state.Hash, cl, newState.Fuid), rt.Context)
//
//    member inline private this.WrapAction (s : State option) (action : ForestExecutionContext -> Fuid option) =
//        let syncAction rt =
//            lock syncRoot (fun () -> action rt)
//
//        let actionInitialState = match s with Some x -> x | None -> st
//        let result =
//            try match this._rt with
//                | ValueNone -> 
//                    use rt = ForestExecutionContext.Create(actionInitialState.Tree, actionInitialState.Models, actionInitialState.ViewStates, ctx)
//                    this._rt <- ValueSome rt
//                    let result = actionInitialState |> ForestStateManager.toResult rt (syncAction rt)
//                    this._rt <- ValueNone
//                    result
//                | ValueSome rt -> actionInitialState |> ForestStateManager.toResult rt (syncAction rt)
//            with :? ForestException as e -> 
//                let resetState = State.initial
//                use rt = ForestExecutionContext.Create(resetState.Tree, resetState.Models, resetState.ViewStates, ctx)
//                //let errorView = Error.Show rt
//                // TODO: set error data
//                resetState |> ForestStateManager.toResult rt (Some st.Fuid)
//        st <- result.State
//        result
//
//    member internal this.SwapState (initialState : State option) (operation : ExecutionContext.Operation) : ForestResult =
//        (fun (rt : ForestExecutionContext) ->
//            operation |> rt.Do |> ExecutionContext.resolve ignore
//            None
//        ) |> this.WrapAction initialState
//
//    member internal this.Update (operation : System.Action<IForestEngine>) : ForestResult = 
//        (fun rt ->
//            ForestExecutionEngine(rt) |> operation.Invoke
//            None
//        ) |> this.WrapAction None
//
//    member internal this.Sync (changes : ChangeList) : ForestResult =
//        let rec _applyChangelog (rt : ForestExecutionContext) (cl : StateChange List) =
//            match cl with
//            | [] -> None
//            | head::tail -> 
//                match head |> rt.Apply with
//                | Some e -> Some e
//                | None -> _applyChangelog rt tail
//        (fun rt ->
//            match _applyChangelog rt (changes.ToList()) with
//            | None -> Some changes.Fuid
//            | Some e -> failwith "error" //TODO
//        ) |> this.WrapAction None
//
//    member internal __.Context with get() = ctx
//    member internal __.InitialResult with get() = ForestResult(State.initial, ChangeList(State.initial.Hash, List.empty, State.initial.Fuid), ctx)
