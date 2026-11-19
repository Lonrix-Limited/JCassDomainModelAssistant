<#
.SYNOPSIS
    Replaces a model project's refs\ folder with the reference assemblies this Assistant ships.

.DESCRIPTION
    Your model has its own refs\ folder, beside its .csproj. It was copied there when the project
    was scaffolded, from the refs\ folder at the root of the Assistant you had at the time.

    NOTHING REFRESHES IT WHEN YOU UPDATE THE ASSISTANT. A newer Assistant brings a newer framework
    reference in its own refs\, and your model carries on compiling against the older one. That is
    silent - it builds, it uploads, and the API reference in docs\framework\ describes a framework
    your project is not the one being compiled against. This script is what closes that gap, and
    running it is a step on the update page.

    IT REPLACES, IT DOES NOT TOP UP, AND THAT IS DELIBERATE. Your .csproj references refs\*.dll
    with a WILDCARD, so an assembly left behind from an older release is not sitting there
    harmlessly - it is compiled against, alongside its replacement. Dropping new files in over old
    ones is worse than doing nothing, so this empties the folder first.

    IT COPIES THE .xml FILES TOO, AND REFUSES TO RUN WITHOUT THEM. A refs\ folder holding only
    .dll files builds perfectly and quietly costs you every framework description - in IntelliSense
    and, more importantly, for the AI assistant that reads them to learn an API it cannot otherwise
    see. That is most of what this Assistant is worth, so their absence is an error rather than a
    warning.

    SOMETHING ELSE ALREADY OVERWRITES THIS FOLDER, so this script is not the only thing that
    touches it and is not doing anything unusual: when you upload your model to the Debug Model
    page, that workspace stages its own, larger set of framework assemblies into refs\ before it
    builds. Your local copy and the sidecar's copy have always been two different things. Nothing
    in refs\ is yours, nothing in it is ever uploaded, and nothing in it is worth preserving.

.PARAMETER Project
    Your model folder - the one with the .csproj in it. Relative paths are resolved against the
    folder your terminal is sitting in, which is normally the Assistant's own folder, so
    ..\MyRoadModel is the usual form.

.PARAMETER UpdateTargetFramework
    Permission to change one line of your .csproj. If the references this copies are built for a
    newer .NET than your project targets (the <TargetFramework> line), your model will not build
    until the line matches, and this script says so. Without this switch it then ASKS before
    changing anything, and if it cannot ask - no console, as when an AI assistant runs it - it
    changes nothing. With it, the target value is changed and every other byte of the file is left
    exactly as it was. Your project file is yours: nothing here edits it without your say-so.

.PARAMETER RefsFolder
    Where to copy FROM. Defaults to this Assistant's own refs\, which is the right answer; the
    switch exists so the script can be pointed at a known-good folder while diagnosing something.

.EXAMPLE
    .\scripts\refresh-model-refs.ps1 -Project ..\MyRoadModel

.EXAMPLE
    .\scripts\refresh-model-refs.ps1 -Project C:\Work\MyRoadModel

.EXAMPLE
    .\scripts\refresh-model-refs.ps1 -Project ..\MyRoadModel -UpdateTargetFramework
#>

