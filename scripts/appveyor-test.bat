@echo off

if "%PLATFORM"=="Any CPU" (
	set CACHED_PLATFORM=%PLATFORM%
	set PLATFORM=
)

:: Determine the dotnet runtime to use
set DOTNET_EXE="C:\Program Files\dotnet\dotnet.exe"
if "%PLATFORM%"=="x86" (
	set DOTNET_EXE="C:\Program Files (x86)\dotnet\dotnet.exe"
)

"%DOTNET_EXE%" test --configuration %CONFIGURATION% --no-build AdvancedDLSupport.Tests
"%DOTNET_EXE%" test --configuration %CONFIGURATION% --no-build Mono.DllMap.Tests

if "%PLATFORM"=="Any CPU" (
	set PLATFORM=%CACHED_PLATFORM%
)
