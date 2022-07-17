module Bix.Types

#if ENABLE_URLPATTERN_POLYFILL
Fable.Core.JsInterop.importSideEffects "urlpattern-polyfill"
#endif

open Fable.Core
open Fable.Core.DynamicExtensions
open Browser.Types
open Fetch
open URLPattern

type RequestHandler = Request -> U2<Response, JS.Promise<Response>>
type RequestErrorHandler = exn -> U2<Response, JS.Promise<Response>>

type BixServerArgs =
    | Port of int
    | Hostname of string
    | BaseURI of string
    | MaxRequestBodySize of float
    | Development of bool
    | KeyFile of string
    | CertFile of string
    | Passphrase of string
    | CaFile of string
    | DhParamsFile of string
    | LowMemoryMode of bool
    | ServerNames of (string * BixServerArgs list) list
    | Fetch of req: RequestHandler
    | Error of req: RequestErrorHandler

type BixResponse =
    | Text of string
    | Html of string
    | Blob of content: Blob * mimeType: string
    | ArrayBuffer of content: JS.ArrayBuffer * mimeType: string
    | ArrayBufferView of content: JS.ArrayBufferView * mimeType: string
    | Json of obj
    | JsonOptions of obj * (obj -> string)
    | Custom of obj * ResponseInitProperties list

type IHostServer =
    abstract hostname: string option
    abstract port: int
    abstract development: bool
    abstract env: Map<string, string>

[<AttachMembers>]
type HttpContext(server: IHostServer, req: Request, res: Response) =
    let mutable _res = res
    let mutable hasStarted = false

    let mutable patternResult: URLPatternResult option = None

    member _.Request: Request = req
    member _.Server: IHostServer = server
    member _.Response: Response = _res
    member _.HasStarted: bool = hasStarted
    member _.RoutePattern: URLPatternResult option = patternResult
    member _.SetStarted(setStarted: bool) = hasStarted <- setStarted

    member _.SetResponse(response: Response) = _res <- response

    member _.SetPattern(pattern: URLPatternResult option) = patternResult <- pattern

    member _.SearchParams: Map<string, string option> =
        match patternResult with
        | Some result ->
            let strings = (result.search.groups["0"] :?> string).Split("&")

            seq {
                for kv in strings do
                    let values = kv.Split("=")

                    match values with
                    | [| key; value |] -> (key, Some value)
                    | [| key |] -> (key, None)
                    | values -> ("__values", Some(System.String.Join(",", values)))
            }
            |> Map.ofSeq
        | None -> Map.empty

    member _.PathParams(index: string) : string option =
        match patternResult with
        | Some result ->
            result.pathname.groups.Item index :?> string
            |> Option.ofObj
        | None -> None

    member _.HashParams(index: string) : string option =
        match patternResult with
        | Some result ->
            result.hash.groups.Item index :?> string
            |> Option.ofObj
        | None -> None

    member this.AnyParams(index: string) : string option =
        this.PathParams index
        |> Option.orElseWith (fun _ ->
            this.SearchParams
            |> Map.tryFind index
            |> Option.flatten)
        |> Option.orElseWith (fun _ -> this.HashParams index)


type HttpFuncResult = JS.Promise<BixResponse option>

type HttpFunc = HttpContext -> HttpFuncResult

type HttpHandler = HttpFunc -> HttpFunc

type RouteType =
    | Get
    | Post
    | Put
    | Delete
    | Patch
    | Head
    | Options
    | All
    | Custom of string

    static member FromString s =
        match s with
        | "GET" -> Get
        | "POST" -> Post
        | "PUT" -> Put
        | "DELETE" -> Delete
        | "PATCH" -> Patch
        | "HEAD" -> Head
        | "OPTIONS" -> Options
        | "ALL" -> All
        | custom -> Custom custom

    member this.asString =
        match this with
        | Get -> "GET"
        | Post -> "POST"
        | Put -> "PUT"
        | Delete -> "DELETE"
        | Patch -> "PATCH"
        | Head -> "HEAD"
        | Options -> "OPTIONS"
        | All -> "ALL"
        | Custom custom -> custom


type RouteDefinition =
    { method: RouteType
      pattern: URLPatternInput
      handler: HttpHandler }

type RouteList = RouteDefinition list

type RouteMap = Map<RouteType * string, HttpHandler>

type RouteMatch =
    | Found of RouteDefinition
    | NotFound
    | MethodNotAllowed
