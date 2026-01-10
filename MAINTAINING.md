<!--
SPDX-FileCopyrightText: 2024-2026 Friedrich von Never <friedrich@fornever.me>

SPDX-License-Identifier: MIT
-->

Maintainer Guide
================

Publish a New Version
---------------------
1. Choose the new version according to [Semantic Versioning][semver].
2. Update the project's status in the `README.md` file, if required.
3. Update the copyright statement in the `LICENSE.txt` file, if required.
4. Update the `<Copyright>` statement in the `Directory.Build.props`, if required.
5. Update the `<Version>` in `Directory.Build.props`.
6. Prepare a corresponding entry in the `CHANGELOG.md` file (usually by renaming the "Unreleased" section).
7. Merge the aforementioned changes via a pull request.
8. Check if the NuGet key is still valid (see the **Rotate NuGet Publishing Key** section if it isn't).
9. Push a tag in form of `v<VERSION>`, e.g. `v1.0.0`. GitHub Actions will do the rest (create a release, push NuGet packages).

Rotate NuGet Publishing Key
---------------------------
CI relies on a NuGet API key being added to the secrets. From time to time, this key requires maintenance: it will become obsolete and will have to be updated.

To update the key:

1. Sign in onto nuget.org.
2. Go to the [API keys][nuget.api-keys] section.
3. Update the existing or create a new key named `ForNeVeR.Nightwatch.github` with a permission to **Push only new package versions** with a glob pattern of `Nightwatch*`.

   (If this is the first publication of a new package, upload a temporary short-living key with permission to add new packages and rotate it afterward.)
4. Paste the generated key to the `NUGET_TOKEN` variable on the [action secrets][github.secrets] section of GitHub settings.

GitHub Pages Documentation
--------------------------
Documentation is automatically published to GitHub Pages on every push to master.

To enable GitHub Pages for the first time:
1. Go to repository Settings > Pages
2. Under "Build and deployment", select "GitHub Actions" as the source

[github.secrets]: https://github.com/ForNeVeR/nightwatch/settings/secrets/actions
[nuget.api-keys]: https://www.nuget.org/account/apikeys
[semver]: https://semver.org/
