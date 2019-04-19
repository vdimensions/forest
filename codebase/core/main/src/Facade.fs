//namespace Forest
//
//open Axle.Verification
//open Forest.Templates
//open Forest.UI
//open System.Threading
//
///// An interface allowing communication between the physical application front-end and the Forest UI layer
//type [<Interface>] IForestFacade = 
//    inherit ICommandDispatcher
//    inherit IMessageDispatcher
//    abstract member LoadTree: tree : string -> unit
//    abstract member LoadTree: tree : string * message : 'msg -> unit
//    abstract member RegisterSystemView<'sv when 'sv :> ISystemView> : unit -> unit
//    abstract member Render<'PV when 'PV :> IPhysicalView> : IPhysicalViewRenderer<'PV> -> ForestResult -> unit
//
//type [<AbstractClass;NoComparison>] ForestFacadeProxy (facade : IForestFacade) =
//    abstract member LoadTree: facade: IForestFacade * name : string -> unit
//    default __.LoadTree (facade, name) = facade.LoadTree name
//    abstract member LoadTree: facade: IForestFacade * name : string * msg : 't -> unit
//    default __.LoadTree (facade, name, msg) = facade.LoadTree (name, msg)
//    abstract member RegisterSystemView<'sv when 'sv :> ISystemView> : IForestFacade -> unit
//    default __.RegisterSystemView<'sv when 'sv :> ISystemView> facade = facade.RegisterSystemView<'sv>()
//    abstract member Render<'pv when 'pv :> IPhysicalView> : IForestFacade -> IPhysicalViewRenderer<'pv> -> ForestResult-> unit
//    default __.Render<'pv when 'pv :> IPhysicalView> facade renderer result = facade.Render<'pv> renderer result
//    abstract member SendMessage<'msg> :  facade: IForestFacade -> 'msg -> unit
//    default __.SendMessage<'msg> facade msg = facade.SendMessage<'msg> msg
//    abstract member ExecuteCommand:  facade: IForestFacade -> cname -> thash -> obj -> unit
//    default __.ExecuteCommand facade name hash arg = facade.ExecuteCommand name hash arg
//    interface IForestFacade with
//        member this.LoadTree name = this.LoadTree (facade, name)
//        member this.LoadTree (name, msg) = this.LoadTree (facade, name, msg)
//        member this.RegisterSystemView<'sv when 'sv :> ISystemView>() = this.RegisterSystemView<'sv> facade
//        member this.Render renderer result = this.Render facade renderer result
//    interface IMessageDispatcher with
//        member this.SendMessage<'msg> (msg:'msg) = this.SendMessage<'msg> facade msg
//    interface ICommandDispatcher with
//        member this.ExecuteCommand name hash arg = this.ExecuteCommand facade name hash arg
//
//type [<Sealed;NoComparison;NoEquality>] DefaultForestFacade<'PV when 'PV :> IPhysicalView>(ctx : IForestContext, renderer : IPhysicalViewRenderer<'PV>) =
//    
//    let stateManager = ForestStateManager(ctx)
//
//    [<DefaultValue>]
//    val mutable private _pvd : PhysicalViewDomProcessor voption
//    /// A counter determining the number of nested facade calls.
//    /// It will be used to determine whether a render call will occur after a facade call.
//    /// For example, if a facade call is called from within the code triggered by another facade call
//    /// such as `LoadTree` being called from within `ExecuteCommand` or `SendMessage`, a render call
//    /// will not be issued for the `LoadTree` operation, but only for the respective encompassing operation.
//    [<VolatileField>]
//    let mutable nestingCount = ref 0
//
//    member private __.PerformSafeFacadeCall 
//            (fn : (unit -> ForestResult)) 
//            (renderer : IPhysicalViewRenderer<'PV>)
//            (facade : IForestFacade) : unit =
//        Interlocked.Increment(nestingCount) |> ignore
//        let mutable result : ForestResult option = None
//        try
//            result <- fn() |> Some
//        finally
//            if Interlocked.Decrement(nestingCount) = 0 then
//                match result with
//                | Some r -> facade.Render renderer r
//                | None -> ()
//
//    member private this.Render(renderer : IPhysicalViewRenderer<'PV>) (result : ForestResult) = 
//        let domProcessor : IDomProcessor =
//            match this._pvd with
//            | ValueNone -> 
//                let viewDomProcessor = PhysicalViewDomProcessor(this, renderer)
//                this._pvd <- ValueSome viewDomProcessor
//                viewDomProcessor
//            | ValueSome viewDomProcessor -> viewDomProcessor
//            :> IDomProcessor
//        result.Render domProcessor
//
//    member private __.ExecuteCommand name target arg = stateManager.Update(fun e -> e.ExecuteCommand name target arg)
//
//    member private __.SendMessage message = stateManager.Update(fun e -> e.SendMessage message)
//
//    member private __.LoadTree tree = stateManager.LoadTree tree
//
//    member private __.RegisterSystemView<'sv when 'sv :> ISystemView> () = stateManager.Update (fun e -> e.RegisterSystemView<'sv>() |> ignore)
//
//    interface ICommandDispatcher with
//        member this.ExecuteCommand name target arg = 
//            this.PerformSafeFacadeCall (fun () -> arg |> this.ExecuteCommand name target) renderer this
//
//    interface IMessageDispatcher with
//        member this.SendMessage(message : 'M): unit = 
//            this.PerformSafeFacadeCall (fun () -> message |> this.SendMessage) renderer this
//
//    interface IForestFacade with
//        member this.LoadTree tree = 
//            this.PerformSafeFacadeCall (fun () -> tree |> this.LoadTree) renderer this
//
//        member this.LoadTree(tree, msg) = 
//            this.PerformSafeFacadeCall (
//                fun () -> 
//                    let mutable result = tree |> this.LoadTree
//                    result <- this.SendMessage msg
//                    result
//                ) renderer this
//
//        member this.RegisterSystemView<'sv when 'sv :> ISystemView>() = 
//            this.PerformSafeFacadeCall (fun () -> this.RegisterSystemView<'sv>()) renderer this
//
//        member this.Render<'pv when 'pv :> IPhysicalView> (NotNull "renderer" renderer : IPhysicalViewRenderer<'pv>) (NotNull "result" result) = 
//            result |> this.Render (renderer :?> IPhysicalViewRenderer<'PV>)
