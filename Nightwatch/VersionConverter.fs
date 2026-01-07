// SPDX-FileCopyrightText: 2017-2026 Nightwatch contributors <https://github.com/ForNeVeR/nightwatch>
//
// SPDX-License-Identifier: MIT

namespace Nightwatch

open System
open YamlDotNet.Serialization

type VersionConverter() =
    interface IYamlTypeConverter with
        member _.Accepts(t) = t = typeof<Version>
        member _.ReadYaml(parser, _, _): obj =
             let scalar = parser.Current :?> YamlDotNet.Core.Events.Scalar
             ignore <| parser.MoveNext()
             let version = scalar.Value
             box <| Version version
        member _.WriteYaml(_, _, _, _) = failwithf "Not supported"
