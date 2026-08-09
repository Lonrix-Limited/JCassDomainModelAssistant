<#
.SYNOPSIS
    Reports which framework build the reference assemblies in refs\ came from.

.DESCRIPTION
    Your model compiles against the assemblies in refs\. Those are a snapshot of the Juno Cassandra
    framework taken when this Assistant was released, and the web app moves on.

    Compiling against an older framework than the server runs is a quiet failure. There is no
    error: a method whose behaviour changed still compiles, and a method that has since been added
    is simply absent from IntelliSense. What you get is a model that behaves differently in the web
    app than you expected from reading your own code, with nothing pointing at the cause. That is
    what this script is for.

    It reads the assemblies themselves, not a note beside them, so it cannot report a version the
    folder does not actually hold.

.PARAMETER RefsFolder
    Which refs\ folder to report on. Defaults to the one at the repository root, which is the
    canonical copy every other one is seeded from.

.EXAMPLE
    .\scripts\check-framework-version.ps1

.EXAMPLE
    .\scripts\check-framework-version.ps1 -RefsFolder .\reference-model\DomainModelSample\refs
#>

[CmdletBinding()]
param(
    [string]$RefsFolder
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
if (-not $RefsFolder) { $RefsFolder = Join-Path $repoRoot 'refs' }

if (-not (Test-Path -LiteralPath $RefsFolder -PathType Container)) {
    Write-Host ''
    Write-Host "No refs folder at: $RefsFolder" -ForegroundColor Red
    Write-Host ''
    exit 2
}

$RefsFolder = (Resolve-Path -LiteralPath $RefsFolder).Path

$assemblies = @(Get-ChildItem -LiteralPath $RefsFolder -Filter '*.dll' -File | Sort-Object Name)
if ($assemblies.Count -eq 0) {
    Write-Host ''
    Write-Host "No framework assemblies in: $RefsFolder" -ForegroundColor Red
    Write-Host 'Re-download this Assistant - refs\ is part of the release and should never be empty.' -ForegroundColor Red
    Write-Host ''
    exit 2
}

Write-Host ''
Write-Host 'Juno Cassandra framework - reference assemblies'
Write-Host "  $RefsFolder"
Write-Host ''

# ProductVersion is the assembly's informational version, and the build appends the framework's own
# git commit SHA to it. Reading it back off the file is what makes this trustworthy: the answer
# comes from the bytes you are compiling against rather than from a note that may have been left
# behind by an earlier refresh.
$shas = @{}
foreach ($assembly in $assemblies) {
    $productVersion = $assembly.VersionInfo.ProductVersion
    $sha = if ($productVersion -match '\+([0-9a-f]{7,40})') { $Matches[1] } else { '(no commit SHA)' }
    if (-not $shas.ContainsKey($sha)) { $shas[$sha] = @() }
    $shas[$sha] += $assembly.BaseName

    $xml = Join-Path $RefsFolder ($assembly.BaseName + '.xml')
    $documented = if (Test-Path -LiteralPath $xml) { 'docs' } else { 'NO DOCS' }

    Write-Host ("  {0,-26} {1,-10} {2}" -f $assembly.BaseName, $documented, $productVersion)
}

Write-Host ''

$missingDocs = @($assemblies | Where-Object { -not (Test-Path -LiteralPath (Join-Path $RefsFolder ($_.BaseName + '.xml'))) })
if ($missingDocs.Count -gt 0) {
    Write-Host "$($missingDocs.Count) assembly/assemblies have no .xml documentation file." -ForegroundColor Yellow
    Write-Host 'Those are what give you framework descriptions in IntelliSense, and what an AI' -ForegroundColor Yellow
    Write-Host 'assistant reads to learn the API. A folder missing them still builds, which is why' -ForegroundColor Yellow
    Write-Host 'this is worth saying out loud. Re-download the Assistant.' -ForegroundColor Yellow
    Write-Host ''
}

if ($shas.Keys.Count -gt 1) {
    Write-Host 'MIXED FRAMEWORK BUILDS in one folder:' -ForegroundColor Red
    foreach ($sha in $shas.Keys) {
        Write-Host ("  {0}  <- {1}" -f $sha, ($shas[$sha] -join ', ')) -ForegroundColor Red
    }
    Write-Host ''
    Write-Host 'The project compiles against refs\*.dll with a wildcard, so all of these are in play' -ForegroundColor Red
    Write-Host 'at once. Re-download the Assistant rather than trying to sort it out by hand.' -ForegroundColor Red
    Write-Host ''
    exit 1
}

$frameworkSha = @($shas.Keys)[0]

Write-Host "Framework commit : $frameworkSha" -ForegroundColor Green
Write-Host ("Assemblies       : {0}, all from the same build" -f $assemblies.Count)

$stamp = Join-Path $RefsFolder 'FRAMEWORK-VERSION.txt'
if (Test-Path -LiteralPath $stamp) {
    # @() around the whole pipeline, not just Get-Content: a single match comes back as a bare
    # string, and indexing a string gives you one character rather than the line.
    $refreshedOn = @(@(Get-Content -LiteralPath $stamp) | Where-Object { $_ -match '^Refreshed on' })
    if ($refreshedOn.Count -gt 0) {
        Write-Host ("Snapshot taken   : " + ($refreshedOn[0] -replace '^Refreshed on\s*:\s*', ''))
    }
}

Write-Host ''
Write-Host 'COMPARING THIS AGAINST WHAT THE SERVER RUNS'
Write-Host ''
Write-Host '  The web app does not currently display the framework version it is running, so there'
Write-Host '  is no screen to read it off. Two things you can do instead:'
Write-Host ''
Write-Host '  1. Check whether a newer release of this Assistant is available, and download it if'
Write-Host '     so. refs\ is part of the release, so a newer framework arrives with it. Your own'
Write-Host '     model lives in its own folder BESIDE this repository and is not touched.'
Write-Host ''
Write-Host '  2. If you suspect a mismatch is causing behaviour you cannot explain, email'
Write-Host '     support@lonrix.com quoting the commit SHA above, your model name, and what your'
Write-Host '     model does locally versus in the web app. That SHA is what makes the question'
Write-Host '     answerable in one reply instead of three.'
Write-Host ''
Write-Host '  Before assuming a version mismatch, rule out the ordinary causes: your model, the'
Write-Host '  client inputs, or a lookup value that differs from the one you tested against. A'
Write-Host '  mismatch is the rarer explanation.'
Write-Host ''

exit 0
