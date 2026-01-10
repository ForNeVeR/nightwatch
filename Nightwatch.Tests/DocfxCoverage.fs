// SPDX-FileCopyrightText: 2026 Nightwatch contributors <https://github.com/ForNeVeR/nightwatch>
//
// SPDX-License-Identifier: MIT

module Nightwatch.Tests.DocfxCoverage

open System
open System.IO
open System.Text.Json
open System.Xml.Linq
open Xunit

type private Marker = class end

let private findRepoRoot() =
    let rec findUp (dir: DirectoryInfo) =
        if File.Exists(Path.Combine(dir.FullName, "docs", "docfx.json")) then
            dir.FullName
        elif dir.Parent <> null then
            findUp dir.Parent
        else
            failwith "Could not find repository root (no docs/docfx.json found)"

    let assemblyLocation = typeof<Marker>.Assembly.Location
    let startDir = DirectoryInfo(Path.GetDirectoryName(assemblyLocation: string))
    findUp startDir

let private getPackableLibraries (repoRoot: string) =
    let fsprojFiles = Directory.GetFiles(repoRoot, "*.fsproj", SearchOption.AllDirectories)
    Assert.NotEmpty fsprojFiles

    fsprojFiles
    |> Array.choose (fun fsprojPath ->
        let doc = XDocument.Load(fsprojPath)
        let ns = doc.Root.Name.Namespace

        let getPropertyValue name =
            doc.Descendants(ns + "PropertyGroup")
            |> Seq.collect _.Elements(ns + name)
            |> Seq.tryHead
            |> Option.map _.Value.Trim().ToLowerInvariant()

        let isPackable = getPropertyValue "IsPackable" = Some "true"
        let isPackAsTool = getPropertyValue "PackAsTool" = Some "true"

        if isPackable && not isPackAsTool then
            let projectName = Path.GetFileNameWithoutExtension(fsprojPath)
            Some projectName
        else
            None)
    |> Set.ofArray

let private getDocfxDocumentedProjects (repoRoot: string) =
    let docfxPath = Path.Combine(repoRoot, "docs", "docfx.json")
    let json = File.ReadAllText(docfxPath)
    let doc = JsonDocument.Parse(json)

    doc.RootElement
        .GetProperty("metadata")
        .EnumerateArray()
    |> Seq.collect (fun metadata ->
        metadata.GetProperty("src").EnumerateArray()
        |> Seq.collect (fun src ->
            src.GetProperty("files").EnumerateArray()
            |> Seq.map (fun file -> file.GetString())))
    |> Seq.choose (fun filePath ->
        // Pattern: "ProjectName/bin/Release/*/ProjectName.dll"
        let parts = filePath.Split('/')
        if parts.Length >= 1 then
            Some parts[0]
        else
            None)
    |> Set.ofSeq

[<Fact>]
let ``All packable libraries should be documented in docfx.json`` () =
    let repoRoot = findRepoRoot()
    let packableLibraries = getPackableLibraries repoRoot
    Assert.NotEmpty packableLibraries
    let documentedProjects = getDocfxDocumentedProjects repoRoot

    let undocumented = Set.difference packableLibraries documentedProjects
    let remainder = Set.difference documentedProjects packableLibraries

    if not (Set.isEmpty undocumented) then
        let message =
            sprintf "The following packable libraries are not documented in docfx.json:\n%s\n\nAdd them to docs/docfx.json metadata.src.files"
                (String.Join("\n", undocumented |> Seq.map (sprintf "  - %s")))
        Assert.Fail(message)

    Assert.Empty remainder
