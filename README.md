# GR Mods - GTA V Platform Launcher

Un launcher intelligent pour GTA V qui permet de déplacer le jeu entre différentes plateformes (Steam, Rockstar Games, Epic Games) sans avoir à maintenir plusieurs copies du jeu.

## ✨ Fonctionnalités

- **Détection automatique intelligente** : Trouve GTA V sur toutes les plateformes, même sur différents disques
- **Détection Steam dynamique** : Lit le registre Windows pour trouver votre bibliothèque Steam
- **Interface moderne** : Interface WPF élégante avec logos des plateformes
- **Déplacement robuste** : Gère les déplacements entre disques différents automatiquement
- **Sécurité maximale** : 
  - Vérifie que GTA V n'est pas en cours d'exécution
  - Système de rollback en cas d'erreur
  - Demande les droits administrateur
- **Détection multiple** : Avertit si plusieurs installations sont présentes
- **Logs détaillés** : Tous les événements sont enregistrés pour le débogage

## 🚀 Installation

### Méthode simple (Recommandée)

1. Téléchargez `GR-Mods-Setup.exe` depuis le dossier `exe/`
2. Lancez l'installeur en tant qu'administrateur
3. Suivez l'assistant d'installation
4. L'application sera installée dans `C:\Program Files\GR Mods\`

### Build depuis les sources

**Prérequis :**
- .NET 8.0 SDK
- Inno Setup 6 (pour créer l'installeur)

**Compilation automatique :**
```powershell
cd C:\Users\Light\Documents\GR-Mods
.\build.ps1
```

Le script fait automatiquement :
- Nettoyage des anciens builds
- Compilation de l'application
- Copie des assets (logos)
- Création de l'installeur
- Copie de l'installeur dans `exe/`
- Nettoyage des fichiers temporaires

## 📋 Utilisation

1. Lancez **GR Mods** (droits administrateur requis)
2. Le launcher détecte automatiquement où GTA V est installé
3. Si plusieurs installations sont trouvées, un avertissement s'affiche
4. Cliquez sur le logo de la plateforme cible
5. Confirmez le déplacement
6. Attendez la fin du transfert (peut prendre plusieurs minutes)

**Note** : Le launcher gère automatiquement les déplacements entre disques différents.

## ⚙️ Améliorations techniques

### v1.0 (Actuelle)
- ✅ Détection Steam via registre Windows
- ✅ Vérification que GTA V n'est pas lancé
- ✅ Déplacement entre disques différents (copie + suppression)
- ✅ Détection de multiples installations
- ✅ Messages d'erreur en français
- ✅ Script de build automatisé
- ✅ Installeur professionnel avec Inno Setup
- ✅ Logs détaillés pour débogage

## Chemins supportés

### Steam
- `C:\Program Files (x86)\Steam\steamapps\common\Grand Theft Auto V`
- `D:\Steam\steamapps\common\Grand Theft Auto V`
- `E:\Steam\steamapps\common\Grand Theft Auto V`

### Rockstar Games
- `C:\Program Files\Rockstar Games\Grand Theft Auto V`
- `C:\Program Files (x86)\Rockstar Games\Grand Theft Auto V`
- `D:\Rockstar Games\Grand Theft Auto V`

### Epic Games
- `C:\Program Files\Epic Games\GTAV`
- `C:\Program Files (x86)\Epic Games\GTAV`
- `D:\Epic Games\GTAV`

## Structure du projet

```
GR-Mods/
├── assets/               # Logos des plateformes
│   ├── steam.png
│   ├── rockstar.png
│   └── epic-games.png
├── GTA5Launcher/        # Code source
│   ├── App.xaml         # Configuration WPF
│   ├── MainWindow.xaml  # Interface principale
│   ├── GameManager.cs   # Logique de déplacement
│   └── app.manifest     # Manifest pour droits admin
└── GTA5Launcher.sln     # Solution Visual Studio
```

## Logs

Les logs sont sauvegardés dans : `%AppData%\GTA5Launcher\logs.txt`

## Avertissements

⚠️ **Important** :
- Fermez le jeu et les launchers avant d'utiliser cet outil
- Le déplacement peut prendre du temps (le jeu fait ~100 GB)
- Assurez-vous d'avoir suffisamment d'espace disque
- Une sauvegarde est recommandée avant la première utilisation

## Licence

Ce projet est fourni tel quel, sans garantie.
