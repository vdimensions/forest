namespace Forest
open Forest.Dom
open System;
open System.Collections.Generic

type [<Interface>] IViewRegistry =
    abstract member Register: t: Type -> IViewRegistry
    abstract member Register<'T when 'T:> IView> : unit -> IViewRegistry
    abstract member Resolve: viewNode: IViewNode -> IView
    abstract member Resolve: name: string -> IView
    abstract member GetViewMetadata: name: string -> IViewMetadata option
and [<Interface>] IView =
    abstract Publish<'M> : message: 'M * [<ParamArray>] topics: string[] -> unit
    abstract Regions: IIndex<string, IRegion> with get
    abstract ViewModel: obj
and [<Interface>] IRegion = 
    abstract Views: IIndex<string, IView> with get 
and [<Interface>] IViewModelStateRepository = 
    abstract member SuspendState: Path*obj -> unit 
    abstract member SuspendState: v:IView -> unit
    abstract member ResumeState: path: Path -> obj
and [<Interface>] IForestContext =
    abstract Registry: IViewRegistry with get
and [<Interface>] IContainer = 
    abstract member Resolve: vm: IViewMetadata -> IView

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
    abstract member Submit: ctx: IForestContext -> unit

type [<Interface>] IForestEngine =
    abstract member CreateDomIndex: ctx: IForestContext -> data: obj -> IDomIndex
    abstract member Execute: rt: IForestContext -> node: IViewNode -> IView

[<Interface>]
type internal IForestContextAware =
    abstract member InitializeContext: ctx: IForestContext -> unit


[<Flags>]
type internal ViewChange =
    | ViewModel

[<AbstractClass>]
type AbstractView<'T when 'T: (new: unit -> 'T)> () as self =
    let mutable _viewModel : 'T  = new 'T()
    let _viewChangeLog: ICollection<ViewChange> = upcast LinkedList<ViewChange>()
    member this.Publish<'M> (message: 'M, [<ParamArray>] topics: string[]) = 
        ()
    member this.ViewModel
        with get ():'T = _viewModel
        and set (v: 'T) = _viewModel <- v

    interface IViewInternal with
        member x.Submit rt =
           ()
    interface IView<'T> with
        member this.ViewModel
            with get() = self.ViewModel
            and set v = 
                self.ViewModel <- v
                _viewChangeLog.Add ViewChange.ViewModel
    interface IView with
        member this.Publish (m, t) : unit = self.Publish (m, t)
        member this.Regions with get() = raise (System.NotImplementedException())
        member this.ViewModel with get() = upcast self.ViewModel
