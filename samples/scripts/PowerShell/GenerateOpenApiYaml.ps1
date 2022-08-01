<#
.SYNOPSIS
Generates the OpenApi doc for the specified version and compares it with the baseline to make sure no breaking changes are introduced
.Parameter SwaggerDir
The working directory
.PARAMETER AssemblyDir
Path for the web projects dll
.PARAMETER Versions
Api versions to generate the OpenApiDoc for and compare with baseline
.PARAMETER SwashbuckleCLIVersion
Version of SwashbuckleCLI to use
#>

param(
    [string]$SwaggerDir,

    [string]$AssemblyDir,

    [String[]]$Versions,

    [string]$SwashbuckleCLIVersion = '6.1.4'
)

echo "ErrorActionPreference is $ErrorActionPreference"
dotnet new tool-manifest --force
dotnet tool install --version $SwashbuckleCLIVersion Swashbuckle.AspNetCore.Cli

echo "Using swagger version ..."
dotnet tool list | Select-String "swashbuckle"

try {
    Write-Host "Testing that swagger will work ..."
    dotnet swagger
} catch {
    Write-Host "In catch now."
    Write-Error "Error occured - $error"
}

