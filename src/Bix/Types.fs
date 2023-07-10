module Bix.Types

#if ENABLE_URLPATTERN_POLYFILL
Fable.Core.JsInterop.importSideEffects "urlpattern-polyfill"
#endif

open Fable.Core
open Fable.Core.DynamicExtensions
open Browser.Types
open Fetch
open URLPattern
open System

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

type SearchParamValue =
    | String of value: string option
    | StringArray of value: string option ResizeArray

    member this.AsString =
        match this with
        | String(Some value) -> value
        | StringArray(values) ->
            let values =
                [| for value in values do
                       let value = defaultArg value String.Empty
                       if String.IsNullOrWhiteSpace value then () else value |]

            System.String.Join(",", values)
        | String None -> String.Empty

    member this.Values =
        match this with
        | String value ->
            let r = ResizeArray()
            r.Add(value)
            r
        | StringArray value -> value


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

    member _.SearchParams: Map<string, SearchParamValue> =
        match patternResult with
        | Some result ->

            let di = System.Collections.Generic.Dictionary<string, SearchParamValue>()

            for param in (result.search.groups["0"] :?> string).Split("&") do
                let split = param.Split("=")
                let key = split[0]
                let hasValue = split.Length = 2
                let hasMultiple = split.Length > 2


                match di.TryGetValue(key) with
                | true, (StringArray values) ->
                    if hasMultiple then
                        let mapped =
                            split[1..]
                            |> Array.map (fun value -> if String.IsNullOrWhiteSpace value then None else Some value)

                        values.AddRange(mapped)
                    else if hasValue && String.IsNullOrWhiteSpace split[1] then
                        values.Add None
                    else
                        values.Add(Some split[1])
                | true, (String value) ->
                    if hasMultiple then
                        let mapped =
                            split[1..]
                            |> Array.map (fun value -> if String.IsNullOrWhiteSpace value then None else Some value)

                        let mapped = ResizeArray(mapped)
                        mapped.Add(value)
                        di[key] <- StringArray mapped
                    else if hasValue && String.IsNullOrWhiteSpace split[1] then
                        let mapped = ResizeArray([| value; None |])
                        di[key] <- StringArray mapped
                    else
                        di[key] <- StringArray(ResizeArray([| value; Some split[1] |]))

                | false, _ ->
                    if hasMultiple then
                        let mapped =
                            split[1..]
                            |> Array.map (fun value -> if String.IsNullOrWhiteSpace value then None else Some value)

                        let mapped = ResizeArray(mapped)
                        di.Add(key, StringArray mapped)
                    else if hasValue && String.IsNullOrWhiteSpace split[1] then
                        di.Add(key, String None)
                    else
                        di.Add(key, String(Some split[1]))

            di |> Seq.map (fun kv -> kv.Key, kv.Value) |> Map.ofSeq
        | None -> Map.empty

    member _.PathParams(index: string) : string option =
        match patternResult with
        | Some result -> result.pathname.groups.Item index :?> string |> Option.ofObj
        | None -> None

    member _.HashParams(index: string) : string option =
        match patternResult with
        | Some result -> result.hash.groups.Item index :?> string |> Option.ofObj
        | None -> None

    member this.AnyParams(index: string) : string option =
        this.PathParams index
        |> Option.orElseWith (fun _ ->
            this.SearchParams
            |> Map.tryFind index
            |> Option.map (fun value -> value.AsString))
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
