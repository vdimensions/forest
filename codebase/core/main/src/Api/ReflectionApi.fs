namespace Forest.Reflection

open Forest

open System


type IMethod = interface 
    abstract member Invoke: target:obj -> args:obj array -> obj
    abstract member ParameterTypes:Type array with get
    abstract member ReturnType:Type with get
    abstract member Name:string with get
end

type ICommandMethod = interface 
    inherit IMethod
    abstract member CommandName:string with get
end

type IEventMethod = interface 
    inherit IMethod
    abstract member Topic:string with get
end

type IProperty = interface
    abstract member GetValue: target:obj -> obj
    abstract member SetValue: target:obj -> value:obj -> unit
    abstract member Name:string with get
end

type IReflectionProvider = interface
    abstract member GetViewAttribute: viewType:Type -> ViewAttribute
    abstract member GetCommandMethods: viewType:Type -> ICommandMethod array
    abstract member GetSubscriptionMethods: viewType:Type -> IEventMethod array
    abstract member GetLocalizeableProperties: vmType:Type -> IProperty array
end
