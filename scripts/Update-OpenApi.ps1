<#
.SYNOPSIS
    Downloads the OpenAPI document served by the LMS API into a tracked git file.

.DESCRIPTION
    Writes a normalized copy of /swagger/<version>/swagger.json to openapi/<version>.json.
    The file is committed to the repository so that Postman (Connect Repository), Newman
    and any OpenAPI tooling consume the exact same document the API serves.

.EXAMPLE
    .\scripts\Update-OpenApi.ps1 -Launch
    Starts the API on the http launch profile, downloads the document, stops the API.

.EXAMPLE
    .\scripts\Update-OpenApi.ps1 -BaseUrl https://lms-api.runasp.net
    Downloads the document from an already running or deployed instance.
#>
[CmdletBinding()]
param(
    [string]$BaseUrl = 'http://localhost:5029',
    [switch]$Launch,
    [string]$Version = 'v1',
    [int]$TimeoutSeconds = 120
)

$ErrorActionPreference = 'Stop'

. (Join-Path $PSScriptRoot 'StableJson.ps1')

$repoRoot = Split-Path -Parent $PSScriptRoot
# Join-Path per segment: a backslash inside a single segment is a literal character on Linux.
$projectPath = Join-Path (Join-Path $repoRoot 'LMS.API') 'LMS.API.csproj'
$specUri = "$($BaseUrl.TrimEnd('/'))/swagger/$Version/swagger.json"

function Wait-ForSpec {
    param(
        [string]$Uri,
        [int]$TimeoutSeconds,
        [System.Diagnostics.Process]$Process = $null,
        [string]$LogsDirectory = ''
    )

    $deadline = (Get-Date).AddSeconds($TimeoutSeconds)
    while ((Get-Date) -lt $deadline) {
        if ($Process -and $Process.HasExited) {
            $errLogPath = if ($LogsDirectory) { Join-Path $LogsDirectory 'api.err.log' } else { '' }
            $outLogPath = if ($LogsDirectory) { Join-Path $LogsDirectory 'api.out.log' } else { '' }
            $errContent = if ($errLogPath -and (Test-Path $errLogPath)) { Get-Content $errLogPath -Raw } else { '' }
            $outContent = if ($outLogPath -and (Test-Path $outLogPath)) { (Get-Content $outLogPath -Tail 20) -join "`n" } else { '' }
            throw "The API process exited unexpectedly with code $($Process.ExitCode).`n[Errors]`n$errContent`n[Output]`n$outContent"
        }

        try {
            $response = Invoke-WebRequest -Uri $Uri -UseBasicParsing -TimeoutSec 15
            if ($response.StatusCode -eq 200 -and $response.Content -match '"openapi"') {
                return $response.Content
            }
            Write-Host "  $($response.StatusCode) without an OpenAPI document, retrying..."
        } catch {
            Write-Host "  not ready yet ($($_.Exception.Message)), retrying..."
        }
        Start-Sleep -Seconds 3
    }

    throw "Timed out after $TimeoutSeconds seconds waiting for $Uri."
}

$appProcess = $null
# $env:TEMP does not exist on the Linux runner: PowerShell 7 runs on .NET, where the temporary
# directory is reported by the runtime rather than by the environment. GetTempPath() resolves to
# %TEMP% on Windows and /tmp on Linux, so the script is portable without a per-host branch.
$tempRoot = [System.IO.Path]::GetTempPath()
$logDir = Join-Path $tempRoot 'lms-openapi'
if (-not (Test-Path -LiteralPath $logDir)) {
    New-Item -ItemType Directory -Path $logDir | Out-Null
}

try {
    if ($Launch) {
        Write-Host "Starting API (http launch profile)..."
        $appProcess = Start-Process -FilePath 'dotnet' -WorkingDirectory $repoRoot -PassThru `
            -ArgumentList @(
                'run', '--project', "`"$projectPath`"", '--launch-profile', 'http'
            ) `
            -RedirectStandardOutput (Join-Path $logDir 'api.out.log') `
            -RedirectStandardError (Join-Path $logDir 'api.err.log')
    } else {
        Write-Host "Using running API at $BaseUrl"
    }

    Write-Host "Reading $specUri"
    $content = Wait-ForSpec -Uri $specUri -TimeoutSeconds $TimeoutSeconds -Process $appProcess -LogsDirectory $logDir

    $tempFile = Join-Path $tempRoot "openapi-$Version.json"
    [System.IO.File]::WriteAllText($tempFile, $content, (New-Object System.Text.UTF8Encoding($false)))

    & (Join-Path $PSScriptRoot 'Save-OpenApiDocument.ps1') -InputPath $tempFile -Version $Version
} finally {
    if ($appProcess) {
        Write-Host "Stopping API..."
        # 'dotnet run' starts the application as a child process, so stopping the runner is not
        # enough. Windows PowerShell 5.1 (.NET Framework) has no tree-aware Kill, and taskkill is
        # Windows-only; PowerShell 7 on either platform has Process.Kill(entireProcessTree).
        if ($PSVersionTable.PSVersion.Major -lt 6) {
            taskkill /PID $appProcess.Id /T /F | Out-Null
        } else {
            try {
                $appProcess.Kill($true)
                $appProcess.WaitForExit(30000) | Out-Null
            } catch {
                Write-Host "Could not stop the API process: $($_.Exception.Message)"
            }
        }
    }
}
