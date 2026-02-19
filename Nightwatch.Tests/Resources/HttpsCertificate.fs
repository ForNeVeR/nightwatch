// SPDX-FileCopyrightText: 2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module Nightwatch.Tests.Resources.HttpsCertificate

open System
open System.Threading.Tasks

open Xunit

open Nightwatch.Resources
open Nightwatch.Resources.HttpsCertificate

let private validCertExpiring (daysFromNow: int) : CertificateChecker = {
    Check = fun _ -> Task.FromResult(Valid(DateTimeOffset.UtcNow.AddDays(float daysFromNow)))
}

let private invalidCert : CertificateChecker = {
    Check = fun _ -> Task.FromResult(Invalid "Certificate validation failed")
}

let private test (checker: CertificateChecker) (validIn: string option) =
    let param =
        match validIn with
        | Some v -> [| "url", "https://example.org"; "valid-in", v |] |> Map.ofArray
        | None -> [| "url", "https://example.org" |] |> Map.ofArray
    let factory = HttpsCertificate.factory checker
    let check = factory.create.Invoke param
    check.Invoke()

[<Fact>]
let ``HttpsCertificate returns true for valid certificate without valid-in check``(): Task =
    task {
        let! result = test (validCertExpiring 30) None
        Assert.True result.IsOk
    }

[<Fact>]
let ``HttpsCertificate returns true when certificate expires after valid-in period``(): Task =
    task {
        // Certificate expires in 30 days, we require it to be valid for 7 days
        let! result = test (validCertExpiring 30) (Some "P7D")
        Assert.True result.IsOk
    }

[<Fact>]
let ``HttpsCertificate returns false when certificate expires within valid-in period``(): Task =
    task {
        // Certificate expires in 2 days, we require it to be valid for 7 days
        let! result = test (validCertExpiring 2) (Some "P7D")
        let errorMessage =
            match result with
            | Ok() -> failwith "Not expected"
            | Error message -> message
        Assert.StartsWith("Certificate Terminates Too Soon: ", errorMessage)
    }

[<Fact>]
let ``HttpsCertificate returns false for invalid certificate``(): Task =
    task {
        let! result = test invalidCert None
        Assert.Equivalent(Error "Certificate validation failed", result)
    }
