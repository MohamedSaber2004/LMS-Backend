<#
.SYNOPSIS
    Converts an OpenAPI 3 document into a Postman collection v2.1 file.

.DESCRIPTION
    Self-contained replacement for the npm openapi-to-postman package, which is not
    available in every npm registry. Produces a deterministic file: the same document
    always yields byte-identical output, so regeneration never creates noisy diffs.

    Supported: OpenAPI 3.x, security schemes (apiKey/bearer), path-level and
    operation-level parameters ($ref resolved), request bodies with examples generated
    from the JSON schema ($ref, allOf, enum, default, format), multipart/form-data and
    x-www-form-urlencoded bodies as Postman form fields (file fields included), response
    codes, and tags mapped to folders.

.EXAMPLE
    .\scripts\Convert-OpenApiToPostman.ps1 -SpecFile .\openapi\v1.json -OutFile .\postman\LMS.postman_collection.json
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$SpecFile,

    [Parameter(Mandatory = $true)]
    [string]$OutFile,

    [string]$BaseUrl = 'https://lms-api.runasp.net',
    [string]$BaseUrlVariable = 'baseUrl',
    [string]$TokenVariable = 'token',
    [string]$UntaggedFolder = 'Untagged'
)

$ErrorActionPreference = 'Stop'
$script:httpMethods = @('get', 'post', 'put', 'patch', 'delete', 'head', 'options', 'trace')

. (Join-Path $PSScriptRoot 'StableJson.ps1')

if (-not (Test-Path -LiteralPath $SpecFile)) {
    throw "OpenAPI document not found: $SpecFile"
}

$script:spec = [System.IO.File]::ReadAllText($SpecFile, [System.Text.Encoding]::UTF8) | ConvertFrom-Json
if (-not $script:spec.openapi) {
    throw "Not an OpenAPI 3 document (missing 'openapi' field): $SpecFile"
}

function Resolve-Schema {
    param($Schema)

    $resolved = $Schema
    $guard = 0
    while ($null -ne $resolved -and $null -ne $resolved.PSObject.Properties['$ref'] -and $guard -lt 10) {
        $ref = [string]$resolved.'$ref'
        if (-not $ref.StartsWith('#/')) { return $resolved }

        $target = $script:spec
        foreach ($segment in $ref.Substring(2).Split('/')) {
            $name = $segment.Replace('~1', '/').Replace('~0', '~')
            $property = $target.PSObject.Properties[$name]
            if ($null -eq $property) { return $resolved }
            $target = $property.Value
        }

        $merged = [ordered]@{}
        if ($null -ne $target) {
            foreach ($property in $target.PSObject.Properties) { $merged[$property.Name] = $property.Value }
        }
        foreach ($property in $resolved.PSObject.Properties) {
            if ($property.Name -ne '$ref') { $merged[$property.Name] = $property.Value }
        }

        $resolved = [PSCustomObject]$merged
        $guard++
    }

    return $resolved
}

function Merge-Schema {
    param($Schema)

    $merged = [ordered]@{}
    $propertyBag = [ordered]@{}
    $required = @()
    $parts = if ($null -ne $Schema.PSObject.Properties['allOf']) { @($Schema.allOf) } else { @($Schema) }

    foreach ($part in $parts) {
        $resolved = Resolve-Schema $part
        if ($null -eq $resolved) { continue }

        foreach ($property in $resolved.PSObject.Properties) {
            if ($property.Name -eq 'properties') {
                foreach ($child in $property.Value.PSObject.Properties) { $propertyBag[$child.Name] = $child.Value }
            } elseif ($property.Name -eq 'required') {
                $required += @($property.Value)
            } else {
                $merged[$property.Name] = $property.Value
            }
        }
    }

    foreach ($property in $Schema.PSObject.Properties) {
        if ($property.Name -in @('allOf', 'properties', 'required')) { continue }
        if (-not $merged.Contains($property.Name)) { $merged[$property.Name] = $property.Value }
    }

    if ($propertyBag.Count -gt 0) { $merged['properties'] = [PSCustomObject]$propertyBag }
    if ($required.Count -gt 0) { $merged['required'] = @($required | Select-Object -Unique) }

    return [PSCustomObject]$merged
}

