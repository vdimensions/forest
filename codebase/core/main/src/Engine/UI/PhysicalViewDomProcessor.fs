namespace Forest.UI

open System
open Forest


[<NoComparison;StructuralEquality>] 
type internal NodeState =
    | NewNode of node : DomNode
    | UpdatedNode of node : DomNode

[<Sealed;NoComparison;NoEquality>] 
type internal PhysicalViewDomProcessor =
    val mutable private physicalViews : Map<thash, IPhysicalView>
    /// Contains a list of nodes to be retained as they represent present views.
    val mutable private _nodesToPreserve : thash Set
    val mutable private _nodeStates : NodeState list
    val private _engine : IForestEngine
    val private _renderer : IPhysicalViewRenderer

    private new (physicalViews, engine : IForestEngine, renderer : IPhysicalViewRenderer) = 
        { 
            _engine = engine; 
            _renderer = renderer; 
            physicalViews = physicalViews; 
            _nodesToPreserve = Set.empty; 
            _nodeStates = List.empty 
        }
    new (engine : IForestEngine, renderer : IPhysicalViewRenderer) = PhysicalViewDomProcessor(Map.empty, engine, renderer)

    member this.PhysicalViews 
        with get() = this.physicalViews
        and set pv = this.physicalViews <- pv

    interface IDomProcessor with
        member this.ProcessNode n =
            this._nodesToPreserve <- this._nodesToPreserve |> Set.add n.Hash
            this._nodeStates <- 
                match this.physicalViews.TryFind n.Hash with
                | Some _ -> (UpdatedNode n)::this._nodeStates
                | None -> (NewNode n)::this._nodeStates
            n

        member this.Complete(_ : DomNode list) = 
            let physicalViews = System.Collections.Generic.Dictionary<thash, IPhysicalView>(StringComparer.Ordinal)
            for kvp in this.physicalViews do
                if not <| this._nodesToPreserve.Contains(kvp.Key) then
                    kvp.Value.Dispose()
                else
                    physicalViews.Add(kvp.Key, kvp.Value)

            let inline dictTryFind (dict : System.Collections.Generic.Dictionary<thash, 'PV>, key) =
                match dict.TryGetValue key with
                | (false, _) -> None
                | (true, value) -> Some value

            // store tuples of renderer and node to initiate an update call after the views are rendered
            let mutable updateCallArguments = List.empty

            for nodeState in this._nodeStates do
                let (n, p, isNewView) = 
                    match nodeState with 
                    | NewNode n -> (n, n.Parent, true) 
                    | UpdatedNode n -> (n, n.Parent, false)
                match (p, isNewView) with
                | (Some p, isNewView) -> 
                    match (isNewView, dictTryFind(physicalViews, p.Hash), dictTryFind(physicalViews, n.Hash)) with
                    | (true, Some parent, None) ->
                        let physicalView = this._renderer.CreateNestedPhysicalView this._engine parent n
                        updateCallArguments <- (physicalView,n)::updateCallArguments
                        physicalViews.Add(n.Hash, physicalView)
                    | (true, Some _, Some _) ->
                        invalidOp(String.Format("Expecting physical view {0} #{1} not to contain child {2} #{3}", p.Name, p.Hash, n.Name, n.Hash))
                    | (false, Some _, Some renderer) ->
                        updateCallArguments <- (renderer,n)::updateCallArguments
                    | (false, Some _, None) ->
                        invalidOp(String.Format("Could not locate physical view {0} #{1}", n.Name, n.Hash))
                    | (_, None, _) ->
                        invalidOp(String.Format("Could not locate physical view {0} #{1} that should be parent of {2} #{3}", p.Name, p.Hash, n.Name, n.Hash))
                | (None, isNewView) ->
                    match (isNewView, dictTryFind(physicalViews, n.Hash)) with
                    | (true, Some _) ->
                        invalidOp(String.Format("Physical view {0} #{1} already exists", n.Name, n.Hash))
                    | (true, None) ->
                        let physicalView = this._renderer.CreatePhysicalView this._engine n
                        updateCallArguments <- (physicalView,n)::updateCallArguments
                        physicalViews.Add(n.Hash, physicalView)
                    | (false, None) -> 
                        invalidOp(String.Format("Could not locate physical view {0} #{1}", n.Name, n.Hash))
                    | (false, Some renderer) ->
                        updateCallArguments <- (renderer,n)::updateCallArguments

            this.physicalViews <- physicalViews |> Seq.map (|KeyValue|) |> Map.ofSeq
            this._nodesToPreserve <- Set.empty
            this._nodeStates <- List.empty

            // Initiate the update calls for renderers. This must happen after the renderers collections is up to date
            // because an update call might trigger an Forest command and case re-rendering before the rendered views are processed.
            // The consequences of a command execution during the physical UI construction could result in attempt to render the 
            // same physical view twice, thus causing an exception.
            // At this point, invoking those update calls is permitted.
            for (renderer,n) in updateCallArguments do 
                renderer.Update n

[<AbstractClass;NoComparison>] 
type AbstractPhysicalViewRenderer<'PV when 'PV :> IPhysicalView>() =
    abstract member CreatePhysicalView: engine : IForestEngine -> n : DomNode -> 'PV
    abstract member CreateNestedPhysicalView: engine : IForestEngine -> parent : 'PV -> n : DomNode  -> 'PV

    interface IPhysicalViewRenderer with
        member this.CreatePhysicalView engine n = 
            upcast this.CreatePhysicalView engine n : IPhysicalView
        member this.CreateNestedPhysicalView engine parent n = 
            upcast this.CreateNestedPhysicalView engine (parent :?> 'PV) n : IPhysicalView

    interface IPhysicalViewRenderer<'PV> with
        member this.CreatePhysicalViewG engine n = this.CreatePhysicalView engine n
        member this.CreateNestedPhysicalViewG engine parent n = this.CreateNestedPhysicalView engine parent n
