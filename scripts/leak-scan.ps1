<#
.SYNOPSIS
    Fails the build if anything in this repository names Juno Cassandra server or admin internals.

.DESCRIPTION
    This repository is PUBLIC. Everything committed to it is published, permanently, to anyone
    who looks — including whatever was pasted in while debugging and meant to be taken out again.

    The rule this enforces is that client-facing material carries no server paths, no service or
    account names, no web-server or ACL configuration, no database filenames, no admin tooling
    and no other client's model name.

    That rule cannot be kept by care alone. Curation by human attention fails *silently*: nobody
    notices the paragraph that should not have shipped, there is no error, and the first signal is
    someone reading it. Curation by CI fails *loudly*, on the push that introduced it, while the
    author still remembers what they were doing. That asymmetry is the entire reason this script
    exists — it is not a security control, it is a smoke alarm.

    Exit codes:
      0  clean (suppressions may have been reported, and are printed either way)
      1  at least one unsuppressed hit
      2  the scan itself could not run

.PARAMETER Path
    Root to scan. Defaults to the repository root (the parent of this script's folder).

.PARAMETER Quiet
    Print only hits and the summary line.

.EXAMPLE
    .\scripts\leak-scan.ps1

.NOTES
    SUPPRESSING A GENUINE FALSE POSITIVE

    Put the marker below on the SAME LINE as the match, with a reason:

        some line containing the term            # jcass-leak-scan:allow reason goes here

    Suppressions are printed in the scan output every run, never applied silently. A suppression
    nobody can see is indistinguishable from a leak nobody noticed, which would defeat the point.

    A suppression is for a word that is legitimately part of the client-facing subject matter and
    happens to collide with a pattern. It is NOT for making a real leak go away. If the hit is in
    prose or code describing how the server works, the content is wrong — change the content.
#>

[CmdletBinding()]
param(
    [string]$Path,
    [switch]$Quiet
)

$ErrorActionPreference = 'Stop'

$repoRoot = if ($Path) { $Path } else { Split-Path -Parent $PSScriptRoot }
if (-not (Test-Path -LiteralPath $repoRoot)) {
    Write-Host "Scan root not found: $repoRoot" -ForegroundColor Red
    exit 2
}
$repoRoot = (Resolve-Path -LiteralPath $repoRoot).Path

# ---------------------------------------------------------------------------
# THE DENYLIST
#
# Each entry is a case-insensitive regular expression. Keep them literal and
# specific: a pattern that fires on ordinary domain-model wording gets switched
# off, and a scanner that is switched off protects nothing.
#
# Adding a term is cheap and almost always right. Removing one needs a reason
# recorded in the commit message.
# ---------------------------------------------------------------------------
$denyList = @(
    # --- Server filesystem layout -------------------------------------------
    @{ Pattern = '/var/cassandra';        Why = 'server filesystem layout' }
    @{ Pattern = '/etc/cassandra';        Why = 'server filesystem layout' }

    # --- Service, unit and account names ------------------------------------
    @{ Pattern = 'cassandra-api';         Why = 'service / unit name' }
    @{ Pattern = 'cassandra-devbox';      Why = 'service / unit name' }
    @{ Pattern = 'cassandra-worker';      Why = 'service account name' }
    @{ Pattern = 'cassandra-usage-export'; Why = 'service / timer name' }
    @{ Pattern = 'systemd-run';           Why = 'process sandboxing internals' }
    @{ Pattern = 'systemd';               Why = 'init system internals' }
    @{ Pattern = 'journalctl';            Why = 'init system internals' }
    @{ Pattern = 'ExecStart';             Why = 'unit-file internals' }

    # --- Permissions and privilege ------------------------------------------
    @{ Pattern = 'setfacl';               Why = 'ACL administration' }
    @{ Pattern = 'getfacl';               Why = 'ACL administration' }
    @{ Pattern = 'sudoers';               Why = 'privilege configuration' }
    @{ Pattern = 'polkit';                Why = 'privilege configuration' }
    @{ Pattern = 'AclConfigurator';       Why = 'server-side permissions internals' }

    # --- Reverse proxy -------------------------------------------------------
    @{ Pattern = 'nginx';                 Why = 'reverse-proxy configuration' }
    @{ Pattern = 'auth_request';          Why = 'reverse-proxy configuration' }
    @{ Pattern = 'proxy_read_timeout';    Why = 'reverse-proxy configuration' }

    # --- Data and admin tooling ---------------------------------------------
    @{ Pattern = 'jcass_web\.db3';        Why = 'application database file' }
    @{ Pattern = 'push-domain-model';     Why = 'admin-only registry tooling' }

    # --- Other clients' models ----------------------------------------------
    # Commercially sensitive: the model names alone identify who runs what.
    @{ Pattern = 'BMSCustomModel';        Why = "another client's model" }
    @{ Pattern = 'LA-Nelson';             Why = "another client's model" }

    # --- Maintainer-machine paths -------------------------------------------
    # Not secret, but a local checkout path in client-facing material is noise
    # at best and a wrong instruction at worst.
    @{ Pattern = 'aa_git_repos';          Why = "maintainer's local checkout path" }
    @{ Pattern = 'cassandra_release_dlls'; Why = 'maintainer build-output folder' }
)

# Binary and near-binary extensions. Grepping these produces noise, not findings.
$skipExtensions = @(
    '.dll', '.pdb', '.exe', '.zip', '.xlsx', '.xls', '.xlsm', '.png', '.jpg', '.jpeg',
    '.gif', '.ico', '.pdf', '.7z', '.gz', '.tar', '.bin', '.so', '.dylib', '.nupkg'
)

$suppressionMarker = 'jcass-leak-scan:allow'

# This script holds the denylist, so scanning it would report every term in it.
$selfPath = $PSCommandPath

# ---------------------------------------------------------------------------
# WHAT GETS SCANNED
#
# Ask git, not the filesystem. The question this script answers is "what would
# be published", and that is precisely: tracked files, plus untracked files that
# are not ignored (i.e. would be swept up by the next `git add .`). Ignored
# files are local-only and irrelevant — scanning them produces alarms about
# build output and local framework assemblies that no reader will ever see, and
# an alarm that cries wolf gets muted.
#
# Outside a git working tree, fall back to walking the filesystem.
# ---------------------------------------------------------------------------
$listedByGit = $null
try {
    $gitOutput = & git -C $repoRoot ls-files --cached --others --exclude-standard 2>$null
    if ($LASTEXITCODE -eq 0 -and $gitOutput) { $listedByGit = @($gitOutput) }
}
catch {
    $listedByGit = $null
}

if ($listedByGit) {
    $source = 'git (tracked + untracked, ignored files excluded)'
    $files = $listedByGit |
        ForEach-Object { Join-Path $repoRoot ($_ -replace '/', '\') } |
        Where-Object { Test-Path -LiteralPath $_ -PathType Leaf } |
        Get-Item -Force
}
else {
    $source = 'filesystem walk (not a git working tree)'
    $skipDirectories = @('.git', 'bin', 'obj', '.vs', '.idea', 'node_modules')
    $files = Get-ChildItem -LiteralPath $repoRoot -Recurse -File -Force | Where-Object {
        $relative = $_.FullName.Substring($repoRoot.Length).TrimStart('\', '/')
        $segments = @($relative -split '[\\/]')
        $inSkippedDirectory = @($segments | Select-Object -SkipLast 1 | Where-Object { $skipDirectories -contains $_ }).Count -gt 0
        -not $inSkippedDirectory
    }
}

$files = @($files | Where-Object {
    ($skipExtensions -notcontains $_.Extension.ToLowerInvariant()) -and ($_.FullName -ne $selfPath)
})

if (-not $Quiet) {
    Write-Host ''
    Write-Host "Leak scan: $repoRoot"
    Write-Host "  $($denyList.Count) patterns, $($files.Count) files"
    Write-Host "  file list from: $source"
    Write-Host "  not scanned: binary file types, and this script (it holds the denylist)"
    Write-Host ''
}

$hits = @()
$suppressed = @()

foreach ($file in $files) {
    # @() is load-bearing: Get-Content returns a bare string for a single-line file, and
    # indexing a string yields characters. Without it, one-line files are silently never
    # scanned - which is the exact failure mode this whole script exists to prevent.
    $lines = @(Get-Content -LiteralPath $file.FullName -ErrorAction SilentlyContinue)
    if ($lines.Count -eq 0) { continue }

    $relative = $file.FullName.Substring($repoRoot.Length).TrimStart('\', '/').Replace('\', '/')

    for ($i = 0; $i -lt $lines.Count; $i++) {
        $line = $lines[$i]
        if ([string]::IsNullOrWhiteSpace($line)) { continue }

        foreach ($entry in $denyList) {
            if ($line -notmatch $entry.Pattern) { continue }

            $record = [pscustomobject]@{
                File    = $relative
                Line    = $i + 1
                Pattern = $entry.Pattern
                Why     = $entry.Why
                Text    = $line.Trim()
            }

            if ($line -match [regex]::Escape($suppressionMarker)) {
                $reason = ''
                if ($line -match ([regex]::Escape($suppressionMarker) + '\s*(.*)$')) {
                    # Trim the comment terminator the marker was written inside, so an
                    # HTML or block comment does not leave "-->" or "*/" glued to the reason.
                    $reason = ($Matches[1] -replace '\s*(-->|\*/|#>)\s*$', '').Trim()
                }
                $record | Add-Member -NotePropertyName Reason -NotePropertyValue $reason
                $suppressed += $record
            }
            else {
                $hits += $record
            }
        }
    }
}

if ($suppressed.Count -gt 0) {
    Write-Host "SUPPRESSED ($($suppressed.Count)) - shown every run, on purpose:" -ForegroundColor Yellow
    foreach ($s in $suppressed) {
        $reason = if ($s.Reason) { $s.Reason } else { '(NO REASON GIVEN - add one)' }
        Write-Host ("  {0}:{1}  /{2}/  -> {3}" -f $s.File, $s.Line, $s.Pattern, $reason) -ForegroundColor Yellow
    }
    Write-Host ''
}

if ($hits.Count -gt 0) {
    Write-Host "LEAK SCAN FAILED - $($hits.Count) hit(s)" -ForegroundColor Red
    Write-Host ''
    foreach ($h in $hits) {
        Write-Host ("  {0}:{1}" -f $h.File, $h.Line) -ForegroundColor Red
        Write-Host ("      matched /{0}/  ({1})" -f $h.Pattern, $h.Why)
        $text = if ($h.Text.Length -gt 160) { $h.Text.Substring(0, 160) + ' ...' } else { $h.Text }
        Write-Host ("      {0}" -f $text) -ForegroundColor DarkGray
        Write-Host ''
    }
    Write-Host 'This repository is public. The default fix is to CHANGE THE CONTENT, not to suppress.' -ForegroundColor Red
    Write-Host "Suppress only a genuine collision, on the same line:  # $suppressionMarker <reason>" -ForegroundColor Red
    Write-Host ''
    exit 1
}

if (-not $Quiet) {
    Write-Host "Leak scan clean - 0 hits, $($suppressed.Count) suppressed." -ForegroundColor Green
    Write-Host ''
}

exit 0
