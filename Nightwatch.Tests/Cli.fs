// SPDX-FileCopyrightText: 2017-2019 Nightwatch contributors <https://github.com/ForNeVeR/nightwatch>
//
// SPDX-License-Identifier: MIT

module Nightwatch.Tests.Cli

open Xunit
open Argu

open Nightwatch.Program

[<Fact>]
let ``Config path should be parsed correctly``() =
    let parser = ArgumentParser.Create<CliArguments>()
    let testPath = "test/dir/"

    let expected = [ CliArguments.Config testPath ]
    let parseResult = parser.Parse [| "--config"; testPath |]
    let result = parseResult.GetAllResults()

    Assert.Equal<CliArguments list>(expected, result)

[<Fact>]
let ``Version option should be parsed correclty``() =
    let parse = ArgumentParser.Create<CliArguments>()

    let expected = [ CliArguments.Version ]
    let parseResult = parse.Parse [| "--version" |]
    let result = parseResult.GetAllResults()

    Assert.Equal<CliArguments list>(expected, result)

[<Fact>]
let ``Service flag should be parsed correclty``() =
    let parse = ArgumentParser.Create<CliArguments>()

    let expected = [ CliArguments.Service ]
    let parseResult = parse.Parse [| "--service" |]
    let result = parseResult.GetAllResults()

    Assert.Equal<CliArguments list>(expected, result)
