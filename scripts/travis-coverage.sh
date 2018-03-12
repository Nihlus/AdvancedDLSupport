#!/bin/bash

OutputDir=${Configuration}
if [ ${Platform} == "x86" ]; then
	OutputDir="x86/${Configuration}"
fi

if [ ${Platform} == "x64" ]; then
	OutputDir="x64/${Configuration}"
fi

if [ ${Platform} == "Any CPU" ]; then
	CachedPlatform=${Platform}
	Platform=
fi

# Install AltCover
nuget install altcover -OutputDirectory altcover -Version 2.0.324

# Instrument the test assemblies
dotnet run --project altcover/altcover.2.0.324/tools/netcoreapp2.0/AltCover/altcover.core.fsproj --configuration ${Configuration} --\
 -i=AdvancedDLSupport.Tests/bin/${OutputDir}/netcoreapp2.0 -o=instrumented-adl -x=coverage-adl.xml --opencover\
 --assemblyExcludeFilter=.+\.Tests --assemblyExcludeFilter=AltCover.+ --assemblyExcludeFilter=Mono\.DllMap.+

dotnet run --project altcover/altcover.2.0.324/tools/netcoreapp2.0/AltCover/altcover.core.fsproj --configuration ${Configuration} --\
 -i=Mono.DllMap.Tests/bin/${OutputDir}/netcoreapp2.0 -o=instrumented-mdl -x=coverage-mdl.xml --opencover\
 --assemblyExcludeFilter=.+\.Tests --assemblyExcludeFilter=AltCover.+

# Copy them to their build directories
cp instrumented-adl/* AdvancedDLSupport.Tests/bin/${OutputDir}/netcoreapp2.0
cp instrumented-mdl/* Mono.DllMap.Tests/bin/${OutputDir}/netcoreapp2.0

# And run coverage
dotnet run --project altcover/altcover.2.0.324/tools/netcoreapp2.0/AltCover/altcover.core.fsproj --configuration ${Configuration} --no-build --\
 runner -x "dotnet" -r "AdvancedDLSupport.Tests/bin/${OutputDir}/netcoreapp2.0" --\
 test AdvancedDLSupport.Tests --no-build

dotnet run --project altcover/altcover.2.0.324/tools/netcoreapp2.0/AltCover/altcover.core.fsproj --configuration ${Configuration} --no-build --\
 runner -x "dotnet" -r "Mono.DllMap.Tests/bin/${OutputDir}/netcoreapp2.0" --\
 test Mono.DllMap.Tests --no-build

if [ ${CachedPlatform} == "Any CPU" ]; then
	Platform=${CachedPlatform}
fi
