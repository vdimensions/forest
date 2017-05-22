namespace Forest
open System;
open System.Reflection
open System.Collections.Generic
open Forest.Dom

type [<AutoOpen>] Error = 
| NoViewAttribute
| CommandMethodHasInvalidSignature

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

    abstract member GetTypeMetadata: t: Type -> Result<TypeMetadata, Error>

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
        let getAttributes : MemberInfo -> seq<'a> = 
            fun (mi:MemberInfo) ->
                let getAttributesInternal = 
                    mi.GetCustomAttributes 
                    >> Seq.filter(objHasType typeof<'a>) 
                    >> Seq.map(fun attr -> (downcast attr : 'a)) 
                getAttributesInternal(true)

        let inline getCommandMethod (methodInfo: MethodInfo): seq<CommandAttribute> = 
            getAttributes (upcast methodInfo: MemberInfo)

        let attr: Option<ViewAttribute> = (getAttributes t) |> Seq.tryPick Some
        match attr with 
        | Some attr -> 
            let name = attr.Name
            let inline getMethods () = t.GetMethods (BindingFlags.Instance|||BindingFlags.NonPublic|||BindingFlags.Public)
            let inline createParameter (a: seq<CommandAttribute>, mi: MethodInfo) =
                let parameters = mi.GetParameters()
                match mi with
                | mi when mi.ReturnType = typeof<Void> && parameters.Length = 1 -> 
                    let metadata = a |> Seq.map (fun ca -> CommandMetadata(ca.Name, parameters.[0].ParameterType))
                    Some (metadata)
                | _ -> 
                    let message = String.Format("The method `{0}` cannot be used as command, but has `{1}`.", mi.Name, typeof<CommandAttribute>.FullName)
                    Failure (Error.CommandMethodHasInvalidSignature, message)
            let commandMetadata = 
                getMethods 
                >> Seq.map (fun x -> getCommandMethod(x), x)
                >> Seq.map createParameter
                >> Seq.choose id
                >> Seq.concat
                >> Seq.toArray

            Success (new TypeMetadata(name, t, commandMetadata()))
        | _ -> 
            let message = String.Format("The specified type does not have a '{0}' applied.", typeof<ViewAttribute>.FullName)
            Failure (Error.NoViewAttribute, message)



