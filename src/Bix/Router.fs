[<RequireQualifiedAccess>]
module Bix.Router.Router

open Bix.Types


let inline route url handler =
    { method = All
      pattern = unbox url
      handler = handler }

let Empty: list<RouteDefinition> = List.empty

let inline get
    (path: string, [<InlineIfLambda>] handler: HttpHandler)
    (routes: RouteDefinition list)
    : RouteDefinition list =
    { method = Get
      pattern = unbox path
      handler = handler }
    :: routes

let inline post
    (path: string, [<InlineIfLambda>] handler: HttpHandler)
    (routes: RouteDefinition list)
    : RouteDefinition list =
    { method = Post
      pattern = unbox path
      handler = handler }
    :: routes

let inline put
    (path: string, [<InlineIfLambda>] handler: HttpHandler)
    (routes: RouteDefinition list)
    : RouteDefinition list =
    { method = Put
      pattern = unbox path
      handler = handler }
    :: routes

let inline delete
    (path: string, [<InlineIfLambda>] handler: HttpHandler)
    (routes: RouteDefinition list)
    : RouteDefinition list =
    { method = Delete
      pattern = unbox path
      handler = handler }
    :: routes

let inline patch
    (path: string, [<InlineIfLambda>] handler: HttpHandler)
    (routes: RouteDefinition list)
    : RouteDefinition list =
    { method = Patch
      pattern = unbox path
      handler = handler }
    :: routes

let inline head
    (path: string, [<InlineIfLambda>] handler: HttpHandler)
    (routes: RouteDefinition list)
    : RouteDefinition list =
    { method = Head
      pattern = unbox path
      handler = handler }
    :: routes

let inline all
    (path: string, [<InlineIfLambda>] handler: HttpHandler)
    (routes: RouteDefinition list)
    : RouteDefinition list =
    { method = All
      pattern = unbox path
      handler = handler }
    :: routes

let inline custom
    (method: string, path: string, [<InlineIfLambda>] handler: HttpHandler)
    (routes: RouteDefinition list)
    : RouteDefinition list =
    { method = Custom method
      pattern = unbox path
      handler = handler }
    :: routes

let inline subRoute
    (url: string, newRoutes: RouteDefinition list -> RouteDefinition list)
    (routes: RouteDefinition list)
    =
    (newRoutes []
     |> List.map (fun route -> { route with pattern = unbox $"{url}{route.pattern}" }))
    @ routes