function Get-SampleString {
    param($Schema)

    switch ([string]$Schema.format) {
        'date-time' { return '2026-01-01T00:00:00Z' }
        'date' { return '2026-01-01' }
        'time' { return '00:00:00' }
        'email' { return 'user@example.com' }
        'uuid' { return '00000000-0000-0000-0000-000000000000' }
        'uri' { return 'https://example.com' }
        'uri-reference' { return '/relative/path' }
        'byte' { return 'bGFuZHVhZ2U=' }
        'password' { return 'Pa$$w0rd' }
        default { return 'string' }
    }
}

function Get-SchemaExample {
    param($Schema, [int]$Depth = 0)

    if ($null -eq $Schema -or $Depth -gt 8) { return $null }

    $resolved = Merge-Schema $Schema
    if ($null -eq $resolved) { return $null }

    if ($null -ne $resolved.PSObject.Properties['example']) { return $resolved.example }
    if ($null -ne $resolved.PSObject.Properties['default']) { return $resolved.default }
    if ($null -ne $resolved.PSObject.Properties['enum'] -and @($resolved.enum).Count -gt 0) { return @($resolved.enum)[0] }

    $type = [string]$resolved.type
    if ($type -eq 'array' -and @($type) -contains 'null') { return $null }

    if ($type -eq 'object' -or $null -ne $resolved.PSObject.Properties['properties']) {
        $result = [ordered]@{}
        if ($null -ne $resolved.PSObject.Properties['properties']) {
            foreach ($property in $resolved.properties.PSObject.Properties) {
                $result[$property.Name] = Get-SchemaExample -Schema $property.Value -Depth ($Depth + 1)
            }
        }
        return [PSCustomObject]$result
    }

    if ($type -eq 'array') {
        $item = Get-SchemaExample -Schema $resolved.items -Depth ($Depth + 1)
        return , @($item)
    }

    switch ($type) {
        'integer' { return 0 }
        'number' { return 0 }
        'boolean' { return $false }
        'string' { return Get-SampleString $resolved }
        default {
            if ($null -ne $resolved.PSObject.Properties['properties']) {
                return Get-SchemaExample -Schema $resolved -Depth ($Depth + 1)
            }
            return 'string'
        }
    }
}

function ConvertTo-StringValue {
    param($Value)

    if ($null -eq $Value) { return '' }
    if ($Value -is [bool]) { if ($Value) { return 'true' } else { return 'false' } }
    return [string]$Value
}

function ConvertTo-ParameterList {
    param($PathItem, $Operation)

    $parameters = @()
    $sources = @()
    if ($null -ne $PathItem.PSObject.Properties['parameters']) { $sources += @($PathItem.parameters) }
    if ($null -ne $Operation.PSObject.Properties['parameters']) { $sources += @($Operation.parameters) }

    foreach ($parameter in $sources) {
        $resolved = Resolve-Schema $parameter
        if ($null -ne $resolved) { $parameters += , $resolved }
    }

    return , $parameters
}

function ConvertTo-HeaderEntry {
    param($Parameter)

    return [PSCustomObject][ordered]@{
        key         = [string]$Parameter.name
        value       = ConvertTo-StringValue (Get-SchemaExample -Schema $Parameter.schema)
        description = [string]$Parameter.description
    }
}

function ConvertTo-QueryEntry {
    param($Parameter)

    return [PSCustomObject][ordered]@{
        key         = [string]$Parameter.name
        value       = ConvertTo-StringValue (Get-SchemaExample -Schema $Parameter.schema)
        description = [string]$Parameter.description
    }
}

function Test-FileSchema {
    <#
        A form field is a file upload when its own schema, or the item schema of an array
        field, is declared as string/binary - the shape Swashbuckle emits for IFormFile and
        List<IFormFile>.
    #>
    param($Schema)

    $resolved = Merge-Schema $Schema
    if ($null -eq $resolved) { return $false }

    if (([string]$resolved.type -eq 'string') -and ([string]$resolved.format -eq 'binary')) { return $true }
    if ([string]$resolved.type -eq 'array') {
        return Test-FileSchema -Schema $resolved.items
    }

    return $false
}

