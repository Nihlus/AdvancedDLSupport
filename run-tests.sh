#!/usr/bin/env bash
set -ev
if [ "${DOTNETCORE}" == 1 ]; then
	dotnet test AdvancedDLSupport.Tests --no-build
	dotnet test Mono.DllMap.Tests --no-build
fi

if [ "${MONO}" == 1 ]; then
	nuget install xunit.runners -Version 1.9.2 -OutputDirectory testrunner
	mono ./testrunner/xunit.runners.1.9.2/tools/xunit.console.clr4.exe ./AdvancedDLSupport.Tests/bin/Debug/AdvancedDLSupport.Tests.dll
	mono ./testrunner/xunit.runners.1.9.2/tools/xunit.console.clr4.exe ./Mono.DllMap.Tests/bin/Debug/Mono.DllMap.Tests.dll
fi