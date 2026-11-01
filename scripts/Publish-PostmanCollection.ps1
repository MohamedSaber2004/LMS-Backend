<#
.SYNOPSIS
    Publishes the generated Postman collection to Postman through the Postman API.

.DESCRIPTION
    Replaces the contents of an existing Postman collection with the generated file, so the
    team's collection changes the moment a deploy happens and nobody imports anything by hand.
    Monitors, mock servers and published documentation that use the collection keep working
    because the collection is updated in place instead of being recreated.

    Required environment variables (GitHub repository secrets):
      POSTMAN_API_KEY        Postman API key (Postman > Settings > API keys, or workspace > Integrations)
      POSTMAN_COLLECTION_ID  UID of the collection to update

    The key is never stored in a file, a workflow, or this script - only in the secret.

.EXAMPLE
    $env:POSTMAN_API_KEY='...'; $env:POSTMAN_COLLECTION_ID='...'; .\scripts\Publish-PostmanCollection.ps1
#>
[CmdletBinding()]
param(
    [string]$CollectionFile = '',
    [string]$ApiBaseUrl = 'https://api.getpostman.com'
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path -Parent $PSScriptRoot
# Join-Path per segment: a backslash inside a single segment is a literal character on Linux.
if (-not $CollectionFile) { $CollectionFile = Join-Path (Join-Path $repoRoot 'postman') 'LMS.postman_collection.json' }

$apiKey = $env:POSTMAN_API_KEY
$collectionId = $env:POSTMAN_COLLECTION_ID

if (-not $apiKey) {
    throw 'POSTMAN_API_KEY is not set. Add a Postman API key as a repository secret.'
}
if (-not $collectionId) {
    throw 'POSTMAN_COLLECTION_ID is not set. Create the collection once (Import > Link > the deployed swagger URL), then add its UID from the collection URL as a repository secret.'
}
if (-not (Test-Path -LiteralPath $CollectionFile)) {
    throw "Collection file not found: $CollectionFile. Run .\scripts\Update-PostmanCollection.ps1 first."
}

$collection = [System.IO.File]::ReadAllText($CollectionFile, [System.Text.Encoding]::UTF8) | ConvertFrom-Json
$payload = @{ collection = $collection } | ConvertTo-Json -Depth 100 -Compress

$headers = @{
    'X-Api-Key'  = $apiKey
    'Content-Type' = 'application/json'
}

Write-Host "Publishing '$($collection.info.name)' to Postman collection $collectionId..."
$response = Invoke-RestMethod -Method Put -Uri "$ApiBaseUrl/collections/$collectionId" -Headers $headers -Body $payload -ContentType 'application/json'

if ($response.collection.name) {
    Write-Host "Postman collection '$($response.collection.name)' is up to date (uid $($response.collection.uid))."
} else {
    Write-Host 'Postman collection updated.'
}