function ConvertTo-FormDataEntries {
    <#
        Turns the object schema of a multipart/form-data or x-www-form-urlencoded body into
        Postman form rows, so the generated collection can actually send the requests the API
        declares ([FromForm] endpoints such as register and the attachment uploads).
        File fields become Postman file rows with no path filled in, because the path belongs
        to whoever runs the request.
    #>
    param($Schema)

    $resolved = Merge-Schema $Schema
    if ($null -eq $resolved -or $null -eq $resolved.PSObject.Properties['properties']) { return , @() }

    $required = @()
    if ($null -ne $resolved.PSObject.Properties['required']) {
        $required = @($resolved.required | ForEach-Object { [string]$_ })
    }

    $entries = @()
    foreach ($property in $resolved.properties.PSObject.Properties) {
        $name = [string]$property.Name
        $field = Resolve-Schema $property.Value
        $description = if ($null -ne $field -and $null -ne $field.PSObject.Properties['description']) {
            [string]$field.description
        } else { '' }
        $marker = if ($required -contains $name) { 'required' } else { 'optional' }
        $description = if ($description) { "$description ($marker)" } else { $marker }

        if (Test-FileSchema -Schema $property.Value) {
            $entries += , ([PSCustomObject][ordered]@{
                key         = $name
                type        = 'file'
                src         = @()
                description = $description
            })
            continue
        }

        $entries += , ([PSCustomObject][ordered]@{
            key         = $name
            value       = ConvertTo-StringValue (Get-SchemaExample -Schema $property.Value)
            type        = 'text'
            description = $description
        })
    }

    return , $entries
}

function ConvertTo-RequestBody {
    param($Operation)

    if ($null -eq $Operation.PSObject.Properties['requestBody']) { return $null }

    $body = Resolve-Schema $Operation.requestBody
    if ($null -eq $body -or $null -eq $body.PSObject.Properties['content']) { return $null }

    $mediaTypes = @($body.content.PSObject.Properties.Name)
    $mediaType = $mediaTypes | Where-Object { $_ -like '*json*' } | Select-Object -First 1
    if (-not $mediaType) { $mediaType = $mediaTypes | Select-Object -First 1 }
    if (-not $mediaType) { return $null }

    $media = $body.content.$mediaType
    $formMode = switch ($mediaType) {
        'multipart/form-data' { 'formdata' }
        'application/x-www-form-urlencoded' { 'urlencoded' }
        default { $null }
    }

    if ($formMode) {
        $entries = if ($null -ne $media.PSObject.Properties['schema']) {
            ConvertTo-FormDataEntries -Schema $media.schema
        } else { @() }

        return [PSCustomObject][ordered]@{
            mode      = $formMode
            mediaType = [string]$mediaType
            entries   = $entries
            raw       = ''
        }
    }

    if ($null -ne $media.PSObject.Properties['example']) {
        $example = $media.example
    } elseif ($null -ne $media.PSObject.Properties['schema']) {
        $example = Get-SchemaExample -Schema $media.schema
    } else {
        $example = $null
    }

    $raw = if ($null -eq $example) { '' } else { ConvertTo-StringValue (ConvertTo-StableJson -InputObject $example) }

    return [PSCustomObject][ordered]@{
        mode      = 'raw'
        mediaType = [string]$mediaType
        entries   = @()
        raw       = ($raw -replace "`r`n", "`n")
    }
}

