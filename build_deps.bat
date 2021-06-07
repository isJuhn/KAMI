cd "pine/bindings/c"
meson "build"
cd "build"
ninja
cd "../../../../"
xcopy "pine\bindings\c\build\pine_c.dll" "KAMI" /Y

devenv "inputsimulator/WindowsInput/WindowsInput.csproj" /Build "Release"
xcopy "inputsimulator\WindowsInput\bin\Release\WindowsInput.dll" "KAMI" /Y