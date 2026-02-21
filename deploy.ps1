<#
.SYNOPSIS
    Deploy RecipeVault to Fly.io with migration safety checks.

.DESCRIPTION
    This script ensures database migrations are handled before deploying:
    1. Generates idempotent migration SQL
    2. Shows pending migrations (if any)
    3. Requires confirmation before deploying
    4. Optionally copies SQL to clipboard for Supabase

.PARAMETER SkipMigrationCheck
    Skip migration check (dangerous - use only if you're sure schema is in sync)

.PARAMETER Force
    Deploy without confirmation prompts

.EXAMPLE
    .\deploy.ps1
    .\deploy.ps1 -Force
#>

param(
    [switch]$SkipMigrationCheck,
    [switch]$Force
)

$ErrorActionPreference = "Stop"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  RecipeVault Deploy" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Step 1: Check for pending migrations
if (-not $SkipMigrationCheck) {
    Write-Host "[1/3] Checking for pending migrations..." -ForegroundColor Yellow
    
    $projectPath = "src\RecipeVault.Data"
    $startupPath = "src\RecipeVault.WebApi"
    $migrationSql = "migrations\pending.sql"
    
    # Generate idempotent script
    $output = dotnet ef migrations script --idempotent --project $projectPath --startup-project $startupPath --output $migrationSql 2>&1
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to generate migration script!" -ForegroundColor Red
        Write-Host $output
        exit 1
    }
    
    # Check if there are actual migrations
    $content = Get-Content $migrationSql -Raw -ErrorAction SilentlyContinue
    $hasMigrations = $content -match "INSERT INTO.*__EFMigrationsHistory"
    
    if ($hasMigrations) {
        $migrationCount = ([regex]::Matches($content, "INSERT INTO.*__EFMigrationsHistory")).Count
        
        Write-Host ""
        Write-Host "WARNING: Found $migrationCount pending migration(s)!" -ForegroundColor Red
        Write-Host ""
        Write-Host "You MUST run the SQL in Supabase before deploying." -ForegroundColor Yellow
        Write-Host "SQL file: $migrationSql" -ForegroundColor Gray
        Write-Host ""
        
        if (-not $Force) {
            $copyToClipboard = Read-Host "Copy SQL to clipboard? (y/n)"
            if ($copyToClipboard -eq "y") {
                Get-Content $migrationSql -Raw | Set-Clipboard
                Write-Host "SQL copied to clipboard!" -ForegroundColor Green
                Write-Host ""
                Write-Host "1. Go to Supabase SQL Editor" -ForegroundColor Cyan
                Write-Host "2. Paste and run the SQL" -ForegroundColor Cyan
                Write-Host "3. Come back here and continue" -ForegroundColor Cyan
                Write-Host ""
            }
            
            $confirm = Read-Host "Have you run the migrations in Supabase? (yes/no)"
            if ($confirm -ne "yes") {
                Write-Host "Deploy cancelled. Run migrations first!" -ForegroundColor Red
                exit 1
            }
        }
    } else {
        Write-Host "No pending migrations. Schema is in sync." -ForegroundColor Green
        Remove-Item $migrationSql -ErrorAction SilentlyContinue
    }
} else {
    Write-Host "[1/3] Skipping migration check (--SkipMigrationCheck)" -ForegroundColor Yellow
}

Write-Host ""

# Step 2: Build check
Write-Host "[2/3] Verifying build..." -ForegroundColor Yellow
dotnet build src\RecipeVault.WebApi -c Release --nologo -v q
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build failed!" -ForegroundColor Red
    exit 1
}
Write-Host "Build OK" -ForegroundColor Green
Write-Host ""

# Step 3: Deploy
Write-Host "[3/3] Deploying to Fly.io..." -ForegroundColor Yellow
Write-Host ""

fly deploy --remote-only

if ($LASTEXITCODE -eq 0) {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Green
    Write-Host "  Deploy successful!" -ForegroundColor Green
    Write-Host "========================================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Live at: https://myrecipevault.io" -ForegroundColor Cyan
} else {
    Write-Host ""
    Write-Host "Deploy failed!" -ForegroundColor Red
    exit 1
}
