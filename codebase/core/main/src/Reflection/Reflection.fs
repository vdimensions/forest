namespace Forest.Reflection

open Forest
open Forest.NullHandling

open System
open System.Reflection


type IMethod = interface 
    abstract member Invoke: target:obj -> args:obj array -> obj
    abstract member ParameterTypes:Type array with get
    abstract member ReturnType:Type with get
    abstract member Name:string with get
end
type ICommandMethod = interface 
    inherit IMethod
    abstract member CommandName: string with get
end
type IEventMethod = interface 
    inherit IMethod
    abstract member Topic: string with get
end

type IProperty = interface
    abstract member GetValue: target:obj -> obj
    abstract member SetValue: target:obj -> value:obj -> unit
    abstract member Name: string with get
end

type IReflectionProvider = interface
    abstract member GetViewAttribute: viewType:Type -> ViewAttribute
    abstract member GetCommandMethods: viewType:Type -> ICommandMethod array
    abstract member GetSubscriptionMethods: viewType:Type -> IEventMethod array
    abstract member GetLocalizeableProperties: vmType:Type -> IProperty array
end

type [<AbstractClass>] private AbstractMethod(method:MethodInfo) =
    do ignore <| isNotNull "method" method
    interface IMethod with
        member __.Invoke target args = method.Invoke(target, args)
        member __.ParameterTypes with get() = method.GetParameters() |> Array.map (fun p -> p.ParameterType)
        member __.ReturnType with get() = method.ReturnType
        member __.Name with get() = method.Name

type [<Sealed>] private DefaultCommandMethod(method:MethodInfo, commandName:string) =
    inherit AbstractMethod(method)
    do ignore <| isNotNull "commandName" commandName
    interface ICommandMethod with member __.CommandName with get() = commandName

type [<Sealed>] private DefaultEventMethod(method:MethodInfo, topic:string) =
    inherit AbstractMethod(method)
    do ignore <| isNotNull "topic" topic
    interface IEventMethod with member __.Topic with get() = topic

type [<Sealed>] private DefaultProperty(property: PropertyInfo) =
    do ignore <| isNotNull "property" property
    interface IProperty with
        member __.GetValue target = property.GetValue(target)
        member __.SetValue target value = property.SetValue(target, value)
        member __.Name with get() = property.Name

type [<Sealed>] internal DefaultReflectionProvider() =
    [<Literal>]
    let flags = BindingFlags.Instance|||BindingFlags.NonPublic|||BindingFlags.Public
    member inline private __.isOfType<'a> obj = (obj.GetType() = typeof<'a>)
    member inline private __.getAttributes(mi:#MemberInfo):seq<'a:>Attribute> =
        let getAttributesInternal = 
            mi.GetCustomAttributes 
            >> Seq.filter __.isOfType<'a>  
            >> Seq.map(fun attr -> (downcast attr:'a)) 
        getAttributesInternal(true)
    member inline private __.getMethods (f) (t:Type) = 
        t.GetMethods (f) |> Seq.filter (fun mi -> not mi.IsSpecialName)
    member inline private __.getCommandAttribs (methodInfo:MethodInfo): seq<CommandAttribute> = 
        __.getAttributes methodInfo
    member inline private __.getEventAttribs (methodInfo:MethodInfo): seq<SubscriptionAttribute> = 
        __.getAttributes methodInfo
    interface IReflectionProvider with
        member __.GetViewAttribute viewType =
            match (viewType |> __.getAttributes |> Seq.tryPick<ViewAttribute, ViewAttribute> Some) with
            | Some p -> p
            | None -> nil<ViewAttribute>            
        member __.GetCommandMethods viewType =
            viewType
            |> __.getMethods flags 
            |> Seq.map (fun x -> (x |> __.getCommandAttribs) |> Seq.map (fun a -> a.Name), x)
            |> Seq.filter (fun (a, _) -> not <| Seq.isEmpty a)
            |> Seq.collect (fun (n, m) -> n |> Seq.map (fun x -> upcast DefaultCommandMethod(m, x) : ICommandMethod))
            |> Seq.toArray
        member __.GetSubscriptionMethods viewType = 
            viewType
            |> __.getMethods flags 
            |> Seq.map (fun x -> (x |> __.getEventAttribs) |> Seq.map (fun a -> a.Topic), x)
            |> Seq.filter (fun (a, _) -> not <| Seq.isEmpty a)
            |> Seq.collect (fun (n, m) -> n |> Seq.map (fun x -> upcast DefaultEventMethod(m, x) : IEventMethod))
            |> Seq.toArray
        member __.GetLocalizeableProperties vmType = 
            Array.empty
