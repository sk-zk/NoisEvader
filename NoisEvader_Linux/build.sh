#!/bin/sh

dotnet build -c Release && rm -rf ./bin/Release/netcoreapp3.1/FFmpeg/
