cd "pcsx2_ipc/bindings/c"
meson "build"
cd "build"
ninja
cd "../../../../"
xcopy "pcsx2_ipc\bindings\c\build\pcsx2_ipc_c.dll" "KAMI" /Y

devenv "inputsimulator/WindowsInput/WindowsInput.csproj" /Build "Release"
xcopy "inputsimulator\WindowsInput\bin\Release\WindowsInput.dll" "KAMI" /Y