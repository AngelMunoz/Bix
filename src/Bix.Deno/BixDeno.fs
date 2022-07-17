namespace Bix.Deno

open Fable.Core
open Fable.Core.JsInterop

open Fetch

open Fable.Deno
open Fable.Deno.Http

open Bix
open Bix.Types

type BixDenoServer(server: Server) =

    interface IHostServer with
        override _.hostname =
            server.addrs
            |> Array.tryHead
            |> Option.map (fun f ->
                let address: NetAddr = unbox f
                address.hostname)

        override _.port =
            server.addrs
            |> Array.tryHead
            |> Option.map (fun f ->
                let address: NetAddr = unbox f
                address.port)
            |> Option.defaultValue 0

        override _.development = true
        override _.env = Map.empty

module Server =

    let inline run (args: seq<BixServerArgs>) =
        let initOptions =
            args
            |> Seq.choose (fun arg ->
                match arg with
                | Port port -> Some(Port port)
                | Hostname hostname -> Some(Hostname hostname)
                | Error err -> Some(Error err)
                | Fetch handler -> Some(Fetch handler)
                | _ -> None)
            |> keyValueList CaseRules.LowerFirst
            :?> {| port: int option
                   hostname: string option
                   error: exn -> U2<Response, JS.Promise<Response>>
                   fetch: Request -> U2<Response, JS.Promise<Response>> |}


        Http.serve (initOptions.fetch, unbox initOptions)

    let BixHandler
        (server: Server)
        (req: Request)
        (connInfo: ConnInfo)
        (routes: RouteDefinition list)
        (notFound: HttpHandler option)
        : JS.Promise<Response> =
        let notFound = defaultArg notFound Handlers.notFoundHandler
        let server: IHostServer = BixDenoServer(server)
        let ctx = HttpContext(server, req, Response.create (""))
        let reqUrl = Browser.Url.URL.Create ctx.Request.url

        Server.getRouteMatch (ctx, reqUrl.origin, notFound, routes)
        |> Server.handleRouteMatch ctx

    let withRouter (routes: RouteDefinition list) (args: ResizeArray<BixServerArgs>) =
        // HACK: we need to ensure that fable doesn't wrap the request handler
        // in an anonymnous function or we will lose "this" which equates to the
        // bun's server instance
        emitJsStatement
            (routes)
            "function handler(req, connInfo) { return Server_BixHandler(this, req, connInfo, $0); }"

        args.Add(Fetch(emitJsExpr () "handler"))
        args

    let withRouterAndNotFound
        (routes: RouteDefinition list)
        (notFound: HttpHandler)
        (args: ResizeArray<BixServerArgs>)
        =
        // HACK: we need to ensure that fable doesn't wrap the request handler
        // in an anonymnous function or we will lose "this" which equates to the
        // bun's server instance
        emitJsStatement
            (routes, notFound)
            "function handler(req, connInfo) { return Server_BixHandler(this, req, connInfo, $0, $1); }"

        args.Add(Fetch(emitJsExpr () "handler"))
        args
