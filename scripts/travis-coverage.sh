#!/bin/bash

nuget install altcover -OutputDirectory altcover -Version 1.6.230
dotnet run --project altcover/altcover.1.6.230/tools/netcoreapp2.0/AltCover/altcover.core.fsproj --configuration Debug -- -i=AdvancedDLSupport.Tests/bin/Debug/netcoreapp2.0 -o=instrumented-adl -x=coverage-adl.xml --assemblyExcludeFilter=.+\.Tests --assemblyExcludeFilter=AltCover.+ --assemblyExcludeFilter=Mono\.DllMap.+
dotnet run --project altcover/altcover.1.6.230/tools/netcoreapp2.0/AltCover/altcover.core.fsproj --configuration Debug -- -i=Mono.DllMap.Tests/bin/Debug/netcoreapp2.0 -o=instrumented-mdl -x=coverage-mdl.xml --assemblyExcludeFilter=.+\.Tests --assemblyExcludeFilter=AltCover.+
cp instrumented-adl/* AdvancedDLSupport.Tests/bin/Debug/netcoreapp2.0
cp instrumented-mdl/* Mono.DllMap.Tests/bin/Debug/netcoreapp2.0
dotnet run --project altcover/altcover.1.6.230/tools/netcoreapp2.0/AltCover/altcover.core.fsproj --no-build -- runner -x "dotnet" -r "AdvancedDLSupport.Tests/bin/Debug/netcoreapp2.0" -- test AdvancedDLSupport.Tests --no-build
dotnet run --project altcover/altcover.1.6.230/tools/netcoreapp2.0/AltCover/altcover.core.fsproj --no-build -- runner -x "dotnet" -r "Mono.DllMap.Tests/bin/Debug/netcoreapp2.0" -- test Mono.DllMap.Tests --no-build
