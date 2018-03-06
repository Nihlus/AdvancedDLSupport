@echo off

:: Install AltCover
nuget install altcover -OutputDirectory altcover -Version 1.6.230

:: Instrument the test assemblies
dotnet run^
 --project altcover\altcover.1.6.230\tools\netcoreapp2.0\AltCover\altcover.core.fsproj --configuration %CONFIGURATION% --^
 -i=AdvancedDLSupport.Tests\bin\%CONFIGURATION%\netcoreapp2.0 -o=instrumented-adl -x=coverage-adl.xml^
 --assemblyExcludeFilter=.+\.Tests --assemblyExcludeFilter=AltCover.+ --assemblyExcludeFilter=Mono\.DllMap.+

dotnet run^
 --project altcover\altcover.1.6.230\tools\netcoreapp2.0\AltCover\altcover.core.fsproj --configuration %CONFIGURATION% --^
 -i=Mono.DllMap.Tests\bin\%CONFIGURATION%\netcoreapp2.0 -o=instrumented-mdl -x=coverage-mdl.xml^
 --assemblyExcludeFilter=.+\.Tests --assemblyExcludeFilter=AltCover.+

:: Copy them to their build directories
copy /y instrumented-adl\* AdvancedDLSupport.Tests\bin\%CONFIGURATION%\netcoreapp2.0
copy /y instrumented-mdl\* Mono.DllMap.Tests\bin\%CONFIGURATION%\netcoreapp2.0

:: And run coverage
dotnet run^
 --project altcover\altcover.1.6.230\tools\netcoreapp2.0\AltCover\altcover.core.fsproj^ --no-build --configuration %CONFIGURATION% --^
 runner -x "dotnet" -r "AdvancedDLSupport.Tests\bin\%CONFIGURATION%\netcoreapp2.0" --^
 test AdvancedDLSupport.Tests --no-build

dotnet run^
 --project altcover\altcover.1.6.230\tools\netcoreapp2.0\AltCover\altcover.core.fsproj --no-build --configuration %CONFIGURATION% --^
 runner -x "dotnet" -r "Mono.DllMap.Tests\bin\%CONFIGURATION%\netcoreapp2.0" --^
 test Mono.DllMap.Tests --no-build
