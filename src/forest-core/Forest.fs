namespace Forest

open System;
open System.Collections.Generic;

type [<AutoOpen>] IIndex<'T, 'TKey> =
    inherit IEnumerable<'T>

    abstract Count          : int with get
    abstract Keys           : IEnumerable<'TKey> with get
    abstract Item           : 'TKey -> 'T with get

type [<AutoOpen>] IMutableIndex<'T, 'TKey> =
    inherit IIndex<'T, 'TKey>
    abstract member Remove  : key: 'TKey -> IMutableIndex<'T, 'TKey>
    abstract member Insert  : key: 'TKey -> item: 'T -> IMutableIndex<'T, 'TKey>
    abstract member Clear   : unit -> IMutableIndex<'T, 'TKey>

type [<AutoOpen>] ICommand =
    abstract ArgumentType   : Type with get

type [<AutoOpen>] IViewNode = 
    abstract Name           : string with get
    abstract ID             : string with get
    //abstract Container      : IRegionNode with get
    abstract Regions        : IIndex<IRegionNode, string> with get
    abstract Commands       : IIndex<ICommand, string> with get
and [<AutoOpen>] IRegionNode = 
    abstract Name           : string with get
    //abstract Parent         : IViewNode with get
    abstract Views          : IIndex<IViewNode, string> with get

type [<AutoOpen>] IRegistry = 
    abstract member Register<'T>    : name: string -> model: 'T -> IRegistry
    abstract member Unregister      : name: string -> IRegistry
    abstract member Lookup          : name: string -> IViewNode

type [<AutoOpen>] IResolver = 
    abstract member Resolve<'T when 'T: (new: unit -> 'T)> : model : 'T -> IView<'T>
and IView =
    //abstract member ExchangeViewModel : model : 'T -> IView<'T>
    /// <summary>
    /// Submits the current view state to the specified <see cref="IForestContext"/> instance.
    /// </summary>
    /// <param name="context">
    /// The <see cref="IForestContext" /> instance to manage the state of the current view.
    /// </param>
    abstract member Submit: context : IForestContext -> unit
    abstract ViewModel: obj
and [<AutoOpen>] IView<'T when 'T: (new: unit -> 'T)> =
    inherit IView
    abstract ViewModel: 'T with get, set
and [<AutoOpen>] IForestContext =
    abstract Registry: IRegistry with get
    abstract Resolver: IResolver with get

type IForestEngine =
    abstract member Execute<'T when 'T: (new: unit -> 'T)> : context: IForestContext -> node: IViewNode -> IView<'T>



type internal IForestContextAware =
    abstract member InitializeContext : context : IForestContext -> unit

type internal IParentRegionAware =
    abstract member InitializeParentRegion : region : IRegion -> unit

[<Flags>]
type internal ViewChange =
    | None          = 0b00
    | ViewModel     = 0b01
    | RegionState   = 0b10

[<AbstractClass>]
type [<AutoOpen>] View<'T when 'T: (new: unit -> 'T)> () =
    let mutable _viewModel : 'T  = new 'T()
    let mutable _viewChanges : ViewChange = ViewChange.None
    //interface IForestContextAware with
    //    member x.InitializeContext ctx =
    //       context <- ctx
    //       ()
    interface IView<'T> with 
        member x.Submit context = ()
        member x.ViewModel 
            with get () = _viewModel
            and set value =
                _viewModel <- value
                _viewChanges <- _viewChanges ||| ViewChange.ViewModel


