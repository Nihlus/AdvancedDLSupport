@echo off

if "%PLATFORM"=="Any CPU" (
	set CACHED_PLATFORM=%PLATFORM%
	set PLATFORM=
)

dotnet test --configuration %CONFIGURATION% --no-build AdvancedDLSupport.Tests
dotnet test --configuration %CONFIGURATION% --no-build Mono.DllMap.Tests

if "%PLATFORM"=="Any CPU" (
	set PLATFORM=%CACHED_PLATFORM%
)
