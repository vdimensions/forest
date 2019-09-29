#nowarn "52"

#r "paket:
nuget Fake.IO.FileSystem
nuget Fake.DotNet.Cli
nuget Fake.DotNet.Paket
nuget Fake.Net.Http
nuget Fake.Tools.Git
nuget Fake.Core.Target //"

//#load "./.fake/build.fsx/intellisense.fsx"

#load "../submodules/vdimensions_fake_sdk/src/vdbuild.fsx"
open VDimensions.Fake

open Fake.Core
open Fake.IO

let projectLocations = [
    "engine/main"
    "core/main"
    "core/forms"
    "web/main"
    "web/websharper"
]

Target.create "Prepare" VDBuild.cleanNupkg
Target.create "Complete" (fun _ -> 
    // TODO: create tag
    ()
)
open Fake.Core.TargetOperators

"Prepare" ==> "Complete"

projectLocations 
|> List.map (VDBuild.createDynamicTarget "Forest.Common.props")
|> List.rev
|> List.fold (fun a b -> b ==> a |> ignore; b) "Complete"
|> ignore

Target.runOrDefaultWithArguments "Complete"
