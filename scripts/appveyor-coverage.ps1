$DOTNET = "C:\Program Files\dotnet\dotnet.exe"
$ASSEMBLY_OUTPUT_DIR = "$env:CONFIGURATION"

$ALTCOVER_VERSION = "3.0.422"
$ALTCOVER_PATH = "altcover\altcover.$ALTCOVER_VERSION\tools\netcoreapp2.0\AltCover.dll"

$RUNTIME_VERSION = (dotnet --info | Select-String -Pattern "Microsoft .NET Core Shared Framework Host" -Context 0,10).Context.PostContext[3] | % { [regex]::Match($_, "(\d\.?)+").Value }

if ($env:PLATFORM -eq "x86" -Or $env:PLATFORM -eq "x64")
{
    if ($env:PLATFORM -eq "x86")
    {
        $DOTNET = "C:\Program Files (x86)\dotnet\dotnet.exe"
    }

    $ASSEMBLY_OUTPUT_DIR = "$env:PLATFORM\$env:CONFIGURATION"
}

function Run-Coverage([string]$project, [string]$framework)
{
    $INPUT_DIRECTORY = "$project\bin\$ASSEMBLY_OUTPUT_DIR\$framework"
    $OUTPUT_DIRECTORY = "instrumented\$project-$framework"
    $XMLREPORT = "coverage-$project-$framework.xml"

    & $DOTNET $ALTCOVER_PATH `
    --inputDirectory $INPUT_DIRECTORY `
    --outputDirectory $OUTPUT_DIRECTORY `
    --xmlReport $XMLREPORT `
    --opencover `
    --save `
    --inplace `
    --assemblyExcludeFilter ".+\.Tests" `
    --assemblyExcludeFilter "AltCover.+"

    Push-Location -Path $project

    & $DOTNET xunit -nobuild -noshadow -framework $FRAMEWORK -fxversion $RUNTIME_VERSION -configuration $env:CONFIGURATION

    Pop-Location

    & $DOTNET $ALTCOVER_PATH runner --recorderDirectory $INPUT_DIRECTORY --collect
}

nuget install altcover -OutputDirectory altcover -Version $ALTCOVER_VERSION

Run-Coverage "AdvancedDLSupport.Tests" "netcoreapp2.0"
Run-Coverage "AdvancedDLSupport.Tests" "net461"

Run-Coverage "Mono.DllMap.Tests" "netcoreapp2.0"
Run-Coverage "Mono.DllMap.Tests" "net461"