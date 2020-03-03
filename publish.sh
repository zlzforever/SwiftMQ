#!/usr/bin/env bash
export NUGET_SERVER=https://api.nuget.org/v3/index.json
rm -rf src/SwiftMQ/bin/Release
rm -rf src/SwiftMQ.DependencyInjection/bin/Release

dotnet build -c Release
dotnet pack -c Release

nuget push src/SwiftMQ/bin/Release/*.nupkg -SkipDuplicate -Source $NUGET_SERVER
nuget push src/SwiftMQ.DependencyInjection/bin/Release/*.nupkg -SkipDuplicate  -Source $NUGET_SERVER

sudo cp src/SwiftMQ/bin/Release/*.nupkg  /usr/local/share/dotnet/sdk/NuGetFallbackFolder
sudo cp src/SwiftMQ.DependencyInjection/bin/Release/*.nupkg  /usr/local/share/dotnet/sdk/NuGetFallbackFolder