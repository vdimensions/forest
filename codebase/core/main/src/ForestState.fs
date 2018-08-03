namespace Forest

open Forest
open Forest.Dom
open Forest.Rop

open System

//type [<Interface>] IForestState = 
    //abstract member Push: path: Path -> context: IForestContext -> IDomIndex
    //abstract member DomIndex: IDomIndex with get

type ID = DomNodeType * Guid
type Node = 
    | Root 
    | Leaf of Parent: Node * Target: ID

// ----------------------------------

type ForestOperation =
    | UpdateViewModel of Node*obj
    | DestroyView of Node
    | ReorderView of Node
    | HandleEvent of Node
    | DestroyRegion of Node
    | InstantiateView of Node * string
    | InvokeCommand of Node * string * obj
    | Multiple of ForestOperation list

type internal ForestStateData =
    | ViewState of Node * IViewDescriptor * IViewInternal
    | RegionState of Node * string
    
// contains the active mutable forest state, such as the latest dom index and view state changes

and [<Sealed>] internal ViewState(id: Node, descriptor: IViewDescriptor) =
    let mutable _viewInstance = Unchecked.defaultof<IViewInternal>

    member this.ID with get() = id
    member this.Descriptor with get() = descriptor
    member this.View
        with get() = _viewInstance
        and set(v: IViewInternal) = _viewInstance <- v
   

module State =
    type Error =
        | MultipleErrors of Error[]
        | ViewNotFound of string
        | UnexpectedModelState of Node
        | RegionNodeExpected of Node
        | ViewNodeExpected of Node

    let (|View|_|) (node: Node) = 
        match node with 
        | Node.Leaf(_, target) ->
            match target with
            | (DomNodeType.View, id) -> Some id
            | _ -> None
        | Root -> None

    let (|Region|_|) (node: Node) = 
        match node with 
        | Node.Leaf(_, target) ->
            match target with
            | (DomNodeType.Region, id) -> Some id
            | _ -> None
        | Root -> Some Guid.Empty


    type private StateData = { ModelData: WriteableIndex<obj, Node>; ViewStateData: WriteableIndex<ViewState[], Node> }

    let inline private _insertViewModel (view: IViewInternal) (instanceID: Node) (sd: StateData): Result<StateData, Error> =
        match sd.ModelData.[instanceID] with
        | Some _ -> Failure (UnexpectedModelState instanceID)
        | None ->
            let newVMData = sd.ModelData.Insert instanceID view.ViewModel
            Success { ModelData = newVMData; ViewStateData = sd.ViewStateData }

    let private _insertViewState(viewState: ViewState) (node: Node) (sd: StateData): Result<StateData, Error> =
        let vs = 
            match sd.ViewStateData.[node] with
            | Some states -> sd.ViewStateData.Insert node (Array.concat [| states; [|viewState|] |])
            | None -> sd.ViewStateData.Insert node [|viewState|]
        Success { ModelData = sd.ModelData; ViewStateData = vs }

    type [<Sealed>] private T =
        // transferrable across machines
        val mutable private _viewModelData: WriteableIndex<obj, Node>
        // not transferrable across machines
        val mutable private _viewStateData: WriteableIndex<ViewState[], Node>

        member this.Update (ctx: IForestContext, change: ForestOperation) =
            let inline makeViewID id = ID(DomNodeType.View, id)
            let rec processChanges(stateData: StateData, c: ForestOperation): Result<StateData, Error> =
                match c with
                | Multiple changes -> 
                    let rec loopStates(sd, c) =
                        match c with
                        | [] -> Success sd
                        | [change] -> processChanges(sd, change)
                        | head::tail ->
                            match processChanges(sd, head) with
                            | Success tmp -> loopStates(tmp, tail)
                            | Failure e -> Failure e
                    loopStates(stateData, changes)

                | InstantiateView (node, viewID) ->
                    match node with
                    | Region regionID ->
                        let instanceID = Node.Leaf(node, makeViewID(Guid.NewGuid()))
                        match ctx.ViewRegistry.GetViewMetadata(viewID) with
                        | Some descriptor ->
                            let viewInstance = (ctx.ViewRegistry.Resolve viewID) :?> IViewInternal
                            let viewState = ViewState(instanceID, descriptor)
                            viewState.View <- viewInstance

                            Success stateData 
                            >>= _insertViewModel viewInstance instanceID
                            >>= _insertViewState viewState node
                        | None -> Failure (ViewNotFound viewID)
                    | _ -> Failure (RegionNodeExpected node)
                | _ -> Success stateData
                
            match processChanges({ ModelData = this._viewModelData; ViewStateData = this._viewStateData }, change) with
            | Success state ->
                this._viewModelData <- state.ModelData
                this._viewStateData <- state.ViewStateData
                ()
            | Failure e ->
                // TODO exception
                ()
