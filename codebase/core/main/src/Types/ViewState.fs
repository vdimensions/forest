namespace Forest

open Axle
open Axle.Verification
open Forest

module internal ViewState =

    let internal withModelUnchecked model =
        match null2opt model with
        | Some m -> ViewState.Create(m)
        | None -> ViewState.Empty

    let withModel (model) =  model |> ViewState.Create

    let enableCommand (command) viewState =
        ViewState.EnableCommand (viewState, command)

    let disableCommand (command) viewState =
        ViewState.DisableCommand (viewState, command)

    let enableLink (link) viewState =
        ViewState.EnableLink (viewState, link)

    let disableLink (link) viewState =
        ViewState.DisableLink (viewState, link)

    let isCommandEnabled (NotNull "command" command) (viewState : ViewState) =
        viewState.DisabledCommands.Contains command |> not

    let isLinkEnabled (NotNull "command" command) (viewState : ViewState) =
        viewState.DisabledLinks.Contains command |> not