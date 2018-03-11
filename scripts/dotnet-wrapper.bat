@echo off

:: Determine the dotnet runtime to use
set "DOTNET_EXE=C:\Program Files\dotnet\dotnet.exe"
if "%PLATFORM%"=="x86" (
	set "DOTNET_EXE=C:\Program Files (x86)\dotnet\dotnet.exe"
)

:: Clear the platform if it's Any CPU
if "%PLATFORM"=="Any CPU" (
	set "CACHED_PLATFORM=%PLATFORM%"
	set PLATFORM=
)

"%DOTNET_EXE%" %*
"%DOTNET_EXE%" %*

if "%PLATFORM"=="Any CPU" (
	set "PLATFORM=%CACHED_PLATFORM%"
)
