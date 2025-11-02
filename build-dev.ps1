# Script de build RAPIDE pour developpement
# Ce script compile l'application SANS creer l'installeur
# Beaucoup plus rapide pour tester les modifications

$startTime = Get-Date

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "    GR Mods - Build Developpement Rapide" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Chemins
$projectRoot = "C:\Users\Light\Documents\GR-Mods"
$projectFile = "$projectRoot\GTA5Launcher\GTA5Launcher.csproj"
$outputDir = "$projectRoot\GTA5Launcher\bin\Debug\net8.0-windows\win-x64"
$assetsSource = "$projectRoot\assets"
$assetsTarget = "$outputDir\assets"
$exePath = "$outputDir\GR-Mods.exe"

# Etape 1: Compilation rapide
Write-Host "[1/2] Compilation de l'application..." -ForegroundColor Yellow
Set-Location $projectRoot
$buildResult = dotnet build $projectFile -c Debug --nologo 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERREUR: La compilation a echoue!" -ForegroundColor Red
    Write-Host $buildResult
    exit 1
}
Write-Host "   Compilation reussie!" -ForegroundColor Green
Write-Host ""

# Etape 2: Copier les assets
Write-Host "[2/2] Copie des assets (images)..." -ForegroundColor Yellow
if (Test-Path $assetsTarget) {
    Remove-Item $assetsTarget -Recurse -Force -ErrorAction SilentlyContinue
}
Copy-Item -Path $assetsSource -Destination $assetsTarget -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "   Assets copies!" -ForegroundColor Green
Write-Host ""

# Informations finales
$endTime = Get-Date
$duration = ($endTime - $startTime).TotalSeconds

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "          BUILD TERMINE" -ForegroundColor Green
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Executable:" -ForegroundColor Yellow
Write-Host "  $exePath" -ForegroundColor White
Write-Host ""
Write-Host "Duree: $([math]::Round($duration, 1)) secondes" -ForegroundColor Gray
Write-Host ""
Write-Host "Pour creer l'installeur final: .\build.ps1" -ForegroundColor Cyan
Write-Host ""

# Proposer de lancer l'application
$response = Read-Host "Voulez-vous lancer l'application maintenant? (O/N)"
if ($response -eq "O" -or $response -eq "o") {
    Write-Host ""
    Write-Host "Lancement de l'application..." -ForegroundColor Green
    Start-Process $exePath
} else {
    Write-Host ""
    Write-Host "Pour lancer manuellement: $exePath" -ForegroundColor Gray
}

Write-Host ""
Write-Host "Script termine!" -ForegroundColor Cyan
