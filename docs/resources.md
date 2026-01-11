<!--
SPDX-FileCopyrightText: 2017-2026 Friedrich von Never <friedrich@fornever.me>

SPDX-License-Identifier: MIT
-->

Resource Types
==============

Nightwatch monitors _resources_ — entities that can be checked periodically to verify they are functioning correctly. Each resource type has its own configuration parameters and check logic.

## Shell Resource

Executes a shell command and considers the check successful if the command exits with code 0.

```yaml
version: 0.0.1.0 # should always be 0.0.1.0 for the current version
id: test # task identifier
schedule: 00:05:00 # run every 5 minutes
type: shell
param:
    cmd: ping localhost # check command
notifications: # optional list of notification provider IDs to use
    - myNotifications/telegram
```

## HTTP Resource

Performs an HTTP request and considers the check successful if the response status code matches one of the expected codes.

```yaml
version: 0.0.1.0 # should always be 0.0.1.0 for the current version
id: test # task identifier
schedule: 00:05:00 # run every 5 minutes
type: http
param:
    url: http://localhost:8080/ # URL to visit
    ok-codes: 200, 304 # the list of the codes considered as a success
notifications: # optional list of notification provider IDs to use
    - myNotifications/telegram
```

## HTTPS Certificate Resource

Validates an HTTPS server's SSL/TLS certificate. The check performs full certificate chain validation (similar to what browsers do) and optionally verifies that the certificate doesn't expire within a specified time period.

```yaml
version: 0.0.1.0 # should always be 0.0.1.0 for the current version
id: test # task identifier
schedule: 01:00:00 # run every hour
type: https-certificate
param:
    url: https://example.com/
    valid-in: P3D # optional: certificate should be valid for at least 3 more days (ISO8601 duration format)
notifications: # optional list of notification provider IDs to use
    - myNotifications/telegram
```

### Certificate Validation

The resource performs the following checks:

1. **Certificate Chain Validation** — Verifies the entire certificate chain up to a trusted root CA
2. **Hostname Verification** — Ensures the certificate is valid for the specified hostname
3. **Expiration Check** — Verifies the certificate is not expired
4. **Expiration Threshold** (optional) — If `valid-in` is specified, verifies the certificate expires after the threshold date

This mirrors browser behavior, ensuring that if Nightwatch reports a certificate as valid, users will also see it as valid in Chrome, Firefox, Safari, and other browsers.
