# Scripts de Build GR Mods

## build-dev.ps1 - Developpement Rapide ⚡

**Utilisation:** `.\build-dev.ps1`

**Caracteristiques:**
- ✅ **Ultra rapide** (~3 secondes)
- ✅ Build en mode Debug
- ✅ Copie automatique des assets
- ✅ Propose de lancer l'application directement
- ❌ Ne cree pas l'installeur

**Ideal pour:**
- Tests rapides pendant le developpement
- Voir les modifications immediatement
- Debugging

**Output:** 
`GTA5Launcher\bin\Debug\net8.0-windows\win-x64\GR-Mods.exe`

---

## build.ps1 - Build Complet 📦

**Utilisation:** `.\build.ps1`

**Caracteristiques:**
- ✅ Build en mode Release
- ✅ Self-contained (inclut .NET)
- ✅ Single-file executable
- ✅ Cree l'installeur avec Inno Setup
- ✅ Copie dans le dossier `exe/`
- ⏱️ Plus lent (~150 secondes)

**Ideal pour:**
- Version finale a distribuer
- Creation de l'installeur
- Release officielle

**Output:** 
`exe\GR-Mods-Setup.exe`

---

## Workflow Recommande

### Developpement quotidien:
```powershell
.\build-dev.ps1
# Tester, modifier le code
.\build-dev.ps1
# Tester a nouveau...
```

### Avant distribution:
```powershell
.\build.ps1
# Creer l'installeur final
```

---

## Comparatif

| Script | Temps | Mode | Installeur | Usage |
|--------|-------|------|------------|-------|
| `build-dev.ps1` | **3 sec** ⚡ | Debug | Non | Dev/Tests |
| `build.ps1` | **150 sec** 🐌 | Release | Oui | Distribution |

---

## Notes

- Les deux scripts copient automatiquement les assets (images)
- `build-dev.ps1` peut lancer l'application automatiquement
- `build.ps1` nettoie les anciens builds avant de compiler
- Tous les accents ont ete supprimes pour compatibilite PowerShell
