namespace Forest


type [<Interface>] IStateVisitor =
    abstract member BFS: id:HierarchyKey -> region:string -> view:string -> index:int -> viewModel:obj -> descriptor:IViewDescriptor -> unit
    abstract member DFS: id:HierarchyKey -> region:string -> view:string -> index:int -> viewModel:obj -> descriptor:IViewDescriptor -> unit

module Renderer =
    let rec private _traverseState (v: IStateVisitor) parent (ids: HierarchyKey list) siblingsCount (st: State) =
        match ids with
        | [] -> ()
        | head::tail ->
            let ix = siblingsCount - ids.Length // TODO
            let vm = st.ViewModels.[head]
            let descriptor = st.ViewStates.[head].Descriptor
            v.BFS parent head.Region head.View ix vm descriptor
            // visit siblings 
            _traverseState v parent tail siblingsCount st
            // visit children
            let children = st.Hierarchy.Hierarchy.[head]
            _traverseState v head children children.Length st
            v.DFS parent head.Region head.View ix vm descriptor
            ()

    let traverse (v: IStateVisitor) (st: State) =
        let root = HierarchyKey.shell
        let ch = st.Hierarchy.Hierarchy.[root]
        _traverseState v root ch ch.Length st