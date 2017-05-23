namespace Forest
open System;
open System.Reflection
open System.Collections.Generic
open Forest.Dom


type [<AbstractClass>] AbstractViewRegistry() = 
    let storage: IDictionary<string, View.Metadata> = upcast new Dictionary<string, View.Metadata>(StringComparer.Ordinal)

    abstract member GetTypeMetadata: t: Type -> Result<View.Metadata, ViewRegistryError>

    member this.ResolveError (e: ViewRegistryError): Exception = 
        match e with
        | ViewError ve -> (this.ResolveViewError ve)
        | CommandError ce -> (this.ResolveCommandError ce)
    abstract member ResolveViewError: ve: View.Error -> Exception
    default this.ResolveViewError ve =
        // TODO
        match ve with
        | View.Error.ViewAttributeMissing t -> upcast new InvalidOperationException()
        | View.Error.ViewTypeIsAbstract t -> upcast new InvalidOperationException()
    abstract member ResolveCommandError: ce: Command.Error -> Exception
    default this.ResolveCommandError ce =
        // TODO
        match ce with
        | Command.Error.MoreThanOneArgument t -> upcast new InvalidOperationException()
        | Command.Error.NonVoidReturnType t -> upcast new InvalidOperationException()
        | Command.Error.MultipleErrors t -> upcast new InvalidOperationException()

    member this.Register (t: Type) = 
        match t with
        | null -> nullArg "t"
        | _ -> 
            let metadata = this.GetTypeMetadata t
            match metadata with
            | Success metadata -> storage.[metadata.Name] <- metadata
            | Failure e -> raise (e |> this.ResolveError)
            this
    member this.Register<'T when 'T:> IView> () = this.Register typeof<'T>

type [<Sealed>] ViewRegistry() = 
    inherit AbstractViewRegistry()
    override this.GetTypeMetadata t =
        match t with | null -> nullArg "t" | _ -> ()
        let inline isOfType ty obj = (obj.GetType() = ty)
        let inline getAttributes (mi: MemberInfo) : seq<'a> =
            let getAttributesInternal = 
                mi.GetCustomAttributes 
                >> Seq.filter(isOfType typeof<'a>) 
                >> Seq.map(fun attr -> (downcast attr : 'a)) 
            getAttributesInternal(true)
        let inline getCommandAttribs (methodInfo: MethodInfo): seq<CommandAttribute> = 
            getAttributes (upcast methodInfo: MemberInfo)

        let attr: Option<ViewAttribute> = (getAttributes t) |> Seq.tryPick Some
        match attr with 
        | Some attr -> 
            let name = attr.Name
            let flags = BindingFlags.Instance|||BindingFlags.NonPublic|||BindingFlags.Public
            let inline getMethods (f) = t.GetMethods (f)
            let inline createParameter (a: seq<CommandAttribute>, mi: MethodInfo) =
                let parameters = mi.GetParameters()
                match mi with
                | mi when mi.ReturnType <> typeof<Void> -> Failure (Command.Error.NonVoidReturnType(mi))
                | mi when parameters.Length > 1 -> Failure (Command.Error.MoreThanOneArgument(mi))
                | _ -> 
                    let parameterType = 
                        match parameters with
                        | parameters when parameters.Length > 0 -> parameters.[0].ParameterType
                        | _ -> typeof<Void>
                    let metadata = a |> Seq.map (fun ca -> Command.Metadata(ca.Name, parameterType, mi))
                    Success (metadata)
                    
            let commandMetadata = 
                getMethods 
                >> Seq.map (fun x -> (x |> getCommandAttribs), x)
                >> Seq.map createParameter

            let commandMetadataResults = commandMetadata(flags)
            let inline failureFilter a = 
                match a with 
                | Failure b -> Some b 
                | _ -> None
            let errorList = 
                commandMetadataResults 
                |> Seq.choose failureFilter
                |> Seq.toArray

            match errorList with
            | list when list.Length > 0 -> Failure ((Command.Error.MultipleErrors list) |> ViewRegistryError.CommandError)
            | _ -> 
                let metaDataArray = 
                    commandMetadataResults 
                    |> Seq.map (fun item -> match item with | Success data -> Some data | Failure _ -> None)
                    |> Seq.choose id
                    |> Seq.concat
                    |> Seq.toArray
                Success (View.Metadata(name, t, metaDataArray))
        | None -> Failure ((View.Error.ViewAttributeMissing t) |> ViewRegistryError.ViewError)



