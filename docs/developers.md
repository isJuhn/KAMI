---
layout: article
title: Developers
description: Build KAMI on Windows, understand the source layout, and contribute game support or documentation.
eyebrow: Build and contribute
permalink: /developers/
---

KAMI is a C# application with a WPF interface and a native [PINE](https://github.com/GovanifY/pine) dependency for emulator communication. Development happens in the [GitHub repository]({{ site.links.source }}).

## Build on Windows

You need Git, the **.NET 10 SDK**, Visual Studio C++ build tools, **Meson**, and **Ninja**. Use the **x64 Native Tools Command Prompt for VS 2026**. The SDK is needed for development; the runtime alone cannot build the project.

Clone the repository with its PINE submodule:

```console
git clone --recurse-submodules https://github.com/isJuhn/KAMI.git
cd KAMI
```

For an existing checkout, initialize or update the submodule:

```console
git submodule update --init --recursive
```

From the repository root in the x64 Native Tools command prompt, run:

```console
build_deps.bat
build_publish.bat
```

The first script builds PINE's C bindings and copies `pine_c.dll` into `KAMI.Core`. The second publishes the Windows application to **`bin/publish/`**. Keep the published files together when running or distributing a build.

To debug in Visual Studio, build the native dependency first, open **`KAMI.sln`**, and select the **KAMI.Windows** startup project with the **x64** configuration.

The repository also contains `KAMI.Linux` and shell scripts, but the supported download and active CI target Windows. Their presence does not imply a supported Linux release.

## Find your way around

<dl class="source-map">
  <dt><code>KAMI.Core/Games/</code></dt>
  <dd>Game-specific addresses, camera handling, and injection logic.</dd>
  <dt><code>KAMI.Core/GameManager.cs</code></dt>
  <dd>Maps supported title IDs to game handlers.</dd>
  <dt><code>KAMI.Core/Cameras/</code></dt>
  <dd>Shared camera representations.</dd>
  <dt><code>KAMI.Core/PineIPC.cs</code></dt>
  <dd>Native PINE interop.</dd>
  <dt><code>KAMI.Core/KAMICore.cs</code></dt>
  <dd>Emulator connection, settings, and the update loop.</dd>
  <dt><code>KAMI.Windows/</code></dt>
  <dd>WPF application and Windows interface.</dd>
  <dt><code>docs/</code></dt>
  <dd>This Jekyll website and its compatibility content.</dd>
</dl>

## Contribute a change

Start with a focused issue or pull request. Describe the behavior you want to change, keep unrelated changes separate, and explain how you checked the result. For a larger feature, an issue is a useful place to discuss the approach first.

For new game support, use an existing handler as a reference, implement the [`IGame` interface](https://github.com/isJuhn/KAMI/blob/master/KAMI.Core/Games/IGame.cs) or a suitable base class, and register the exact supported title IDs in [`GameManager`](https://github.com/isJuhn/KAMI/blob/master/KAMI.Core/GameManager.cs). Document version and game-mode limits on the [compatibility page]({{ '/compatibility/' | relative_url }}).

Include the emulator version, game title ID and version, tested modes or levels, and remaining limitations in your pull request. Test injection on and off, sensitivity changes, and relevant level or camera transitions. A handler being present is not the same as every release of that game being verified.

For interface changes, check keyboard operation and Windows display scaling. For website changes, check phone widths, both color themes, keyboard navigation, and operation with JavaScript disabled. Local website preview and publishing instructions are in [`docs/README.md`](https://github.com/isJuhn/KAMI/blob/master/docs/README.md).

Remove personal paths and unrelated local files before submitting logs, screenshots, or a patch. Contributions are distributed under the project's [MIT license]({{ '/about/#license' | relative_url }}).

## Build references

The [build scripts](https://github.com/isJuhn/KAMI/blob/master/build_publish.bat), [dependency script](https://github.com/isJuhn/KAMI/blob/master/build_deps.bat), and [CI workflow](https://github.com/isJuhn/KAMI/blob/master/.github/workflows/main.yml) describe the current build. Follow those files if they change ahead of these instructions.
