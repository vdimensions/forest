namespace Forest
open Forest.Dom

//type [<Interface>] IForestState = 
    //abstract member Push: path: Path -> context: IForestContext -> IDomIndex
    //abstract member DomIndex: IDomIndex with get

// contains the active mutable forest state, such as the latest dom index and view state changes
type [<Sealed>] ForestState =
    val mutable private _domIndex: IDomIndex
    val mutable private _viewState: IViewState

type [<Sealed>] internal ViewState() =
    let mutable _viewInstance = Unchecked.defaultof<IViewInternal>
    let mutable _viewDescriptor = Unchecked.defaultof<IViewDescriptor>

    member this.ViewInstance
        with get() = _viewInstance
        and set(v) = _viewInstance <- v
    member this.ViewDescriptor
        with get() = _viewDescriptor
        and set(v) = _viewDescriptor <- v
   