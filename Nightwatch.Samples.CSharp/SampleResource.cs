// SPDX-FileCopyrightText: 2017-2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.FSharp.Core;
using static Nightwatch.Core.Resources;

namespace Nightwatch.Samples.CSharp;

[PublicAPI]
public class SampleResource
{
    public static ResourceFactory Factory { get; } = new("sample", CreateChecker);

    private static Func<Task<FSharpResult<Unit, string>>> CreateChecker(IDictionary<string, string> param) =>
        () => Task.FromResult(param["a"] == param["b"]
            ? FSharpResult<Unit, string>.NewOk(null)
            : FSharpResult<Unit, string>.NewError("Invalid state"));
}
