﻿namespace Forest.Reflection

open Forest
open Forest.NullHandling

open System
open System.Reflection


type [<AbstractClass;NoComparison>] private AbstractMethod<'TT, 'T when 'TT :> IView>(method:MethodInfo, md:Action<'TT, 'T>) =
    do 
        ignore <| isNotNull "method" method
        ignore <| isNotNull "md" md
    new (method:MethodInfo) = AbstractMethod(method, (downcast method.CreateDelegate(typeof<Action<'TT, 'T>>):Action<'TT, 'T>))

    interface IMethod with
        member __.Invoke target arg = md.Invoke((downcast target:'TT), (downcast arg:'T)) |> ignore
        member __.ParameterTypes with get() = method.GetParameters() |> Array.map (fun p -> p.ParameterType)
        member __.ReturnType with get() = method.ReturnType
        member __.Name with get() = method.Name

type [<Sealed;NoComparison>] private DefaultCommandMethod<'TT, 'T when 'TT :> IView>(method:MethodInfo, commandName:cname) =
    inherit AbstractMethod<'TT, 'T>(method)
    do ignore <| isNotNull "commandName" commandName
    interface ICommandMethod with 
        member __.CommandName with get() = commandName

type [<Sealed;NoComparison>] private DefaultEventMethod<'TT, 'T when 'TT :> IView>(method:MethodInfo, topic:string) =
    inherit AbstractMethod<'TT, 'T>(method)
    do ignore <| isNotNull "topic" topic
    interface IEventMethod with 
        member __.Topic with get() = topic

type [<Sealed;NoComparison>] private DefaultProperty(property: PropertyInfo) =
    do ignore <| isNotNull "property" property
    interface IProperty with
        member __.GetValue target = property.GetValue(target)
        member __.SetValue target value = property.SetValue(target, value)
        member __.Name with get() = property.Name

type [<Sealed;NoComparison>] DefaultReflectionProvider() =

    let createCmd (method:MethodInfo) (name:string) : ICommandMethod =
        let lastParam = method.GetParameters() |> Seq.last
        let mt = typedefof<DefaultCommandMethod<_,_>>.MakeGenericType(method.DeclaringType, lastParam.ParameterType)
        downcast Activator.CreateInstance(mt, method, name)

    let createEvt (method:MethodInfo) (name:string) : IEventMethod =
        let lastParam = method.GetParameters() |> Seq.last
        let mt = typedefof<DefaultEventMethod<_,_>>.MakeGenericType(method.DeclaringType, lastParam.ParameterType)
        downcast Activator.CreateInstance(mt, method, name)

    [<Literal>]
    let flags = BindingFlags.Instance|||BindingFlags.NonPublic|||BindingFlags.Public
    member inline private __.isOfType<'a> obj = (obj.GetType() = typeof<'a>)
    member inline private __.getAttributes(mi:#MemberInfo):seq<'a:>Attribute> =
        mi.GetCustomAttributes(true)
        |> Seq.filter __.isOfType<'a> 
        |> Seq.map(fun attr -> (downcast attr:'a)) 
    member inline private __.getMethods (f) (t:Type) = 
        t.GetMethods (f) |> Seq.filter (fun mi -> not mi.IsSpecialName)
    member inline private __.getCommandAttribs (methodInfo:MethodInfo): seq<CommandAttribute> = 
        __.getAttributes methodInfo
    member inline private __.getEventAttribs (methodInfo:MethodInfo): seq<SubscriptionAttribute> = 
        __.getAttributes methodInfo
    interface IReflectionProvider with
        member __.GetViewAttribute viewType =
            #if NETSTANDARD2_0_OR_NEWER || NETFRAMEWORK
            match (viewType |> __.getAttributes |> Seq.tryPick<ViewAttribute, ViewAttribute> Some) with
            #else
            match (viewType.GetTypeInfo() |> __.getAttributes |> Seq.tryPick<ViewAttribute, ViewAttribute> Some) with
            #endif
            | Some p -> p
            | None -> nil<ViewAttribute>            
        member __.GetCommandMethods viewType =
            viewType
            |> __.getMethods flags 
            |> Seq.map (fun x -> (x |> __.getCommandAttribs) |> Seq.map (fun a -> a.Name), x)
            |> Seq.filter (fun (a, _) -> not <| Seq.isEmpty a)
            |> Seq.collect (fun (n, m) -> n |> Seq.map (fun x -> createCmd m x))
            |> Seq.toArray
        member __.GetSubscriptionMethods viewType = 
            viewType
            |> __.getMethods flags 
            |> Seq.map (fun x -> (x |> __.getEventAttribs) |> Seq.map (fun a -> a.Topic), x)
            |> Seq.filter (fun (a, _) -> not <| Seq.isEmpty a)
            |> Seq.collect (fun (n, m) -> n |> Seq.map (fun x -> createEvt m x))
            |> Seq.toArray
        member __.GetLocalizeableProperties vmType = 
            // TODO
            Array.empty
