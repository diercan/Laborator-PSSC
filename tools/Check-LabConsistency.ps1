#Requires -Version 7.0
<#
.SYNOPSIS
    Verifică faptul că fișierele partajate între laboratoare (nucleul funcțional, modelul de domeniu,
    testele, stratul de date, scriptul SQL, configurația de build) sunt identice sau, unde e intenționat,
    subseturi ale variantei "de aur" din ultimul laborator al setului.

.DESCRIPTION
    Citește tools/lab-consistency.json și, pentru fiecare set descris acolo, compară conținutul normalizat
    (UTF-8 fără BOM, terminatori de linie LF) al fișierelor text din fiecare laborator listat față de
    laboratorul de referință (ultimul din listă). Găsirea unei diferențe neautorizate (neprezentă în
    secțiunea "allow" din manifest) produce o eroare (exit code 1 în modul -CI).

.PARAMETER RepoRoot
    Rădăcina depozitului. Implicit, directorul părinte al folderului tools/.

.PARAMETER Manifest
    Calea către fișierul JSON cu descrierea seturilor. Implicit tools/lab-consistency.json.

.PARAMETER CI
    Dacă este prezent, scriptul se încheie cu exit code 1 atunci când există cel puțin o constatare
    neautorizată (util pentru integrarea continuă); altfel doar afișează tabelul de constatări.

.EXAMPLE
    pwsh tools/Check-LabConsistency.ps1
    pwsh tools/Check-LabConsistency.ps1 -CI
#>
[CmdletBinding()]
param(
    [string]$RepoRoot = (Split-Path -Parent $PSScriptRoot),
    [string]$Manifest = (Join-Path $PSScriptRoot 'lab-consistency.json'),
    [switch]$CI
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$manifestContent = Get-Content -LiteralPath $Manifest -Raw | ConvertFrom-Json
$excludePatterns = $manifestContent.exclude

function Test-ExcludedPath {
    param([string]$RelativePath)
    foreach ($pattern in $excludePatterns) {
        if ($RelativePath -like "*$pattern*") { return $true }
    }
    return $false
}

$textExtensions = '.cs', '.csproj', '.props', '.targets', '.json', '.sql', '.slnx', '.md'

function Get-NormalizedHash {
    param([string]$FilePath)
    $extension = [System.IO.Path]::GetExtension($FilePath).ToLowerInvariant()
    $bytes = [System.IO.File]::ReadAllBytes($FilePath)
    if ($textExtensions -contains $extension) {
        $text = [System.Text.Encoding]::UTF8.GetString($bytes).TrimStart([char]0xFEFF)
        $text = $text -replace "`r`n", "`n"
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($text)
    }
    $sha256 = [System.Security.Cryptography.SHA256]::Create()
    try { return [Convert]::ToHexString($sha256.ComputeHash($bytes)) }
    finally { $sha256.Dispose() }
}

function Get-LabFiles {
    param([string]$LabRoot)
    if (-not (Test-Path -LiteralPath $LabRoot)) { return @{} }
    $result = @{}
    Get-ChildItem -LiteralPath $LabRoot -Recurse -File | ForEach-Object {
        $relative = [System.IO.Path]::GetRelativePath($LabRoot, $_.FullName) -replace '\\', '/'
        if (-not (Test-ExcludedPath $relative)) {
            $result[$relative] = $_.FullName
        }
    }
    return $result
}

function Test-Allowed {
    param([string]$SetName, [string]$Lab, [string]$RelativeFile)
    foreach ($entry in $manifestContent.allow) {
        if ($entry.set -ne $SetName) { continue }
        if ($entry.labs -notcontains $Lab) { continue }
        if ($entry.file -eq '*' -or $entry.file -eq $RelativeFile) { return $entry.reason }
    }
    return $null
}

$findings = [System.Collections.Generic.List[pscustomobject]]::new()
$usedAllowEntries = [System.Collections.Generic.HashSet[string]]::new()

foreach ($set in $manifestContent.sets) {
    $labs = $set.labs
    $referenceLab = $labs[-1]
    $referenceRoot = Join-Path $RepoRoot "Lucrarea-$referenceLab" $set.path
    $referenceFiles = Get-LabFiles -LabRoot $referenceRoot
    $referenceHashes = @{}
    foreach ($kv in $referenceFiles.GetEnumerator()) {
        $referenceHashes[$kv.Key] = Get-NormalizedHash -FilePath $kv.Value
    }

    foreach ($lab in $labs) {
        if ($lab -eq $referenceLab) { continue }
        $labRoot = Join-Path $RepoRoot "Lucrarea-$lab" $set.path
        $labFiles = Get-LabFiles -LabRoot $labRoot

        # Fișiere lipsă în laboratorul curent față de referință.
        foreach ($relative in $referenceHashes.Keys) {
            if (-not $labFiles.ContainsKey($relative)) {
                $reason = Test-Allowed -SetName $set.name -Lab $lab -RelativeFile $relative
                if ($reason) { $usedAllowEntries.Add("$($set.name)|$lab|$relative") | Out-Null; continue }
                $findings.Add([pscustomobject]@{ Set = $set.name; Lab = $lab; File = $relative; Status = 'MISSING' })
            }
        }

        foreach ($relative in $labFiles.Keys) {
            $labHash = Get-NormalizedHash -FilePath $labFiles[$relative]

            if (-not $referenceHashes.ContainsKey($relative)) {
                # Fișier suplimentar față de referință: permis doar în seturile "subset" (laboratorul mai mic
                # poate avea mai puțin, nu mai mult) — pentru "identical" e o abatere.
                if ($set.mode -eq 'subset') { continue }
                $reason = Test-Allowed -SetName $set.name -Lab $lab -RelativeFile $relative
                if ($reason) { $usedAllowEntries.Add("$($set.name)|$lab|$relative") | Out-Null; continue }
                $findings.Add([pscustomobject]@{ Set = $set.name; Lab = $lab; File = $relative; Status = 'EXTRA' })
                continue
            }

            if ($labHash -ne $referenceHashes[$relative]) {
                $reason = Test-Allowed -SetName $set.name -Lab $lab -RelativeFile $relative
                if ($reason) { $usedAllowEntries.Add("$($set.name)|$lab|$relative") | Out-Null; continue }
                $findings.Add([pscustomobject]@{ Set = $set.name; Lab = $lab; File = $relative; Status = 'DIFF' })
            }
        }
    }
}

# Semnalează intrările din "allow" care nu au mai fost folosite (curățenie a manifestului).
foreach ($entry in $manifestContent.allow) {
    foreach ($lab in $entry.labs) {
        if ($entry.file -eq '*') { continue } # intrările cu wildcard nu pot fi urmărite individual
        $key = "$($entry.set)|$lab|$($entry.file)"
        if (-not $usedAllowEntries.Contains($key)) {
            Write-Warning "Intrare 'allow' neutilizată (posibil învechită): set=$($entry.set) lab=$lab fișier=$($entry.file)"
        }
    }
}

if ($findings.Count -eq 0) {
    Write-Host "Consistență OK: niciun fișier partajat nu diferă neautorizat." -ForegroundColor Green
    exit 0
}

$findings | Sort-Object Set, Lab, File | Format-Table -AutoSize
Write-Host ""
Write-Host "$($findings.Count) constatare(-ări) neautorizată(-e). Vezi tools/lab-consistency.json pentru a adăuga o excepție intenționată." -ForegroundColor Yellow

if ($CI) { exit 1 }
exit 0
