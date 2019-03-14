namespace Forest.Web.WebSharper.Sitelets
open Forest
open Forest.Web.WebSharper.UI
open WebSharper
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Html
open WebSharper.UI.Server

module internal ForestSitelet =
    let Render (f : IForestFacade) (pvs: array<vname*WebSharperPhysicalView>) (dop : (Doc -> Doc -> Doc)) (h : Doc) (b : Doc list) (endpoint : ForestEndPoint<'msg>) =
        match endpoint with
        | ForestEndPoint.ForestTreeWithMessage (tree, msg) -> 
            f.LoadTree(tree, msg)
        | ForestEndPoint.ForestTree tree -> 
            f.LoadTree tree
        | ForestEndPoint.ForestCommand (name, hash, arg) -> 
            f.ExecuteCommand name hash arg
        let shellID = "shell"
        let body = [
            div [ attr.id shellID; on.afterRender <@ fun this -> Client.init(); Client.render() |> Doc.RunReplace this @> ] []
        ] 
        dop h (b @ body |> Doc.Concat)
        |> Content.Page

    let Run (f : IForestFacade) (pvs: array<vname*WebSharperPhysicalView>) (dop : (Doc -> Doc -> Doc)) (h : Doc) (b : Doc list) =
        Application.MultiPage (fun _ endpoint ->
            Render f pvs dop h b endpoint
        )