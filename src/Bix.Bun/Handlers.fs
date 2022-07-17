module Bix.Bun.Handlers

open Fable.Bun
open Bix.Types

let sendHtmlFile (path: string) : HttpHandler =
    fun next ctx ->
        ctx.SetStarted true
        let content = Bun.file (path, unbox {| ``type`` = "text/html" |}), "text/html"
        let content = Blob content |> Some
        Promise.lift content
