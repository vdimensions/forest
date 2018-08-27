namespace Forest

open Forest.NullHandling

open System


type [<Interface>] IViewFactory = 
    abstract member Resolve: vm:IViewDescriptor -> IView

type [<Interface>] IView<'T when 'T: (new: unit -> 'T)> =
    inherit IView
    abstract ViewModel: 'T with get, set

[<Serializable>]
type AbstractViewException(message:string, inner:Exception) =
    inherit ForestException(isNotNull "message" message, inner)
    new (message: string) = AbstractViewException(message, null)

[<Serializable>]
type ViewAttributeMissingException(viewType:Type, inner:Exception) =
    inherit AbstractViewException(String.Format("The type `{0}` must be annotated with a `{1}`", (isNotNull "viewType" viewType).FullName, typeof<ViewAttribute>.FullName), inner)
    new (viewType: Type) = ViewAttributeMissingException(viewType, null)

[<Serializable>]
type ViewTypeIsAbstractException(viewType:Type, inner:Exception) =
    inherit AbstractViewException(String.Format("Cannot instantiate view from type `{0}` because it is an interface or an abstract class.", (isNotNull "viewType" viewType).FullName), inner)
    new (viewType: Type) = ViewTypeIsAbstractException(viewType, null)