module Bix.Handlers

open Fable.Core
open Fetch
open Browser.Types

open Bix.Types


let sendJson<'T> (value: 'T) : HttpHandler =
    fun next ctx ->
        ctx.SetStarted true
        Promise.lift (Some(Json value))

let encodeJson<'T> (value: 'T, encoder: 'T -> string) : HttpHandler =
    fun next ctx ->
        ctx.SetStarted true

        let jsonResult = JsonOptions(value, unbox encoder) |> Some
        Promise.lift jsonResult

let sendText (value: string) : HttpHandler =
    fun next ctx ->
        ctx.SetStarted true
        Promise.lift (Some(Text value))

let sendHtml (value: string) : HttpHandler =
    fun next ctx ->
        ctx.SetStarted true
        Promise.lift (Some(Html value))

let setContentType (contentType: string) : HttpHandler =
    fun next ctx ->
        let headers = ctx.Response.Headers
        headers.append ("content-type", contentType)

        let res =
            Response.create (
                "",
                unbox
                    {| headers = headers
                       status = ctx.Response.Status
                       statusText = ctx.Response.StatusText |}
            )

        ctx.SetResponse(res)
        next ctx

let setStatusCode (code: int) : HttpHandler =
    fun next ctx ->
        let headers = ctx.Response.Headers

        let res =
            Response.create (
                "",
                unbox
                    {| headers = headers
                       status = code
                       statusText = ctx.Response.StatusText |}
            )

        ctx.SetResponse(res)
        next ctx

let notFoundHandler: HttpHandler =
    fun next ctx -> (setStatusCode 404 >=> sendText "Not Found") next ctx

let cleanResponse: HttpHandler =
    fun next ctx ->
        ctx.SetStarted true
        ctx.SetResponse(Response.create ("", []))
        Promise.lift None

let tryBindJson<'T>
    (
        binder: obj -> Result<'T, exn>,
        success: 'T -> HttpHandler,
        error: exn -> HttpHandler
    ) : HttpHandler =
    fun next ctx ->
        ctx.Request.json ()
        |> Promise.bind (fun content ->
            let content =
                try
                    binder content |> Result.map id
                with ex ->
                    Result.Error ex

            match content with
            | Ok content -> (success content) next ctx
            | Result.Error err -> (error err) next ctx)

let tryDecodeJson<'T>
    (
        binder: string -> Result<'T, exn>,
        success: 'T -> HttpHandler,
        error: exn -> HttpHandler
    ) : HttpHandler =
    fun next ctx ->
        ctx.Request.text ()
        |> Promise.bind (fun content ->
            let content =
                try
                    binder content |> Result.map id
                with ex ->
                    Result.Error ex

            match content with
            | Ok content -> (success content) next ctx
            | Result.Error err -> (error err) next ctx)


[<RequireQualifiedAccess>]
module BixResponse =

    let NoValue (contentType: string, options: ResponseInitProperties list) =
        Response.create (
            "",
            Headers [| "content-type", contentType |]
            :: options
        )

    let OnText (text: string, options: ResponseInitProperties list) =
        Response.create (
            text,
            Headers [| "content-type", "text/plain" |]
            :: options
        )

    let OnHtml (html: string, options: ResponseInitProperties list) =
        Response.create (
            html,
            Headers [| "content-type", "text/html" |]
            :: options
        )

    let OnJson (json: obj, options: ResponseInitProperties list) =
        let content = JS.JSON.stringify (json)

        Response.create (
            content,
            Headers [| "content-type", "application/json" |]
            :: options
        )

    let OnJsonOptions (value: obj, encoder: obj -> string, options: ResponseInitProperties list) =

        Response.create (
            encoder value,
            Headers [| "content-type", "application/json" |]
            :: options
        )

    let OnBlob (blob: Blob, mimeType: string, options: ResponseInitProperties list) =
        Response.create (blob, Headers [| "content-type", mimeType |] :: options)

    let OnArrayBuffer (arrayBuffer: JS.ArrayBuffer, mimeType: string, options: ResponseInitProperties list) =
        Response.create (arrayBuffer, Headers [| "content-type", mimeType |] :: options)

    let OnArrayBufferView
        (
            arrayBufferView: JS.ArrayBufferView,
            mimeType: string,
            options: ResponseInitProperties list
        ) =
        Response.create (arrayBufferView, Headers [| "content-type", mimeType |] :: options)

    let OnCustom (content, contentType, args) =
        Response.create (
            // it might not be a string but
            // it is just to satisfy the F# compiler
            unbox<string> content,
            Headers [| "content-type", contentType |] :: args
        )
