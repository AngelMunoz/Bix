module Bix.Router

open URLPattern
open Fable.Core
open Bix.Types

type Router =

    static member inline Empty: list<RouteDefinition> = List.empty

    static member inline get
        (path: string, [<InlineIfLambda>] handler: HttpHandler)
        (routes: RouteDefinition list)
        : RouteDefinition list =
        { method = Get
          pattern = unbox path
          handler = handler }
        :: routes

    static member inline post
        (path: string, [<InlineIfLambda>] handler: HttpHandler)
        (routes: RouteDefinition list)
        : RouteDefinition list =
        { method = Post
          pattern = unbox path
          handler = handler }
        :: routes

    static member inline put
        (path: string, [<InlineIfLambda>] handler: HttpHandler)
        (routes: RouteDefinition list)
        : RouteDefinition list =
        { method = Put
          pattern = unbox path
          handler = handler }
        :: routes

    static member inline delete
        (path: string, [<InlineIfLambda>] handler: HttpHandler)
        (routes: RouteDefinition list)
        : RouteDefinition list =
        { method = Delete
          pattern = unbox path
          handler = handler }
        :: routes

    static member inline patch
        (path: string, [<InlineIfLambda>] handler: HttpHandler)
        (routes: RouteDefinition list)
        : RouteDefinition list =
        { method = Patch
          pattern = unbox path
          handler = handler }
        :: routes

    static member inline head
        (path: string, [<InlineIfLambda>] handler: HttpHandler)
        (routes: RouteDefinition list)
        : RouteDefinition list =
        { method = Head
          pattern = unbox path
          handler = handler }
        :: routes

    static member inline all
        (path: string, [<InlineIfLambda>] handler: HttpHandler)
        (routes: RouteDefinition list)
        : RouteDefinition list =
        { method = All
          pattern = unbox path
          handler = handler }
        :: routes

    static member inline custom
        (method: string, path: string, [<InlineIfLambda>] handler: HttpHandler)
        (routes: RouteDefinition list)
        : RouteDefinition list =
        { method = Custom method
          pattern = unbox path
          handler = handler }
        :: routes

    static member inline getP
        (patternArgs: UrlInitArgs list, [<InlineIfLambda>] handler: HttpHandler)
        (routes: RouteDefinition list)
        : RouteDefinition list =
        { method = Get
          pattern = unbox (patternArgs |> UrlPatternInit.fromArgs)
          handler = handler }
        :: routes


    static member inline postP
        (patternArgs: UrlInitArgs list, [<InlineIfLambda>] handler: HttpHandler)
        (routes: RouteDefinition list)
        : RouteDefinition list =
        { method = Post
          pattern = unbox (patternArgs |> UrlPatternInit.fromArgs)
          handler = handler }
        :: routes

    static member inline putP
        (patternArgs: UrlInitArgs list, [<InlineIfLambda>] handler: HttpHandler)
        (routes: RouteDefinition list)
        : RouteDefinition list =
        { method = Put
          pattern = unbox (patternArgs |> UrlPatternInit.fromArgs)
          handler = handler }
        :: routes

    static member inline deleteP
        (patternArgs: UrlInitArgs list, [<InlineIfLambda>] handler: HttpHandler)
        (routes: RouteDefinition list)
        : RouteDefinition list =
        { method = Delete
          pattern = unbox (patternArgs |> UrlPatternInit.fromArgs)
          handler = handler }
        :: routes

    static member inline patchP
        (patternArgs: UrlInitArgs list, [<InlineIfLambda>] handler: HttpHandler)
        (routes: RouteDefinition list)
        : RouteDefinition list =
        { method = Patch
          pattern = unbox (patternArgs |> UrlPatternInit.fromArgs)
          handler = handler }
        :: routes

    static member inline headP
        (patternArgs: UrlInitArgs list, [<InlineIfLambda>] handler: HttpHandler)
        (routes: RouteDefinition list)
        : RouteDefinition list =
        { method = Head
          pattern = unbox (patternArgs |> UrlPatternInit.fromArgs)
          handler = handler }
        :: routes

    static member inline allP
        (patternArgs: UrlInitArgs list, [<InlineIfLambda>] handler: HttpHandler)
        (routes: RouteDefinition list)
        : RouteDefinition list =
        { method = All
          pattern = unbox (patternArgs |> UrlPatternInit.fromArgs)
          handler = handler }
        :: routes

    static member inline customP
        (method: string, patternArgs: UrlInitArgs list, [<InlineIfLambda>] handler: HttpHandler)
        (routes: RouteDefinition list)
        : RouteDefinition list =
        { method = Custom method
          pattern = unbox (patternArgs |> UrlPatternInit.fromArgs)
          handler = handler }
        :: routes

