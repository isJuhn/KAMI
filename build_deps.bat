cd "pine/bindings/c"
meson "build"
cd "build"
ninja
cd "../../../../"
xcopy "pine\bindings\c\build\pine_c.dll" "KAMI.Core" /Y
