namespace Forest

open Forest
open Forest.Dom

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
    | Multiple of ForestOperation[]

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
        | NoSuchView of string
        | UnexpectedModelState of Node

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

    type [<Sealed>] private T =
        // transferrable across machines
        val mutable private _viewModelData: WriteableIndex<obj, Node>
        // not transferrable across machines
        val mutable private _viewStateData: WriteableIndex<ViewState[], Node>

        member this.Update (ctx: IForestContext, change: ForestOperation) =
            let inline makeViewID id = ID(DomNodeType.View, id)
            let rec processChanges(c: ForestOperation): Result<obj, Error> =
                let mutable result: Result<obj, Error> = Success null
                match c with
                | Multiple changes -> 
                    let mutable errors: Error[] = Array.empty
                    for c in changes do 
                        let innerResult = processChanges c
                        match innerResult with
                        | Success _ -> ()
                        | Failure error ->
                            errors <- Array.concat [|errors; [|error|]|]
                            ()
                    result <- match errors with
                    | [||] -> Success null
                    | _ -> Failure (MultipleErrors errors)
                    ()
                | InstantiateView (node, viewID) ->
                    match node with
                    | Region regionID ->
                        let instanceID = Node.Leaf(node, makeViewID(Guid.NewGuid()))
                        match ctx.ViewRegistry.GetViewMetadata(viewID) with
                        | Some descriptor ->
                            let viewInstance = (ctx.ViewRegistry.Resolve viewID) :?> IViewInternal
                            let viewState = ViewState(instanceID, descriptor)
                            viewState.View <- viewInstance
                            
                            match this._viewModelData.[instanceID] with
                            | Some _ ->
                                result <- Failure (UnexpectedModelState instanceID)
                                ()
                            | None ->
                                this._viewModelData <- this._viewModelData.Insert instanceID viewInstance.ViewModel
                                ()

                            match result with
                            | Success _ ->
                                match this._viewStateData.[node] with
                                | Some states ->
                                     this._viewStateData <- this._viewStateData.Insert instanceID (Array.concat [| states; [|viewState|] |])
                                | None -> 
                                     this._viewStateData <- this._viewStateData.Insert instanceID [|viewState|]
                            | _ -> ()
                            ()
                        | None ->
                            result <- Failure (NoSuchView viewID)
                            ()
                        ()
                    | _ -> ()
                | _ -> ()
                result
            processChanges change
