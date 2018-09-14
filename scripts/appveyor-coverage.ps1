$DOTNET = "C:\Program Files\dotnet\dotnet.exe"

if ($env:PLATFORM -eq "x86")
{
    $DOTNET = "C:\Program Files (x86)\dotnet\dotnet.exe"
}

function Run-Coverage([string]$project)
{
    $XMLREPORT = "coverage-$project.xml"

    Push-Location -Path $project

    & $DOTNET test /p:AltCover=true /p:CopyLocalLockFileAssemblies=true /p:AltCoverXmlReport=$XMLREPORT --configuration $env:CONFIGURATION

    Move-Item coverage*.xml -Destination ..

    Pop-Location
}

Run-Coverage "AdvancedDLSupport.Tests"
Run-Coverage "AdvancedDLSupport.AOT.Tests"
Run-Coverage "Mono.DllMap.Tests"
