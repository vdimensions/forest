namespace Forest.Web.WebSharper.Sitelets

open Forest
open Forest.Web.WebSharper
open WebSharper
open WebSharper.Sitelets
open WebSharper.UI
open WebSharper.UI.Html
open WebSharper.UI.Server

module internal ForestSitelet =
    [<Website>]
    let Run (f : IForestFacade) (dop : (Doc -> Doc -> Doc)) (h : Doc) (b : Doc list) =
        let shellHash = TreeNode.shell.Hash
        Application.MultiPage (fun _ endpoint ->
            match endpoint with
            | ForestEndPoint.ForestTree tree -> f.LoadTemplate tree
            | ForestEndPoint.ForestCommand (hash, name, arg) -> f.ExecuteCommand hash name arg
            dop h ((div [ on.afterRender <@ ClientCode.afterRender @> ] [])::[client <@ (ClientCode.render shellHash) @>] |> Doc.Concat)
            |> Content.Page
        )