#!/usr/bin/env bash
dotnet build /p:DebugType=None /p:DebugSymbols=false "./KAMI.Linux/KAMI.Linux.csproj" -c Release -o "./bin/publish/"
