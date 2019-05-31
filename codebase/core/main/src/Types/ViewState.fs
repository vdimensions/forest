namespace Forest

open System
open System.Runtime.InteropServices
open Axle.Verification

#if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
[<Serializable>]
[<StructLayout(LayoutKind.Sequential)>]
#endif
[<NoComparison>] 
type ViewState = 
    {
        Model : obj
        DisabledCommands : Set<cname>
        DisabledLinks : Set<string>
    }

module internal ViewState =

    let internal withModelUnchecked (model) = 
        {
            Model = model
            DisabledCommands = Set.empty
            DisabledLinks = Set.empty
        }

    let withModel (NotNull "model" model) =  withModelUnchecked model

    let enableCommand (NotNull "command" command) viewState =
        { viewState with DisabledCommands = Set.remove command viewState.DisabledCommands }

    let disableCommand (NotNull "command" command) viewState =
        { viewState with DisabledCommands = Set.add command viewState.DisabledCommands }

    let enableLink (NotNull "link" link) viewState =
        { viewState with DisabledLinks = Set.remove link viewState.DisabledLinks }

    let disableLink (NotNull "link" link) viewState =
        { viewState with DisabledLinks = Set.add link viewState.DisabledLinks }

    let isCommandEnabled (NotNull "command" command) viewState =
        viewState.DisabledCommands |> Set.contains command |> not

    let isLinkEnabled (NotNull "command" command) viewState =
        viewState.DisabledLinks |> Set.contains command |> not