<#
.SYNOPSIS
    Normalizes an OpenAPI document that was already downloaded into openapi/<version>.json.

.DESCRIPTION
    Used by the deploy workflow, which takes the document the deployed site already serves
    (the same file its smoke test downloads) instead of starting a local copy of the API.
    The output is byte-stable across PowerShell hosts and uses LF line endings, so the
    committed file only changes when the API surface really changed.

.EXAMPLE
    .\scripts\Save-OpenApiDocument.ps1 -InputPath C:\temp\swagger.json -Version v1
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$InputPath,

    [string]$Version = 'v1'
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'StableJson.ps1')

$repoRoot = Split-Path -Parent $PSScriptRoot
# Join-Path per segment: a backslash inside a single segment is a literal character on Linux.
$outDir = Join-Path $repoRoot 'openapi'
$outFile = Join-Path $outDir "$Version.json"

if (-not (Test-Path -LiteralPath $InputPath)) {
    throw "OpenAPI document not found: $InputPath"
}

$content = [System.IO.File]::ReadAllText($InputPath, [System.Text.Encoding]::UTF8)
if ($content -notmatch '"openapi"') {
    throw "The downloaded document is not an OpenAPI document: $InputPath"
}

$document = $content | ConvertFrom-Json
$normalized = ((ConvertTo-StableJson -InputObject $document) -replace "`r`n", "`n") + "`n"

$utf8 = New-Object System.Text.UTF8Encoding($false)
$unchanged = $false
if (Test-Path -LiteralPath $outFile) {
    $sha = [System.Security.Cryptography.SHA256]::Create()
    try {
        $oldHash = [System.BitConverter]::ToString($sha.ComputeHash([System.IO.File]::ReadAllBytes($outFile)))
        $newHash = [System.BitConverter]::ToString($sha.ComputeHash($utf8.GetBytes($normalized)))
        $unchanged = $oldHash -eq $newHash
    } finally {
        $sha.Dispose()
    }
}

if (-not $unchanged) {
    if (-not (Test-Path -LiteralPath $outDir)) { New-Item -ItemType Directory -Path $outDir | Out-Null }
    [System.IO.File]::WriteAllText($outFile, $normalized, $utf8)
}

if ($unchanged) {
    Write-Host "openapi\$Version.json is already up to date"
} else {
    Write-Host "openapi\$Version.json updated from $InputPath"
}
