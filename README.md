[bun.sh]: https://bun.sh
[deno]: https://deno.land
[giraffe]: https://giraffe.wiki
[saturn]: https://github.com/SaturnFramework/Saturn
[fable]: https://fable.io
[fable.bun]: https://github.com/AngelMunoz/fable-bun
[fable.deno]: https://github.com/AngelMunoz/fable-deno
[cloudflare workers]: https://developers.cloudflare.com/workers/

# Bix

> the "**_Bix_**" name is just a _codename_ for now (until I decide it's good to go).

> ## Templates
>
> `dotnet new --install Bix.Templates::*`
>
> - `dotnet new bix.bun -o BunProject`
> - `dotnet new bix.cloudflare -o CloudFlareWorker`
> - `dotnet new bix.deno -o DenoProject`

An F# microframework that provides a router and http handler abstractions for web frameworks that work with a `Request -> Response` http server model.

Examples of runtimes that work with this model:

- [Bun.sh] -> [Fable.Bun] + Bix.Bun
- [Deno] -> [Fable.Deno] + Bix.Deno
- Service Workers
  - [Cloudflare Workers] -> Bix.Cloudflare
  - Browser Service Worker

This microframework is heavily inspired by [Giraffe], and [Saturn] frameworks from F# land so if you have ever used that server model then Bix will feel fairly similar.

An hypotetical example could be like the following code:

```fsharp
// define a function that takes HttpHandlers to satisfy existing handler constrains
let authenticateOrRedirect (authenticatedRoute: HttpHandler, notAuthenticatedRoute: HttpHandler) =
    Handlers.authenticateUser
        authenticatedRoute
        notAuthenticatedRoute

// compose different handlers for code reusability
// and granular control of handler execution
let checkAdminCredentials successRoute =
    authenticateOrRedirect (successRoute, Admin.Login)
    >=> Handlers.requiresAdmin

let checkUserCredentials successRoute =
    authenticateOrRedirect (successRoute, Views.Login)
    >=> Handlers.requiresUserOrAbove

// define routes for this application
let routes =
    // check the Cloud flare Worker sample/tempalte to see other router options
    // basic, giraffe, and saturn like
    Router.Empty
    |> Router.get("/", authenticateOrRedirect >=> Views.Landing)
    |> Router.get ("/login", authenticateOrRedirect >=> Views.Login)
    |> Router.get ("/me", checkUserCredentials(Views.Login))
    |> Router.get ("/portal", checkUserCredentials(Views.Portal))
    |> Router.get ("/admin", checkAdminCredentials(Admin.Portal))
    |> Router.post ("/users", checkAdminCredentials(Api.Users.Create >=> negotiateContent))
    |> Router.patch ("/users/:id", checkAdminCredentials(Api.Users.Update >=> negotiateContent))

// Start the web server
Server.Empty
|> Server.withPort 5000
|> Server.withDevelopment true
|> Server.withRouter routes
|> Server.run
```

The idea is to create simple and single purposed functions that work like middleware so you can organize and re-use

## Adapters

Bix currently has two adapters

- Bix.Deno
- Bix.Bun

Adapters under investigation:

- Bix.ServiceWorker
- Bix.CloudflareWorker

# Development

This project is developed with VSCode in Linux/Windows/WSL but either rider, and visual studio should work just fine.

## Requirements

- .NET6 and above - https://get.dot.net
- Bun - [bun.sh] - (in case of running bun)
- Deno - [deno] - (in case of running deno)

## Try the samples

Depending on what you want to try change the directory to your selected sample, example: `cd samples/Bix.Bun.Sample` and run one of the following commands

1. `dotnet tool restore` (run once per clone)
2. start the project
   - `bun start`
   - `deno task start`

both commands will restore the projects and run fable, bun/deno in watch mode.
