module Worker

open Bix
open Bix.Router
open Bix.Handlers

open Bix.Cloudflare


let routes =
    Router.Empty
    |> Router.get ("/", sendHtml "<h1>Hello, World!</h1>")
    |> Router.get ("/text", sendText "Hello World!")
    |> Router.get ("/json", sendJson {| message = "Hello, World!" |})

let worker = Worker.Empty |> Worker.withRouter routes |> Worker.build

// ES Modules need to export the fetch handler as per cloudflare documentation
Fable.Core.JsInterop.exportDefault worker
