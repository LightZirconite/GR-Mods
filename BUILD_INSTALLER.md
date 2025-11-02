# Comment créer l'installeur GR Mods

## Prérequis

1. **Télécharger et installer Inno Setup** (gratuit)
   - Site officiel : https://jrsoftware.org/isdl.php
   - Téléchargez la version la plus récente (innosetup-6.x.x.exe)
   - Installez avec les options par défaut

## Étapes pour créer l'installeur

### 1. Compiler l'application

Ouvrez PowerShell dans le dossier du projet et exécutez :

```powershell
cd "C:\Users\Light\Documents\GR-Mods"
dotnet publish GTA5Launcher\GTA5Launcher.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

### 2. Copier les assets

```powershell
xcopy "assets" "GTA5Launcher\bin\Release\net8.0-windows\win-x64\publish\assets\" /E /I /Y
```

### 3. Créer l'installeur avec Inno Setup

**Option A : Via l'interface graphique**
1. Lancez Inno Setup Compiler
2. Fichier → Ouvrir → Sélectionnez `GR-Mods-Setup.iss`
3. Build → Compile (ou F9)
4. L'installeur sera créé dans le dossier `Installer\`

**Option B : Via la ligne de commande**
```powershell
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" "C:\Users\Light\Documents\GR-Mods\GR-Mods-Setup.iss"
```

### 4. Résultat

L'installeur sera créé ici :
```
C:\Users\Light\Documents\GR-Mods\Installer\GR-Mods-Setup-1.0.0.exe
```

## Utilisation de l'installeur

1. Double-cliquez sur `GR-Mods-Setup-1.0.0.exe`
2. Suivez l'assistant d'installation
3. L'application sera installée dans `C:\Program Files\GR Mods\`
4. Un raccourci sera créé dans le menu Démarrer
5. Optionnel : Créer un raccourci sur le bureau

## Désinstallation

- Via le Panneau de configuration → Programmes et fonctionnalités
- Ou via le menu Démarrer → GR Mods → Désinstaller

## Personnalisation

Pour modifier l'installeur, éditez le fichier `GR-Mods-Setup.iss` :
- Changez le numéro de version : `#define MyAppVersion "1.0.0"`
- Modifiez le nom de l'éditeur : `#define MyAppPublisher "VotreNom"`
- Ajoutez des fichiers supplémentaires dans la section `[Files]`

## Signature de l'installeur (optionnel)

Pour signer numériquement l'installeur avec un certificat :
1. Obtenez un certificat de signature de code
2. Ajoutez dans la section `[Setup]` :
```
SignTool=signtool sign /f "chemin\vers\certificat.pfx" /p "motdepasse" $f
```

## Distribution

Une fois créé, l'installeur `GR-Mods-Setup-1.0.0.exe` peut être :
- Partagé directement
- Hébergé sur GitHub Releases
- Distribué via votre site web
- Aucune autre dépendance nécessaire
