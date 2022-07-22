module Pages

open Bix
open Bix.Types
open Bix.Handlers
open Feliz.ViewEngine
open Fetch

type private Views =
    static member inline Layout(content: ReactElement, ?head: ReactElement seq, ?scripts: ReactElement seq) =
        let head = defaultArg head []
        let scripts = defaultArg scripts []

        Html.html
            [ Html.head [ prop.children [ yield! head ] ]
              Html.body [ prop.children [ content; yield! scripts ] ] ]

let Home (req: Request) =
    Views.Layout(
        Html.article
            [ Html.nav
                  [ Html.li [ Html.a [ prop.href "/"; prop.text "Home" ] ]
                    Html.li [ Html.a [ prop.href "/about"; prop.text "About" ] ] ]
              Html.main [ Html.h1 $"Hello from {req.method} - {req.url}" ]
              Html.footer [] ]
    )
    |> Render.htmlDocument

let About () =
    Views.Layout(
        Html.article
            [ Html.nav []
              Html.main [ Html.h1 $"This is the about page!" ]
              Html.footer [] ]
    )
    |> Render.htmlDocument
