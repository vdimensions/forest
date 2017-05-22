namespace Forest
open System;
open System.Reflection
open System.Collections.Generic
open Forest.Dom

type CommandMetadata(name: string, argType: Type) = 
    member this.Name with get() = name
    member this.ArgumentType with get() = argType

type TypeMetadata(name: string, viewType: Type, commands: CommandMetadata[]) = 
    member this.Name with get() = name
    member this.ViewType with get() = viewType
    member this.Commands with get() = upcast commands: IEnumerable<CommandMetadata>



[<AbstractClass>]
type AbstractViewRegistry() = 
    let storage: IDictionary<string, TypeMetadata> = upcast new Dictionary<string, TypeMetadata>(StringComparer.Ordinal)

    abstract member GetTypeMetadata: t: Type -> Option<TypeMetadata>

    member this.Register (t: Type) = 
        match t with
        | null -> nullArg "t"
        | _ -> 
            let metadata = this.GetTypeMetadata t
            match metadata with
            | None -> invalidArg "t" "The specified type does not implement"
            | Some metadata -> storage.[metadata.Name] <- metadata
            this
    member this.Register<'T when 'T:> IView> () = 
        this.Register typeof<'T>

type [<Sealed>] ViewRegistry() = 
    inherit AbstractViewRegistry()
    override this.GetTypeMetadata t =
        let inline objHasType ty obj = (obj.GetType() = ty)
        let hasAttr : MemberInfo -> Option<'a> = fun mi ->
            mi.GetCustomAttributes(true)
            |> Seq.tryFind(objHasType typeof<'a>)
            |> Option.map(fun attr -> (downcast attr : 'a))

        let attr: Option<ViewAttribute> = hasAttr t 
        let name = match attr with | Some a -> a.Name | _ -> String.Empty
        let commandMethods = 
            t.GetMethods(BindingFlags.Instance|||BindingFlags.NonPublic|||BindingFlags.Public)
            |> List.map (fun x -> let r: Option<CommandAttribute> = hasAttr x; r)
            |> List.filter (fun x -> match x with | None: false | Some data when data -> data)

        ()
            
        



