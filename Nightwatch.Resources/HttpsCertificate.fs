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

let private validateServerCertificate
    (_sender: obj)
    (certificate: X509Certificate)
    (_chain: X509Chain) // The chain parameter is provided by the callback signature but we build our own for custom validation
    (sslPolicyErrors: SslPolicyErrors)
    : bool =

    if isNull certificate then
        false
    else
        use cert2 = new X509Certificate2(certificate)
        use chain = new X509Chain()
        chain.ChainPolicy.RevocationMode <- X509RevocationMode.Online
        chain.ChainPolicy.RevocationFlag <- X509RevocationFlag.ExcludeRoot
        chain.ChainPolicy.VerificationFlags <- X509VerificationFlags.NoFlag
        chain.ChainPolicy.VerificationTime <- DateTime.UtcNow
        chain.ChainPolicy.UrlRetrievalTimeout <- TimeSpan.FromSeconds 30.0

        let isChainValid = chain.Build cert2
        isChainValid && sslPolicyErrors = SslPolicyErrors.None

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
       let port = if uri.Port <> -1 then uri.Port else 443
       use client = new TcpClient()
       try
           do! client.ConnectAsync(uri.Host, port)
           use stream = new SslStream(client.GetStream(), false, RemoteCertificateValidationCallback validateServerCertificate)
           do! stream.AuthenticateAsClientAsync(uri.Host)
           match stream.RemoteCertificate with
           | null -> return Invalid "No certificate received"
           | cert ->
               use x509 = new X509Certificate2(cert)
               return Valid(DateTimeOffset x509.NotAfter)
       with
       | :? AuthenticationException ->
           // TLS handshake or certificate validation failed.
           return Invalid "TLS authentication failed"
       | :? SocketException as ex ->
           // Network-level issues such as timeouts or connection refused.
           match ex.SocketErrorCode with
           | SocketError.TimedOut ->
               return Invalid "Connection timed out"
           | SocketError.ConnectionRefused ->
               return Invalid "Connection was refused by remote host"
           | _ ->
               return Invalid "Network error while connecting to remote host"
       | :? TimeoutException ->
           return Invalid "Connection timed out"
       | _ ->
           // Fallback for unexpected errors without exposing internal details.
           return Invalid "Unexpected error while checking certificate"
   }
}

let factory (checker: CertificateChecker) : ResourceFactory =
    Factory.create "https-certificate" (create checker)
