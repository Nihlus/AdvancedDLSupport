@echo off

dotnet test --configuration %CONFIGURATION% --no-build AdvancedDLSupport.Tests
dotnet test --configuration %CONFIGURATION% --no-build Mono.DllMap.Tests
