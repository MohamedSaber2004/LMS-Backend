<#
.SYNOPSIS
    Verifies that a Postman collection covers every operation in the OpenAPI document.

.DESCRIPTION
    Turns "is Postman up to date?" into a check instead of a guess. Every path+method in the
    OpenAPI document is looked up in the collection by method and URL (path parameters are
    compared as {name} on both sides), and any missing operation is reported.

    Two modes:
      (default)     compare the document with the generated file postman\LMS.postman_collection.json
      -FromPostman  compare the document with the collection stored in Postman (needs
                    POSTMAN_API_KEY and POSTMAN_COLLECTION_ID)

.EXAMPLE
    .\scripts\Verify-PostmanCollection.ps1
.EXAMPLE
    .\scripts\Verify-PostmanCollection.ps1 -FromPostman
#>
[CmdletBinding()]
param(
    [string]$SpecFile = '',
    [string]$CollectionFile = '',
    [switch]$FromPostman,
    [string]$ApiBaseUrl = 'https://api.getpostman.com'
)

$ErrorActionPreference = 'Stop'

function Get-CollectionName {
    param($Collection)

    if ($null -ne $Collection.PSObject.Properties['info'] -and $Collection.info.PSObject.Properties['name']) {
        return [string]$Collection.info.name
    }
    if ($null -ne $Collection.PSObject.Properties['name']) { return [string]$Collection.name }
    return 'unnamed'
}

function Get-NormalizedPath {
    <#
        Reduces a Postman URL to the path shape used by the OpenAPI document:
        the base URL variable is dropped, path variables become {name} no matter whether
        Postman wrote {{name}}, {name} or :name, and the query string is removed because
        query parameters are not part of an operation's identity.
    #>
    param($Url)

    if ($null -eq $Url) { return '' }

    $segments = @()
    if ($Url -is [string]) {
        $raw = $Url
    } elseif ($null -ne $Url.PSObject.Properties['path'] -and @($Url.path).Count -gt 0) {
        $raw = @($Url.path) -join '/'
    } elseif ($null -ne $Url.PSObject.Properties['raw']) {
        $raw = [string]$Url.raw
    } else {
        $raw = [string]$Url
    }

    $raw = ($raw -split '\?')[0]
    $raw = $raw -replace '^[a-zA-Z][a-zA-Z0-9+.-]*://[^/]*', ''          # absolute URL: drop scheme and host
    foreach ($segment in $raw.Split('/')) {
        if (-not $segment) { continue }
        if ($segment -match '(?i)^\{\{(?:baseUrl|base_url|host|siteUrl|site_url)\}\}$') { continue }                  # the baseUrl variable
        $segments += ($segment -replace '^\{\{([^}]+)\}\}$', '{$1}' -replace '^:([A-Za-z0-9_]+)$', '{$1}')
    }

    return '/' + ($segments -join '/')
}

function Get-Requests {
    param($Items)

    $requests = @()
    foreach ($item in $Items) {
        if ($null -ne $item.PSObject.Properties['item'] -and $item.item) {
            $requests += Get-Requests $item.item
        } elseif ($null -ne $item.PSObject.Properties['request']) {
            $requests += [PSCustomObject]@{
                Key     = "$($item.request.method.ToUpper()) $(Get-NormalizedPath $item.request.url)"
                Summary = [string]$item.name
            }
        }
    }
    return $requests
}

$repoRoot = Split-Path -Parent $PSScriptRoot
# Join-Path per segment: a backslash inside a single segment is a literal character on Linux.
if (-not $SpecFile) { $SpecFile = Join-Path (Join-Path $repoRoot 'openapi') 'v1.json' }
if (-not $CollectionFile) { $CollectionFile = Join-Path (Join-Path $repoRoot 'postman') 'LMS.postman_collection.json' }

if (-not (Test-Path -LiteralPath $SpecFile)) { throw "OpenAPI document not found: $SpecFile" }

$spec = [System.IO.File]::ReadAllText($SpecFile, [System.Text.Encoding]::UTF8) | ConvertFrom-Json
$expected = @()
foreach ($pathProperty in $spec.paths.PSObject.Properties) {
    # "/" is the browser redirect to the Swagger UI (Program.cs), not an API operation.
    if ($pathProperty.Name -eq '/') { continue }

    foreach ($property in $pathProperty.Value.PSObject.Properties) {
        $method = $property.Name.ToUpper()
        if ($method -notin @('GET', 'POST', 'PUT', 'PATCH', 'DELETE', 'HEAD', 'OPTIONS', 'TRACE')) { continue }
        $expected += [PSCustomObject]@{
            Key     = "$method $($pathProperty.Name)"
            Summary = [string]$property.Value.summary
        }
    }
}

if ($FromPostman) {
    $apiKey = $env:POSTMAN_API_KEY
    $collectionId = $env:POSTMAN_COLLECTION_ID
    if (-not $apiKey -or -not $collectionId) {
        throw 'POSTMAN_API_KEY and POSTMAN_COLLECTION_ID are required with -FromPostman.'
    }
    Write-Host "Reading collection $collectionId from Postman..."
    $response = Invoke-RestMethod -Method Get -Uri "$ApiBaseUrl/collections/$collectionId" `
        -Headers @{ 'X-Api-Key' = $apiKey }
    $collection = $response.collection
    $source = "Postman collection '$((Get-CollectionName $collection))'"
} else {
    if (-not (Test-Path -LiteralPath $CollectionFile)) { throw "Collection file not found: $CollectionFile" }
    $collection = [System.IO.File]::ReadAllText($CollectionFile, [System.Text.Encoding]::UTF8) | ConvertFrom-Json
    $source = $CollectionFile
}

$actual = Get-Requests $collection.item
Write-Host "$source holds $($actual.Count) requests; the document declares $($expected.Count) operations."

$missing = @($expected | Where-Object { $_.Key -notin $actual.Key })
$unexpected = @($actual | Where-Object { $_.Key -notin $expected.Key })

foreach ($item in $unexpected) {
    Write-Host "note: $source has $($item.Key) which the document does not declare ($($item.Summary))"
}

if ($missing.Count -gt 0) {
    foreach ($item in $missing) {
        Write-Host "::error::missing from ${source}: $($item.Key) - $($item.Summary)"
    }
    throw "$($missing.Count) operation(s) from the OpenAPI document are missing."
}

Write-Host "All $($expected.Count) operations are present."
