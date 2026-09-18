<#
  Starts the full local dev stack:
    - 4 Docker Compose stacks (Postgres/RabbitMQ x2, MinIO, SeaweedFS)
    - CloudBackend and RaspberryPiAPI (.NET, "http" launch profile)
    - Frontend (Vite dev server)
  Each dotnet/npm process opens in its own window so you can see its logs.
  PIDs of the opened windows are recorded so stop-dev-environment.ps1 can
  shut them back down cleanly.
#>

$ErrorActionPreference = "Stop"
$root = $PSScriptRoot
$pidFile = "$root\.dev-pids.json"

$composeFiles = @(
    "$root\EventManagement\RaspberryPiAPI\docker-compose.yml",
    "$root\EventManagement\CloudBackend\compose.yml",
    "$root\Frontend\docker-compose.yml",
    "$root\SeaweedFS\compose.yml"
)

Write-Host "Starting Docker Compose stacks..." -ForegroundColor Cyan
foreach ($file in $composeFiles) {
    Write-Host "  -> $file"
    docker compose -f $file up -d
    if (-not $?) { throw "docker compose up failed for $file" }
}

$launched = @()

Write-Host "Starting CloudBackend (dotnet, http profile, :5291)..." -ForegroundColor Cyan
$p = Start-Process powershell -PassThru -ArgumentList @(
    "-NoExit", "-Command",
    "cd '$root\EventManagement\CloudBackend'; dotnet run --launch-profile http"
)
$launched += [PSCustomObject]@{ Name = "CloudBackend"; Pid = $p.Id }

Write-Host "Starting RaspberryPiAPI (dotnet, http profile, :5182)..." -ForegroundColor Cyan
$p = Start-Process powershell -PassThru -ArgumentList @(
    "-NoExit", "-Command",
    "cd '$root\EventManagement\RaspberryPiAPI'; dotnet run --launch-profile http"
)
$launched += [PSCustomObject]@{ Name = "RaspberryPiAPI"; Pid = $p.Id }

Write-Host "Starting Frontend (npm run dev)..." -ForegroundColor Cyan
$p = Start-Process powershell -PassThru -ArgumentList @(
    "-NoExit", "-Command",
    "cd '$root\Frontend'; npm run dev"
)
$launched += [PSCustomObject]@{ Name = "Frontend"; Pid = $p.Id }

$launched | ConvertTo-Json | Set-Content -Path $pidFile -Encoding utf8

Write-Host "All services launching. Check the opened windows for logs." -ForegroundColor Green
Write-Host "Run stop-dev-environment.ps1 to shut everything back down." -ForegroundColor Green
