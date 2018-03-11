#!/bin/bash

# Install AltCover
nuget install altcover -OutputDirectory altcover -Version 2.0.324

# Instrument the test assemblies
dotnet run --project altcover/altcover.2.0.324/tools/netcoreapp2.0/AltCover/altcover.core.fsproj --configuration ${BuildConfiguration} --\
 -i=AdvancedDLSupport.Tests/bin/${BuildConfiguration}/netcoreapp2.0 -o=instrumented-adl -x=coverage-adl.xml --opencover\
 --assemblyExcludeFilter=.+\.Tests --assemblyExcludeFilter=AltCover.+ --assemblyExcludeFilter=Mono\.DllMap.+

dotnet run --project altcover/altcover.2.0.324/tools/netcoreapp2.0/AltCover/altcover.core.fsproj --configuration ${BuildConfiguration} --\
 -i=Mono.DllMap.Tests/bin/${BuildConfiguration}/netcoreapp2.0 -o=instrumented-mdl -x=coverage-mdl.xml --opencover\
 --assemblyExcludeFilter=.+\.Tests --assemblyExcludeFilter=AltCover.+

# Copy them to their build directories
cp instrumented-adl/* AdvancedDLSupport.Tests/bin/${BuildConfiguration}/netcoreapp2.0
cp instrumented-mdl/* Mono.DllMap.Tests/bin/${BuildConfiguration}/netcoreapp2.0

# And run coverage
dotnet run --project altcover/altcover.2.0.324/tools/netcoreapp2.0/AltCover/altcover.core.fsproj --configuration ${BuildConfiguration} --no-build --\
 runner -x "dotnet" -r "AdvancedDLSupport.Tests/bin/${BuildConfiguration}/netcoreapp2.0" --\
 test AdvancedDLSupport.Tests --no-build

dotnet run --project altcover/altcover.2.0.324/tools/netcoreapp2.0/AltCover/altcover.core.fsproj --configuration ${BuildConfiguration} --no-build --\
 runner -x "dotnet" -r "Mono.DllMap.Tests/bin/${BuildConfiguration}/netcoreapp2.0" --\
 test Mono.DllMap.Tests --no-build
