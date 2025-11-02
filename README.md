# GTA V Enhanced Launcher

Un launcher intelligent pour GTA V qui permet de déplacer le jeu entre différentes plateformes (Steam, Rockstar Games, Epic Games) sans avoir à maintenir plusieurs copies du jeu.

## Fonctionnalités

- **Détection automatique** : Trouve automatiquement où GTA V est installé
- **Interface moderne** : Interface WPF élégante et intuitive avec les logos des plateformes
- **Déplacement simple** : Un clic pour déplacer le jeu vers la plateforme de votre choix
- **Permissions admin** : Demande automatiquement les droits administrateur nécessaires
- **Sécurisé** : Système de rollback en cas d'erreur pendant le déplacement

## Prérequis

- Windows 10 ou supérieur
- .NET 6.0 ou supérieur
- Visual Studio 2022 (pour compiler)
- Droits administrateur

## Installation

1. Clonez ce repository
2. Ouvrez `GTA5Launcher.sln` dans Visual Studio 2022
3. Compilez le projet en mode Release
4. L'exécutable se trouvera dans `bin/Release/net6.0-windows/`

## Utilisation

1. Lancez `GTA5Launcher.exe` en tant qu'administrateur
2. Le launcher détectera automatiquement où GTA V est installé
3. Cliquez sur le logo de la plateforme vers laquelle vous souhaitez déplacer le jeu
4. Confirmez l'opération
5. Attendez la fin du déplacement (cela peut prendre plusieurs minutes)

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
