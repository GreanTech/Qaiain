#r @"Src/packages/FAKE/tools/FakeLib.dll"

open Fake
open System.IO

let ``Qaiain.CommandLine project path`` = "Src/Qaiain.CommandLine"
let ``Qaiain.UnitTests project path``   = "Src/Qaiain.UnitTests"

let (+/) path1 path2 = Path.Combine(path1, path2)

Target "Clean" (fun _ ->
    CleanDirs [
      ``Qaiain.CommandLine project path`` +/ "bin"
      ``Qaiain.CommandLine project path`` +/ "obj"
      ``Qaiain.UnitTests project path``   +/ "bin"
      ``Qaiain.UnitTests project path``   +/ "obj" ])

RunTargetOrDefault "Clean"
