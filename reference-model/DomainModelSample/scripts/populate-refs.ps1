<#
.SYNOPSIS
    Populates the project's refs\ folder with the Juno Cassandra framework assemblies.

.DESCRIPTION
    The project compiles against framework DLLs that are not distributed in this repository.
    This script copies them into refs\ from wherever you keep them.

    The source can be either a folder of DLLs or a .zip containing them, and can be supplied
    three ways, in order of precedence:

      1. The -Source parameter.
      2. The JCASS_FRAMEWORK_DLLS environment variable.
      3. Nothing — in which case the script explains what to do and exits non-zero.

    There is no built-in default path on purpose. A path that happens to be right on one
    machine is wrong everywhere else, and silently copying stale DLLs from a forgotten folder
    is a worse failure than being told to say where they are.

.PARAMETER Source
    Folder containing the framework DLLs, or a .zip file containing them.

.PARAMETER NoXmlDocs
    Skip the framework's .xml documentation files. They are copied by default because they
    are what give you IntelliSense tooltips on framework types — you rarely want this.

.PARAMETER Clean
    Empty refs\ before copying. Use this when moving between framework versions — a leftover
    DLL from an older release is a genuinely confusing way to fail.

.EXAMPLE
    .\scripts\populate-refs.ps1 -Source "D:\juno\framework-dlls"

.EXAMPLE
    .\scripts\populate-refs.ps1 -Source "$HOME\Downloads\refs.zip" -Clean

.EXAMPLE
    # Set it once per machine, then run the script with no arguments from then on:
    [Environment]::SetEnvironmentVariable('JCASS_FRAMEWORK_DLLS', 'D:\juno\framework-dlls', 'User')
    .\scripts\populate-refs.ps1

.NOTES
    You do not need this script when working on the web Debug Model page — that workspace
    stages refs\ for you when you initialise it.
#>

[CmdletBinding()]
param(
    [string]$Source = $env:JCASS_FRAMEWORK_DLLS,
    [switch]$NoXmlDocs,
    [switch]$Clean
)

$ErrorActionPreference = 'Stop'

# refs\ sits next to this script's parent folder, so the script works from any working directory.
$projectRoot = Split-Path -Parent $PSScriptRoot
$refsFolder = Join-Path $projectRoot 'refs'

# User-facing failures are written plainly and exit non-zero. Write-Error would wrap them in
# PowerShell's stack-trace decoration, and this is the first thing someone sees on a bad first run.
function Stop-WithMessage([string]$Message) {
    Write-Host ''
    Write-Host $Message -ForegroundColor Red
    Write-Host ''
    exit 1
}

if ([string]::IsNullOrWhiteSpace($Source)) {
    Stop-WithMessage @'
No framework DLL source given.

Supply one of:

  .\scripts\populate-refs.ps1 -Source "<folder-with-the-dlls>"
  .\scripts\populate-refs.ps1 -Source "<path-to-refs.zip>"

or set it once for your user account and run the script with no arguments afterwards:

  [Environment]::SetEnvironmentVariable('JCASS_FRAMEWORK_DLLS', '<folder-with-the-dlls>', 'User')

If you do not have the framework assemblies, ask your Juno Cassandra contact for the set
matching your framework version. They are not distributed in this repository.
'@
}

if (-not (Test-Path -LiteralPath $Source)) {
    Stop-WithMessage "Source not found: $Source"
}

# A .zip source is expanded to a temporary folder first, then treated like any other folder.
$temporaryFolder = $null
$sourceFolder = $Source

if ((Test-Path -LiteralPath $Source -PathType Leaf) -and ([IO.Path]::GetExtension($Source) -eq '.zip')) {
    $temporaryFolder = Join-Path ([IO.Path]::GetTempPath()) ("jcass-refs-" + [Guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $temporaryFolder | Out-Null
    Write-Host "Expanding $Source ..."
    Expand-Archive -LiteralPath $Source -DestinationPath $temporaryFolder -Force
    $sourceFolder = $temporaryFolder
}

try {
    # The DLLs may sit at the root of the source or one level down (a zip built from a folder
    # usually nests them). Recurse so both layouts work.
    $dlls = @(Get-ChildItem -Path $sourceFolder -Filter '*.dll' -File -Recurse)
    if ($dlls.Count -eq 0) {
        Stop-WithMessage "No .dll files found under: $sourceFolder"
    }

    if (-not (Test-Path -LiteralPath $refsFolder)) {
        New-Item -ItemType Directory -Path $refsFolder | Out-Null
    }

    if ($Clean) {
        Write-Host "Cleaning $refsFolder ..."
        # README.md in refs\ is tracked documentation, not a framework file. Keep it.
        Get-ChildItem -Path $refsFolder -File |
            Where-Object { $_.Name -ne 'README.md' } |
            Remove-Item -Force
    }

    Copy-Item -Path $dlls.FullName -Destination $refsFolder -Force
    Write-Host "Copied $($dlls.Count) assemblies to $refsFolder"

    if (-not $NoXmlDocs) {
        $xmlDocs = @(Get-ChildItem -Path $sourceFolder -Filter '*.xml' -File -Recurse)
        if ($xmlDocs.Count -gt 0) {
            Copy-Item -Path $xmlDocs.FullName -Destination $refsFolder -Force
            Write-Host "Copied $($xmlDocs.Count) XML documentation files"
        }
    }

    # Stamp what was copied. These DLLs are committed to the repo so the project builds
    # and gives IntelliSense straight after a clone, which means they go stale silently
    # when the framework moves on. The stamp is what makes that visible: ProductVersion
    # carries the framework's own git commit SHA, so two sets can be compared exactly.
    $stampPath = Join-Path $refsFolder 'FRAMEWORK-VERSION.txt'
    $lines = @(
        'Framework assemblies in this folder - provenance stamp.',
        '',
        'Regenerated by scripts\populate-refs.ps1 on every run. Do not hand-edit.',
        'If the ProductVersion SHA below does not match the framework build you are',
        'expected to run against, re-run the script against a current DLL set.',
        '',
        ("Copied on : " + (Get-Date -Format 'yyyy-MM-dd HH:mm:ss')),
        ("Copied from: " + $Source),
        '',
        ("{0,-32} {1,-12} {2}" -f 'Assembly', 'FileVersion', 'ProductVersion')
    )
    foreach ($dll in ($dlls | Sort-Object Name)) {
        $target = Join-Path $refsFolder $dll.Name
        $info = (Get-Item -LiteralPath $target).VersionInfo
        $lines += ("{0,-32} {1,-12} {2}" -f $dll.Name, $info.FileVersion, $info.ProductVersion)
    }
    Set-Content -Path $stampPath -Value $lines -Encoding utf8
    Write-Host "Wrote $stampPath"
}
finally {
    if ($temporaryFolder -and (Test-Path -LiteralPath $temporaryFolder)) {
        Remove-Item -LiteralPath $temporaryFolder -Recurse -Force
    }
}

Write-Host ''
Write-Host 'Done. Next:  dotnet build DomainModelSample.csproj -c Debug'

# Explicit, so a caller checking $LASTEXITCODE sees this run's result rather than whatever the
# previous command in their session left behind.
exit 0
