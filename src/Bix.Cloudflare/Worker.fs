module Bix.Cloudflare.Worker

open Fable.Core
open Browser.Types
open Bix
open Bix.Router
open Bix.Handlers
open Bix.Types
open Fetch

type FetchEvent =
    inherit Event

    abstract request: Request
    abstract clientId: string
    abstract preloadResponse: JS.Promise<Response> option
    abstract replacesClientId: string
    abstract resultingClientId: string

    abstract waitUntil: unit -> JS.Promise<unit>
    abstract respondWith: U2<Response, JS.Promise<Response>> -> unit
    abstract respondWith: JS.Promise<Response> -> unit

type BixWorkerArgs = Fetch of response: (Request -> JS.Promise<Response>)

type WorkerServer(url: URL) =
    interface IHostServer with
        override _.hostname: string option = url.origin |> Option.ofObj
        override _.port: int = 0
        override _.development: bool = false
        override _.env: Map<string, string> = Map.empty

let BixHandler (routes: RouteDefinition list) (notFound: HttpHandler option) (request: Request) =
    let notFound = defaultArg notFound notFoundHandler
    let url = Browser.Url.URL.Create request.url
    let server: IHostServer = WorkerServer(url)
    let ctx = HttpContext(server, request, Response.create (""))

    Server.getRouteMatch (ctx, server.hostname.Value, notFound, routes)
    |> (Server.handleRouteMatch ctx)

let Empty = ResizeArray<BixWorkerArgs>()

let withRouter (routes: RouteDefinition list) (args: ResizeArray<BixWorkerArgs>) =
    args.Add(Fetch(BixHandler routes None))
    args

let withRouterAndNotFound (routes: RouteDefinition list) (notFound: HttpHandler) (args: ResizeArray<BixWorkerArgs>) =
    args.Add(Fetch(BixHandler routes (Some notFound)))
    args

let inline build (args: seq<BixWorkerArgs>) : {| fetch: Request -> JS.Promise<Response> |} =
    let handler =
        args
        |> Seq.tryPick (fun f ->
            match f with
            | Fetch handler -> Some handler)

    match handler with
    | Some handler -> {| fetch = handler |}
    | None ->
        eprintfn "A Handler was not assigned for this worker"
        eprintfn "Please ensure you called `Worker.withRouter routes |> Worker.run` somewhere in your worker"

        eprintfn
            "Or that at least you provided a custom handler via `Worker.run [Fetch (fun event -> Response.create())]`"

        failwith "Missing Handler"
