#!/usr/bin/env bash
cd "pine/bindings/c"
meson "build"
cd "build"
ninja
cd "../../../../"
cp "./pine/bindings/c/build/libpine_c.so" "./KAMI.Core/"
