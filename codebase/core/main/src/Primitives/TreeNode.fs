namespace Forest

open Forest.NullHandling

open System
open System.Text
open System.Diagnostics
open System.Runtime.CompilerServices


[<DebuggerDisplay("{this.ToString()}")>]
[<CustomComparison>]
[<CustomEquality>]
type TreeNode = 
    | [<DebuggerBrowsable(DebuggerBrowsableState.Never)>] Shell_
    | [<DebuggerBrowsable(DebuggerBrowsableState.Never)>] ViewID_ of parent:TreeNode * region:rname * view:vname * hash:hash
    [<CompiledName("Shell")>]
    static member shell = 
        TreeNode.Shell_
    [<CompiledName("NewKey")>]
    static member newKey region view parent = 
        TreeNode.ViewID_(parent, region, view, Fuid.newID().Hash)
    member this.Parent 
        with get() = match this with Shell_ -> Shell_ | ViewID_ (p, _, _, _) -> p
    member this.Region 
        with get() = match this with Shell_ -> rname.Empty | ViewID_ (_, r, _, _) -> r
    member this.View 
        with get() = match this with Shell_ -> vname.Empty | ViewID_ (_, _, v, _) -> v
    member this.Hash 
        with get() = match this with Shell_ -> Fuid.empty.Hash | ViewID_ (_, _, _, h) -> h
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>]
    member private this.cmp (other:TreeNode) = 
        StringComparer.Ordinal.Compare(this.Hash, other.Hash)
    [<MethodImpl(MethodImplOptions.AggressiveInlining)>] 
    member private this.eq (other: TreeNode) = 
        StringComparer.Ordinal.Equals(this.Hash, other.Hash)
    override this.Equals o = 
        match null2opt o with
        | Some v ->
            match v with
            | :? TreeNode as other -> this.eq other
            | _ -> false
        | None -> false
    override this.GetHashCode() = 
        this.Hash.GetHashCode()
    member private this.ToStringBuilder(stopAtRegion:bool):StringBuilder =
        let sb =
            match this with 
            | ViewID_ (p, r, v, h) -> 
                let sb:StringBuilder = p.ToStringBuilder(false).Append((if r.Length = 0 then "shell" else r))
                if (stopAtRegion) then sb
                else sb.AppendFormat("/{0}({1})", h, v)
            | Shell_ -> StringBuilder("")
        sb.Append('/')
    member this.RegionFragment 
        with get() = this.ToStringBuilder(true).ToString()
    override this.ToString() = 
        this.ToStringBuilder(false).ToString()
    interface IComparable<TreeNode> with 
        member this.CompareTo other = this.cmp other
    interface IComparable with
        member this.CompareTo o = 
            match o with
            | :? TreeNode as id -> this.cmp id
            | :? IComparable as c -> (-1)*(c.CompareTo this)
            | _ -> raise (NotSupportedException ())
    interface IEquatable<TreeNode> with 
        member this.Equals other = this.eq other

module internal TreeNode =
    //let add id region name parent = HierarchyKey.ViewID_(parent, region, name, id)
    //let addNew region name parent = add (Fuid.newID().Hash) region name parent
    let isShell (id:TreeNode) = match id with TreeNode.Shell_ -> true | _ -> false
    
