<!--
SPDX-FileCopyrightText: 2017-2026 Friedrich von Never <friedrich@fornever.me>

SPDX-License-Identifier: MIT
-->

Nightwatch [![Status Umbra][status-umbra]][andivionian-status-classifier]
==========

Nightwatch is a monitoring service intedned to monitor daily and nightly
activities and notify the administrator if something wrong happens.

Configure
---------

Nightwatch could be configured by placing the `nightwatch.yml` file in the
current directory. You may also override the configuration file path by using
the command-line arguments, see the Run section of this document.

`nightwatch.yml` has the following form:

```yaml
resource-directory: "some/path"
notification-directory: "notifications/path"
```

Nightwatch searches the resource directory for the configuration files. At
start, it will recursively read all the `*.yml` files in the resource
directory, and set them up as periodic tasks. Each configuration file describes
a _Resource_.

Similarly, Nightwatch reads all `*.yml` files from the notification directory to configure _Notification_ providers. When a resource check fails or recovers, Nightwatch sends notifications via all configured providers.

Currently supported resources are documented below.

### Shell Resource

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

### HTTP Resource

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

Notifications
-------------

Notification providers are configured in the notification directory. Each `*.yml` file describes a notification provider that will be used to send alerts when resource checks fail or recover.

Currently supported notification providers are documented below.

### Telegram Notification

```yaml
version: 0.0.1.0 # should always be 0.0.1.0 for the current version
id: myNotifications/telegram # notification provider identifier
type: telegram
param:
    bot-token: YOUR_BOT_TOKEN_HERE # Telegram bot API token
    chat-id: YOUR_CHAT_ID_HERE # target chat ID for notifications
```

Run
---

In developer mode:

```console
$ dotnet run --project Nightwatch
```

To stop the program, press `Ctrl-C`.

Add `--config ./some/path.yml` option to set the configuration file path
(`nightwatch.yml` in the current directory is the default).

Add `--service` to run in a Windows service mode.

To install the service on Windows, execute the following commands in your shell:

```pwsh
$ sc.exe Nightwatch binpath= "D:\Path\To\Nightwatch.exe --config D:\Path\To\nightwatch.yml --service" start= auto
$ sc.exe start Nightwatch
```

_(note the space and quote placement, that's important)_

Test
----

```console
$ dotnet test Nightwatch.Tests
```

Documentation
-------------
- [Implementing a Resource type][implementing-a-resource-type]
- [Contributor Guide][docs.contributing]

License
-------
The project is distributed under the terms of [the MIT license][docs.license].

The license indication in the project's sources is compliant with the [REUSE specification v3.3][reuse.spec].

[andivionian-status-classifier]: https://github.com/ForNeVeR/andivionian-status-classifier#status-umbra-
[docs.contributing]: CONTRIBUTING.md
[docs.license]: LICENSE.txt
[implementing-a-resource-type]: docs/implementing-a-resource-type.md
[net-core-sdk]: https://www.microsoft.com/net/download/core#/sdk
[reuse.spec]: https://reuse.software/spec-3.3/
[status-umbra]: https://img.shields.io/badge/status-umbra-red.svg
