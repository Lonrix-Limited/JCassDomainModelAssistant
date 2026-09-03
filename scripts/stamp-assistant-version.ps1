<#
.SYNOPSIS
    MAINTAINER SCRIPT. Writes ASSISTANT-VERSION.txt from the current commit.

.DESCRIPTION
    Nothing in a downloaded Assistant used to say WHICH Assistant it was.
    refs\FRAMEWORK-VERSION.txt identifies the framework the assemblies came from, and
    tools\jcass-dm.build.txt identifies the tool - but the tool is rebuilt only when its source
    changes, so it read the same commit for every download over a month while the documentation
    around it moved several times.

    That matters in two places. An engineer reporting a problem cannot say what they have, so the
    first reply is always a request for basics; and the update page cannot tell them whether the
    download they are holding is actually newer than the folder they are about to replace.

    So: one file at the root, written from git, refreshed as part of cutting a release.

    RUN THIS AFTER COMMITTING AND BEFORE TAGGING. The stamp names the commit that was HEAD when it
    ran, which is the commit before the one that carries the stamp itself - the same one-commit lag
    tools\jcass-dm.build.txt has, and harmless for the same reason: it identifies the content, and
    the content is what somebody is asking about.

    RELEASES ARE NAMED BY DATE, and CHANGELOG.md promises engineers that two dates answer "is
    the one I am holding older than the one on GitHub?" without their knowing any numbering
    convention. A version number here would also collide with jcass-dm's own, which is a
    different thing. Keep both the -Release name and the tag in the date form.

    Release procedure, in order:

        1. Commit everything that is going out.
        2. .\scripts\stamp-assistant-version.ps1 -Release <yyyy-MM-dd>
        3. Commit ASSISTANT-VERSION.txt and the CHANGELOG entry together.
        4. git tag -a release-<yyyy-MM-dd> -m "..."  and push the tag.

.PARAMETER Release
    The release name, which is the release date - "2026-09-03". Recorded as-is, and it must match
    the CHANGELOG heading it belongs to. Omit it and the file says the release is unnamed, which
    is honest rather than absent.

.EXAMPLE
    .\scripts\stamp-assistant-version.ps1

.EXAMPLE
    .\scripts\stamp-assistant-version.ps1 -Release 2026-09-03
#>

[CmdletBinding()]
param(
    [string]$Release
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
$stampPath = Join-Path $repoRoot 'ASSISTANT-VERSION.txt'

if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
    Write-Host ''
    Write-Host 'git is not on PATH. This is a maintainer script and cannot invent a commit.' -ForegroundColor Red
    Write-Host ''
    exit 2
}

$commit = (& git -C $repoRoot rev-parse HEAD 2>$null)
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($commit)) {
    Write-Host ''
    Write-Host "Not a git checkout: $repoRoot" -ForegroundColor Red
    Write-Host 'Run this in the maintainer clone, not in an unpacked release.' -ForegroundColor Red
    Write-Host ''
    exit 2
}
$commit = $commit.Trim()

$dirty = -not [string]::IsNullOrWhiteSpace((& git -C $repoRoot status --porcelain))

# The framework stamp is read rather than re-derived. Two version lines that disagree are worse
# than one, and refs\ is the only thing that knows which framework build it holds.
$frameworkSha = '(refs/FRAMEWORK-VERSION.txt not found)'
$frameworkStamp = Join-Path $repoRoot 'refs\FRAMEWORK-VERSION.txt'
if (Test-Path -LiteralPath $frameworkStamp) {
    $stampLines = @(Get-Content -LiteralPath $frameworkStamp)
    $line = @($stampLines | Where-Object { $_ -match '^Framework commit' })
    if ($line.Count -gt 0) {
        $frameworkSha = ($line[0] -replace '^Framework commit\s*:\s*', '').Trim()
    }

    # FRAMEWORK-VERSION.txt warns when the assemblies were built from a modified working tree, in
    # which case its commit is the nearest one rather than an exact description of those bytes.
    # Carry the qualifier through: a stamp somebody quotes to support must not be more confident
    # than the file it was read from.
    if (@($stampLines | Where-Object { $_ -match '^WARNING' }).Count -gt 0) {
        $frameworkSha = "$frameworkSha (nearest commit - built from a modified working tree)"
    }
}

$releaseName = if ([string]::IsNullOrWhiteSpace($Release)) { '(unnamed - see CHANGELOG.md)' } else { $Release.Trim() }

$lines = @(
    'Juno Cassandra - Domain Model Assistant'
    ''
    'WHICH VERSION OF THE ASSISTANT IS THIS?'
    ''
    "release          : $releaseName"
    "assistant commit : $commit"
    "stamped on       : $(Get-Date -Format 'yyyy-MM-dd')"
    "framework commit : $frameworkSha"
    ''
    'Quote the release and the assistant commit when you email support@lonrix.com. They say'
    'exactly which documentation, tool and reference assemblies you are looking at, which is'
    'the difference between one reply and three.'
    ''
    'What changed between releases, and what to re-check in your own model, is in CHANGELOG.md'
    'beside this file. How to move to a newer version without losing anything is in'
    'docs/orientation/updating-the-assistant.md.'
    ''
    'Written by scripts/stamp-assistant-version.ps1 at release time. Do not hand-edit: a version'
    'file somebody typed is a version file that is wrong.'
)

if ($dirty) {
    $lines += ''
    $lines += 'WARNING: the working tree had uncommitted changes when this was stamped, so the'
    $lines += '         commit above does not fully describe this folder. Re-stamp after'
    $lines += '         committing.'
}

$lines += ''

Set-Content -LiteralPath $stampPath -Value $lines -Encoding utf8

Write-Host ''
Write-Host "Stamped ASSISTANT-VERSION.txt" -ForegroundColor Green
Write-Host "  release          : $releaseName"
Write-Host "  assistant commit : $commit"
Write-Host "  framework commit : $frameworkSha"
if ($dirty) {
    Write-Host ''
    Write-Host 'The working tree is dirty and the stamp says so. Commit, then re-stamp.' -ForegroundColor Yellow
}
Write-Host ''
Write-Host 'Next: commit this file with the CHANGELOG entry, then tag the release.'
Write-Host ''

exit 0
