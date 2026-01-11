// SPDX-FileCopyrightText: 2026 Friedrich von Never <friedrich@fornever.me>
//
// SPDX-License-Identifier: MIT

module Nightwatch.Resources.HttpsCertificate

open System
open System.Collections.Generic
open System.Net.Security
open System.Net.Sockets
open System.Security.Authentication
open System.Security.Cryptography.X509Certificates
open System.Threading.Tasks
open System.Xml

open Nightwatch.Core.Resources

/// Result of a certificate check operation.
type CertificateCheckResult =
    | Valid of notAfter: DateTimeOffset
    | Invalid of reason: string

/// Abstraction for certificate checking, allowing for testability.
type CertificateChecker = {
    Check: Uri -> Task<CertificateCheckResult>
}

let private parseValidIn (validInStr: string) : TimeSpan =
    XmlConvert.ToTimeSpan validInStr

let private create (checker: CertificateChecker) (param: IDictionary<string, string>) =
    let url = param.["url"] |> Uri
    let validIn =
        match param.TryGetValue("valid-in") with
        | true, v -> Some(parseValidIn v)
        | false, _ -> None

    fun () -> task {
        let! result = checker.Check url
        match result with
        | Invalid _ -> return false
        | Valid notAfter ->
            match validIn with
            | Some duration ->
                let threshold = DateTimeOffset.UtcNow + duration
                return notAfter > threshold
            | None -> return true
    }

/// System implementation that performs real SSL certificate checks.
let system: CertificateChecker = {
    Check = fun uri -> task {
       let port = if uri.Port > 0 then uri.Port else 443
       use client = new TcpClient()
       try
           do! client.ConnectAsync(uri.Host, port)
           use stream = new SslStream(client.GetStream())
           do! stream.AuthenticateAsClientAsync(uri.Host)
           match stream.RemoteCertificate with
           | null -> return Invalid "No certificate received"
           | cert ->
               use x509 = new X509Certificate2(cert)
               return Valid(DateTimeOffset x509.NotAfter)
       with
       | :? AuthenticationException as ex ->
           return Invalid ex.Message
       | ex ->
           return Invalid ex.Message
   }
}

let factory (checker: CertificateChecker) : ResourceFactory =
    Factory.create "https-certificate" (create checker)
