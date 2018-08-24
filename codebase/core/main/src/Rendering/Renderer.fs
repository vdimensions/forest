namespace Forest


type [<Interface>] IStateVisitor =
    abstract member BFS: key:HierarchyKey -> region:string -> view:string -> index:int -> viewModel:obj -> descriptor:IViewDescriptor -> unit
    abstract member DFS: key:HierarchyKey -> region:string -> view:string -> index:int -> viewModel:obj -> descriptor:IViewDescriptor -> unit

module Renderer =
    let rec private _traverseState (v: IStateVisitor) parent (ids: HierarchyKey list) siblingsCount (st: State) =
        match ids with
        | [] -> ()
        | head::tail ->
            let ix = siblingsCount - ids.Length // TODO
            let vm = st.ViewModels.[head]
            let vs = st.ViewStates.[head]
            let descriptor = vs.Descriptor
            v.BFS vs.InstanceID head.Region head.View ix vm descriptor
            // visit siblings 
            _traverseState v parent tail siblingsCount st
            // visit children
            match st.Hierarchy.Hierarchy.TryFind head with
            | Some children -> _traverseState v head children children.Length st
            | None -> ()
            v.DFS vs.InstanceID head.Region head.View ix vm descriptor
            ()

    let traverse (v: IStateVisitor) (st: State) =
        let root = HierarchyKey.shell
        match st.Hierarchy.Hierarchy.TryFind root with
        | Some ch -> _traverseState v root ch ch.Length st
        | None -> ()