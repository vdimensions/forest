#r "bin/Debug/net45/Forest.Core.dll"
#r "System.Core"
#r "System"
#r "System.Numerics"
#r "System.Runtime.Serialization"
open System
open System.Collections.Generic
open System.IO
open System.Linq
open System.Runtime.Serialization
open System.Runtime.Serialization.Json
open System.Text
open Forest
open Forest.Dom
//open Forest.Sdk


/// Object to Json 
let internal toJson<'T> (myObj:'T) =   
    use ms = new MemoryStream() 
    (DataContractJsonSerializer(typeof<'T>)).WriteObject(ms, myObj) 
    Encoding.UTF8.GetString(ms.ToArray()) 

/// Object from Json 
let internal fromJson<'T> (jsonString: string) : 'T =  
    use ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)) 
    let obj = (DataContractJsonSerializer(typeof<'T>)).ReadObject(ms) 
    obj :?> 'T

let jsonFile = "/home/islavov/Projects/forest/src/forest-core/exampleTemplateDefinition.json"

[<DataContract>]
[<Serializable>]
type JsonData() = inherit Dictionary<string, JsonData>(StringComparer.Ordinal)

let rawTemplateStructureFromJson = upcast new Dictionary<string, obj>(): IDictionary<string, obj>
let add (key: string) (target: IDictionary<string, obj>) = target.Add(key, new Dictionary<string, obj>()); target
let get (key: string) (target: IDictionary<string, obj>) = downcast target.[key] : IDictionary<string, obj>

rawTemplateStructureFromJson 
|> add "rootView" |> get "rootView"
|> add "contentRegion" |> get "contentRegion"
|> add "MyView"
|> add "view2" |> get "view2" |> add "emptyRegion"

/////////////////////////////////////////////////////////

type [<Sealed>] MyViewModel() = class end

[<View("MyView", AutowireCommands = true)>]
type MyView() = class
    inherit AbstractView<MyViewModel>()
    member this.SampleCommand (x: int) = ()
end
let ctx = new DefaultForestContext(DefaultViewRegistry(DefaultContainer()))   
ctx.Registry.Register<MyView>()

/////////////////////////////////////////////////////////

let engine = DefaultForestEngine()
let index = engine.CreateIndex(ctx, rawTemplateStructureFromJson)
printf "dom index contains %i root nodes \n" index.Count
for path in index.Paths do
    match index.[path] with
    | None -> ()
    | Some node -> for x in node do printf"  +-[%s] \n" (x.Path.ToString())
