namespace Forest

open Forest.NullHandling

open System

#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
[<Serializable>]
#endif
type AbstractViewException(message:string, inner:Exception) =
    inherit ForestException(isNotNull "message" message, inner)
    new (message:string) = AbstractViewException(message, null)

#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
[<Serializable>]
#endif
type ViewAttributeMissingException(viewType:Type, inner:Exception) =
    inherit AbstractViewException(String.Format("The type `{0}` must be annotated with a `{1}`", (isNotNull "viewType" viewType).FullName, typeof<ViewAttribute>.FullName), inner)
    new (viewType:Type) = ViewAttributeMissingException(viewType, null)

#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
[<Serializable>]
#endif
type ViewTypeIsAbstractException(viewType:Type, inner:Exception) =
    inherit AbstractViewException(String.Format("Cannot instantiate view from type `{0}` because it is an interface or an abstract class.", (isNotNull "viewType" viewType).FullName), inner)
    new (viewType:Type) = ViewTypeIsAbstractException(viewType, null)

#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
[<Serializable>]
#endif
type ViewTypeIsNotGenericException(viewType:Type, inner:Exception) =
    inherit AbstractViewException(String.Format("Provided view type `{0}` does not implement the `{1}` interface.", (isNotNull "viewType" viewType).FullName, typeof<IView<_>>.FullName), inner)
    new (viewType:Type) = ViewTypeIsNotGenericException(viewType, null)

#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
[<Serializable>]
#endif
type ViewInstantiationException(viewType:Type, inner:Exception) =
    inherit AbstractViewException(String.Format("Unable to resolve view `{0}` .", (isNotNull "viewType" viewType).FullName), inner)
    new (viewType:Type) = ViewInstantiationException(viewType, null)