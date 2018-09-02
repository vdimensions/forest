namespace Forest

open Forest.NullHandling

open System

[<Serializable>]
type AbstractViewException(message:string, inner:Exception) =
    inherit ForestException(isNotNull "message" message, inner)
    new (message:string) = AbstractViewException(message, null)

[<Serializable>]
type ViewAttributeMissingException(viewType:Type, inner:Exception) =
    inherit AbstractViewException(String.Format("The type `{0}` must be annotated with a `{1}`", (isNotNull "viewType" viewType).FullName, typeof<ViewAttribute>.FullName), inner)
    new (viewType:Type) = ViewAttributeMissingException(viewType, null)

[<Serializable>]
type ViewTypeIsAbstractException(viewType:Type, inner:Exception) =
    inherit AbstractViewException(String.Format("Cannot instantiate view from type `{0}` because it is an interface or an abstract class.", (isNotNull "viewType" viewType).FullName), inner)
    new (viewType:Type) = ViewTypeIsAbstractException(viewType, null)

[<Serializable>]
type ViewTypeIsNotGenericException(viewType:Type, inner:Exception) =
    inherit AbstractViewException(String.Format("Provided view type `{0}` does not implement the `{1}` interface.", (isNotNull "viewType" viewType).FullName, typeof<IView<_>>.FullName), inner)
    new (viewType:Type) = ViewTypeIsNotGenericException(viewType, null)

type ViewInstantiationException(viewType:Type, inner:Exception) =
    inherit AbstractViewException(String.Format("Unable to resolve view `{0}` .", (isNotNull "viewType" viewType).FullName), inner)
    new (viewType:Type) = ViewInstantiationException(viewType, null)