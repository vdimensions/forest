namespace Forest
open Forest.Dom

type [<Interface>] IForestState = 
    abstract member Push: path: Path -> context: IForestContext -> IDomIndex
    abstract member DomIndex: IDomIndex with get
   