[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$Project,

    [string]$RefsFolder,

    [switch]$UpdateTargetFramework
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
if (-not $RefsFolder) { $RefsFolder = Join-Path $repoRoot 'refs' }

# ---------------------------------------------------------------------------
# VALIDATE THE SOURCE COMPLETELY BEFORE TOUCHING THE TARGET.
#
# This script deletes a folder. Every reason it might refuse is established
# first, so that a refusal always leaves the model exactly as it was rather
# than emptied and half-filled - which would be a worse state than the stale
# one it was called to fix.
# ---------------------------------------------------------------------------

if (-not (Test-Path -LiteralPath $RefsFolder -PathType Container)) {
    Write-Host ''
    Write-Host "No refs folder to copy from at: $RefsFolder" -ForegroundColor Red
    Write-Host 'refs\ is part of every release. Re-download the Assistant.' -ForegroundColor Red
    Write-Host ''
    exit 2
}

$RefsFolder = (Resolve-Path -LiteralPath $RefsFolder).Path

$sourceDlls = @(Get-ChildItem -LiteralPath $RefsFolder -Filter '*.dll' -File)
if ($sourceDlls.Count -eq 0) {
    Write-Host ''
    Write-Host "No framework assemblies in: $RefsFolder" -ForegroundColor Red
    Write-Host 'Re-download the Assistant - refs\ should never be empty.' -ForegroundColor Red
    Write-Host ''
    exit 2
}

$missingDocs = @($sourceDlls | Where-Object {
    -not (Test-Path -LiteralPath (Join-Path $RefsFolder ($_.BaseName + '.xml')))
})
if ($missingDocs.Count -gt 0) {
    Write-Host ''
    Write-Host "REFUSING TO COPY. $($RefsFolder)" -ForegroundColor Red
    Write-Host "is missing the .xml documentation file for: $(($missingDocs.BaseName) -join ', ')" -ForegroundColor Red
    Write-Host ''
    Write-Host 'Those .xml files are what give you framework descriptions in IntelliSense, and what' -ForegroundColor Red
    Write-Host 'an AI coding assistant reads to learn the framework API. A refs folder without them' -ForegroundColor Red
    Write-Host 'builds perfectly and silently costs you most of what this Assistant is for, so this' -ForegroundColor Red
    Write-Host 'stops rather than copying half a folder into your model.' -ForegroundColor Red
    Write-Host ''
    Write-Host 'Your model has NOT been touched. Re-download the Assistant and run this again.' -ForegroundColor Red
    Write-Host ''
    exit 1
}

# ---------------------------------------------------------------------------
# VALIDATE THE TARGET. It must look like a model project.
#
# The whole point of the -Project switch is that it is a path the engineer
# typed, and ..\ is relative to a terminal folder they cannot see. Requiring a
# .csproj at the top is what stops a typo emptying the refs subfolder of
# something else entirely.
# ---------------------------------------------------------------------------

if (-not (Test-Path -LiteralPath $Project -PathType Container)) {
    Write-Host ''
    Write-Host "No folder at: $Project" -ForegroundColor Red
    Write-Host ''
    Write-Host 'The path is relative to the folder your terminal is in - run  pwd  to see which one' -ForegroundColor Red
    Write-Host 'that is. From the Assistant folder, your model is normally  ..\MyRoadModel .' -ForegroundColor Red
    Write-Host ''
    exit 2
}

$projectPath = (Resolve-Path -LiteralPath $Project).Path

# This one is checked before the .csproj test, not after it. The Assistant root holds no
# .csproj, so the generic "that is not a model project" message would otherwise be the only
# thing an engineer who pointed -Project at the Assistant ever saw.
if ($projectPath -eq $repoRoot) {
    Write-Host ''
    Write-Host 'That is the Assistant folder itself, not a model.' -ForegroundColor Red
    Write-Host 'Your model is a separate folder beside this one - normally  ..\MyRoadModel .' -ForegroundColor Red
    Write-Host ''
    exit 2
}

$csprojFiles = @(Get-ChildItem -LiteralPath $projectPath -Filter '*.csproj' -File)
if ($csprojFiles.Count -eq 0) {
    Write-Host ''
    Write-Host "That folder holds no .csproj file, so it is not a model project:" -ForegroundColor Red
    Write-Host "  $projectPath" -ForegroundColor Red
    Write-Host ''
    Write-Host 'Point -Project at the folder that has your <ModelName>.csproj in it. Nothing was' -ForegroundColor Red
    Write-Host 'changed.' -ForegroundColor Red
    Write-Host ''
    exit 2
}

$target = Join-Path $projectPath 'refs'

# ---------------------------------------------------------------------------
# AND CHECK THE FOLDER CAN ACTUALLY BE EMPTIED, BEFORE EMPTYING ANY OF IT.
#
# Remove-Item -Recurse deletes files one at a time and stops dead at the first
# one something else has open - which on Windows is the ordinary state of a
# refs\ folder whenever VS Code has the model loaded, because the C# extension
# holds the assemblies it is reading. Without this the folder is left half
# deleted and the model no longer builds, which is precisely the outcome every
# other refusal in this script is written to avoid.
# ---------------------------------------------------------------------------

if (Test-Path -LiteralPath $target -PathType Container) {
    $heldOpen = @()
    foreach ($file in Get-ChildItem -LiteralPath $target -File -Recurse) {
        try {
            $handle = [System.IO.File]::Open($file.FullName, 'Open', 'Read', 'None')
            $handle.Close()
        }
        catch {
            $heldOpen += $file.Name
        }
    }

    if ($heldOpen.Count -gt 0) {
        Write-Host ''
        Write-Host 'REFUSING TO REPLACE. Something else has these files open:' -ForegroundColor Red
        Write-Host ("  {0}" -f ((@($heldOpen) | Select-Object -Unique) -join ', ')) -ForegroundColor Red
        Write-Host ''
        Write-Host 'Windows will not let the folder be emptied while they are held, and emptying half of' -ForegroundColor Red
        Write-Host 'it would leave your model unable to build - worse than the stale state this was' -ForegroundColor Red
        Write-Host 'called to fix. Close VS Code, all of it, and any terminal sitting inside your model' -ForegroundColor Red
        Write-Host 'folder. Then run this again.' -ForegroundColor Red
        Write-Host ''
        Write-Host 'Your model has NOT been touched.' -ForegroundColor Red
        Write-Host ''
        exit 1
    }
}

Write-Host ''
Write-Host 'Refreshing framework reference assemblies'
Write-Host "  from : $RefsFolder"
Write-Host "  into : $target"
Write-Host ''

# ---------------------------------------------------------------------------
# WHAT IS BEING REPLACED, said out loud before it goes.
# ---------------------------------------------------------------------------

if (Test-Path -LiteralPath $target -PathType Container) {
    $existingDlls = @(Get-ChildItem -LiteralPath $target -Filter '*.dll' -File)
    if ($existingDlls.Count -gt 0) {
        $oldShas = @{}
        foreach ($assembly in $existingDlls) {
            $productVersion = $assembly.VersionInfo.ProductVersion
            $sha = if ($productVersion -match '\+([0-9a-f]{7,40})') { $Matches[1] } else { '(no commit SHA)' }
            $oldShas[$sha] = $true
        }
        Write-Host ("Replacing {0} assembly/assemblies built from: {1}" -f $existingDlls.Count, (@($oldShas.Keys) -join ', '))
    }

    # The probe above covers everything that was open a moment ago. If something grabbed a file
    # in between, say what happened in words rather than letting a raw PowerShell error land in
    # front of a civil engineer - and be honest that the folder is now incomplete.
    try {
        Remove-Item -LiteralPath $target -Recurse -Force
    }
    catch {
        Write-Host ''
        Write-Host 'THE FOLDER COULD NOT BE EMPTIED, and it is now incomplete.' -ForegroundColor Red
        Write-Host ("  {0}" -f $_.Exception.Message) -ForegroundColor Red
        Write-Host ''
        Write-Host 'Something opened a file while this was running. Close VS Code, all of it, and any' -ForegroundColor Red
        Write-Host 'terminal sitting inside your model folder, then run this again - a second run will' -ForegroundColor Red
        Write-Host 'finish the job and your model will build.' -ForegroundColor Red
        Write-Host ''
        exit 1
    }
}

New-Item -ItemType Directory -Path $target -Force | Out-Null

# ---------------------------------------------------------------------------
# COPY EVERYTHING EXCEPT SYMBOLS.
#
# Not just the .dll and .xml files: FRAMEWORK-VERSION.txt goes across too, and
# so does the folder's README. The stamp is what check-framework-version.ps1
# reads to work out how old a snapshot is, so a refs folder without it can be
# reported on but not aged. .pdb files are skipped for the same reason nothing
# else carries them - there are no method bodies to step into.
# ---------------------------------------------------------------------------

$copied = 0
foreach ($file in Get-ChildItem -LiteralPath $RefsFolder -File) {
    if ($file.Extension -ieq '.pdb') { continue }
    Copy-Item -LiteralPath $file.FullName -Destination (Join-Path $target $file.Name) -Force
    $copied++
}

# ---------------------------------------------------------------------------
# VERIFY WHAT LANDED, rather than trusting that the copy loop ran.
# ---------------------------------------------------------------------------

$landedDlls = @(Get-ChildItem -LiteralPath $target -Filter '*.dll' -File)
$landedMissingDocs = @($landedDlls | Where-Object {
    -not (Test-Path -LiteralPath (Join-Path $target ($_.BaseName + '.xml')))
})

if ($landedDlls.Count -ne $sourceDlls.Count -or $landedMissingDocs.Count -gt 0) {
    Write-Host ''
    Write-Host 'THE COPY DID NOT COMPLETE.' -ForegroundColor Red
    Write-Host ("  expected {0} assemblies, found {1}" -f $sourceDlls.Count, $landedDlls.Count) -ForegroundColor Red
    if ($landedMissingDocs.Count -gt 0) {
        Write-Host ("  missing documentation for: {0}" -f (($landedMissingDocs.BaseName) -join ', ')) -ForegroundColor Red
    }
    Write-Host ''
    Write-Host 'Your model will not build correctly against this folder. The usual cause is a file' -ForegroundColor Red
    Write-Host 'held open by something - close VS Code and run this again.' -ForegroundColor Red
    Write-Host ''
    exit 1
}

$newSha = '(no commit SHA)'
$productVersion = $landedDlls[0].VersionInfo.ProductVersion
if ($productVersion -match '\+([0-9a-f]{7,40})') { $newSha = $Matches[1] }

Write-Host ("Copied {0} files. {1} assemblies, all with documentation." -f $copied, $landedDlls.Count) -ForegroundColor Green
Write-Host "Framework commit : $newSha" -ForegroundColor Green
Write-Host ''

# ---------------------------------------------------------------------------
# DOES THE PROJECT TARGET THE .NET THESE REFERENCES WERE BUILT FOR?
#
# The references just copied say which .NET they were built for, on the
# "Build :" line of FRAMEWORK-VERSION.txt. A project on an older .NET cannot
# compile against them - the build stops with error CS1705 - so a plain
# "now rebuild" would send the engineer straight into an error they cannot
# read. jcass-dm check reports the same thing as a PROBLEM; this says it at
# the moment it becomes true.
#
# THE PROJECT FILE IS NEVER CHANGED WITHOUT PERMISSION. It is the engineer's
# file, not ours. Permission is either -UpdateTargetFramework, passed on
# purpose, or an explicit "y" at the prompt below. Anything else - no answer,
# no console to ask on, a different answer - leaves the file byte for byte as
# it was. When the edit is made it changes the target value and nothing else:
# same encoding, same line endings, every other byte untouched.
# ---------------------------------------------------------------------------

$targetMismatch = $false
$stampPath = Join-Path $target 'FRAMEWORK-VERSION.txt'
$refsTarget = $null
if (Test-Path -LiteralPath $stampPath) {
    foreach ($line in Get-Content -LiteralPath $stampPath) {
        if ($line -match '^\s*Build\s*:(.*)$') {
            if ($Matches[1] -match '\b(net\d+\.\d+)\b') { $refsTarget = $Matches[1].ToLowerInvariant() }
            break
        }
    }
}

# Two project files - typically a stray "MyModel - Copy.csproj" - mean there is no telling which one
# is the model, and "the first one" is whichever sorts first, which is usually the copy. Compare and
# edit nothing; jcass-dm check refuses two project files and says which.
$projectTarget = $null
if ($csprojFiles.Count -gt 1) {
    Write-Host "That folder holds $($csprojFiles.Count) .csproj files: $(($csprojFiles.Name) -join ', ')." -ForegroundColor Yellow
    Write-Host 'So this did not compare their .NET target with the references, and changed none of them.' -ForegroundColor Yellow
    Write-Host 'A model has exactly one. Remove the extra one, then run' -ForegroundColor Yellow
    Write-Host ("  .\tools\jcass-dm.exe check --project `"{0}`"" -f $projectPath) -ForegroundColor Yellow
    Write-Host ''
    $targetMismatch = $true
}
else {
    $projectFile = $csprojFiles[0].FullName
    $projectText = [System.IO.File]::ReadAllText($projectFile)
    $withoutComments = [regex]::Replace($projectText, '<!--.*?-->', '', 'Singleline')
    $targetMatch = [regex]::Match($withoutComments, '<TargetFramework>\s*([^<]*?)\s*</TargetFramework>', 'IgnoreCase')
    if ($targetMatch.Success -and $targetMatch.Groups[1].Value.Length -gt 0) { $projectTarget = $targetMatch.Groups[1].Value }
}

if ($refsTarget -and $projectTarget -and ($projectTarget -ne $refsTarget)) {
    $projectName = $csprojFiles[0].Name
    Write-Host "YOUR PROJECT TARGETS $projectTarget. These references are built for $refsTarget." -ForegroundColor Yellow
    Write-Host ''
    Write-Host 'It will not build until the two match: the compiler stops with error CS1705. Anything'
    Write-Host 'you have already published keeps running in the web app - this is about building on'
    Write-Host 'this machine. You also need the matching .NET SDK installed; check with'
    Write-Host '  dotnet --list-sdks'
    Write-Host ''
    Write-Host "The fix is one line in ${projectName}:"
    Write-Host "  <TargetFramework>$projectTarget</TargetFramework>  ->  <TargetFramework>$refsTarget</TargetFramework>"
    Write-Host ''

    $permitted = [bool]$UpdateTargetFramework
    if (-not $permitted) {
        $canAsk = [Environment]::UserInteractive -and -not [Console]::IsInputRedirected
        if ($canAsk) {
            $answer = $null
            try { $answer = Read-Host "Change that line now? Nothing else in the file changes. [y/N]" }
            catch { $answer = $null }
            $permitted = ($answer -match '^\s*(y|yes)\s*$')
        }
    }

    if ($permitted) {
        $bytes = [System.IO.File]::ReadAllBytes($projectFile)
        $hasBom = $bytes.Length -ge 3 -and $bytes[0] -eq 0xEF -and $bytes[1] -eq 0xBB -and $bytes[2] -eq 0xBF

        # Replace the value inside the one element that was matched above, located in the
        # original text rather than the comment-stripped copy, so the offsets are real.
        $elementPattern = '(<TargetFramework>\s*)' + [regex]::Escape($projectTarget) + '(\s*</TargetFramework>)'
        $element = [regex]::Matches($projectText, $elementPattern, 'IgnoreCase') |
            Where-Object { -not ($projectText.Substring(0, $_.Index) -match '(?s)<!--(?:(?!-->).)*$') } |
            Select-Object -First 1

        if ($null -eq $element) {
            Write-Host "Could not find that line to change, so $projectName was left as it was. Change it by hand." -ForegroundColor Yellow
            $targetMismatch = $true
        }
        else {
            $replacement = $element.Groups[1].Value + $refsTarget + $element.Groups[2].Value
            $newText = $projectText.Substring(0, $element.Index) + $replacement +
                $projectText.Substring($element.Index + $element.Length)
            [System.IO.File]::WriteAllText($projectFile, $newText, (New-Object System.Text.UTF8Encoding($hasBom)))
            Write-Host "Changed $projectName to target $refsTarget. Nothing else in it was touched." -ForegroundColor Green
            Write-Host ''
        }
    }
    else {
        Write-Host "$projectName was NOT changed. Make that one edit yourself, or run this again with" -ForegroundColor Yellow
        Write-Host '-UpdateTargetFramework to have it made. Then rebuild, and run' -ForegroundColor Yellow
        Write-Host ("  .\tools\jcass-dm.exe check --project `"{0}`"" -f $projectPath) -ForegroundColor Yellow
        Write-Host ''
        $targetMismatch = $true
    }
}

if ($targetMismatch) {
    exit 0
}

Write-Host 'Now rebuild your model, so that anything the new framework changed shows up as a build'
Write-Host 'error here rather than as odd behaviour in the web app:'
Write-Host ''
Write-Host ("  dotnet build `"{0}`" -c Debug --no-incremental" -f $csprojFiles[0].FullName)
Write-Host ''

exit 0
