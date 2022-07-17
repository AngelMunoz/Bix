// For more information see https://aka.ms/fsharp-console-apps
open Bix
open Bix.Router
open Bix.Deno

[<EntryPoint>]
let main argv =
    Server.Empty
    |> Server.withDevelopment true
    |> Server.withPort 5000
    |> Server.withRouter (
        Router.Empty
        |> Router.get ("/", Handlers.home)
        |> Router.get ("/json", Handlers.json)
        |> Router.get ("/params/:name/:value", Handlers.paramsHandler)
        |> Router.post ("/json", Handlers.jsonPostHandler)
        |> Router.get ("/text", Handlers.text)
        |> Router.get ("/login", Handlers.login)
        |> Router.get ("/protected", (Handlers.checkCredentials >=> Handlers.home))
    )
    |> Server.run
    |> Promise.start

    0
