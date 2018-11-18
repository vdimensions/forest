namespace Forest.UI

open System
open Forest


type [<NoComparison;StructuralEquality>] internal NodeState =
    | NewNode of node:DomNode
    | UpdatedNode of node:DomNode

type [<AbstractClass;NoComparison>] AbstractUIRenderer<'PV when 'PV :> IPhysicalView> =
    val mutable private renderers : Map<thash, 'PV>
    /// Contains a list of nodes to be retained as they represent present views.
    val mutable private nodesToPreserve : thash Set
    val mutable private nodeStates : NodeState list

    new() = { renderers = Map.empty; nodesToPreserve = Set.empty; nodeStates = List.empty }

    abstract member CreatePhysicalView: n : DomNode -> 'PV
    abstract member CreateNestedPhysicalView: n : DomNode * parent : 'PV -> 'PV

    interface IDomProcessor with
        member this.ProcessNode n =
            this.nodesToPreserve <- this.nodesToPreserve |> Set.add n.Hash
            // Message dispatcher is an internal component and must not be rendered
            if not(StringComparer.Ordinal.Equals(n.Name, MessageDispatcher.Name)) then
                this.nodeStates <- 
                    match this.renderers.TryFind n.Hash with
                    | Some _ -> (UpdatedNode n)::this.nodeStates
                    | None -> (NewNode n)::this.nodeStates
            n

        member this.Complete(_ : DomNode list) = 
            let renderers = System.Collections.Generic.Dictionary<thash, 'PV>(StringComparer.Ordinal)
            for kvp in this.renderers do
                if not <| this.nodesToPreserve.Contains(kvp.Key) then
                    kvp.Value.Dispose()
                else
                    renderers.Add(kvp.Key, kvp.Value)

            let inline dictTryFind (dict : System.Collections.Generic.Dictionary<thash, 'PV>, key) =
                match dict.TryGetValue key with
                | (false, _) -> None
                | (true, value) -> Some value

            // store tuples of renderer and node to initiate an update call after the views are rendered
            let mutable updateCalls = List.empty

            for nodeState in this.nodeStates do
                let (n, p, isNewView) = match nodeState with | NewNode n -> (n, n.Parent, true) | UpdatedNode n -> (n, n.Parent, false)
                match (p, isNewView) with
                | (Some p, isNewView) -> 
                    match (isNewView, dictTryFind(renderers, p.Hash), dictTryFind(renderers, n.Hash)) with
                    | (true, Some parent, None) ->
                        let renderer = this.CreateNestedPhysicalView(n, parent)
                        updateCalls <- (renderer,n)::updateCalls
                        renderers.Add(n.Hash, renderer)
                    | (true, Some _, Some _) ->
                        invalidOp(String.Format("Expecting physical view {0} #{1} not to contain child {2} #{3}", p.Name, p.Hash, n.Name, n.Hash))
                    | (false, Some _, Some renderer) ->
                        updateCalls <- (renderer,n)::updateCalls
                    | (false, Some _, None) ->
                        invalidOp(String.Format("Could not locate physical view {0} #{1}", n.Name, n.Hash))
                    | (_, None, _) ->
                        invalidOp(String.Format("Could not locate physical view {0} #{1} that should be parent of {2} #{3}", p.Name, p.Hash, n.Name, n.Hash))
                | (None, isNewView) ->
                    match (isNewView, dictTryFind(renderers, n.Hash)) with
                    | (true, Some _) ->
                        invalidOp(String.Format("Physical view {0} #{1} already exists", n.Name, n.Hash))
                    | (true, None) ->
                        let renderer = this.CreatePhysicalView(n)
                        updateCalls <- (renderer,n)::updateCalls
                        renderers.Add(n.Hash, renderer)
                    | (false, None) -> 
                        invalidOp(String.Format("Could not locate physical view {0} #{1}", n.Name, n.Hash))
                    | (false, Some renderer) ->
                        updateCalls <- (renderer,n)::updateCalls

            this.renderers <- renderers |> Seq.map ``|KeyValue|`` |> Map.ofSeq
            this.nodesToPreserve <- Set.empty
            this.nodeStates <- List.empty

            // Initiate the update calls for renderers. This must happen after the renderers collections is up to date
            // because an update call might trigger an Forest command and case re-rendering before the rendered views are processed.
            // The consequences of a command execution during the physical UI construction could result in attempt to render the 
            // same physical view twice, thus causing an exception.
            // At this point, invoking those update calls is permitted.
            for (renderer,n) in updateCalls do 
                renderer.Update n