let inline itemsHandler (handler: HttpContext -> JS.Promise<BixResponse>) (next: HttpFunc) (ctx: HttpContext) =
    handler ctx
    |> Promise.map (fun bixRes ->
        ctx.SetStarted true
        Some bixRes)

let inline singleItemhandler
    (handler: string option -> HttpContext -> JS.Promise<BixResponse>)
    (next: HttpFunc)
    (ctx: HttpContext)
    =
    handler (ctx.PathParams "id") ctx
    |> Promise.map (fun bixRes ->
        ctx.SetStarted true
        Some bixRes)

type ControllerBuilder() =
    member inline _.Yield _ = []

    [<CustomOperation("find")>]
    member inline _.Find(state, [<InlineIfLambda>] handler: HttpContext -> JS.Promise<BixResponse>) =

        { method = Get
          pattern = unbox ""
          handler = itemsHandler handler }
        :: state

    [<CustomOperation("findOne")>]
    member inline _.FindOne
        (
            state,
            [<InlineIfLambda>] handler: string option -> HttpContext -> JS.Promise<BixResponse>
        ) =
        { method = Get
          pattern = unbox "/:id"
          handler = singleItemhandler handler }
        :: state

    [<CustomOperation("create")>]
    member inline _.Create(state, [<InlineIfLambda>] handler: HttpContext -> JS.Promise<BixResponse>) =
        { method = Post
          pattern = unbox ""
          handler = itemsHandler handler }
        :: state

    [<CustomOperation("update")>]
    member inline _.Update(state, [<InlineIfLambda>] handler: HttpContext -> JS.Promise<BixResponse>) =
        { method = Put
          pattern = unbox ""
          handler = itemsHandler handler }
        :: state

    [<CustomOperation("updateOne")>]
    member inline _.UpdateOne
        (
            state,
            [<InlineIfLambda>] handler: string option -> HttpContext -> JS.Promise<BixResponse>
        ) =
        { method = Put
          pattern = unbox "/:id"
          handler = singleItemhandler handler }
        :: state

    [<CustomOperation("delete")>]
    member inline _.Delete(state, [<InlineIfLambda>] handler: string option -> HttpContext -> JS.Promise<BixResponse>) =
        { method = Delete
          pattern = unbox "/:id"
          handler = singleItemhandler handler }
        :: state

    member inline _.Run(state) : RouteDefinition list = state

type RouterBuilder() =

    member inline _.Yield _ = []

    [<CustomOperation("get")>]
    member inline _.Get(state, path: string, [<InlineIfLambda>] handler: HttpHandler) =
        { method = Get
          pattern = unbox path
          handler = handler }
        :: state

    [<CustomOperation("post")>]
    member inline _.Post(state, path: string, [<InlineIfLambda>] handler: HttpHandler) =
        { method = Post
          pattern = unbox path
          handler = handler }
        :: state

    [<CustomOperation("put")>]
    member inline _.Put(state, path: string, [<InlineIfLambda>] handler: HttpHandler) =
        { method = Put
          pattern = unbox path
          handler = handler }
        :: state

    [<CustomOperation("patch")>]
    member inline _.Patch(state, path: string, [<InlineIfLambda>] handler: HttpHandler) =
        { method = Patch
          pattern = unbox path
          handler = handler }
        :: state

    [<CustomOperation("delete")>]
    member inline _.Delete(state, path: string, [<InlineIfLambda>] handler: HttpHandler) =
        { method = Delete
          pattern = unbox path
          handler = handler }
        :: state

    [<CustomOperation("custom")>]
    member inline _.Delete(state, method: string, path: string, [<InlineIfLambda>] handler: HttpHandler) =
        { method = Custom method
          pattern = unbox path
          handler = handler }
        :: state

    [<CustomOperation("forward")>]
    member inline _.Forward(state: RouteDefinition list, url: string, routes: RouteDefinition list) =

        let routes =
            routes
            |> List.map (fun r -> { r with pattern = unbox ($"{url}{r.pattern}") })
            |> List.append state

        routes

    member inline _.Run(state) : RouteDefinition list = state

let router = RouterBuilder()
let controller = ControllerBuilder()
