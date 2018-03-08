@echo off

set CACHED_PLATFORM=%PLATFORM%
set PLATFORM=

dotnet test --configuration %CONFIGURATION% --no-build AdvancedDLSupport.Tests
dotnet test --configuration %CONFIGURATION% --no-build Mono.DllMap.Tests

set PLATFORM=%CACHED_PLATFORM%
