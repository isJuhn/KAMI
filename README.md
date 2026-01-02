# KAMI
Kot And Mouse Injector

Inject mouse movements into emulated games

# Requirements
Common requirements:
* Windows
* [.NET 10 Runtime](https://dotnet.microsoft.com/en-us/download/dotnet/10.0)

For RPCS3:
* A recent build with IPC enabled, oldest supported build is `0.0.22-13800`

For PCSX2:
* A recent devbuild with IPC enabled, oldest supported build is `v1.7.0-dev-1177-gedeb0d7bd`
* Use IPC Port 28012

# Compatibility
The compatibility list can be found [here](https://isjuhn.github.io/index.html).

# Running
The application will automatically detect if RPCS3 is running and which game is running. If the game is supported it will automatically connect. If it doesn't, make sure IPC is enabled, you can find it under Configuration -> IPC.

For RPCS3 the only thing you need to change is the On/Off bind, the button you bind to this will act as a global hotkey and right now KAMI will eat those inputs. Therefore it's wise to pick a button you will have no use for when the application is running. Additionally you can change the sensitivity to fit your needs, the units for sensitivity right now is number of radians of change per pixel moved. The green/red circle next to the sensitvity indicates if the sensitivity is applied.

For PCSX2 it can also be beneficial to use the Mouse1 and Mouse2 binds since you cannot combine PCSX2 mouse bindings with KAMI. There is also the option of hiding your mouse cursor when injecting, this is also useful for PCSX2 or if you're running RPCS3 windowed.

Note:
* binding Mouse1 and/or Mouse2 uses [low-level mouse hooks](https://docs.microsoft.com/en-us/previous-versions/windows/desktop/legacy/ms644986(v=vs.85))
* Hiding the mouse cursor sets your system cursor to an invisible cursor and to show your cursor again, KAMI will reload your system cursors

These features are rather intrusive and as such they are entirely optional. For RPCS3, none of them are needed and I hope once PCSX2 moves to QT the need for these features will be gone.

# Building
## Requirements
* .Net 10 SDK
* To build dependencies, needs C++ build tools and meson + ninja

## How to build
* To build dependencies, run `build_deps.bat` in 'x64 Native Tools Command Prompt for VS 2026'
* To build KAMI using commandline, run `build_publish.bat` in 'x64 Native Tools Command Prompt for VS 2026'
* To build KAMI using Visual Studio, open `KAMI.sln` and press the big green button.
