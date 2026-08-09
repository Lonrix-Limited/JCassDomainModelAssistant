<#
.SYNOPSIS
    Rebuilds tools/jcass-dm.exe from tools/src and stamps what it was built from.

.DESCRIPTION
    MAINTAINER SCRIPT. Nobody using this kit ever needs to run it - that is the point.
    jcass-dm ships pre-compiled and self-contained so that "clone and go" stays true: an
    engineer who has never installed the .NET SDK can download this repository and run the
    tool. Putting a NuGet restore in front of them would cost an afternoon and some of them
    would not get past it.

    The exe is committed. That is deliberate and it is not free - see tools/README.md for
    the reasoning and the cost.

    Run this whenever tools/src changes, then commit the exe and the stamp together.
    The tests must be green first; this script runs them and refuses to publish if they
    are not.

    Exit codes:
      0  published
      1  the build, the tests or the publish failed
      2  the script could not run (wrong folder, no SDK)

.PARAMETER SkipTests
    Publish without running the tests first. The stamp records that it was used, so the
    weaker guarantee travels with the binary instead of living in somebody's memory.

.EXAMPLE
    .\scripts\build-jcass-dm.ps1
#>

[CmdletBinding()]
param(
    [switch]$SkipTests
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$toolSource = Join-Path $repoRoot 'tools\src\JcassDm\JcassDm.csproj'
$testProject = Join-Path $repoRoot 'tools\src\JcassDm.Tests\JcassDm.Tests.csproj'
$toolsFolder = Join-Path $repoRoot 'tools'
$exePath = Join-Path $toolsFolder 'jcass-dm.exe'
$stampPath = Join-Path $toolsFolder 'jcass-dm.build.txt'

if (-not (Test-Path -LiteralPath $toolSource)) {
    Write-Host "Cannot find the tool source. Run this from the repository, as .\scripts\build-jcass-dm.ps1" -ForegroundColor Red
    exit 2
}
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "The .NET SDK is not on PATH. A MAINTAINER needs it to rebuild the tool; a USER does not." -ForegroundColor Red
    exit 2
}

# ---------------------------------------------------------------------------
# Tests first. A committed binary that nobody can see the source behaviour of
# is exactly the thing that should not ship on a hunch.
# ---------------------------------------------------------------------------
if ($SkipTests) {
    Write-Host "SKIPPING TESTS (-SkipTests). This is recorded in the stamp file." -ForegroundColor Yellow
}
else {
    Write-Host "Running the jcass-dm tests..."
    & dotnet test $testProject --nologo --verbosity quiet
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Tests failed. Nothing published." -ForegroundColor Red
        exit 1
    }
}

# ---------------------------------------------------------------------------
# Publish.
#
# self-contained    - no .NET runtime needed on the machine that runs it
# PublishSingleFile - one file to commit, and one file to hand somebody
# compression       - roughly halves it; the extraction cost is a few tens of ms
#
# NOT trimmed. ClosedXML resolves types by reflection in places, so a trimmed
# build can publish cleanly and then fail at run time on a workbook feature the
# tests happened not to exercise. Not a trade worth making to save disk on a
# tool whose whole job is to be trusted with somebody's model.
# ---------------------------------------------------------------------------
$publishFolder = Join-Path ([System.IO.Path]::GetTempPath()) ("jcass-dm-publish-" + [guid]::NewGuid().ToString('N'))

Write-Host "Publishing win-x64, self-contained, single file..."
& dotnet publish $toolSource `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:EnableCompressionInSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:DebugType=none `
    -o $publishFolder `
    --nologo

if ($LASTEXITCODE -ne 0) {
    Write-Host "Publish failed. tools\jcass-dm.exe is unchanged." -ForegroundColor Red
    exit 1
}

$published = Join-Path $publishFolder 'jcass-dm.exe'
if (-not (Test-Path -LiteralPath $published)) {
    Write-Host "The publish succeeded but produced no jcass-dm.exe." -ForegroundColor Red
    exit 1
}

Copy-Item -LiteralPath $published -Destination $exePath -Force
Remove-Item -LiteralPath $publishFolder -Recurse -Force -ErrorAction SilentlyContinue

# ---------------------------------------------------------------------------
# Smoke test the thing that was actually copied, not the thing that was built.
# ---------------------------------------------------------------------------
$version = (& $exePath version) -join ''
if ($LASTEXITCODE -ne 0) {
    Write-Host "The published exe does not run." -ForegroundColor Red
    exit 1
}

$commit = 'unknown'
$dirty = $false
try {
    $commit = (& git -C $repoRoot rev-parse HEAD).Trim()
    $dirty = -not [string]::IsNullOrWhiteSpace((& git -C $repoRoot status --porcelain -- tools/src))
}
catch {
    # Not a git checkout. The stamp says so rather than inventing a SHA.
}

$sizeMb = [math]::Round((Get-Item -LiteralPath $exePath).Length / 1MB, 1)

$lines = @(
    "jcass-dm build stamp"
    ""
    "version:        $version"
    "runtime:        win-x64, self-contained, single file, compressed, not trimmed"
    "built from:     $commit"
    "built on:       $(Get-Date -Format 'yyyy-MM-dd')"
    "size:           $sizeMb MB"
)
if ($dirty) {
    $lines += ""
    $lines += "WARNING: tools/src had uncommitted changes when this was published, so the"
    $lines += "         commit above does not fully describe the binary. Rebuild after committing."
}
if ($SkipTests) {
    $lines += ""
    $lines += "WARNING: published with -SkipTests. The test suite was not run against this build."
}
$lines += ""
$lines += "Rebuild with .\scripts\build-jcass-dm.ps1 - see tools/README.md."

Set-Content -LiteralPath $stampPath -Value $lines -Encoding utf8

Write-Host ""
Write-Host "Published $version to tools\jcass-dm.exe ($sizeMb MB)." -ForegroundColor Green
if ($dirty) {
    Write-Host "tools/src is dirty - commit it and rebuild before releasing." -ForegroundColor Yellow
}
Write-Host "Commit tools\jcass-dm.exe and tools\jcass-dm.build.txt together."
exit 0
