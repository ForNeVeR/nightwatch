<!--
SPDX-FileCopyrightText: 2017-2026 Friedrich von Never <friedrich@fornever.me>

SPDX-License-Identifier: MIT
-->

Contributor Guide
=================

Prerequisites
-------------
To work with the project, you'll need [.NET SDK 10][dotnet-sdk] or later.

Build
-----
Use the following shell command:
```console
$ dotnet build
```

If you need a standalone executable (useful for service deployment), then add the following options:
```console
$ cd Nightwatch
$ dotnet build --configuration Release --runtime win-x64 --output out
```

Run
---
Use the following shell command:

```console
$ dotnet run --project Nightwatch.Tool
```

Test
----
Use the following shell command:

```console
$ dotnet test
```

Local Packaging
---------------
To prepare the NuGet packages locally:

1. Pack the packages:
   ```console
   $ dotnet pack --configuration Release
   ```

2. Install the tool locally:
   ```console
   $ dotnet tool install --global --add-source ./Nightwatch.Tool/bin/Release FVNever.Nightwatch.Tool
   ```

3. Verify installation:
   ```console
   $ nightwatch --help
   ```

4. To uninstall:
   ```console
   $ dotnet tool uninstall --global FVNever.Nightwatch.Tool
   ```

License Automation
------------------
If the CI asks you to update the file licenses, follow one of these:
1. Update the headers manually (look at the existing files), something like this:
   ```fsharp
   // SPDX-FileCopyrightText: %year% %your name% <%your contact info, e.g. email%>
   //
   // SPDX-License-Identifier: MIT
   ```
   (accommodate to the file's comment style if required).
2. Alternately, use the [REUSE][reuse] tool:
   ```console
   $ reuse annotate --license MIT --copyright '%your name% <%your contact info, e.g. email%>' %file names to annotate%
   ```

(Feel free to attribute the changes to "Nightwatch contributors <https://github.com/ForNeVeR/nightwatch>" instead of your name in a multi-author file, or if you don't want your name to be mentioned in the project's source: this doesn't mean you'll lose the copyright.)

File Encoding Changes
---------------------
If the automation asks you to update the file encoding (line endings or UTF-8 BOM) in certain files, run the following PowerShell script ([PowerShell Core][powershell] is recommended to run this script):
```console
$ pwsh -c "Install-Module VerifyEncoding -Repository PSGallery -RequiredVersion 2.2.1 -Force && Test-Encoding -AutoFix"
```

The `-AutoFix` switch will automatically fix the encoding issues, and you'll only need to commit and push the changes.

Documentation
-------------
The project uses [docfx][docfx] to generate API documentation. To build and preview the documentation locally:

1. Restore the docfx tool:
   ```console
   $ dotnet tool restore
   ```

2. Build the project in Release mode (required for API docs):
   ```console
   $ dotnet build -c Release
   ```

3. Start the documentation server:
   ```console
   $ dotnet docfx docs/docfx.json --serve
   ```

4. Open http://localhost:8080 in your browser to preview the documentation.

GitHub Actions
--------------
If you want to update the GitHub Actions used in the project, edit the file that generated them: `scripts/github-actions.fsx`.

Then run the following shell command:
```console
$ dotnet fsi scripts/github-actions.fsx
```

[docfx]: https://dotnet.github.io/docfx/
[dotnet-sdk]: https://dotnet.microsoft.com/en-us/download
[powershell]: https://learn.microsoft.com/en-us/powershell/scripting/install/installing-powershell
[reuse]: https://reuse.software/
