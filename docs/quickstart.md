---
layout: article
title: Quickstart
description: Set up KAMI with RPCS3 or PCSX2, bind a toggle key, and start using mouse input.
eyebrow: Getting started
permalink: /quickstart/
---

KAMI handles mouse input for specific games. Set up the rest of your keyboard controls in your emulator, then use KAMI to control the camera.

## Before you start

- A Windows PC and the x64 [.NET 10 Desktop Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/10.0). Choose **Desktop Runtime**, which includes support for Windows desktop applications.
- [RPCS3](https://rpcs3.net/) or [PCSX2](https://pcsx2.net/) with IPC enabled.
- A game and title ID listed on the [compatibility page]({{ '/compatibility/' | relative_url }}). Check any version or game-mode restrictions before starting.

## 1. Download KAMI

Get the ZIP from the [latest release]({{ site.links.download }}), extract the whole archive into a writable folder, and open `KAMI.Windows.exe`. Keep the accompanying files together; KAMI needs its native PINE library as well as the executable.

These instructions match the v0.0.8 release. The redesigned interface shown on the home page is a work in progress.

## 2. Enable the emulator connection

### RPCS3

Enable IPC in RPCS3 under **Configuration → IPC**, then start your game. Leave **Use PCSX2** unchecked in KAMI. RPCS3 builds from `0.0.22-13800` include the required IPC support; use a current build where possible.

### PCSX2

Enable IPC/PINE in PCSX2 and use port **28012**. Check **Use PCSX2** in KAMI, then start your game. The documented minimum is `v1.7.0-dev-1177-gedeb0d7bd`; the setting's location can vary between emulator versions.

KAMI checks the selected emulator automatically. When it recognizes a supported game, the status becomes **Ready**. Matching a game title alone is not enough: its title ID and, for some games, version must also match.

## 3. Choose a toggle key

Click the button beside **On/Off Button**, then press a key. Pick one you do not use in the emulator or other applications: this is a global hotkey, and KAMI consumes it while running. Press **Escape** while assigning a key to clear the binding.

With the game focused, press your chosen key to start injecting. Press it again to stop and release the mouse. There is no default toggle key; it starts unbound.

## 4. Adjust the feel

Start with the default sensitivity of **0.003**, then adjust it while playing. Use a decimal point when entering values. A green indicator means the value was accepted; red means it was not. **Invert X** and **Invert Y** change the direction of mouse movement.

### Optional mouse controls

**Mouse1** and **Mouse2** map the left and right mouse buttons to keyboard keys while injecting. Bind those keys to the desired actions in your emulator. These options can help with PCSX2, where the README notes that its mouse bindings cannot be combined with KAMI.

**Hide mouse cursor** hides the Windows cursor during injection and restores the system cursors when injection stops. Mouse button mappings use a Windows mouse hook. Both features are optional and are not required for RPCS3.

## If it does not connect

Check that the game is running, IPC is enabled, the correct emulator is selected, and PCSX2 uses port `28012`. Compare the title ID and game version against [compatibility]({{ '/compatibility/' | relative_url }}), including any restrictions. If a binding does nothing, choose a different toggle key that another application is not using.

For a reproducible problem, [open an issue](https://github.com/isJuhn/KAMI/issues) with the KAMI version, emulator version, game title ID and version, steps to reproduce, and the error message. Remove personal paths from logs or screenshots before posting.

The [release README](https://github.com/isJuhn/KAMI/blob/v0.0.8/README.md#running) and [Windows interface source](https://github.com/isJuhn/KAMI/blob/v0.0.8/KAMI.Windows/MainWindow.xaml) are the reference for these controls.
