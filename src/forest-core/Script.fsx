#r "bin/Debug/Forest.Core.dll"
//#r "System.Core"
//#r "System"
//#r "System.Numerics"
//#r "System.Runtime.Serialization"
open System
open System.Collections.Generic
open System.IO
open System.Linq
//open System.Runtime.Serialization
//open System.Runtime.Serialization.Json
open System.Text
open Forest
open Forest.Dom
open Forest.Sdk


/// Object to Json 
//let internal toJson<'T> (myObj:'T) =   
//    use ms = new MemoryStream() 
//    (new DataContractJsonSerializer(typeof<'T>)).WriteObject(ms, myObj) 
//    Encoding.UTF8.GetString(ms.ToArray()) 

/// Object from Json 
//let internal fromJson<'T> (jsonString: string) : 'T =  
//    use ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonString)) 
//    let obj = (new DataContractJsonSerializer(typeof<'T>)).ReadObject(ms) 
//    obj :?> 'T

//let jsonFile = "/home/islavov/Projects/forest/src/forest-core/exampleTemplateDefinition.json"

//[<DataContract>]
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

[<Sealed>]
type MyViewModel() = class end

[<View("MyView", AutowireCommands = true)>]
type MyView() =
    inherit AbstractView<MyViewModel>()
    member this.SampleCommand (x: int) = ()

let rt = new DefaultForestRuntime()   
rt.Registry.Register<MyView>()

/////////////////////////////////////////////////////////
let engine = DefaultForestEngine()
let result = engine.CreateIndex(rt, rawTemplateStructureFromJson)
printf "dom index contains %i root nodes \n" result.Count
for path in result.Paths do 
    let item = result.[path]
    match item with
    | None -> ()
    | Some node -> for x in node do Console.WriteLine("  +-[{0}]", x.Path)

