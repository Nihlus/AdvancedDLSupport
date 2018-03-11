@echo off

:: Extract all but the first of the arguments
for /f "tokens=1,* delims= " %%a in ("%*") do set ALL_BUT_FIRST=%%b

:: Cache the current platform variable, and set it to the passed-in variable
set "INTERNAL_CACHED_PLATFORM=%PLATFORM%"
set "PLATFORM=%1"

:: Clear the platform if it's Any CPU
if "%PLATFORM"=="Any CPU" (
	set "CACHED_PLATFORM=%PLATFORM%"
	set PLATFORM=
)

:: Determine the dotnet runtime to use
set "DOTNET_EXE=C:\Program Files\dotnet\dotnet.exe"
if "%PLATFORM%"=="x86" (
	set "DOTNET_EXE=C:\Program Files (x86)\dotnet\dotnet.exe"
)

"%DOTNET_EXE%" %ALL_BUT_FIRST%

:: Restore the cached variable
set "PLATFORM=%INTERNAL_CACHED_PLATFORM%"

if "%CACHED_PLATFORM"=="Any CPU" (
	set "PLATFORM=%CACHED_PLATFORM%"
)
