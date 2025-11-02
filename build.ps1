# Script de build automatique pour GR Mods
# Usage:
#   .\build.ps1           -> Build rapide (developpement) sans installeur
#   .\build.ps1 -Installer -> Build complet avec creation de l'installeur
#   .\build.ps1 -Release  -> Build Release rapide sans installeur

param(
    [switch]$Installer,  # Creer l'installeur (LENT)
    [switch]$Release     # Build en Release (sinon Debug)
)

$startTime = Get-Date

Write-Host "================================================" -ForegroundColor Cyan
Write-Host "       GR Mods - Build Automatique" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

# Configuration
$config = if ($Release -or $Installer) { "Release" } else { "Debug" }
$buildMode = if ($Installer) { "COMPLET (avec installeur)" } else { "RAPIDE (dev)" }

Write-Host "Mode: $buildMode" -ForegroundColor $(if ($Installer) { "Yellow" } else { "Green" })
Write-Host "Configuration: $config" -ForegroundColor Cyan
Write-Host ""

# Chemins
$projectRoot = "C:\Users\Light\Documents\GR-Mods"
$projectFile = "$projectRoot\GTA5Launcher\GTA5Launcher.csproj"
$assetsSource = "$projectRoot\assets"
$innoSetupScript = "$projectRoot\GR-Mods-Setup.iss"
$innoSetupCompiler = "C:\Program Files (x86)\Inno Setup 6\ISCC.exe"
$installerOutput = "$projectRoot\Installer\GR-Mods-Setup-1.0.0.exe"
$exeFolder = "$projectRoot\exe"
$finalExe = "$exeFolder\GR-Mods-Setup.exe"

# Paths differ based on build mode
if ($Installer) {
    # Pour l'installeur, on utilise publish avec self-contained
    $outputDir = "$projectRoot\GTA5Launcher\bin\Release\net8.0-windows\win-x64\publish"
    $buildCommand = "publish"
    $buildArgs = @("-c", "Release", "-r", "win-x64", "--self-contained", "true", "-p:PublishSingleFile=true", "--nologo", "--verbosity", "quiet")
} else {
    # Pour le developpement, build simple et rapide
    $outputDir = "$projectRoot\GTA5Launcher\bin\$config\net8.0-windows\win-x64"
    $buildCommand = "build"
    $buildArgs = @("-c", $config, "--nologo")
}

$assetsTarget = "$outputDir\assets"

# Verifier Inno Setup seulement si necessaire
if ($Installer -and -not (Test-Path $innoSetupCompiler)) {
    Write-Host "ERREUR: Inno Setup n'est pas installe!" -ForegroundColor Red
    Write-Host "Telechargez-le depuis: https://jrsoftware.org/isdl.php" -ForegroundColor Yellow
    exit 1
}

# Etape 1: Nettoyer les anciens builds (seulement en mode installeur)
if ($Installer) {
    Write-Host "[1/5] Nettoyage des anciens builds..." -ForegroundColor Yellow
    if (Test-Path "$projectRoot\GTA5Launcher\bin\Release") {
        Remove-Item "$projectRoot\GTA5Launcher\bin\Release" -Recurse -Force -ErrorAction SilentlyContinue
    }
    if (Test-Path "$projectRoot\Installer") {
        Remove-Item "$projectRoot\Installer" -Recurse -Force -ErrorAction SilentlyContinue
    }
    Write-Host "   Termine!" -ForegroundColor Green
    Write-Host ""
    $step = 2
} else {
    $step = 1
}

# Etape 2: Compiler l'application
$totalSteps = if ($Installer) { 5 } else { 2 }
Write-Host "[$step/$totalSteps] Compilation de l'application..." -ForegroundColor Yellow
Set-Location $projectRoot
$buildResult = & dotnet $buildCommand $projectFile $buildArgs 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host "ERREUR: La compilation a echoue!" -ForegroundColor Red
    Write-Host $buildResult
    exit 1
}
Write-Host "   Compilation reussie!" -ForegroundColor Green
Write-Host ""
$step++

