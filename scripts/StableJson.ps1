<#
.SYNOPSIS
    Host-independent JSON serialization for the generated artifacts.

.DESCRIPTION
    ConvertTo-Json is not stable across PowerShell hosts: Windows PowerShell 5.1 indents
    with 4 spaces and escapes only non-ASCII, while PowerShell 7 indents with 2 spaces and
    also escapes <, >, & and '. A file generated on a developer machine therefore never
    matches one generated on the GitHub runner, and the drift check fails on every run.

    ConvertTo-StableJson writes the same bytes everywhere: 2-space indent, minimal escaping,
    LF line endings, property order preserved from the parsed document, UTF-8 without BOM.
#>

function ConvertTo-StableJson {
    param(
        [Parameter(Mandatory = $true, ValueFromPipeline = $true)]
        $InputObject,

        [int]$Depth = 0
    )

    $builder = New-Object System.Text.StringBuilder
    Write-JsonValue -Builder $builder -Value $InputObject -Indent $Depth
    $builder.ToString()
}

function Write-JsonValue {
    param(
        [System.Text.StringBuilder]$Builder,
        $Value,
        [int]$Indent
    )

    if ($null -eq $Value) {
        [void]$Builder.Append('null')
        return
    }

    if ($Value -is [bool]) {
        [void]$Builder.Append($(if ($Value) { 'true' } else { 'false' }))
        return
    }

    if ($Value -is [string]) {
        [void]$Builder.Append((ConvertTo-StableJsonString $Value))
        return
    }

    if ($Value -is [System.Enum] -or $Value -is [System.DateTime] -or $Value -is [System.Guid] -or $Value -is [System.Version] -or $Value -is [System.TimeSpan] -or $Value -is [System.Uri]) {
        [void]$Builder.Append((ConvertTo-StableJsonString ([string]$Value)))
        return
    }

    if ($Value -is [byte] -or $Value -is [sbyte] -or $Value -is [int16] -or $Value -is [uint16] -or
        $Value -is [int] -or $Value -is [uint32] -or $Value -is [int64] -or $Value -is [uint64] -or
        $Value -is [decimal]) {
        [void]$Builder.Append($Value.ToString([System.Globalization.CultureInfo]::InvariantCulture))
        return
    }

    if ($Value -is [double] -or $Value -is [float] -or $Value -is [single]) {
        $number = [double]$Value
        if ([double]::IsNaN($number) -or [double]::IsInfinity($number)) {
            [void]$Builder.Append('null')
            return
        }
        [void]$Builder.Append($number.ToString('R', [System.Globalization.CultureInfo]::InvariantCulture))
        return
    }

    if ($Value -is [System.Collections.IDictionary]) {
        Write-JsonObject -Builder $Builder -Entries @(
            foreach ($key in $Value.Keys) {
                [PSCustomObject]@{ Name = [string]$key; Value = $Value[$key] }
            }
        ) -Indent $Indent
        return
    }

    if ($Value -is [System.Management.Automation.PSCustomObject]) {
        Write-JsonObject -Builder $Builder -Entries @(
            foreach ($property in $Value.PSObject.Properties) {
                [PSCustomObject]@{ Name = $property.Name; Value = $property.Value }
            }
        ) -Indent $Indent
        return
    }

    if ($Value -is [System.Collections.IEnumerable]) {
        Write-JsonArray -Builder $Builder -Values @($Value) -Indent $Indent
        return
    }

    [void]$Builder.Append((ConvertTo-StableJsonString ([string]$Value)))
}

function Write-JsonObject {
    param(
        [System.Text.StringBuilder]$Builder,
        $Entries,
        [int]$Indent
    )

    $entries = @($Entries)
    if ($entries.Count -eq 0) {
        [void]$Builder.Append('{}')
        return
    }

    [void]$Builder.Append('{')
    $inner = $Indent + 1
    for ($index = 0; $index -lt $entries.Count; $index++) {
        if ($index -gt 0) { [void]$Builder.Append(',') }
        [void]$Builder.Append("`n")
        [void]$Builder.Append(' ' * (2 * $inner))
        [void]$Builder.Append((ConvertTo-StableJsonString $entries[$index].Name))
        [void]$Builder.Append(': ')
        Write-JsonValue -Builder $Builder -Value $entries[$index].Value -Indent $inner
    }
    [void]$Builder.Append("`n")
    [void]$Builder.Append(' ' * (2 * $Indent))
    [void]$Builder.Append('}')
}

function Write-JsonArray {
    param(
        [System.Text.StringBuilder]$Builder,
        $Values,
        [int]$Indent
    )

    $values = @($Values)
    if ($values.Count -eq 0) {
        [void]$Builder.Append('[]')
        return
    }

    [void]$Builder.Append('[')
    $inner = $Indent + 1
    for ($index = 0; $index -lt $values.Count; $index++) {
        if ($index -gt 0) { [void]$Builder.Append(',') }
        [void]$Builder.Append("`n")
        [void]$Builder.Append(' ' * (2 * $inner))
        Write-JsonValue -Builder $Builder -Value $values[$index] -Indent $inner
    }
    [void]$Builder.Append("`n")
    [void]$Builder.Append(' ' * (2 * $Indent))
    [void]$Builder.Append(']')
}

function ConvertTo-StableJsonString {
    param([AllowNull()][string]$Value)

    if ($null -eq $Value) { return '""' }

    $builder = New-Object System.Text.StringBuilder
    [void]$builder.Append('"')
    foreach ($char in $Value.ToCharArray()) {
        switch ($char) {
            '"' { [void]$builder.Append('\"'); continue }
            '\' { [void]$builder.Append('\\'); continue }
            "`b" { [void]$builder.Append('\b'); continue }
            "`f" { [void]$builder.Append('\f'); continue }
            "`n" { [void]$builder.Append('\n'); continue }
            "`r" { [void]$builder.Append('\r'); continue }
            "`t" { [void]$builder.Append('\t'); continue }
            default {
                if ([int]$char -lt 32) {
                    [void]$builder.Append('\u').Append(([int]$char).ToString('x4'))
                } else {
                    [void]$builder.Append($char)
                }
            }
        }
    }
    [void]$builder.Append('"')
    return $builder.ToString()
}
