<#
.SYNOPSIS
    Converts openapi/v1.json into the tracked Postman collection.

.DESCRIPTION
    The output is a generated artifact. Never hand-edit postman\LMS.postman_collection.json:
    every regeneration overwrites it. Team members get their editable copy from the Postman
    team workspace, which Postman keeps in sync with openapi\v1.json.

.EXAMPLE
    .\scripts\Update-PostmanCollection.ps1
#>
[CmdletBinding()]
param(
    [string]$Version = 'v1',
    [string]$BaseUrl = 'https://lms-api.runasp.net'
)

$ErrorActionPreference = 'Stop'

if ($BaseUrl -notmatch '^https?://') {
    throw "BaseUrl must be an absolute http(s) URL, got '$BaseUrl'. Pass -BaseUrl https://your-site, otherwise the generated collection would point at an unusable host."
}

function Count-Requests {
    param($Items)

    $count = 0
    foreach ($item in $Items) {
        if ($null -ne $item.PSObject.Properties['item'] -and $item.item) { $count += Count-Requests $item.item }
        elseif ($null -ne $item.PSObject.Properties['request']) { $count++ }
    }
    return $count
}

$repoRoot = Split-Path -Parent $PSScriptRoot
# Join-Path per segment: a backslash inside a single segment is a literal character on Linux.
$specFile = Join-Path (Join-Path $repoRoot 'openapi') "$Version.json"
$outFile = Join-Path (Join-Path $repoRoot 'postman') 'LMS.postman_collection.json'

if (-not (Test-Path -LiteralPath $specFile)) {
    throw "OpenAPI document not found: $specFile. Run .\scripts\Update-OpenApi.ps1 first."
}

& (Join-Path $PSScriptRoot 'Convert-OpenApiToPostman.ps1') -SpecFile $specFile -OutFile $outFile -BaseUrl $BaseUrl

$collection = [System.IO.File]::ReadAllText($outFile, [System.Text.Encoding]::UTF8) | ConvertFrom-Json
Write-Host "postman\LMS.postman_collection.json - '$($collection.info.name)' with $(Count-Requests $collection.item) requests"

git -C $repoRoot --no-pager diff --stat -- postman
