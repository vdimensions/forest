namespace Forest.Web.WebSharper.UI.PhysicalViews.Provided

open Microsoft.FSharp.Core.CompilerServices
open ProviderImplementation.ProvidedTypes
open System
open System.Reflection
open Axle.Reflection.Extensions.Type
open Forest

[<TypeProvider>]
type public ProvidedWebSharperPhysicalViewProvider(config : TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces(config)

    let ns = "Forest.Web.WebSharper.UI.PhysicalViews.Provided"
    let assembly = Assembly.GetExecutingAssembly()
    let name = "ClientView"
    //let baseTy = typeof<Forest.Web.WebSharper.UI.WebSharperPhysicalView>
    let baseTy = typeof<obj>
    let staticParams = [ProvidedStaticParameter("Target", typeof<string>)]
    let typeDef = ProvidedTypeDefinition(assembly, ns, name, Some baseTy)

    let setup() =
        typeDef.DefineStaticParameters(
            parameters = staticParams,
            instantiationFunction = (fun typeName paramValues ->
                match paramValues with
                | [| :? string as tn |] ->
                    let viewType =
                        try Type.GetType tn
                        with e -> failwithf "Invalid view type name '%s'. Detailed message is '%s' " tn e.Message
                    let vname = viewType.GetCustomAttribute<ViewAttribute>().Name

                    let ty = 
                        ProvidedTypeDefinition(
                            assembly, 
                            ns, 
                            typeName, 
                            baseType = Some baseTy)

                    let prop = 
                        ProvidedProperty(
                            isStatic = true,
                            propertyName = vname, 
                            propertyType = typeof<string>, 
                            getterCode = fun _ -> <@@ vname @@>)
                            //prop.AddXmlDoc(sprintf @"Gets the ""%s"" group from this match" group)
                    ty.AddMember(prop)

                    let ctor = 
                        ProvidedConstructor(
                            parameters = [], 
                            invokeCode = fun _ -> <@@ vname :> obj @@>)

                    ty.AddMember ctor

                    ty
                | _ -> failwith "unexpected parameter values"
            )
        )
        this.AddNamespace(ns, [typeDef]) 
        
    do setup()

    override this.ResolveAssembly(args) =
        eprintfn "Type provider looking for assembly: %s" args.Name
        let name = AssemblyName(args.Name).Name.ToLowerInvariant()
        // First try to find the assembly in the same directory as this one
        let nextToThis = System.IO.FileInfo(System.IO.Path.Combine(System.IO.Path.GetDirectoryName(assembly.Location), name + ".dll"))
        if nextToThis.Exists then Assembly.LoadFrom nextToThis.FullName else
        let an =
            config.ReferencedAssemblies
            |> Seq.tryFind (fun an ->
                System.IO.Path.GetFileNameWithoutExtension(an).ToLowerInvariant() = name)
        match an with
        | Some f -> Assembly.LoadFrom f
        | None ->
            eprintfn "Type provider didn't find assembly: %s" args.Name
            null

