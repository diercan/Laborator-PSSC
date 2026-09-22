#Requires -Version 7.0
<#
.SYNOPSIS
    Propagă fișierele unui set partajat (vezi tools/lab-consistency.json) de la un laborator sursă către
    unul sau mai multe laboratoare țintă.

.DESCRIPTION
    Pentru fiecare set din manifest care conține atât -Source cât și un laborator țintă, copiază fișierele
    care diferă (după conținut normalizat) din laboratorul sursă către țintă. Pentru seturile "subset"
    (de exemplu Domain-types din Lucrarea 2), nu adaugă niciodată fișiere absente din țintă — doar
    actualizează conținutul celor deja existente acolo, ca să nu strice sfera redusă intenționat a
    laboratorului mai mic. Cu -Mirror, șterge și fișierele din țintă care nu (mai) există în sursă
    (permis doar pentru seturi "identical").

.PARAMETER Source
    Numărul laboratorului sursă (de exemplu "08").

.PARAMETER Target
    Unul sau mai multe numere de laborator țintă (de exemplu "05","06","07").

.PARAMETER Set
    Opțional: limitează operația la seturile numite (implicit toate seturile care includ atât sursa cât
    și cel puțin o țintă).

.PARAMETER Mirror
    Șterge din țintă fișierele care nu există în sursă (doar pentru seturi "identical").

.EXAMPLE
    pwsh tools/Sync-Labs.ps1 -Source 08 -Target 07,06,05 -WhatIf
    pwsh tools/Sync-Labs.ps1 -Source 08 -Target 07 -Set Data

.NOTES
    Puneți numerele laboratoarelor între ghilimele: -Target "07","06","05". Fără ghilimele, PowerShell
    interpretează "07" ca literalul numeric 7 (zero-ul din față dispare la conversia în șir), iar
    "Lucrarea-7" nu există — scriptul rulează fără nicio eroare, dar nu copiază nimic.
#>
[CmdletBinding(SupportsShouldProcess)]
param(
    [Parameter(Mandatory)][string]$Source,
    [Parameter(Mandatory)][string[]]$Target,
    [string[]]$Set,
    [switch]$Mirror,
    [string]$RepoRoot = (Split-Path -Parent $PSScriptRoot),
    [string]$Manifest = (Join-Path $PSScriptRoot 'lab-consistency.json')
)

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$manifestContent = Get-Content -LiteralPath $Manifest -Raw | ConvertFrom-Json
$excludePatterns = $manifestContent.exclude

# Verificare explicită înainte de orice: dacă -Source/-Target au fost date fără ghilimele ("07" devine
# literalul 7, apoi șirul "7"), folderul "Lucrarea-7" nu există și, altfel, scriptul ar rula fără eroare
# fără să copieze nimic. Eșuăm zgomotos aici în loc să eșuăm tăcut mai jos.
foreach ($lab in (@($Source) + $Target)) {
    $labRoot = Join-Path $RepoRoot "Lucrarea-$lab"
    if (-not (Test-Path -LiteralPath $labRoot)) {
        throw "Nu găsesc '$labRoot'. Dacă ați scris -Source/-Target fără ghilimele (ex. -Target 07,06), " +
              "PowerShell le interpretează ca numere și pierd zero-ul din față — folosiți -Target `"07`",`"06`"."
    }
}

function Test-ExcludedPath {
    param([string]$RelativePath)
    foreach ($pattern in $excludePatterns) {
        if ($RelativePath -like "*$pattern*") { return $true }
    }
    return $false
}

function Get-LabFiles {
    param([string]$LabRoot)
    if (-not (Test-Path -LiteralPath $LabRoot)) { return @{} }
    $result = @{}
    Get-ChildItem -LiteralPath $LabRoot -Recurse -File | ForEach-Object {
        $relative = [System.IO.Path]::GetRelativePath($LabRoot, $_.FullName) -replace '\\', '/'
        if (-not (Test-ExcludedPath $relative)) { $result[$relative] = $_.FullName }
    }
    return $result
}

foreach ($setDef in $manifestContent.sets) {
    if ($Set -and $setDef.name -notin $Set) { continue }
    if ($setDef.labs -notcontains $Source) { continue }

    $sourceRoot = Join-Path $RepoRoot "Lucrarea-$Source" $setDef.path
    $sourceFiles = Get-LabFiles -LabRoot $sourceRoot
    if ($sourceFiles.Count -eq 0) {
        Write-Warning "Setul '$($setDef.name)' nu are fișiere în Lucrarea $Source (cale: $sourceRoot) — sărit."
        continue
    }

    foreach ($targetLab in $Target) {
        if ($setDef.labs -notcontains $targetLab) { continue }
        if ($targetLab -eq $Source) { continue }

        $targetRoot = Join-Path $RepoRoot "Lucrarea-$targetLab" $setDef.path
        $targetFiles = Get-LabFiles -LabRoot $targetRoot
        $isSubset = $setDef.mode -eq 'subset'

        foreach ($relative in $sourceFiles.Keys) {
            $targetPath = Join-Path $targetRoot ($relative -replace '/', [System.IO.Path]::DirectorySeparatorChar)
            $existsInTarget = $targetFiles.ContainsKey($relative)

            if ($isSubset -and -not $existsInTarget) {
                # Nu adăugăm fișiere noi într-un set "subset": țintele mai mici rămân intenționat mai mici.
                continue
            }

            $sourceBytes = [System.IO.File]::ReadAllBytes($sourceFiles[$relative])
            $shouldCopy = $true
            if ($existsInTarget) {
                $targetBytes = [System.IO.File]::ReadAllBytes($targetFiles[$relative])
                $shouldCopy = -not [System.Linq.Enumerable]::SequenceEqual($sourceBytes, $targetBytes)
            }

            if ($shouldCopy -and $PSCmdlet.ShouldProcess($targetPath, "Copiază din Lucrarea $Source ($($setDef.name))")) {
                $targetDir = Split-Path -Parent $targetPath
                if (-not (Test-Path -LiteralPath $targetDir)) { New-Item -ItemType Directory -Path $targetDir -Force | Out-Null }
                [System.IO.File]::WriteAllBytes($targetPath, $sourceBytes)
            }
        }

        if ($Mirror -and -not $isSubset) {
            foreach ($relative in $targetFiles.Keys) {
                if (-not $sourceFiles.ContainsKey($relative)) {
                    if ($PSCmdlet.ShouldProcess($targetFiles[$relative], "Șterge (nu există în Lucrarea $Source)")) {
                        Remove-Item -LiteralPath $targetFiles[$relative] -Force
                    }
                }
            }
        }
    }
}

Write-Host "Gata. Rulați 'pwsh tools/Check-LabConsistency.ps1' ca să confirmați consistența." -ForegroundColor Cyan
