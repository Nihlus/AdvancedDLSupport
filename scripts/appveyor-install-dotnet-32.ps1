if ($env:PLATFORM -eq "x86")
{
    Invoke-WebRequest -Uri 'https://dot.net/v1/dotnet-install.ps1' -OutFile C:\dotnet-install.ps1
    C:\dotnet-install.ps1 -Channel Current -InstallDir "C:\Program Files (x86)\dotnet" -Architecture x86 -NoPath
}
