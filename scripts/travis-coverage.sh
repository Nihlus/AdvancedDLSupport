#!/bin/bash

OutputDir="${Configuration}"
if [ "${Platform}" == "x86" ]; then
	OutputDir="x86/${Configuration}"
fi

if [ "${Platform}" == "x64" ]; then
	OutputDir="x64/${Configuration}"
fi

AdditionalTestArgs=
if [ "${Platform}" == "Any CPU" ]; then
	AdditionalTestArgs="/property:Platform=AnyCPU"
fi

# Install AltCover
nuget install altcover -OutputDirectory altcover -Version 2.0.324

# Instrument the test assemblies
dotnet run --project altcover/altcover.2.0.324/tools/netcoreapp2.0/AltCover/altcover.core.fsproj --configuration ${Configuration} --\
 -i=AdvancedDLSupport.Tests/bin/${OutputDir}/netcoreapp2.0 -o=instrumented-adl-netcoreapp2.0 -x=coverage-adl-netcoreapp2.0.xml --opencover\
 --assemblyExcludeFilter=.+\.Tests --assemblyExcludeFilter=AltCover.+ --assemblyExcludeFilter=Mono\.DllMap.+

dotnet run --project altcover/altcover.2.0.324/tools/netcoreapp2.0/AltCover/altcover.core.fsproj --configuration ${Configuration} --\
 -i=Mono.DllMap.Tests/bin/${OutputDir}/netcoreapp2.0 -o=instrumented-mdl-netcoreapp2.0 -x=coverage-mdl-netcoreapp2.0.xml --opencover\
 --assemblyExcludeFilter=.+\.Tests --assemblyExcludeFilter=AltCover.+

dotnet run --project altcover/altcover.2.0.324/tools/netcoreapp2.0/AltCover/altcover.core.fsproj --configuration ${Configuration} --\
 -i=AdvancedDLSupport.Tests/bin/${OutputDir}/net461 -o=instrumented-adl-net461 -x=coverage-adl-net461.xml --opencover\
 --assemblyExcludeFilter=.+\.Tests --assemblyExcludeFilter=AltCover.+ --assemblyExcludeFilter=Mono\.DllMap.+

dotnet run --project altcover/altcover.2.0.324/tools/netcoreapp2.0/AltCover/altcover.core.fsproj --configuration ${Configuration} --\
 -i=Mono.DllMap.Tests/bin/${OutputDir}/net461 -o=instrumented-mdl-net461 -x=coverage-mdl-net461.xml --opencover\
 --assemblyExcludeFilter=.+\.Tests --assemblyExcludeFilter=AltCover.+

# Copy them to their build directories
cp instrumented-adl-netcoreapp2.0/* AdvancedDLSupport.Tests/bin/${OutputDir}/netcoreapp2.0
cp instrumented-mdl-netcoreapp2.0/* Mono.DllMap.Tests/bin/${OutputDir}/netcoreapp2.0

cp instrumented-adl-net461/* AdvancedDLSupport.Tests/bin/${OutputDir}/net461
cp instrumented-mdl-net461/* Mono.DllMap.Tests/bin/${OutputDir}/net461

# And run coverage
dotnet run --project altcover/altcover.2.0.324/tools/netcoreapp2.0/AltCover/altcover.core.fsproj --configuration ${Configuration} --no-build --\
 runner -x "dotnet" -r "AdvancedDLSupport.Tests/bin/${OutputDir}/netcoreapp2.0" --\
 test AdvancedDLSupport.Tests --no-build --framework netcoreapp2.0 ${AdditionalTestArgs}

dotnet run --project altcover/altcover.2.0.324/tools/netcoreapp2.0/AltCover/altcover.core.fsproj --configuration ${Configuration} --no-build --\
 runner -x "dotnet" -r "Mono.DllMap.Tests/bin/${OutputDir}/netcoreapp2.0" --\
 test Mono.DllMap.Tests --no-build --framework netcoreapp2.0 ${AdditionalTestArgs}
