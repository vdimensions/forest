namespace Forest.Resources

open System
open System.Globalization
open System.Reflection
open Axle
open Axle.References
open Axle.Resources
open Forest.Templates.Raw


type [<Sealed>] ResourceTemplateProvider(rm:ResourceManager) =
    [<DefaultValue>]
    val mutable private _bundles : string list voption
    [<DefaultValue>]
    val mutable private _assemblies : Set<string> Nullsafe

    static member BundleName = "ForestTemplates"

    member internal this.AddBundle(bundle : string) = 
        this._bundles <- 
            match this._bundles with
            | ValueSome b -> bundle::b
            | ValueNone -> [bundle]
            |> ValueSome

    member internal this.RegisterAssemblySource(asm : Assembly) =
        let asmname = asm.GetName().Name
        let set =
            match Option.ns2vopt this._assemblies with
            | ValueSome x -> x
            | ValueNone -> Set.empty
        if set |> Set.contains asmname |> not then
            let parseUri = (Axle.Conversion.Parsing.UriParser()).Parse
            for bundle in this.Bundles do
                rm.Bundles
                    .Configure(bundle)
                    .Register(String.Format("assembly://{0}/{1}", asmname, bundle) |> parseUri)
                    |> ignore
            this._assemblies <- set |> Set.add asmname |> Nullsafe.Some

    member internal this.Bundles 
        with get() = 
            match this._bundles with
            | ValueSome b -> b
            | ValueNone -> []
        
    interface ITemplateProvider with
        member this.Load name =
            let rec load bundles =
                match bundles with
                | [] -> raise <| ResourceNotFoundException(name, ResourceTemplateProvider.BundleName, CultureInfo.InvariantCulture)
                | bundle::rest ->
                    let template = rm.Load(bundle, name, CultureInfo.InvariantCulture)
                    if template.HasValue
                    then template.Value.Resolve<Template>()
                    else load rest
            this.Bundles |> load
            