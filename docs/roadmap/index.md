# Roadmap

This is the project roadmap. Below is a list of features that are planned for the future.

## Tree-Links

The initial version of Forest (which is not publicly available) during its early conceiving phases had the concept of a _tree-link_. 
A tree-link is simply an instruction that allows a view to expose a number of connectiosn to other trees, referred to by names.
This would allow the UI to visualize those connections as part of the view's physical layout and invoke them. Each such invocation
is currently backed by the `ITreeNavigator` interface, enabling the user to directly switch to a different tree. The user could also 
pass an optional parameter object to the respective `ITreeNavigator.LoadTree` call.

In web-related project this is very useful, as it allows the web application to expose static links to trees, rather than relying
on the backend for navigation (such as calling `ITreeNavigator.LoadTree` from within a command handler).

## State Distribution

## Integration with WebApi

## Integration with `Windows.Forms`

## Integration with `Eto.Forms`

## Integration with `XAML`