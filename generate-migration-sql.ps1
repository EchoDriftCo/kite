# Generate idempotent SQL script for pending EF Core migrations
# Run this BEFORE deploying to Fly.io, then execute the SQL in Supabase

$ErrorActionPreference = "Stop"

$projectPath = "src\RecipeVault.Data"
$startupPath = "src\RecipeVault.WebApi"
$outputDir = "migrations"
$outputFile = "$outputDir\pending.sql"
$archiveDir = "$outputDir\applied"

# Ensure directories exist
if (-not (Test-Path $outputDir)) { New-Item -ItemType Directory -Path $outputDir | Out-Null }
if (-not (Test-Path $archiveDir)) { New-Item -ItemType Directory -Path $archiveDir | Out-Null }

Write-Host "Generating idempotent migration SQL..." -ForegroundColor Cyan

# Generate idempotent script (safe to run multiple times)
dotnet ef migrations script `
    --idempotent `
    --project $projectPath `
    --startup-project $startupPath `
    --output $outputFile

if ($LASTEXITCODE -ne 0) {
    Write-Host "Failed to generate migration script!" -ForegroundColor Red
    exit 1
}

# Check if there's actual content (not just boilerplate)
$content = Get-Content $outputFile -Raw
if ($content -match "INSERT INTO.*__EFMigrationsHistory") {
    $migrationCount = ([regex]::Matches($content, "INSERT INTO.*__EFMigrationsHistory")).Count
    Write-Host ""
    Write-Host "Generated: $outputFile" -ForegroundColor Green
    Write-Host "Contains $migrationCount migration(s)" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "NEXT STEPS:" -ForegroundColor Cyan
    Write-Host "1. Review the SQL in $outputFile"
    Write-Host "2. Run it in Supabase SQL Editor BEFORE deploying"
    Write-Host "3. Then deploy to Fly.io"
    Write-Host ""
} else {
    Write-Host ""
    Write-Host "No pending migrations found." -ForegroundColor Green
    Write-Host "Schema is up to date - safe to deploy." -ForegroundColor Green
    Remove-Item $outputFile -ErrorAction SilentlyContinue
}
