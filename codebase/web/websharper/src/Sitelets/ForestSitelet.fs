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
            | ForestEndPoint.ForestTree tree -> f.LoadTree tree
            | ForestEndPoint.ForestCommand (name, hash, arg) -> f.ExecuteCommand name hash arg
            let shellID = "shell"
            let body = [
                div [ attr.id shellID; on.afterRender <@ fun this -> ClientCode.init(); ClientCode.render() |> Doc.RunReplace this @> ] []
            ] 
            dop h (b @ body |> Doc.Concat)
            |> Content.Page
        )