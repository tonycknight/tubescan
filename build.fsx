#r "paket:
nuget Fake.IO.FileSystem
nuget Fake.DotNet.Cli
nuget Fake.DotNet.MSBuild
nuget Fake.BuildServer.GitHubActions
nuget Fake.Core.Target //"
#if !FAKE
  #load "./.fake/fakebuild.fsx/intellisense.fsx"
#endif

open Fake.Core
open Fake.DotNet
open Fake.IO
open Fake.IO.FileSystemOperators
open Fake.IO.Globbing.Operators
open Fake.Core.TargetOperators
open Fake.SystemHelper

let packageDir = "./package"
let publishDir = "./publish"
let strykerDir = "./StrykerOutput"
let mainSolution = "./TubeScan.sln"


let ghVersionNumber = (match Fake.BuildServer.GitHubActions.Environment.CI false with
                        | true -> Fake.Core.Environment.environVar "build-version-number" |> Some
                        | _ -> None )

let commitSha = Fake.BuildServer.GitHubActions.Environment.Sha

let version =  (match ghVersionNumber with
                | Some vsn -> vsn
                | None -> "0.0.0"
                )

let infoVersion = match commitSha with
                    | null -> sprintf "%s alpha" version
                    | sha -> sprintf "%s.%s alpha" version sha                    

let assemblyInfoParams (buildParams)=
    [ ("Version", version); ("AssemblyInformationalVersion", infoVersion) ] |> List.append buildParams

let codeCoverageParams (buildParams)=
    [   ("CollectCoverage", "true"); 
        ("CoverletOutput", "./TestResults/coverage.info"); 
        ("CoverletOutputFormat", "cobertura");        
    ]  |> List.append buildParams

let packBuildParams (buildParams) =
    [ ("PackageVersion", version); ] |> List.append buildParams

let buildOptions = fun (opts: DotNet.BuildOptions) -> 
                                { opts with
                                    Configuration = DotNet.BuildConfiguration.Release;
                                    MSBuildParams = { opts.MSBuildParams with Properties = assemblyInfoParams opts.MSBuildParams.Properties; WarnAsError = Some [ "*" ]; }
                                    }
let restoreOptions = fun (opts: DotNet.RestoreOptions) -> opts

let testOptions (opts: DotNet.TestOptions)=
    let properties = codeCoverageParams opts.MSBuildParams.Properties

    { opts with NoBuild = false; 
                Configuration = DotNet.BuildConfiguration.Debug;  // Temporary, to ensure Coverlet can find otherwise optimised-out code
                Logger = Some "trx;LogFileName=test_results.trx";
                Filter = Some "OS!=Windows";
                MSBuildParams = { opts.MSBuildParams with Properties = properties } }

let packOptions = fun (opts: DotNet.PackOptions) -> 
                                { opts with 
                                    Configuration = DotNet.BuildConfiguration.Release; 
                                    NoBuild = false; 
                                    MSBuildParams = { opts.MSBuildParams with Properties = (packBuildParams opts.MSBuildParams.Properties |> assemblyInfoParams )};
                                    OutputPath = Some packageDir }

let publishByRuntimeOptions = fun(runtime: string) (opts: DotNet.PublishOptions) -> 
    let props = ("AssemblyFileVersion", version)
                    :: opts.MSBuildParams.Properties

    { opts with Configuration = DotNet.BuildConfiguration.Release;
                MSBuildParams = { opts.MSBuildParams with       
                                    DisableInternalBinLog = true

                                    Properties = props} 
                SelfContained = Some true
                Runtime =       Some runtime}

let publishProjects = !! "src/**/TubeScan.csproj" |> List.ofSeq

let publishAndCopy runtime =
    publishProjects 
        |> Seq.iter (fun p -> p |> DotNet.publish (publishByRuntimeOptions runtime) )

    publishProjects
        |> Seq.iter (fun p ->   let subdir = sprintf "bin/Release/net6.0/%s/publish" runtime
                                let dir = Path.getDirectory p
                                let name = System.IO.Path.GetFileNameWithoutExtension(p)

                                let sourceDir = System.IO.Path.Combine(dir, subdir)
                                let targetDir = sprintf @"./%s/%s/%s" publishDir runtime (name.ToLower())

                                name |> sprintf "name: %s" |> Fake.Core.Trace.log
                                sourceDir |> sprintf "sourceDir: %s" |> Fake.Core.Trace.log
                                targetDir |> sprintf "targetDir: %s" |> Fake.Core.Trace.log

                                Shell.copyDir targetDir sourceDir (fun _ -> true) 
                    )

Target.create "Clean" (fun _ ->
    !! "src/**/bin"
    ++ "src/**/obj"
    |> Shell.cleanDirs
    
    !! "tests/**/bin"
    ++ "tests/**/obj"
    ++ "tests/**/TestResults"
    ++ packageDir
    ++ publishDir
    |> Shell.cleanDirs

    !! strykerDir
    |> Shell.cleanDirs
)

Target.create "Restore" (fun _ ->
    !! mainSolution
    |> Seq.iter (DotNet.restore restoreOptions)
)

Target.create "Build" (fun _ ->
    !! mainSolution
    |> Seq.iter (DotNet.build buildOptions)
)

Target.create "Pack" (fun _ -> publishProjects |> Seq.iter (DotNet.pack packOptions ) )

Target.create "Unit Tests" (fun _ ->
    !! "tests/**/*.csproj"
    |> Seq.iter (DotNet.test testOptions)    
)

Target.create "Run Stryker" (fun _ ->
    !! "tests/**/*.csproj"
    |> Seq.iter (fun p ->   let args = sprintf "-tp %s -b 60" p
                            let result = DotNet.exec id "dotnet-stryker" args
                            if not result.OK then failwithf "Stryker failed!"
                            )
)

Target.create "Consolidate code coverage" (fun _ ->  
    let args = sprintf @"-reports:""./tests/**/coverage.info"" -targetdir:""./%s/codecoverage"" -reporttypes:""Html""" publishDir
    let result = DotNet.exec id "reportgenerator" args
  
    if not result.OK then failwithf "reportgenerator failed!"  
)

Target.create "PublishRuntime-ubuntu-x64" (fun _ -> publishAndCopy "ubuntu-x64")

Target.create "All" ignore

"Clean"
  ==> "Restore"
  ==> "Build"
  ==> "Unit Tests"
  ==> "Consolidate code coverage"
  ==> "Run Stryker"
  ==> "All"

Target.runOrDefault "All"
