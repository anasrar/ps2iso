#!/usr/bin/env sh

dotnet publish -r win-x64 -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true --self-contained true
dotnet publish -r linux-x64 -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true --self-contained true
dotnet publish -r osx-x64 -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true --self-contained true
