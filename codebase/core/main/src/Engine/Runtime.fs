namespace Forest

open System
open Forest

[<RequireQualifiedAccess>]
module Runtime =
    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [<System.Serializable>]
    #endif
    [<CompiledName("Operation")>]
    [<NoComparison>]
    type Operation =
        | InstantiateView of viewHandle : ViewHandle * region : rname * parent : TreeNode * model : obj option
        | InstantiateViewByNode of node : TreeNode * model : obj option
        | UpdateModel of node : thash * model : obj
        | DestroyView of subtree : TreeNode
        | InvokeCommand of command : cname * node : thash * commandArg : obj
        | SendMessage of node : thash * message : obj * topics : string array
        | DispatchMessage of message : obj * topics : string array
        | ClearRegion of owner : TreeNode * region : rname
        | Multiple of operations : Operation list

    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [<System.Serializable>]
    #endif
    [<NoComparison>]
    type Status =
        | ViewCreated of view : IView
        | ModelUpdated of model : obj
        | ViewDestoyed
        | CommandInvoked
        | MesssagePublished
        | MessageSourceNotFound
        | RegionCleared
        | Multiple of Status list

    #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
    [<Serializable>]
    #endif
    [<NoComparison>] 
    type Error =
        /// Error when the view that is a target of a command or message is not found.
        | ViewstateAbsent of hash : thash
        /// Unable to find a descriptor for the given view.
        | NoDescriptor of viewHandle : ViewHandle
        /// A tree node was found to be already present in the hierarchy at a place where it was about to be created.
        | SubTreeNotExpected of node : TreeNode
        /// A tree node was expected to be present in the hierarchy but was not found.
        | SubTreeAbsent of node : TreeNode
        /// An command-specific error occurred while invoking a command. See `cause` for details on the error.
        | CommandError of cause : Command.Error
        /// An view-specific error occurred while invoking a command. See `cause` for details on the error.
        | ViewError of cause : View.Error
        /// Multiple errors have been captured during execution.
        | Multiple of errors : Error list

    let private handleError (error : Error) =
        match error with
        | ViewError ve -> ve |> View.handleError 
        | CommandError ce -> ce |> Command.handleError 
        | NoDescriptor vh -> 
            match vh with
            | ByName vn -> invalidOp <| String.Format("Unable to obtain descriptor for view '{0}'", vn)
            | ByType vt -> invalidOp <| String.Format("Unable to obtain descriptor for view `{0}`", vt.AssemblyQualifiedName)
        // TODO
        | _ -> ()

    let resolve (resultMap : ('x -> 'a)) (result : Result<'x, Error>) =
        match result with
        | Error e -> 
            handleError e
            // NB: It is possible for `handleError` to not recognize the error type
            // hence it will return without throwing an exception.
            // Therefore, we must ensure execution is terminated in that case by 
            // explicitly thowing an `invalidOp`.
            invalidOp "An unexpected error occurred"
        | Ok x -> resultMap x