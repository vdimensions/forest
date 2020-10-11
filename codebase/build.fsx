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

let dir = Shell.pwd()

let projectLocations = [
    "core/main"
    "core/forms"
    "web/main"
]

Target.create "---Prepare---" VDBuild.cleanNupkg
Target.create "---Complete---" (fun _ -> 
    //Shell.rm_rf (sprintf "%s/../dist/restore" dir)
    // TODO: create tag
    ()
)
open Fake.Core.TargetOperators

("---Prepare---")::(projectLocations |> List.map (VDBuild.createDynamicTarget "Directory.Build.props"))
|> List.rev
|> List.fold (fun a b -> b ==> a |> ignore; b) "---Complete---"
|> ignore

Target.runOrDefaultWithArguments "---Complete---"
