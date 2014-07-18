#r @"Src/packages/FAKE.3.2.3/tools/FakeLib.dll"

open Fake

let (+/) path1 path2 = System.IO.Path.Combine(path1, path2)

let ``Qaiain.CommandLine project path`` = "Src" +/ "Qaiain.CommandLine"
let ``Qaiain.UnitTests project path``   = "Src" +/ "Qaiain.UnitTests"
let ``Qaiain solution path``            = "Src" +/ "Qaiain.sln"
let ``Qaiain xUnit.net unit tests``     = "Src/**/bin/Release/*.UnitTests.dll"

Target "Clean" (fun _ ->
    CleanDirs [
      ``Qaiain.CommandLine project path`` +/ "bin"
      ``Qaiain.CommandLine project path`` +/ "obj"
      ``Qaiain.UnitTests project path``   +/ "bin"
      ``Qaiain.UnitTests project path``   +/ "obj" ])

Target "Build" (fun _ ->
    !! (``Qaiain solution path``)
    |> MSBuildRelease "" "Rebuild"
    |> ignore)

Target "Tests" (fun _ ->
    !! ``Qaiain xUnit.net unit tests``
    |> xUnit (fun options -> options))

"Clean"
  ==> "Build"
  ==> "Tests"

RunTargetOrDefault "Tests"
