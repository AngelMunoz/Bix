namespace Bix.Bun

open Fable.Core
open Fable.Core.JsInterop

open Fable.Bun
open Fetch

open Bix
open Bix.Types

type BixBunServer(server: BunServer) =

    interface IHostServer with
        override _.hostname = server.hostname |> Option.ofObj
        override _.port = server.port
        override _.development = server.development
        override _.env = Map.empty

module Server =

    let inline run (args: seq<BixServerArgs>) =

        let serverNamesObj =
            args
            |> Seq.tryPick (fun f ->
                match f with
                | ServerNames names -> Some names
                | _ -> None)
            |> Option.map (fun args ->
                [ for (name, args) in args do
                      let args = keyValueList CaseRules.LowerFirst args

                      name, args ]
                |> createObj)

        let restArgs =
            args
            |> Seq.choose (fun f ->
                match f with
                | ServerNames _ -> None
                | others -> Some others)
            |> keyValueList CaseRules.LowerFirst

        match serverNamesObj with
        | Some names ->
            let options =
                unbox Fable.Core.JS.Constructors.Object.assign ({|  |}, restArgs, names)

            Bun.serve (options)
        | None -> Bun.serve (unbox restArgs)

    let BixHandler
        (server: BunServer)
        (req: Request)
        (routes: RouteDefinition list)
        (notFound: HttpHandler option)
        : JS.Promise<Response> =

        let notFound = defaultArg notFound Handlers.notFoundHandler
        let server: IHostServer = BixBunServer(server)
        let ctx = HttpContext(server, req, Response.create (""))

        Server.getRouteMatch (ctx, server.hostname.Value, notFound, routes)
        |> Server.handleRouteMatch ctx

    let withRouter (routes: RouteDefinition list) (args: ResizeArray<BixServerArgs>) =
        // HACK: we need to ensure that fable doesn't wrap the request handler
        // in an anonymnous function or we will lose "this" which equates to the
        // bun's server instance
        emitJsStatement (routes) "function handler(req) { return Server_BixHandler(this, req, $0); }"
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
        emitJsStatement (routes, notFound) "function handler(req) { return Server_BixHandler(this, req, $0, $1); }"
        args.Add(Fetch(emitJsExpr () "handler"))
        args
