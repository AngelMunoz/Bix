open Bix
open Bix.Router
open Bix.Types
open Bix.Handlers
open Bix.Cloudflare

let private postHandler: HttpHandler =
    fun next ctx ->
        promise {
            let! content = ctx.Request.json ()
            return! (sendJson content) next ctx
        }


let private basicRouter =
    Router.Empty
    |> Router.get ("/basic", sendHtml "<h1>Hello, World!</h1>")
    |> Router.post ("/text", postHandler)
    |> Router.patch ("/json", sendJson {| message = "posted to /json" |})
    |> Router.subRoute (
        "/profiles",
        (Router.get ("/", sendText "profiles")
         >> Router.post ("/", sendText "create profile")
         >> Router.put ("/:id", sendText "update profile")
         >> Router.delete ("/:id", sendText "delete profile"))
    )

[<RequireQualifiedAccess>]
module GiraffeLike =

    open Bix.Router.Giraffe

    let routes =
        choose
            [ route "/g" (sendText "g")
              GET [ route "/g1" (sendText "G1") ]
              POST [ route "/g2" postHandler ]
              subRoute
                  "/products"
                  [ GET
                        [ route "/" (sendText "hit get all products")
                          route "/:id" (sendText "hit get id product") ]
                    POST
                        [ route "/" (sendText "hit products post")
                          route "/:id/category" (sendText "hit products id category post") ]
                    subRoute
                        "/:id/offers"
                        [ route "/admin" (sendText "hit all offers for product id admin")
                          GET
                              [ route "" (sendText "hit get offers")
                                route "/:id" (sendText "hit get offer id get") ] ] ] ]

[<RequireQualifiedAccess>]
module SaturnLike =
    open Bix.Router.Saturn

    let routes =
        router {
            get "/" (sendHtml "<h1>Hello, World!</h1>")
            post "/json" postHandler

            forward
                "/offers"
                (router {
                    get "/" (sendText "hit get offers")
                    forward "/:id/tags" (router { get "/" (sendText "hit get offers id tags") })
                })

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

let private worker =
    Worker.Empty
    |> Worker.withRouter [ yield! basicRouter; yield! SaturnLike.routes; yield! GiraffeLike.routes ]
    |> Worker.build

// ES Modules need to export the fetch handler as per cloudflare documentation
Fable.Core.JsInterop.exportDefault worker
