namespace Forest.Web.WebSharper.Sitelets
open Forest
open Forest.Engine
open Forest.Web.WebSharper.UI
open WebSharper
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Html
open WebSharper.UI.Server

module internal ForestSitelet =
    //let Render (ctx : Context<ForestEndPoint<'msg>>) (f : IForestEngine) (pvs: array<vname*WebSharperPhysicalView>) (dop : (Doc -> Doc -> Doc)) (h : Doc) (b : Doc list) (endpoint : ForestEndPoint<'msg>) =
    let Render (f : IForestEngine) (pvs: array<string*WebSharperPhysicalView>) (dop : (Doc -> Doc -> Doc)) (h : Doc) (b : Doc list) (endpoint : ForestEndPoint<'msg>) =
        match endpoint with
        | ForestEndPoint.ForestTreeWithMessage (tree, msg) -> 
            f.Navigate(tree, msg)
        | ForestEndPoint.ForestTree tree -> 
            f.Navigate tree
        | ForestEndPoint.ForestCommand (name, instanceID, arg) -> 
            f.ExecuteCommand(name, instanceID, arg)
        let shellID = "shell"
        let body = [
            div [ attr.id shellID; on.afterRender <@ fun this -> Client.init(); Client.render() |> Doc.RunReplace this @> ] []
        ] 
        dop h (b @ body |> Doc.Concat)
        |> Content.Page

    let Run (f : IForestEngine) (pvs: array<string*WebSharperPhysicalView>) (dop : (Doc -> Doc -> Doc)) (h : Doc) (b : Doc list) =
        Application.MultiPage (fun ctx endpoint ->
            //Render ctx f pvs dop h b endpoint
            Render f pvs dop h b endpoint
        )