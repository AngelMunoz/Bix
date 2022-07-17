[<AutoOpen>]
module Extensions

open Bix.Types

let compose (h1: HttpHandler) (h2: HttpHandler) : HttpHandler =
    fun final ->
        let fn = final |> h2 |> h1

        fun ctx ->
            match ctx.HasStarted with
            | true -> final ctx
            | false -> fn ctx

let inline (>=>) h1 h2 = compose h1 h2