function ConvertTo-ResponseList {
    <#
        Turns the responses of an operation into Postman saved examples. Every named example
        in the OpenAPI document (success, validationError, businessError, ...) becomes its own
        saved response with a real body, so the examples stay visible in Postman instead of an
        empty placeholder.
    #>
    param($Operation, $Request)

    $responses = @()
    if ($null -eq $Operation.PSObject.Properties['responses']) { return , $responses }

    foreach ($property in $Operation.responses.PSObject.Properties) {
        $description = [string]$property.Value.description
        $code = $property.Name -replace '[^0-9]', ''

        $mediaType = ''
        $examples = @()
        if ($null -ne $property.Value.PSObject.Properties['content']) {
            $mediaTypes = @($property.Value.content.PSObject.Properties.Name)
            $mediaType = $mediaTypes | Where-Object { $_ -like '*json*' } | Select-Object -First 1
            if (-not $mediaType) { $mediaType = $mediaTypes | Select-Object -First 1 }

            if ($mediaType) {
                $media = $property.Value.content.$mediaType
                if ($null -ne $media.PSObject.Properties['examples']) {
                    foreach ($example in $media.examples.PSObject.Properties) {
                        $examples += , ([PSCustomObject]@{
                            Name  = [string]$example.Name
                            Value = $example.Value.value
                        })
                    }
                } elseif ($null -ne $media.PSObject.Properties['example']) {
                    $examples += , ([PSCustomObject]@{ Name = ''; Value = $media.example })
                }
            }
        }

        $header = @()
        if ($mediaType) { $header += , ([PSCustomObject]@{ key = 'Content-Type'; value = $mediaType }) }
        $previewLanguage = if ($mediaType -like '*json*') { 'json' } else { 'text' }

        if ($examples.Count -eq 0) {
            $responses += , ([PSCustomObject][ordered]@{
                name                     = "$($property.Name) $description".Trim()
                originalRequest          = $Request
                status                   = $description
                code                     = $(if ($code) { [int]$code } else { 0 })
                header                   = $header
                body                     = ''
                _postman_previewlanguage = $previewLanguage
            })
            continue
        }

        foreach ($example in $examples) {
            $name = "$($property.Name) $description".Trim()
            if ($example.Name) { $name = "$name - $($example.Name)" }

            $responses += , ([PSCustomObject][ordered]@{
                name                     = $name
                originalRequest          = $Request
                status                   = $description
                code                     = $(if ($code) { [int]$code } else { 0 })
                header                   = $header
                body                     = (ConvertTo-StableJson -InputObject $example.Value)
                _postman_previewlanguage = $previewLanguage
            })
        }
    }

    return , $responses
}

function ConvertTo-RequestUrl {
    param([string]$Path, $Parameters, [string]$VariableName)

    $template = $Path -replace '\{([^}]+)\}', '{{$1}}'
    $pathSegments = @($template.Split('/') | Where-Object { $_ -ne '' })
    $urlVariables = @()
    $query = @()

    foreach ($parameter in $Parameters) {
        $location = [string]$parameter.in
        if ($location -eq 'path') {
            $urlVariables += , ([PSCustomObject][ordered]@{
                key         = [string]$parameter.name
                value       = ConvertTo-StringValue (Get-SchemaExample -Schema $parameter.schema)
                description = [string]$parameter.description
            })
        } elseif ($location -eq 'query') {
            $query += , (ConvertTo-QueryEntry $parameter)
        }
    }

    return [PSCustomObject][ordered]@{
        raw      = "{{$VariableName}}$template"
        host     = @("{{$VariableName}}")
        path     = $pathSegments
        query    = $query
        variable = $urlVariables
    }
}

function ConvertTo-RequestItem {
    param([string]$Path, [string]$Method, $Operation, $PathItem, [string]$VariableName)

    $parameters = ConvertTo-ParameterList -PathItem $PathItem -Operation $Operation

    $headers = @()
    foreach ($parameter in $parameters) {
        if ([string]$parameter.in -eq 'header') { $headers += , (ConvertTo-HeaderEntry $parameter) }
    }

    $body = ConvertTo-RequestBody $Operation
    # Postman adds the boundary to a multipart Content-Type itself, so a fixed header taken
    # from the document would break the request instead of describing it.
    if ($null -ne $body -and $body.mode -eq 'raw') {
        $headers += , ([PSCustomObject][ordered]@{ key = 'Content-Type'; value = $body.mediaType; description = '' })
    }

    $summary = if ($null -ne $Operation.PSObject.Properties['summary']) { [string]$Operation.summary } else { '' }
    if (-not $summary) { $summary = "$($Method.ToUpper()) $Path" }

    $description = if ($null -ne $Operation.PSObject.Properties['description']) { [string]$Operation.description } else { '' }
    if (-not $description) { $description = $summary }

    $requestBody = $null
    if ($null -ne $body) {
        if ($body.mode -eq 'raw') {
            $requestBody = [PSCustomObject][ordered]@{
                mode    = 'raw'
                raw     = $body.raw
                options = [PSCustomObject][ordered]@{ raw = [PSCustomObject][ordered]@{ language = 'json' } }
            }
        } elseif ($body.mode -eq 'formdata') {
            $requestBody = [PSCustomObject][ordered]@{
                mode     = 'formdata'
                formdata = $body.entries
            }
        } else {
            $requestBody = [PSCustomObject][ordered]@{
                mode       = 'urlencoded'
                urlencoded = $body.entries
            }
        }
    }

    $request = [PSCustomObject][ordered]@{
        method      = $Method.ToUpper()
        header      = $headers
        url         = ConvertTo-RequestUrl -Path $Path -Parameters $parameters -VariableName $VariableName
        description = $description
        body        = $requestBody
    }

    $responses = ConvertTo-ResponseList -Operation $Operation -Request $request

    return [PSCustomObject][ordered]@{
        name     = $summary
        request  = $request
        response = $responses
    }
}

