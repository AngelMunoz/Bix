// For more information see https://aka.ms/fsharp-console-apps
open Bix.Types
open Bix
open Bix.Router
open Bix.Router.Saturn
open Bix.Handlers
open Bix.Deno

let private postHandler: HttpHandler =
    fun next ctx ->
        promise {
            let! content = ctx.Request.json ()
            return! (sendJson content) next ctx
        }

let routes =
    router {
        get "/" (fun next ctx -> sendHtml (Pages.Home ctx.Request) next ctx)

        get "/about" (sendHtml (Pages.About()))
        post "/json" postHandler

        forward
            "/offers"
            (router { get "/" (sendText "hit get offers") })

            forward
            "/todos"
            (controller {
                find (fun _ -> Text "Found Hit" |> Promise.lift)
                findOne (fun id _ -> Text $"Found Hit %A{id}" |> Promise.lift)
                create (fun _ -> Text $"Create Hit" |> Promise.lift)
                update (fun _ -> Text $"Update Hit" |> Promise.lift)
                updateOne (fun id _ -> Text $"Update Hit %A{id}" |> Promise.lift)
                delete (fun id _ -> Text $"Delete Hit %A{id}" |> Promise.lift)
            })
    }

Server.Empty
|> Server.withDevelopment true
|> Server.withPort 5000
|> Server.withRouter routes
|> Server.run
|> Promise.start
