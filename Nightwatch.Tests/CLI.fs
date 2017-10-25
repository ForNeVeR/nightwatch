module Nightwatch.Tests.CLI

open System
open System.IO
open System.Text
open System.Threading.Tasks

open Xunit
open Argu

open Nightwatch.Program

[<Fact>]
let ``Config path should be parsed correctly`` () =
    let parser = ArgumentParser.Create<CLIArguments>()
    let testPath = "test/dir/"

    let expected = [ CLIArguments.Arguments testPath ]
    let parseResult = parser.Parse [| testPath |]
    let result = parseResult.GetAllResults()

    Assert.Equal<CLIArguments list>(expected, result)

[<Fact>]
let ``Version option should be parsed correclty`` () =
    let parse = ArgumentParser.Create<CLIArguments>()

    let expected = [ CLIArguments.Version ]
    let parseResult = parse.Parse [| "--version" |]
    let result = parseResult.GetAllResults()

    Assert.Equal<CLIArguments list>(expected, result)
