[bun.sh]: https://bun.sh
[deno]: https://deno.land
[giraffe]: https://giraffe.wiki
[saturn]: https://github.com/SaturnFramework/Saturn
[fable]: https://fable.io
[fable.bun]: https://github.com/AngelMunoz/fable-bun
[fable.deno]: https://github.com/AngelMunoz/fable-deno
[Cloudflare Workers]: https://developers.cloudflare.com/workers/

## Bix

An F# microframework that provides a router and http handler abstractions for web frameworks that work with a Request -> Response model.

Examples of runtimes that work with this model:

- [Bun.sh] -> [Fable.Bun] + Bix.Bun
- [Deno] -> [Fable.Deno] + Bix.Deno
- Service Workers -> (Adapter and Investigation in progress)
  - [Cloudflare Workers] -> (Under Investigation)


> the "**_Bix_** name is just a _codename_ for now (until I decide it's good to go).

This microframework is heavily inspired by [Giraffe], and [Saturn] frameworks from F# land so if you have ever used that server model then Bix will feel fairly similar, I plan to add a saturn like router eventually

## Bix.Bun

Bix.Bun exposes a [bun.sh] specific request handler, and other http handlers that may contain Bun specific APIs like `Bun.file` to read files

Check the sample at `samples/Bix.Bun.Sample/Program.fs`

## Bix.Deno

Bix.Deno exposes a [deno] specific request handler, and other http handlers that may contain Bun specific APIs like `Deno.open` to read files

Check the sample at `samples/Bix.Deno.Sample/Program.fs`

```fsharp
// For more information see https://aka.ms/fsharp-console-apps
open Bix
open Bix.Types
open Bix.Handlers
open Bix.Router

open Bix.Deno

let checkCredentials: HttpHandler =
    fun next ctx ->
        let req: Request = ctx.Request
        let bearer = req.headers.get "Authorization" |> Option.ofObj
        // dummy handler
        match bearer with
        | None -> (setStatusCode (401) >=> sendText "Not Authorized") next ctx
        | Some token -> next ctx

let routes =
    Router.Empty
    |> Router.get ("/", fun next ctx -> sendText "Hello, World!" next ctx)
    |> Router.get ("/posts/:slug", fun next ctx ->
        promise { // promise based handlers are supported
            let slug = ctx.PathParams "slug"
            let! post = Database.find slug // database from somewhere
            let! html = Views.renderPost post // views from somewhere
            return! sendHtml html next ctx
        }
    )
    |> Router.get ("/json", fun next ctx ->
        let content = {| name = "Bix Server!"; Date = System.DateTime.Now |}
        sendJson content next ctx
    )
    |> Router.get ("/protected", (checkCredentials >=> (fun next ctx -> sendText "I'm protected!" next ctx)))


Server.Empty
|> Server.withRouter routes
|> Server.withDevelopment true
|> Server.withPort 5000
|> Server.run
|> Promise.start
```

> **Note**: When Working with deno, you need to have the following fields in your import map
>
> ```json
> {
>   "imports": {
>     "fable-deno-http": "https://deno.land/std/http/mod.ts"
>   }
> }
> ```

This is to ensure bare imports within `Bix.Browser.Types` and `Bix.Deno` work just fine.

# Development

This project is developed with VSCode in Linux/Windows/WSL but either rider, and visual studio should work just fine.

## Requirements

- .NET6 and above - https://get.dot.net
- Bun - [bun.sh] - (in case of running bun)
- Deno - [deno] - (in case of running deno)

## Try the sample

if you have bun installed along deno the simplest way to run the samples is

- `bun start`
- `bun start:deno`

both commands will restore the projects and run fable, bun/deno in watch mode

### Bun

After installing .NET + Bun just run

`bun start` on your terminal and it should just work

### Deno

After installing .NET + Deno just run

```sh
dotnet tool restore
dotnet fable watch samples/Bix.Deno.Sample -s && --run deno run -A --watch=./samples/Bix.Deno.Sample/ ./samples/Bix.Deno.Sample/Program.fs.js`
```