# Etape 3: Copier les assets
Write-Host "[$step/$totalSteps] Copie des assets (images)..." -ForegroundColor Yellow
if (Test-Path $assetsTarget) {
    Remove-Item $assetsTarget -Recurse -Force -ErrorAction SilentlyContinue
}
Copy-Item -Path $assetsSource -Destination $assetsTarget -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "   Assets copies!" -ForegroundColor Green
Write-Host ""
$step++

# Etapes 4-5: Creer l'installeur (seulement si demande)
if ($Installer) {
    # Etape 4: Creer l'installeur avec Inno Setup
    Write-Host "[$step/$totalSteps] Creation de l'installeur..." -ForegroundColor Yellow
    $innoResult = & $innoSetupCompiler $innoSetupScript 2>&1

    if ($LASTEXITCODE -ne 0) {
        Write-Host "ERREUR: La creation de l'installeur a echoue!" -ForegroundColor Red
        Write-Host $innoResult
        exit 1
    }
    Write-Host "   Installeur cree avec succes!" -ForegroundColor Green
    Write-Host ""
    $step++

    # Etape 5: Copier l'installeur
    Write-Host "[$step/$totalSteps] Copie de l'installeur final..." -ForegroundColor Yellow
    if (-not (Test-Path $exeFolder)) {
        New-Item -ItemType Directory -Path $exeFolder -Force | Out-Null
    }

    if (Test-Path $finalExe) {
        Remove-Item $finalExe -Force
        Write-Host "   Ancien installeur supprime" -ForegroundColor Gray
    }

    Copy-Item -Path $installerOutput -Destination $finalExe -Force
    Write-Host "   Installeur copie vers: $finalExe" -ForegroundColor Green

    if (Test-Path "$projectRoot\Installer") {
        Remove-Item "$projectRoot\Installer" -Recurse -Force -ErrorAction SilentlyContinue
        Write-Host "   Dossier temporaire Installer supprime" -ForegroundColor Gray
    }
    Write-Host ""
}

# Etape finale: Informations
$endTime = Get-Date
$duration = ($endTime - $startTime).TotalSeconds

Write-Host "Build termine!" -ForegroundColor Green
Write-Host ""
Write-Host "================================================" -ForegroundColor Cyan
Write-Host "              RESUME" -ForegroundColor Cyan
Write-Host "================================================" -ForegroundColor Cyan
Write-Host ""

if ($Installer) {
    Write-Host "Installeur final:" -ForegroundColor Yellow
    Write-Host "  $finalExe" -ForegroundColor White
    Write-Host ""
    $exeSize = (Get-Item $finalExe).Length / 1MB
    Write-Host "Taille: $([math]::Round($exeSize, 2)) MB" -ForegroundColor Gray
} else {
    Write-Host "Executable de developpement:" -ForegroundColor Yellow
    Write-Host "  $outputDir\GR-Mods.exe" -ForegroundColor White
    Write-Host ""
}

Write-Host "Duree: $([math]::Round($duration, 1)) secondes" -ForegroundColor Gray
Write-Host ""

if ($Installer) {
    Write-Host "L'installeur est pret a etre distribue!" -ForegroundColor Green
    Write-Host ""
    $response = Read-Host "Voulez-vous ouvrir le dossier exe? (O/N)"
    if ($response -eq "O" -or $response -eq "o") {
        explorer $exeFolder
    }
} else {
    Write-Host "Build de developpement termine!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Conseils:" -ForegroundColor Cyan
    Write-Host "  - Pour lancer: cd '$outputDir' ; .\GR-Mods.exe" -ForegroundColor Gray
    Write-Host "  - Pour creer l'installeur: .\build.ps1 -Installer" -ForegroundColor Gray
    Write-Host ""
    $response = Read-Host "Voulez-vous lancer l'application? (O/N)"
    if ($response -eq "O" -or $response -eq "o") {
        Start-Process "$outputDir\GR-Mods.exe"
    }
}

Write-Host ""
Write-Host "Script termine avec succes!" -ForegroundColor Cyan
