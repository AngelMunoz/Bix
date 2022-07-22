open Bix.Types
open Bix
open Bix.Router
open Bix.Router.Saturn
open Bix.Handlers
open Bix.Cloudflare

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

let private worker = Worker.Empty |> Worker.withRouter routes |> Worker.build

// ES Modules need to export the fetch handler as per cloudflare documentation
Fable.Core.JsInterop.exportDefault worker
