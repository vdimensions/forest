namespace Forest
open Forest.Dom
open System;


type [<Interface>] IViewRegistry = 
    abstract member Register: t: Type -> IViewRegistry
    abstract member Register<'T when 'T:> IView> : unit -> IViewRegistry
    abstract member Resolve: viewNode: IViewNode -> IView
    abstract member Resolve: name: string -> IView
    abstract member GetViewMetadata: name: string -> IViewMetadata option
and [<Interface>] IView =
    abstract Regions: IIndex<string, IRegion> with get
    abstract ViewModel: obj
and [<Interface>] IRegion =
    abstract Views: IIndex<string, IView> with get
and [<Interface>] IForestRuntime =
    abstract Registry: IViewRegistry with get

[<Interface>]
type IView<'T when 'T: (new: unit -> 'T)> =
    inherit IView
    abstract ViewModel: 'T with get, set

// internal functionality needed by the forest engine
[<Interface>]
type internal IViewInternal =
    inherit IView
    /// <summary>
    /// Submits the current view state to the specified <see cref="IForestContext"/> instance.
    /// </summary>
    /// <param name="context">
    /// The <see cref="IForestRuntime" /> instance to manage the state of the current view.
    /// </param>
    abstract member Submit: context : IForestRuntime -> unit

type [<Interface>] IForestEngine =
    abstract member CreateDomIndex: rt: IForestRuntime -> data: obj -> IDomIndex
    abstract member Execute<'T when 'T: (new: unit -> 'T)> : rt: IForestRuntime -> node: IViewNode -> IView<'T>

[<Interface>]
type internal IForestContextAware =
    abstract member InitializeContext : rt : IForestRuntime -> unit


[<Flags>]
type internal ViewChange =
    | None          = 0b00
    | ViewModel     = 0b01
    | RegionState   = 0b10

[<AbstractClass>]
type AbstractView<'T when 'T: (new: unit -> 'T)> () as self =
    let mutable _viewModel : 'T  = new 'T()
    let mutable _viewChanges : ViewChange = ViewChange.None
    //interface IViewInternal with
    //    member x.InitializeContext ctx =
    //       context <- ctx
    //       ()
    member this.ViewModel
        with get ():'T = _viewModel
        and set (v: 'T) = _viewModel <- v

    interface IView<'T> with
        member this.ViewModel
            with get() = self.ViewModel
            and set v = 
                self.ViewModel <- v
                _viewChanges <- _viewChanges ||| ViewChange.ViewModel
    interface IView with
        member this.Regions with get() = raise (System.NotImplementedException())
        member this.ViewModel with get() = upcast self.ViewModel
