# GR Mods - GTA V Platform Launcher

Un launcher intelligent pour GTA V qui permet de déplacer le jeu entre différentes plateformes (Steam, Rockstar Games, Epic Games) sans avoir à maintenir plusieurs copies du jeu.

## ✨ Fonctionnalités

### Core Features
- **Détection automatique intelligente** : Trouve GTA V sur toutes les plateformes, même sur différents disques
- **Détection Steam dynamique** : Lit le registre Windows pour trouver votre bibliothèque Steam
- **Interface moderne** : Interface WPF élégante avec logos des plateformes
- **Déplacement robuste** : Gère les déplacements entre disques différents automatiquement

### 🆕 Nouveautés Version 1.1.0
- **✅ Vérification d'espace disque** : Contrôle automatique avant transfert (+ marge de sécurité 10%)
- **📊 Progression en temps réel** : Barre de progression avec pourcentage, vitesse (MB/s) et temps restant
- **🔍 Détection des launchers actifs** : Vérifie que Steam/Epic/Rockstar sont fermés avant transfert
- **✔️ Vérification d'intégrité** : Contrôle des fichiers essentiels après déplacement
- **📋 Viewer de logs intégré** : Consultation des logs directement dans l'interface
- **🎮 Détection de mods** : Avertit si des mods sont détectés (ScriptHookV, OpenIV, etc.)
- **🔔 Notifications Windows** : Toast notification à la fin du transfert
- **🔄 Auto-update** : Vérification automatique des mises à jour via GitHub
- **⏸️ Support d'annulation** : Possibilité d'annuler un transfert en cours

### Sécurité & Fiabilité
- **Vérification que GTA V n'est pas en cours d'exécution**
- **Système de rollback en cas d'erreur**
- **Demande les droits administrateur**
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
3. Si des mods sont détectés, un avertissement s'affiche
4. Si plusieurs installations sont trouvées, un avertissement s'affiche
5. Cliquez sur le logo de la plateforme cible
6. Confirmez le déplacement
7. Suivez la progression en temps réel (pourcentage, vitesse, temps restant)
8. Une notification vous informe de la fin du transfert
9. Le launcher vérifie automatiquement l'intégrité de l'installation

**Note** : Le launcher gère automatiquement les déplacements entre disques différents.

**Nouveau** : Cliquez sur "📋 Logs" en bas pour consulter l'historique des opérations.

## ⚙️ Améliorations techniques

### v1.1.0 (Actuelle)
- ✅ **Vérification d'espace disque** : Contrôle automatique avec marge de sécurité
- ✅ **Progression en temps réel** : Affichage détaillé avec IProgress<T>
- ✅ **Détection launchers actifs** : Vérifie Steam.exe, EpicGamesLauncher.exe, etc.
- ✅ **Vérification d'intégrité** : Contrôle des fichiers essentiels après transfert
- ✅ **Viewer de logs** : Interface dédiée avec actions (actualiser, effacer, copier)
- ✅ **Détection de mods** : Avertit si ScriptHookV, OpenIV ou dossiers mods détectés
- ✅ **Notifications Windows** : Toast notification + flash de fenêtre
- ✅ **Auto-update** : Vérification GitHub Releases au démarrage
- ✅ **Support CancellationToken** : Infrastructure pour annuler les transferts

### v1.0 
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
│   ├── epic-games.png
│   └── GR-Mods.ico
├── GTA5Launcher/        # Code source
│   ├── App.xaml         # Configuration WPF + Styles
│   ├── MainWindow.xaml  # Interface principale
│   ├── MainWindow.xaml.cs
│   ├── LogViewerWindow.xaml  # 🆕 Fenêtre de logs
│   ├── LogViewerWindow.xaml.cs
│   ├── GameManager.cs   # Logique de déplacement
│   ├── UpdateChecker.cs # 🆕 Vérification des mises à jour
│   ├── NotificationService.cs # 🆕 Notifications Windows
│   └── app.manifest     # Manifest pour droits admin
├── build.ps1            # Build Release + Installeur
├── build-dev.ps1        # Build Dev rapide
└── GTA5Launcher.sln     # Solution Visual Studio
```

## Logs

Les logs sont sauvegardés dans : `%AppData%\GTA5Launcher\logs.txt`

**Nouveau** : Vous pouvez maintenant consulter les logs directement depuis l'interface en cliquant sur le bouton "📋 Logs" en bas de la fenêtre principale.

### Actions disponibles dans le viewer de logs :
- 🔄 **Actualiser** : Recharger les logs
- 🗑️ **Effacer** : Supprimer tout l'historique
- 📋 **Copier** : Copier les logs dans le presse-papiers
- 📂 **Ouvrir** : Ouvrir le fichier de logs avec l'éditeur par défaut

## Avertissements

⚠️ **Important** :
- Fermez le jeu et les launchers avant d'utiliser cet outil
- Le déplacement peut prendre du temps (le jeu fait ~100 GB)
- Assurez-vous d'avoir suffisamment d'espace disque
- Une sauvegarde est recommandée avant la première utilisation

## Licence

Ce projet est fourni tel quel, sans garantie.
