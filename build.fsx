#!/usr/bin/env -S dotnet fsi

#r "nuget: Fake.DotNet.Cli, 5.22.0"
#r "nuget: Fake.IO.FileSystem, 5.22.0"
#r "nuget: Fake.Core.Target, 5.22.0"
#r "nuget: Fake.DotNet.MsBuild, 5.22.0"
#r "nuget: MSBuild.StructuredLogger, 2.1.630"
#r "nuget: System.Reactive, 5.0.0"

open System
open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators

// https://github.com/fsharp/FAKE/issues/2517#issuecomment-727282959
Environment.GetCommandLineArgs()
|> Array.skip 2 // skip fsi.exe; build.fsx
|> Array.toList
|> Context.FakeExecutionContext.Create false "build.fsx"
|> Context.RuntimeContext.Fake
|> Context.setExecutionContext

let output = "./nugets"

let projects =
    [| "Bix"
       "Bix.Bun"
       "Bix.Deno"
       "Bix.Cloudflare" |]

Target.initEnvironment ()
Target.create "Clean" (fun _ -> !! "nugets" |> Shell.cleanDirs)

Target.create "PackNugets" (fun _ ->
    projects
    |> Array.iter (fun project ->
        DotNet.pack
            (fun opts ->
                { opts with
                    Configuration = DotNet.BuildConfiguration.Release
                    OutputPath = Some $"./{output}/{project}" })

            $"src/{project}"))


Target.create "Default" (fun _ -> Target.runSimple "PackNugets" [] |> ignore)

"Clean"
==> "PackNugets"
==> "Default"

Target.runOrDefault "Default"
