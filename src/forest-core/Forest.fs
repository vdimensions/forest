namespace Forest
open System;
open System.Collections.Generic
open System.Reflection
open Forest.Dom

[<Obsolete>]
type IRegistry = 
    [<Obsolete>]abstract member Register<'T>    : name: string -> model: 'T -> IRegistry
    [<Obsolete>]abstract member Unregister      : name: string -> IRegistry
    abstract member Lookup          : name: string -> IViewNode

type IViewRegistry = 
    abstract member Resolve: viewNode: IViewNode -> IView
    abstract member Resolve: name: string -> IView
    abstract member Register: t: Type -> IViewRegistry
    abstract member Register<'T when 'T:> IView> : unit -> IViewRegistry
and IView =
    abstract Regions: IIndex<string, IRegion> with get
    abstract ViewModel: obj
and IRegion =
    abstract Views: IIndex<string, IView> with get
and IForestRuntime =
    abstract Registry: IViewRegistry with get

type IView<'T when 'T: (new: unit -> 'T)> =
    inherit IView
    abstract ViewModel: 'T with get, set

// interal functionality needed by the forest engine
type internal IViewInternal =
    inherit IView
    //abstract member ExchangeViewModel : model : 'T -> IView<'T>
    /// <summary>
    /// Submits the current view state to the specified <see cref="IForestContext"/> instance.
    /// </summary>
    /// <param name="context">
    /// The <see cref="IForestContext" /> instance to manage the state of the current view.
    /// </param>
    abstract member Submit: context : IForestRuntime -> unit

type IForestEngine =
    abstract member Execute<'T when 'T: (new: unit -> 'T)> : rt: IForestRuntime -> node: IViewNode -> IView<'T>


type internal IForestContextAware =
    abstract member InitializeContext : rt : IForestRuntime -> unit

//type internal IParentRegionAware =
//    abstract member InitializeParentRegion : region : IRegion -> unit

[<Flags>]
type internal ViewChange =
| None          = 0b00
| ViewModel     = 0b01
| RegionState   = 0b10

[<AbstractClass>]
type AbstractView<'T when 'T: (new: unit -> 'T)> () =
    let mutable _viewModel : 'T  = new 'T()
    let mutable _viewChanges : ViewChange = ViewChange.None
    //interface IForestContextAware with
    //    member x.InitializeContext ctx =
    //       context <- ctx
    //       ()
    //interface IView<'T> with
    //    member this.ViewModel
    //        with get () : 'T = _viewModel
    //        and set value =
    //            _viewModel <- value
    //            _viewChanges <- _viewChanges ||| ViewChange.ViewModel
    //interface IView with
    //    member this.Submit context = ()
    //    member this.ViewModel: obj = upcast _viewModel



type ViewRegistryError = 
| ViewError of View.Error
| CommandError of Command.Error

