$DOTNET = "C:\Program Files\dotnet\dotnet.exe"
$ANY_TESTS_FAILED = 0

if ($env:PLATFORM -eq "x86")
{
    $DOTNET = "C:\Program Files (x86)\dotnet\dotnet.exe"
}

function Run-Coverage([string]$project)
{
    $XMLREPORT = "coverage-$project.xml"

    Push-Location -Path $project

    & $DOTNET test /p:AltCover=true /p:CopyLocalLockFileAssemblies=true /p:AltCoverXmlReport=$XMLREPORT --configuration $env:CONFIGURATION

    if (!($LASTEXITCODE) -eq 0)
    {
        $ANY_TESTS_FAILED = 1;
    }

    Move-Item coverage*.xml -Destination ..

    Pop-Location
}

Run-Coverage "AdvancedDLSupport.Tests"
Run-Coverage "AdvancedDLSupport.AOT.Tests"
Run-Coverage "Mono.DllMap.Tests"

if (!($ANY_TESTS_FAILED -eq 0))
{
    exit 1
}
