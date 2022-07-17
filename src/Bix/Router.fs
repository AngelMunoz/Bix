module Bix.Router

open URLPattern
open Bix.Types

let Empty = List.empty

type Router =
    static member inline get
        (path: string, handler: HttpHandler)
        (routes: RouteDefinition list)
        : RouteDefinition list =
        { method = Get
          pattern = unbox path
          handler = handler }
        :: routes

    static member inline post
        (path: string, handler: HttpHandler)
        (routes: RouteDefinition list)
        : RouteDefinition list =
        { method = Post
          pattern = unbox path
          handler = handler }
        :: routes

    static member inline put
        (path: string, handler: HttpHandler)
        (routes: RouteDefinition list)
        : RouteDefinition list =
        { method = Put
          pattern = unbox path
          handler = handler }
        :: routes

    static member inline delete
        (path: string, handler: HttpHandler)
        (routes: RouteDefinition list)
        : RouteDefinition list =
        { method = Delete
          pattern = unbox path
          handler = handler }
        :: routes

    static member inline patch
        (path: string, handler: HttpHandler)
        (routes: RouteDefinition list)
        : RouteDefinition list =
        { method = Patch
          pattern = unbox path
          handler = handler }
        :: routes

    static member inline head
        (path: string, handler: HttpHandler)
        (routes: RouteDefinition list)
        : RouteDefinition list =
        { method = Head
          pattern = unbox path
          handler = handler }
        :: routes

    static member inline all
        (path: string, handler: HttpHandler)
        (routes: RouteDefinition list)
        : RouteDefinition list =
        { method = All
          pattern = unbox path
          handler = handler }
        :: routes

    static member inline custom
        (method: string, path: string, handler: HttpHandler)
        (routes: RouteDefinition list)
        : RouteDefinition list =
        { method = Custom method
          pattern = unbox path
          handler = handler }
        :: routes

    static member inline getP
        (patternArgs: UrlInitArgs list, handler: HttpHandler)
        (routes: RouteDefinition list)
        : RouteDefinition list =
        { method = Get
          pattern = unbox (patternArgs |> UrlPatternInit.fromArgs)
          handler = handler }
        :: routes


    static member inline postP
        (patternArgs: UrlInitArgs list, handler: HttpHandler)
        (routes: RouteDefinition list)
        : RouteDefinition list =
        { method = Post
          pattern = unbox (patternArgs |> UrlPatternInit.fromArgs)
          handler = handler }
        :: routes

    static member inline putP
        (patternArgs: UrlInitArgs list, handler: HttpHandler)
        (routes: RouteDefinition list)
        : RouteDefinition list =
        { method = Put
          pattern = unbox (patternArgs |> UrlPatternInit.fromArgs)
          handler = handler }
        :: routes

    static member inline deleteP
        (patternArgs: UrlInitArgs list, handler: HttpHandler)
        (routes: RouteDefinition list)
        : RouteDefinition list =
        { method = Delete
          pattern = unbox (patternArgs |> UrlPatternInit.fromArgs)
          handler = handler }
        :: routes

    static member inline patchP
        (patternArgs: UrlInitArgs list, handler: HttpHandler)
        (routes: RouteDefinition list)
        : RouteDefinition list =
        { method = Patch
          pattern = unbox (patternArgs |> UrlPatternInit.fromArgs)
          handler = handler }
        :: routes

    static member inline headP
        (patternArgs: UrlInitArgs list, handler: HttpHandler)
        (routes: RouteDefinition list)
        : RouteDefinition list =
        { method = Head
          pattern = unbox (patternArgs |> UrlPatternInit.fromArgs)
          handler = handler }
        :: routes

    static member inline allP
        (patternArgs: UrlInitArgs list, handler: HttpHandler)
        (routes: RouteDefinition list)
        : RouteDefinition list =
        { method = All
          pattern = unbox (patternArgs |> UrlPatternInit.fromArgs)
          handler = handler }
        :: routes

    static member inline customP
        (method: string, patternArgs: UrlInitArgs list, handler: HttpHandler)
        (routes: RouteDefinition list)
        : RouteDefinition list =
        { method = Custom method
          pattern = unbox (patternArgs |> UrlPatternInit.fromArgs)
          handler = handler }
        :: routes