function Get-TagFolders {
    param($Document, [string]$DefaultFolder)

    $folders = [ordered]@{}

    foreach ($pathProperty in $Document.paths.PSObject.Properties) {
        # "/" is the browser redirect to the Swagger UI (Program.cs), not an API operation.
        if ($pathProperty.Name -eq '/') { continue }

        $pathItem = $pathProperty.Value
        foreach ($method in $script:httpMethods) {
            if ($null -eq $pathItem.PSObject.Properties[$method]) { continue }
            $operation = $pathItem.$method

            $folderName = $DefaultFolder
            if ($null -ne $operation.PSObject.Properties['tags'] -and @($operation.tags).Count -gt 0) {
                $folderName = [string]@($operation.tags)[0]
            }

            $item = ConvertTo-RequestItem -Path $pathProperty.Name -Method $method -Operation $operation -PathItem $pathItem -VariableName $BaseUrlVariable

            if ($folders.Contains($folderName)) { $folders[$folderName] += , $item }
            else { $folders[$folderName] = @($item) }
        }
    }

    return $folders
}

function Get-CollectionAuth {
    param($Document, [string]$TokenName)

    $auth = $null
    if ($null -ne $Document.PSObject.Properties['components'] -and $null -ne $Document.components.PSObject.Properties['securitySchemes']) {
        foreach ($scheme in $Document.components.securitySchemes.PSObject.Properties) {
            $definition = $scheme.Value
            $isBearer = ([string]$definition.type -eq 'http' -and [string]$definition.scheme -eq 'bearer') -or
                        ([string]$definition.type -eq 'apiKey' -and [string]$definition.in -eq 'header' -and [string]$definition.name -eq 'Authorization')
            if ($isBearer) {
                $auth = [PSCustomObject][ordered]@{
                    type   = 'bearer'
                    bearer = @([PSCustomObject][ordered]@{ key = 'token'; value = "{{$TokenName}}"; type = 'string' })
                }
                break
            }
        }
    }

    return $auth
}

$folders = Get-TagFolders -Document $script:spec -DefaultFolder $UntaggedFolder

$collectionItems = @()
foreach ($folder in $folders.GetEnumerator()) {
    $collectionItems += , ([PSCustomObject][ordered]@{
        name        = [string]$folder.Key
        description = ''
        item        = $folder.Value
    })
}

$collection = [PSCustomObject][ordered]@{
    info = [PSCustomObject][ordered]@{
        name        = [string]$script:spec.info.title
        description = [string]$script:spec.info.description
        schema      = 'https://schema.getpostman.com/json/collection/v2.1.0/collection.json'
    }
    item = $collectionItems
    variable = @([PSCustomObject][ordered]@{
        key         = $BaseUrlVariable
        value       = $BaseUrl
        type        = 'string'
        description = 'API base URL. Point it at the environment you want to call.'
    })
    auth = Get-CollectionAuth -Document $script:spec -TokenName $TokenVariable
}

$outDir = Split-Path -Parent $OutFile
if ($outDir -and -not (Test-Path -LiteralPath $outDir)) { New-Item -ItemType Directory -Path $outDir | Out-Null }

$json = ((ConvertTo-StableJson -InputObject $collection) -replace "`r`n", "`n")
[System.IO.File]::WriteAllText($OutFile, $json + "`n", (New-Object System.Text.UTF8Encoding($false)))

Write-Host "Wrote $OutFile"
