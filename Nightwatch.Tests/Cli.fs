module Nightwatch.Tests.Cli

open System
open System.IO
open System.Text
open System.Threading.Tasks

open Xunit
open Argu

open Nightwatch.Program

[<Fact>]
let ``Config path should be parsed correctly`` () =
    let parser = ArgumentParser.Create<CliArguments>()
    let testPath = "test/dir/"

    let expected = [ CliArguments.Arguments testPath ]
    let parseResult = parser.Parse [| testPath |]
    let result = parseResult.GetAllResults()

    Assert.Equal<CliArguments list>(expected, result)

[<Fact>]
let ``Version option should be parsed correclty`` () =
    let parse = ArgumentParser.Create<CliArguments>()

    let expected = [ CliArguments.Version ]
    let parseResult = parse.Parse [| "--version" |]
    let result = parseResult.GetAllResults()

    Assert.Equal<CliArguments list>(expected, result)
