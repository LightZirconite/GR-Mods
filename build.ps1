# Script de build automatique pour GR Mods
# Ce script compile l'application, copie les assets et crée l'installeur

$startTime = Get-Date

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "       GR Mods - Build Automatique" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Chemins
$projectRoot = "C:\Users\Light\Documents\GR-Mods"
$projectFile = "$projectRoot\GTA5Launcher\GTA5Launcher.csproj"
$publishDir = "$projectRoot\GTA5Launcher\bin\Release\net8.0-windows\win-x64\publish"
$assetsSource = "$projectRoot\assets"
$assetsTarget = "$publishDir\assets"
$innoSetupScript = "$projectRoot\GR-Mods-Setup.iss"
$innoSetupCompiler = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
$installerOutput = "$projectRoot\Installer\GR-Mods-Setup-1.0.0.exe"
$exeFolder = "$projectRoot\exe"
$finalExe = "$exeFolder\GR-Mods-Setup.exe"

# Vérifier que Inno Setup est installé
if (-not (Test-Path $innoSetupCompiler)) {
    Write-Host "ERREUR: Inno Setup n'est pas installé!" -ForegroundColor Red
    Write-Host "Téléchargez-le depuis: https://jrsoftware.org/isdl.php" -ForegroundColor Yellow
    exit 1
}

# Étape 1: Nettoyer les anciens builds
Write-Host "[1/6] Nettoyage des anciens builds..." -ForegroundColor Yellow
if (Test-Path "$projectRoot\GTA5Launcher\bin\Release") {
    Remove-Item "$projectRoot\GTA5Launcher\bin\Release" -Recurse -Force -ErrorAction SilentlyContinue
}
if (Test-Path "$projectRoot\Installer") {
    Remove-Item "$projectRoot\Installer" -Recurse -Force -ErrorAction SilentlyContinue
}
Write-Host "   Terminé!" -ForegroundColor Green
Write-Host ""

# Étape 2: Compiler l'application
Write-Host "[2/6] Compilation de l'application..." -ForegroundColor Yellow
Set-Location $projectRoot
$buildResult = dotnet publish $projectFile -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true --nologo --verbosity quiet 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERREUR: La compilation a échoué!" -ForegroundColor Red
    Write-Host $buildResult
    exit 1
}
Write-Host "   Compilation réussie!" -ForegroundColor Green
Write-Host ""

# Étape 3: Copier les assets
Write-Host "[3/6] Copie des assets (images)..." -ForegroundColor Yellow
if (Test-Path $assetsTarget) {
    Remove-Item $assetsTarget -Recurse -Force
}
Copy-Item -Path $assetsSource -Destination $assetsTarget -Recurse -Force
Write-Host "   Assets copiés!" -ForegroundColor Green
Write-Host ""

# Étape 4: Créer l'installeur avec Inno Setup
Write-Host "[4/6] Création de l'installeur..." -ForegroundColor Yellow
$innoResult = & $innoSetupCompiler $innoSetupScript 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERREUR: La création de l'installeur a échoué!" -ForegroundColor Red
    Write-Host $innoResult
    exit 1
}
Write-Host "   Installeur créé avec succès!" -ForegroundColor Green
Write-Host ""

# Étape 5: Créer le dossier exe et copier l'installeur
Write-Host "[5/6] Copie de l'installeur final..." -ForegroundColor Yellow
if (-not (Test-Path $exeFolder)) {
    New-Item -ItemType Directory -Path $exeFolder -Force | Out-Null
}

# Supprimer l'ancien exe s'il existe
if (Test-Path $finalExe) {
    Remove-Item $finalExe -Force
    Write-Host "   Ancien installeur supprimé" -ForegroundColor Gray
}

# Copier le nouveau exe
Copy-Item -Path $installerOutput -Destination $finalExe -Force
Write-Host "   Installeur copié vers: $finalExe" -ForegroundColor Green

# Nettoyer le dossier Installer (plus besoin après la copie)
if (Test-Path "$projectRoot\Installer") {
    Remove-Item "$projectRoot\Installer" -Recurse -Force -ErrorAction SilentlyContinue
    Write-Host "   Dossier temporaire Installer supprimé" -ForegroundColor Gray
}
Write-Host ""

# Étape 6: Informations finales
$endTime = Get-Date
$duration = ($endTime - $startTime).TotalSeconds

Write-Host "[6/6] Build terminé!" -ForegroundColor Green
Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "              RÉSUMÉ" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Installeur final:" -ForegroundColor Yellow
Write-Host "  $finalExe" -ForegroundColor White
Write-Host ""

$exeSize = (Get-Item $finalExe).Length / 1MB
Write-Host "Taille: $([math]::Round($exeSize, 2)) MB" -ForegroundColor Gray
Write-Host "Durée: $([math]::Round($duration, 1)) secondes" -ForegroundColor Gray
Write-Host ""
Write-Host "L'installeur est prêt à être distribué!" -ForegroundColor Green
Write-Host ""

# Option pour ouvrir le dossier
$response = Read-Host "Voulez-vous ouvrir le dossier exe? (O/N)"
if ($response -eq "O" -or $response -eq "o") {
    explorer $exeFolder
}

Write-Host ""
Write-Host "Script terminé avec succès!" -ForegroundColor Cyan
