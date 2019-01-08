namespace Forest.Web.WebSharper.Sitelets

open Forest
open Forest.Web.WebSharper
open WebSharper
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Client
open WebSharper.UI.Html
open WebSharper.UI.Server

module internal ForestSitelet =
    [<Website>]
    let Run (f : IForestFacade) (dop : (Doc -> Doc -> Doc)) (h : Doc) (b : Doc list) =
        Application.MultiPage (fun _ endpoint ->
            match endpoint with
            | ForestEndPoint.ForestTree tree -> f.LoadTemplate tree
            | ForestEndPoint.ForestCommand (hash, name, arg) -> f.ExecuteCommand hash name arg
            let shellID = "shell"
            let body = [
                div [ Attr.Create "id" shellID ] []
                script [ on.afterRender <@ fun _ -> ClientCode.render() |> Doc.RunReplaceById shellID @> ] []
                script [ on.afterRender <@ fun _ -> ClientCode.init() @> ] []
            ] 
            dop h (body |> Doc.Concat)
            |> Content.Page
        )