module Bix.Router.Giraffe

open Fable.Core
open Bix.Types

type GiraffeRoute =
    | Route of RouteDefinition
    | Routes of RouteDefinition list

let inline route url handler =
    { method = All
      pattern = unbox url
      handler = handler }
    |> Route

let inline flattenGRoutes (routeType: RouteType) (routes: GiraffeRoute list) : GiraffeRoute =
    routes
    |> List.fold
        (fun (prev: RouteDefinition list) next ->
            match next with
            | Route route -> { route with method = routeType } :: prev
            | Routes routes -> (routes |> List.map (fun r -> { r with method = routeType })) @ prev)
        List.empty<RouteDefinition>
    |> Routes

let inline choose (routes: GiraffeRoute list) : RouteDefinition list =
    [ for routes in routes do
          match routes with
          | Route route -> route
          | Routes routes -> yield! routes ]

let inline subRoute (url: string) (routes: GiraffeRoute list) =
    [ for route in routes do
          match route with
          | Route route -> { route with pattern = unbox $"{url}{route.pattern}" }
          | Routes routes ->
              yield!
                  routes
                  |> List.map (fun route -> { route with pattern = unbox $"{url}{route.pattern}" }) ]
    |> Routes

let inline GET (routes: GiraffeRoute list) = flattenGRoutes Get routes

let inline POST (routes: GiraffeRoute list) = flattenGRoutes Post routes

let inline PUT (routes: GiraffeRoute list) = flattenGRoutes Put routes

let inline PATCH (routes: GiraffeRoute list) = flattenGRoutes Patch routes

let inline DELETE (routes: GiraffeRoute list) = flattenGRoutes Delete routes

let inline HEAD (routes: GiraffeRoute list) = flattenGRoutes Head routes

let inline OPTIONS (routes: GiraffeRoute list) = flattenGRoutes Options routes

let inline CUSTOM (method: string) (routes: GiraffeRoute list) = flattenGRoutes (Custom method) routes
