#r "nuget: Fake.DotNet.Cli, 5.22.0"
#r "nuget: Fake.IO.FileSystem, 5.22.0"
#r "nuget: Fake.Core.Target, 5.22.0"
#r "nuget: Fake.DotNet.MsBuild, 5.22.0"
#r "nuget: MSBuild.StructuredLogger, 2.1.630"
#r "nuget: System.IO.Compression.ZipFile, 4.3.0"
#r "nuget: System.Reactive, 5.0.0"

open System
open System.IO.Compression
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
       "Bix.Deno" |]

let fsharpSourceFiles =
    !! "src/**/*.fs"
    ++ "src/**/*.fsi"
    ++ "src/**/*.fsx"
    ++ "build.fsx"
    -- "**/obj/**/*.fs"
    -- "**/fable_modules/**/*.fs"

Target.initEnvironment ()
Target.create "Clean" (fun _ -> !! "nugets" |> Shell.cleanDirs)

Target.create "Format" (fun _ ->
    let result =
        fsharpSourceFiles
        |> Seq.map (sprintf "\"%s\"")
        |> String.concat " "
        |> DotNet.exec id "fantomas"

    if not result.OK then
        printfn $"Errors while formatting all files: %A{result.Messages}")

Target.create "CheckFormat" (fun _ ->
    let result =
        fsharpSourceFiles
        |> Seq.map (sprintf "\"%s\"")
        |> String.concat " "
        |> sprintf "%s --check"
        |> DotNet.exec id "fantomas"

    if result.ExitCode = 0 then
        Trace.log "No files need formatting"
    elif result.ExitCode = 99 then
        failwith "Some files need formatting, check output for more info"
    else
        Trace.logf $"Errors while formatting: %A{result.Errors}")

Target.create "PackNugets" (fun _ ->
    projects
    |> Array.iter (fun project ->
        DotNet.pack
            (fun opts ->
                { opts with
                    Configuration = DotNet.BuildConfiguration.Release
                    OutputPath = Some $"./{output}/{project}" })

            $"src/{project}"))

Target.create "Zip" (fun _ ->
    projects
    |> Array.Parallel.iter (fun project -> ZipFile.CreateFromDirectory($"{output}/{project}", $"{output}/{project}.zip")))

Target.create "Default" (fun _ -> Target.runSimple "Zip" [] |> ignore)

"Clean"
==> "CheckFormat"
==> "PackNugets"
==> "Default"

Target.runOrDefault "Default"
