open Bix
open Bix.Types
open Bix.Router
open Bix.Handlers
open Bix.Cloudflare

let private basicRouter =
    Router.Empty
    |> Router.get ("/", sendHtml "<h1>Hello, World!</h1>")
    |> Router.post ("/text", sendText "Posted to /text")
    |> Router.patch ("/json", sendJson {| message = "posted to /json" |})

let postHandler: HttpHandler =
    fun next ctx ->
        promise {
            let! content = ctx.Request.json ()
            return! (sendJson content) next ctx
        }

let todoController =
    controller {
        find (fun _ -> Text "Found Hit" |> Promise.lift)
        findOne (fun id _ -> Text $"Found Hit %A{id}" |> Promise.lift)
        create (fun _ -> Text $"Create Hit" |> Promise.lift)
        update (fun _ -> Text $"Update Hit" |> Promise.lift)
        updateOne (fun id _ -> Text $"Update Hit %A{id}" |> Promise.lift)
        delete (fun id _ -> Text $"Delete Hit %A{id}" |> Promise.lift)
    }

let private saturnRouter =
    router {
        get "/" (sendHtml "<h1>Hello, World!</h1>")
        get "/text" (sendText "Hello World!")
        get "/json" (sendJson {| message = "Hello, World!" |})
        post "/json" postHandler
        forward "/todos" todoController
    }

let private worker =
    Worker.Empty
    |> Worker.withRouter ([ yield! basicRouter; yield! saturnRouter ])
    |> Worker.build

// ES Modules need to export the fetch handler as per cloudflare documentation
Fable.Core.JsInterop.exportDefault worker
