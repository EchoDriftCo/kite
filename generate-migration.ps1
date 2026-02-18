# Generate EF Core migration SQL and fix for Supabase compatibility
param(
    [string]$OutputFile = "migrate.sql"
)

Write-Host "Generating migration script..." -ForegroundColor Cyan
dotnet ef migrations script --idempotent -o $OutputFile --project src/RecipeVault.Data --startup-project src/RecipeVault.WebApi

if (Test-Path $OutputFile) {
    # Fix SELECT setval -> PERFORM setval for PL/pgSQL compatibility
    $content = Get-Content $OutputFile -Raw
    $fixed = $content -replace 'SELECT setval\(', 'PERFORM setval('
    Set-Content -Path $OutputFile -Value $fixed -NoNewline
    
    Write-Host "Migration script generated: $OutputFile" -ForegroundColor Green
    Write-Host "Fixed SELECT setval -> PERFORM setval for Supabase compatibility" -ForegroundColor Yellow
} else {
    Write-Host "Failed to generate migration script" -ForegroundColor Red
}
