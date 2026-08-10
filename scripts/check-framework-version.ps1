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

.PARAMETER ServerVersion
    The framework commit the web app is running, if you have been told it. Supply it and this
    script answers the question directly instead of leaving you to compare two strings by eye.

.PARAMETER StaleAfterDays
    How old this snapshot may be before it is called out. Defaults to 90 days, which is roughly
    one release cycle - long enough not to nag, short enough that a genuinely abandoned copy is
    named as one.

.EXAMPLE
    .\scripts\check-framework-version.ps1

.EXAMPLE
    .\scripts\check-framework-version.ps1 -ServerVersion 2de6b35

.EXAMPLE
    .\scripts\check-framework-version.ps1 -RefsFolder .\reference-model\DomainModelSample\refs
#>

[CmdletBinding()]
param(
    [string]$RefsFolder,
    [string]$ServerVersion,
    [int]$StaleAfterDays = 90
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

$snapshotAgeDays = $null

$stamp = Join-Path $RefsFolder 'FRAMEWORK-VERSION.txt'
if (Test-Path -LiteralPath $stamp) {
    # @() around the whole pipeline, not just Get-Content: a single match comes back as a bare
    # string, and indexing a string gives you one character rather than the line.
    $refreshedOn = @(@(Get-Content -LiteralPath $stamp) | Where-Object { $_ -match '^Refreshed on' })
    if ($refreshedOn.Count -gt 0) {
        $refreshedText = ($refreshedOn[0] -replace '^Refreshed on\s*:\s*', '').Trim()
        Write-Host ("Snapshot taken   : " + $refreshedText)

        # The stamp is written in a fixed, culture-independent format by the maintainer script.
        # Parse it as such rather than with Get-Date, which would read it through whatever
        # regional settings this machine happens to have and could silently swap month and day.
        $parsed = [datetime]::MinValue
        # [string[]] is load-bearing. A bare @(...) is an object[], which binds TryParseExact to
        # the single-format overload instead of the multi-format one and silently returns false -
        # so the age would never be reported and the script would claim there is no date stamp.
        $formats = [string[]]@('yyyy-MM-dd HH:mm:ss', 'yyyy-MM-dd')
        if ([datetime]::TryParseExact($refreshedText, $formats, [System.Globalization.CultureInfo]::InvariantCulture, [System.Globalization.DateTimeStyles]::None, [ref]$parsed)) {
            $snapshotAgeDays = [int][math]::Floor(((Get-Date) - $parsed).TotalDays)
            Write-Host ("Age              : {0} day(s)" -f $snapshotAgeDays)
        }
    }
}

# ---------------------------------------------------------------------------
# IS THIS OLDER THAN WHAT THE SERVER RUNS?
#
# That is the only question an engineer actually has, and it is worth answering in those words.
# "SHA mismatch" is not an answer to it; it is a fact about two strings.
#
# Two ways of getting at it, because the web app does not yet display the framework version it
# runs. Given -ServerVersion, the comparison is exact. Without it, the snapshot's own age is the
# honest proxy: nobody can say from here whether the server has moved, but a reference taken
# several months ago almost certainly is behind, and saying so is more use than saying nothing.
# ---------------------------------------------------------------------------

Write-Host ''
Write-Host 'IS YOUR REFERENCE OLDER THAN THE SERVER?'
Write-Host ''

$isStale = $false

if ($ServerVersion) {
    $serverSha = $ServerVersion.Trim().ToLowerInvariant()
    $localSha = $frameworkSha.ToLowerInvariant()

    if ($serverSha.Length -lt 7) {
        Write-Host "  '$ServerVersion' is too short to identify a commit - 7 characters or more, please." -ForegroundColor Yellow
        Write-Host ''
    }
    elseif ($localSha.StartsWith($serverSha) -or $serverSha.StartsWith($localSha)) {
        Write-Host '  No. Your reference is the same framework build the web app runs.' -ForegroundColor Green
        Write-Host '  Whatever you are chasing, it is not a version mismatch - look at your model, the'
        Write-Host '  client inputs, or a lookup value that differs from the one you tested against.'
        Write-Host ''
    }
    else {
        $isStale = $true
        Write-Host '  YES - almost certainly.' -ForegroundColor Red
        Write-Host ''
        Write-Host "    your reference : $localSha" -ForegroundColor Red
        Write-Host "    the web app    : $serverSha" -ForegroundColor Red
        Write-Host ''
        Write-Host '  These are different framework builds, and the web app only ever moves forward, so'
        Write-Host '  yours is the older one. Your framework reference is from an older release: ask Juno'
        Write-Host '  for an updated Assistant before trusting the API reference in docs\framework\, and'
        Write-Host '  before concluding anything from what IntelliSense does or does not offer you.'
        Write-Host ''
        Write-Host '  Email support@lonrix.com with both lines above. Nothing you have written is lost -'
        Write-Host '  your model lives in its own folder BESIDE this repository and a newer Assistant'
        Write-Host '  does not touch it.'
        Write-Host ''
    }
}
elseif ($null -eq $snapshotAgeDays) {
    Write-Host '  Cannot tell - this folder has no readable date stamp, so there is nothing to age.' -ForegroundColor Yellow
    Write-Host '  Re-download the Assistant; refs\FRAMEWORK-VERSION.txt is part of every release.' -ForegroundColor Yellow
    Write-Host ''
}
elseif ($snapshotAgeDays -ge ($StaleAfterDays * 2)) {
    $isStale = $true
    Write-Host ("  PROBABLY, and by some distance - this reference was taken {0} days ago." -f $snapshotAgeDays) -ForegroundColor Red
    Write-Host ''
    Write-Host '  Your framework reference is from an older release. Ask Juno for an updated Assistant'
    Write-Host '  before trusting the API reference in docs\framework\ - at this age it describes'
    Write-Host '  signatures that may have changed and is missing anything added since.'
    Write-Host ''
    Write-Host '  Nothing you have written is lost. Your model lives in its own folder BESIDE this'
    Write-Host '  repository and a newer Assistant does not touch it.'
    Write-Host ''
}
elseif ($snapshotAgeDays -ge $StaleAfterDays) {
    Write-Host ("  Possibly - this reference was taken {0} days ago, which is about a release cycle." -f $snapshotAgeDays) -ForegroundColor Yellow
    Write-Host ''
    Write-Host '  Worth asking Juno whether a newer Assistant is available before you spend long on'
    Write-Host '  behaviour you cannot explain from your own code.'
    Write-Host ''
}
else {
    Write-Host ("  Probably not - this reference was taken {0} days ago, which is recent." -f $snapshotAgeDays) -ForegroundColor Green
    Write-Host ''
    Write-Host '  Rule out the ordinary causes first: your model, the client inputs, or a lookup value'
    Write-Host '  that differs from the one you tested against. A version mismatch is the rarer'
    Write-Host '  explanation.'
    Write-Host ''
}

Write-Host '  If you know the framework commit the web app is running, this script can answer the'
Write-Host '  question exactly rather than by age:'
Write-Host ''
Write-Host '    .\scripts\check-framework-version.ps1 -ServerVersion <commit-sha>'
Write-Host ''
Write-Host '  There is no screen in the web app that shows it yet; support@lonrix.com will tell you.'
Write-Host '  Quote the commit above when you ask - it makes the question answerable in one reply'
Write-Host '  instead of three.'
Write-Host ''

if ($isStale) { exit 3 }

exit 0
