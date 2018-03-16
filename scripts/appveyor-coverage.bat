@echo off

:: Determine the output folder of the binaries and the dotnet runtime to use
set "DOTNET_EXE=C:\Program Files\dotnet\dotnet.exe"
set "OUTPUT_DIR=%CONFIGURATION%"

if "%PLATFORM%"=="x86" (
	set "DOTNET_EXE=C:\Program Files (x86)\dotnet\dotnet.exe"
	set "OUTPUT_DIR=x86\%CONFIGURATION%"
)

set ADDITIONAL_TEST_ARGS=
if "%PLATFORM%"=="x64" (
	set "OUTPUT_DIR=x64\%CONFIGURATION%"
	set "ADDITIONAL_TEST_ARGS=/property:PlatformTarget=x64"
)

if "%PLATFORM%"=="Any CPU" (
	set "ADDITIONAL_TEST_ARGS=/property:Platform=AnyCPU /property:PlatformTarget=x64"
)

:: Install AltCover
nuget install altcover -OutputDirectory altcover -Version 2.0.324

:: Instrument the test assemblies
"%DOTNET_EXE%" run^
 --project altcover\altcover.2.0.324\tools\netcoreapp2.0\AltCover\altcover.core.fsproj --configuration %CONFIGURATION% --^
 -i=AdvancedDLSupport.Tests\bin\%OUTPUT_DIR%\netcoreapp2.0 -o=instrumented-adl -x=coverage-adl.xml --opencover^
 --assemblyExcludeFilter=.+\.Tests --assemblyExcludeFilter=AltCover.+ --assemblyExcludeFilter=Mono\.DllMap.+

"%DOTNET_EXE%" run^
 --project altcover\altcover.2.0.324\tools\netcoreapp2.0\AltCover\altcover.core.fsproj --configuration %CONFIGURATION% --^
 -i=Mono.DllMap.Tests\bin\%OUTPUT_DIR%\netcoreapp2.0 -o=instrumented-mdl -x=coverage-mdl.xml --opencover^
 --assemblyExcludeFilter=.+\.Tests --assemblyExcludeFilter=AltCover.+

:: Copy them to their build directories
copy /y instrumented-adl\* AdvancedDLSupport.Tests\bin\%OUTPUT_DIR%\netcoreapp2.0
copy /y instrumented-mdl\* Mono.DllMap.Tests\bin\%OUTPUT_DIR%\netcoreapp2.0

:: And run coverage
"%DOTNET_EXE%" run^
 --project altcover\altcover.2.0.324\tools\netcoreapp2.0\AltCover\altcover.core.fsproj --no-build --configuration %CONFIGURATION% --^
 runner -x "dotnet" -r "AdvancedDLSupport.Tests\bin\%OUTPUT_DIR%\netcoreapp2.0" --^
 test AdvancedDLSupport.Tests --no-build %ADDITIONAL_TEST_ARGS%

"%DOTNET_EXE%" run^
 --project altcover\altcover.2.0.324\tools\netcoreapp2.0\AltCover\altcover.core.fsproj --no-build --configuration %CONFIGURATION% --^
 runner -x "dotnet" -r "Mono.DllMap.Tests\bin\%OUTPUT_DIR%\netcoreapp2.0" --^
 test Mono.DllMap.Tests --no-build %ADDITIONAL_TEST_ARGS%
