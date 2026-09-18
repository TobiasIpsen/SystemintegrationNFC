<#
  Shuts down everything started by start-dev-environment.ps1:
    - Kills the dotnet/npm process trees (via their recorded window PIDs)
    - Brings down all 4 Docker Compose stacks
#>

$ErrorActionPreference = "Continue"
$root = $PSScriptRoot
$pidFile = "$root\.dev-pids.json"

if (Test-Path $pidFile) {
    $launched = Get-Content $pidFile | ConvertFrom-Json
    foreach ($entry in $launched) {
        Write-Host "Stopping $($entry.Name) (PID $($entry.Pid))..." -ForegroundColor Cyan
        # /T kills the whole process tree (the powershell window plus dotnet/npm/node children)
        taskkill /PID $entry.Pid /T /F 2>$null | Out-Null
    }
    Remove-Item $pidFile -Force
} else {
    Write-Host "No .dev-pids.json found - skipping process shutdown (was start-dev-environment.ps1 run?)" -ForegroundColor Yellow
}

$composeFiles = @(
    "$root\EventManagement\RaspberryPiAPI\docker-compose.yml",
    "$root\EventManagement\CloudBackend\compose.yml",
    "$root\Frontend\docker-compose.yml",
    "$root\SeaweedFS\compose.yml"
)

Write-Host "Stopping Docker Compose stacks..." -ForegroundColor Cyan
foreach ($file in $composeFiles) {
    Write-Host "  -> $file"
    docker compose -f $file down
}

Write-Host "Everything stopped." -ForegroundColor Green
