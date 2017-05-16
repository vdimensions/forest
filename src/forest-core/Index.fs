namespace Forest
open System.Collections.Generic;


type [<AutoOpen>] IIndex<'T, 'TKey> =
    inherit IEnumerable<'T>
    abstract Count          : int with get
    abstract Keys           : IEnumerable<'TKey> with get
    abstract Item           : 'TKey -> 'T with get

type [<AutoOpen>] IWritableIndex<'T, 'TKey> =
    inherit IIndex<'T, 'TKey>
    abstract member Remove  : key: 'TKey -> IWritableIndex<'T, 'TKey>
    abstract member Insert  : key: 'TKey -> item: 'T -> IWritableIndex<'T, 'TKey>
    abstract member Clear   : unit -> IWritableIndex<'T, 'TKey>

