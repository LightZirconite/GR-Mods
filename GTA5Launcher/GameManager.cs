using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.Win32;

namespace GTA5Launcher
{
    public enum PlatformType
    {
        Steam,
        Rockstar,
        Epic
    }

    public class ProgressInfo
    {
        public long TotalBytes { get; set; }
        public long ProcessedBytes { get; set; }
        public double PercentComplete => TotalBytes > 0 ? (ProcessedBytes * 100.0) / TotalBytes : 0;
        public string CurrentFile { get; set; }
        public DateTime StartTime { get; set; }
        public double SpeedMBps { get; set; }
        public TimeSpan EstimatedTimeRemaining { get; set; }
    }

    public class FileSnapshot
    {
        public string RelativePath { get; set; }
        public long Size { get; set; }
    }

    public class PlatformInfo
    {
        public PlatformType Type { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public long SizeInBytes { get; set; }
        public bool HasMods { get; set; }
        public List<string> DetectedMods { get; set; } = new List<string>();
        
        public string GetSizeFormatted()
        {
            double sizeInGB = SizeInBytes / (1024.0 * 1024.0 * 1024.0);
            return $"{sizeInGB:F2} GB";
        }
    }

    public class GameManager
    {
        private const string GAME_FOLDER_NAME = "Grand Theft Auto V";
        // Enhanced version uses GTA5_Enhanced.exe instead of GTA5.exe
        private const string GAME_EXE_ENHANCED = "GTA5_Enhanced.exe";
        private const string GAME_EXE_LEGACY = "GTA5.exe";

        private List<string> GetSteamLibraryPaths()
        {
            var paths = new List<string>
            {
                @"C:\Program Files (x86)\Steam\steamapps\common\Grand Theft Auto V Enhanced",
                @"D:\SteamLibrary\steamapps\common\Grand Theft Auto V Enhanced",
                @"E:\SteamLibrary\steamapps\common\Grand Theft Auto V Enhanced"
            };

            // Try to detect Steam installation path from registry
            try
            {
                using (var key = Registry.CurrentUser.OpenSubKey(@"Software\Valve\Steam"))
                {
                    if (key != null)
                    {
                        var steamPath = key.GetValue("SteamPath") as string;
                        if (!string.IsNullOrEmpty(steamPath))
                        {
                            // Normalize path (convert forward slashes to backslashes)
                            steamPath = steamPath.Replace("/", "\\");
                            var gtaPath = Path.Combine(steamPath, "steamapps", "common", "Grand Theft Auto V Enhanced");
                            
                            // Normalize and check if path not already in list (case-insensitive)
                            gtaPath = Path.GetFullPath(gtaPath);
                            if (!paths.Any(p => Path.GetFullPath(p).Equals(gtaPath, StringComparison.OrdinalIgnoreCase)))
                                paths.Insert(0, gtaPath);
                        }
                    }
                }
            }
            catch { }

            // Remove duplicates and return distinct paths (normalize all paths)
            return paths
                .Select(p => Path.GetFullPath(p))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();
        }

        private readonly string[] rockstarPaths = new[]
        {
            @"C:\Program Files\Rockstar Games\Grand Theft Auto V Enhanced",
            @"C:\Program Files (x86)\Rockstar Games\Grand Theft Auto V Enhanced",
            @"D:\Rockstar Games\Grand Theft Auto V Enhanced",
            @"E:\Rockstar Games\Grand Theft Auto V Enhanced"
        };

        private readonly string[] epicPaths = new[]
        {
            @"C:\Program Files\Epic Games\GTA",
            @"C:\Program Files (x86)\Epic Games\GTA",
            @"D:\Epic Games\GTA",
            @"E:\Epic Games\GTA"
        };

        public List<PlatformInfo> DetectAllInstallations()
        {
            var installations = new List<PlatformInfo>();
            var foundPaths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Check Steam (using dynamic detection)
            foreach (var path in GetSteamLibraryPaths())
            {
                if (IsValidGTAInstallation(path))
                {
                    // Normalize path to avoid duplicates
                    var normalizedPath = Path.GetFullPath(path);
                    
                    if (!foundPaths.Contains(normalizedPath))
                    {
                        bool hasMods = DetectMods(path, out var detectedMods);
                        
                        installations.Add(new PlatformInfo
                        {
                            Type = PlatformType.Steam,
                            Name = "Steam",
                            Path = normalizedPath,
                            SizeInBytes = GetDirectorySize(path),
                            HasMods = hasMods,
                            DetectedMods = detectedMods
                        });
                        foundPaths.Add(normalizedPath);
                    }
                }
            }

            // Check Rockstar
            foreach (var path in rockstarPaths)
            {
                if (IsValidGTAInstallation(path))
                {
                    // Normalize path to avoid duplicates
                    var normalizedPath = Path.GetFullPath(path);
                    
                    if (!foundPaths.Contains(normalizedPath))
                    {
                        bool hasMods = DetectMods(path, out var detectedMods);
                        
                        installations.Add(new PlatformInfo
                        {
                            Type = PlatformType.Rockstar,
                            Name = "Rockstar Games",
                            Path = normalizedPath,
                            SizeInBytes = GetDirectorySize(path),
                            HasMods = hasMods,
                            DetectedMods = detectedMods
                        });
                        foundPaths.Add(normalizedPath);
                    }
                }
            }

            // Check Epic Games
            foreach (var path in epicPaths)
            {
                if (IsValidGTAInstallation(path))
                {
                    // Normalize path to avoid duplicates
                    var normalizedPath = Path.GetFullPath(path);
                    
                    if (!foundPaths.Contains(normalizedPath))
                    {
                        bool hasMods = DetectMods(path, out var detectedMods);
                        
                        installations.Add(new PlatformInfo
                        {
                            Type = PlatformType.Epic,
                            Name = "Epic Games",
                            Path = normalizedPath,
                            SizeInBytes = GetDirectorySize(path),
                            HasMods = hasMods,
                            DetectedMods = detectedMods
                        });
                        foundPaths.Add(normalizedPath);
                    }
                }
            }

            // Fallback: Search in all drives if nothing found
            if (installations.Count == 0)
            {
                LogMessage("No installations found in standard paths, performing deep search...");
                installations = PerformDeepSearch();
            }

            return installations;
        }

        public PlatformInfo DetectCurrentInstallation()
        {
            var installations = DetectAllInstallations();
            return installations.FirstOrDefault();
        }

        private bool IsValidGTAInstallation(string path)
        {
            if (!Directory.Exists(path))
                return false;

            // Check for Enhanced version first (priority)
            var exePathEnhanced = Path.Combine(path, GAME_EXE_ENHANCED);
            var exePathLegacy = Path.Combine(path, GAME_EXE_LEGACY);
            
            bool hasEnhanced = File.Exists(exePathEnhanced);
            bool hasLegacy = File.Exists(exePathLegacy);
            
            // Must have at least one main exe
            if (!hasEnhanced && !hasLegacy)
                return false;

            // Additional validation: check for PlayGTAV.exe (present in all versions)
            var playGTAPath = Path.Combine(path, "PlayGTAV.exe");
            if (!File.Exists(playGTAPath))
                return false;

            // We target Enhanced version primarily
            // If folder name contains "Enhanced", it must have GTA5_Enhanced.exe
            if (path.Contains("Enhanced", StringComparison.OrdinalIgnoreCase))
            {
                return hasEnhanced;
            }

            return true;
        }

        private bool VerifyInstallationIntegrity(string path)
        {
            try
            {
                LogMessage("Vérification de l'intégrité de l'installation...");
                
                // Check for main executable (Enhanced or Legacy)
                var enhancedExe = Path.Combine(path, GAME_EXE_ENHANCED);
                var legacyExe = Path.Combine(path, GAME_EXE_LEGACY);
                
                bool hasMainExe = File.Exists(enhancedExe) || File.Exists(legacyExe);
                if (!hasMainExe)
                {
                    LogMessage("Fichier manquant : Aucun exécutable principal trouvé (GTA5_Enhanced.exe ou GTA5.exe)");
                    return false;
                }

                // Check for PlayGTAV.exe (present in all versions)
                var playGTAPath = Path.Combine(path, "PlayGTAV.exe");
                if (!File.Exists(playGTAPath))
                {
                    LogMessage("Fichier manquant : PlayGTAV.exe");
                    return false;
                }

                // Check for essential game folders
                var essentialFolders = new[] { "update", "x64" };
                foreach (var folder in essentialFolders)
                {
                    var folderPath = Path.Combine(path, folder);
                    if (!Directory.Exists(folderPath))
                    {
                        LogMessage($"Dossier manquant : {folder}");
                        return false;
                    }
                }

                LogMessage("Vérification d'intégrité réussie");
                return true;
            }
            catch (Exception ex)
            {
                LogMessage($"Erreur lors de la vérification d'intégrité : {ex.Message}");
                return false;
            }
        }

        private List<FileSnapshot> CreateFileSnapshot(string rootPath)
        {
            var snapshot = new List<FileSnapshot>();
            
            try
            {
                LogMessage($"Création du snapshot de {rootPath}...");
                
                var allFiles = Directory.GetFiles(rootPath, "*", SearchOption.AllDirectories);
                
                foreach (var file in allFiles)
                {
                    try
                    {
                        var fileInfo = new FileInfo(file);
                        var relativePath = file.Substring(rootPath.Length).TrimStart('\\', '/');
                        
                        snapshot.Add(new FileSnapshot
                        {
                            RelativePath = relativePath,
                            Size = fileInfo.Length
                        });
                    }
                    catch
                    {
                        // Skip files we can't access
                        continue;
                    }
                }
                
                LogMessage($"Snapshot créé : {snapshot.Count} fichiers répertoriés");
            }
            catch (Exception ex)
            {
                LogMessage($"Erreur lors de la création du snapshot : {ex.Message}");
            }
            
            return snapshot;
        }

        private bool VerifyFileSnapshot(string rootPath, List<FileSnapshot> originalSnapshot)
        {
            try
            {
                LogMessage($"Vérification du snapshot sur {rootPath}...");
                
                int missingFiles = 0;
                int sizeMismatchFiles = 0;
                
                foreach (var originalFile in originalSnapshot)
                {
                    var targetFilePath = Path.Combine(rootPath, originalFile.RelativePath);
                    
                    if (!File.Exists(targetFilePath))
                    {
                        LogMessage($"Fichier manquant : {originalFile.RelativePath}");
                        missingFiles++;
                        continue;
                    }
                    
                    var targetFileInfo = new FileInfo(targetFilePath);
                    if (targetFileInfo.Length != originalFile.Size)
                    {
                        LogMessage($"Taille différente : {originalFile.RelativePath} (original: {originalFile.Size} bytes, cible: {targetFileInfo.Length} bytes)");
                        sizeMismatchFiles++;
                    }
                }
                
                if (missingFiles > 0 || sizeMismatchFiles > 0)
                {
                    LogMessage($"Vérification échouée : {missingFiles} fichiers manquants, {sizeMismatchFiles} fichiers avec taille différente");
                    return false;
                }
                
                LogMessage($"Vérification réussie : tous les {originalSnapshot.Count} fichiers sont présents et identiques");
                return true;
            }
            catch (Exception ex)
            {
                LogMessage($"Erreur lors de la vérification du snapshot : {ex.Message}");
                return false;
            }
        }

        private bool DetectMods(string path, out List<string> detectedMods)
        {
            detectedMods = new List<string>();
            
            try
            {
                // Common mod files
                var modFiles = new[]
                {
                    "ScriptHookV.dll",
                    "ScriptHookVDotNet.asi",
                    "ScriptHookVDotNet2.dll",
                    "ScriptHookVDotNet3.dll",
                    "dinput8.dll",
                    "OpenIV.asi",
                    "LibertyV.asi",
                    "NativeTrainer.asi"
                };

                foreach (var modFile in modFiles)
                {
                    var filePath = Path.Combine(path, modFile);
                    if (File.Exists(filePath))
                    {
                        detectedMods.Add(modFile);
                    }
                }

                // Check for mods folder
                var modsFolderPath = Path.Combine(path, "mods");
                if (Directory.Exists(modsFolderPath))
                {
                    detectedMods.Add("mods/ (dossier)");
                }

                // Check for scripts folder
                var scriptsFolderPath = Path.Combine(path, "scripts");
                if (Directory.Exists(scriptsFolderPath))
                {
                    detectedMods.Add("scripts/ (dossier)");
                }

                return detectedMods.Count > 0;
            }
            catch (Exception ex)
            {
                LogMessage($"Erreur lors de la détection de mods : {ex.Message}");
                return false;
            }
        }

        private bool IsGameRunning()
        {
            // Check both Enhanced and Legacy versions
            var processesEnhanced = Process.GetProcessesByName("GTA5_Enhanced");
            var processesLegacy = Process.GetProcessesByName("GTA5");
            return processesEnhanced.Length > 0 || processesLegacy.Length > 0;
        }

        private bool AreLaunchersRunning(out string runningLauncher)
        {
            runningLauncher = null;
            
            // Check for Steam
            if (Process.GetProcessesByName("steam").Length > 0)
            {
                runningLauncher = "Steam";
                return true;
            }
            
            // Check for Epic Games Launcher
            if (Process.GetProcessesByName("EpicGamesLauncher").Length > 0)
            {
                runningLauncher = "Epic Games Launcher";
                return true;
            }
            
            // Check for Rockstar Games Launcher
            if (Process.GetProcessesByName("RockstarGamesLauncher").Length > 0 ||
                Process.GetProcessesByName("Launcher").Length > 0)
            {
                runningLauncher = "Rockstar Games Launcher";
                return true;
            }
            
            return false;
        }

        private bool HasSufficientDiskSpace(string targetPath, long requiredBytes)
        {
            try
            {
                string targetDrive = Path.GetPathRoot(targetPath);
                DriveInfo driveInfo = new DriveInfo(targetDrive);
                
                // Add 10% safety margin
                long requiredWithMargin = (long)(requiredBytes * 1.1);
                
                return driveInfo.AvailableFreeSpace >= requiredWithMargin;
            }
            catch
            {
                // If we can't determine, assume there's enough space
                return true;
            }
        }

        public (bool hasSpace, long availableGB, long requiredGB) CheckDiskSpace(string targetPath, long requiredBytes)
        {
            try
            {
                string targetDrive = Path.GetPathRoot(targetPath);
                DriveInfo driveInfo = new DriveInfo(targetDrive);
                
                long availableBytes = driveInfo.AvailableFreeSpace;
                long requiredWithMargin = (long)(requiredBytes * 1.1);
                
                double availableGB = availableBytes / (1024.0 * 1024.0 * 1024.0);
                double requiredGB = requiredWithMargin / (1024.0 * 1024.0 * 1024.0);
                
                bool hasSpace = availableBytes >= requiredWithMargin;
                
                return (hasSpace, (long)availableGB, (long)Math.Ceiling(requiredGB));
            }
            catch
            {
                return (true, 0, 0);
            }
        }

        public bool MoveGameToPlatform(PlatformType targetPlatform, IProgress<ProgressInfo> progress = null, CancellationToken cancellationToken = default)
        {
            var currentInstallation = DetectCurrentInstallation();
            
            if (currentInstallation == null)
            {
                throw new Exception("Aucune installation de GTA V trouvée.");
            }

            if (currentInstallation.Type == targetPlatform)
            {
                throw new Exception("Le jeu est déjà sur cette plateforme.");
            }

            // Check if game is running
            if (IsGameRunning())
            {
                throw new Exception("GTA V est en cours d'exécution. Veuillez fermer le jeu avant de continuer.");
            }

            // Check if launchers are running
            if (AreLaunchersRunning(out string runningLauncher))
            {
                throw new Exception($"{runningLauncher} est en cours d'exécution.\n\nVeuillez fermer tous les launchers avant de continuer.");
            }

            string targetPath = GetTargetPath(targetPlatform);
            
            if (string.IsNullOrEmpty(targetPath))
            {
                throw new Exception("Impossible de déterminer le chemin cible.");
            }

            // Check disk space
            var (hasSpace, availableGB, requiredGB) = CheckDiskSpace(targetPath, currentInstallation.SizeInBytes);
            if (!hasSpace)
            {
                throw new Exception($"Espace disque insuffisant sur le lecteur cible.\n\n" +
                                  $"Requis : {requiredGB} GB (avec marge de sécurité)\n" +
                                  $"Disponible : {availableGB} GB\n\n" +
                                  $"Libérez au moins {requiredGB - availableGB} GB avant de continuer.");
            }

            try
            {
                // Create target directory if it doesn't exist
                string targetDir = Path.GetDirectoryName(targetPath);
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                LogMessage($"Déplacement de {currentInstallation.Path} vers {targetPath}");
                
                if (Directory.Exists(targetPath))
                {
                    throw new Exception($"Le répertoire cible existe déjà : {targetPath}");
                }

                // Create snapshot of all files BEFORE moving
                List<FileSnapshot> fileSnapshot = CreateFileSnapshot(currentInstallation.Path);

                // Try simple move first (works if same drive)
                try
                {
                    Directory.Move(currentInstallation.Path, targetPath);
                    LogMessage("Déplacement rapide réussi (même disque)");
                }
                catch (IOException)
                {
                    // If move fails (different drives), use copy + delete with progress
                    LogMessage("Déplacement entre disques différents détecté, utilisation de copie...");
                    
                    var progressInfo = new ProgressInfo
                    {
                        TotalBytes = currentInstallation.SizeInBytes,
                        StartTime = DateTime.Now
                    };
                    
                    CopyDirectoryWithProgress(currentInstallation.Path, targetPath, progressInfo, progress, cancellationToken);
                    
                    LogMessage("Copie terminée, suppression de l'ancien emplacement...");
                    Directory.Delete(currentInstallation.Path, true);
                    LogMessage("Déplacement entre disques terminé");
                }
                
                // Verify integrity with snapshot
                if (!VerifyFileSnapshot(targetPath, fileSnapshot))
                {
                    throw new Exception("Erreur : La vérification par snapshot a échoué.\n\n" +
                                      "Certains fichiers sont manquants ou ont une taille différente après le déplacement.");
                }
                
                LogMessage("Déplacement terminé avec succès");
                return true;
            }
            catch (Exception ex)
            {
                LogMessage($"Erreur pendant le déplacement : {ex.Message}");
                
                // Try to rollback if something went wrong
                if (Directory.Exists(targetPath) && !Directory.Exists(currentInstallation.Path))
                {
                    try
                    {
                        Directory.Move(targetPath, currentInstallation.Path);
                        LogMessage("Rollback réussi");
                    }
                    catch (Exception rollbackEx)
                    {
                        LogMessage($"Échec du rollback : {rollbackEx.Message}");
                    }
                }
                
                throw;
            }
        }

        private void CopyDirectoryWithProgress(string sourceDir, string destDir, ProgressInfo progressInfo, IProgress<ProgressInfo> progress, CancellationToken cancellationToken)
        {
            Directory.CreateDirectory(destDir);
            long lastReportTime = DateTime.Now.Ticks;
            const long reportInterval = TimeSpan.TicksPerSecond / 2; // Report every 0.5 seconds

            // Copy all files
            foreach (string file in Directory.GetFiles(sourceDir))
            {
                cancellationToken.ThrowIfCancellationRequested();
                
                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                FileInfo fileInfo = new FileInfo(file);
                
                progressInfo.CurrentFile = Path.GetFileName(file);
                
                // Copy file
                File.Copy(file, destFile, true);
                
                progressInfo.ProcessedBytes += fileInfo.Length;
                
                // Calculate speed and ETA
                var elapsed = DateTime.Now - progressInfo.StartTime;
                if (elapsed.TotalSeconds > 0)
                {
                    progressInfo.SpeedMBps = (progressInfo.ProcessedBytes / (1024.0 * 1024.0)) / elapsed.TotalSeconds;
                    
                    if (progressInfo.SpeedMBps > 0)
                    {
                        long remainingBytes = progressInfo.TotalBytes - progressInfo.ProcessedBytes;
                        double remainingSeconds = (remainingBytes / (1024.0 * 1024.0)) / progressInfo.SpeedMBps;
                        progressInfo.EstimatedTimeRemaining = TimeSpan.FromSeconds(remainingSeconds);
                    }
                }
                
                // Report progress at intervals
                long currentTime = DateTime.Now.Ticks;
                if (currentTime - lastReportTime >= reportInterval)
                {
                    progress?.Report(progressInfo);
                    lastReportTime = currentTime;
                }
            }

            // Copy all subdirectories
            foreach (string dir in Directory.GetDirectories(sourceDir))
            {
                cancellationToken.ThrowIfCancellationRequested();
                string destSubDir = Path.Combine(destDir, Path.GetFileName(dir));
                CopyDirectoryWithProgress(dir, destSubDir, progressInfo, progress, cancellationToken);
            }
            
            // Final progress report
            progress?.Report(progressInfo);
        }

        private string GetTargetPath(PlatformType platform)
        {
            switch (platform)
            {
                case PlatformType.Steam:
                    return GetSteamLibraryPaths()[0];
                case PlatformType.Rockstar:
                    return rockstarPaths[0];
                case PlatformType.Epic:
                    return epicPaths[0];
                default:
                    return null;
            }
        }

        private long GetDirectorySize(string path)
        {
            try
            {
                var dirInfo = new DirectoryInfo(path);
                long size = 0;

                // Get all files
                foreach (FileInfo file in dirInfo.GetFiles("*", SearchOption.AllDirectories))
                {
                    size += file.Length;
                }

                return size;
            }
            catch
            {
                return 0;
            }
        }

        private List<PlatformInfo> PerformDeepSearch()
        {
            var installations = new List<PlatformInfo>();
            LogMessage("Starting deep search across all drives...");

            try
            {
                // Get all available drives
                var drives = DriveInfo.GetDrives()
                    .Where(d => d.IsReady && d.DriveType == DriveType.Fixed)
                    .ToList();

                foreach (var drive in drives)
                {
                    LogMessage($"Scanning drive: {drive.Name}");
                    
                    // Search in common game directories
                    var searchPaths = new[]
                    {
                        Path.Combine(drive.Name, "Program Files", "Rockstar Games"),
                        Path.Combine(drive.Name, "Program Files (x86)", "Rockstar Games"),
                        Path.Combine(drive.Name, "Program Files", "Epic Games"),
                        Path.Combine(drive.Name, "Program Files (x86)", "Epic Games"),
                        Path.Combine(drive.Name, "Program Files (x86)", "Steam", "steamapps", "common"),
                        Path.Combine(drive.Name, "Steam", "steamapps", "common"),
                        Path.Combine(drive.Name, "SteamLibrary", "steamapps", "common"),
                        Path.Combine(drive.Name, "Games"),
                        Path.Combine(drive.Name, "Rockstar Games"),
                        Path.Combine(drive.Name, "Epic Games")
                    };

                    foreach (var searchPath in searchPaths)
                    {
                        if (!Directory.Exists(searchPath))
                            continue;

                        try
                        {
                            // Look for GTA V folders
                            var gtaFolders = Directory.GetDirectories(searchPath, "*Grand Theft Auto*", SearchOption.TopDirectoryOnly)
                                .Concat(Directory.GetDirectories(searchPath, "GTA*", SearchOption.TopDirectoryOnly))
                                .ToList();

                            foreach (var folder in gtaFolders)
                            {
                                if (IsValidGTAInstallation(folder))
                                {
                                    var platformType = DeterminePlatformFromPath(folder);
                                    var platformName = GetPlatformName(platformType);

                                    // Check if not already in list
                                    if (!installations.Any(i => i.Path.Equals(folder, StringComparison.OrdinalIgnoreCase)))
                                    {
                                        bool hasMods = DetectMods(folder, out var detectedMods);
                                        
                                        installations.Add(new PlatformInfo
                                        {
                                            Type = platformType,
                                            Name = platformName,
                                            Path = folder,
                                            SizeInBytes = GetDirectorySize(folder),
                                            HasMods = hasMods,
                                            DetectedMods = detectedMods
                                        });
                                        
                                        LogMessage($"Found installation: {folder} ({platformName})");
                                    }
                                }
                            }
                        }
                        catch (UnauthorizedAccessException)
                        {
                            // Skip directories we don't have access to
                            continue;
                        }
                        catch (Exception ex)
                        {
                            LogMessage($"Error searching {searchPath}: {ex.Message}");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error during deep search: {ex.Message}");
            }

            LogMessage($"Deep search completed. Found {installations.Count} installation(s).");
            return installations;
        }

        private PlatformType DeterminePlatformFromPath(string path)
        {
            if (path.Contains("Steam", StringComparison.OrdinalIgnoreCase))
                return PlatformType.Steam;
            if (path.Contains("Epic", StringComparison.OrdinalIgnoreCase))
                return PlatformType.Epic;
            if (path.Contains("Rockstar", StringComparison.OrdinalIgnoreCase))
                return PlatformType.Rockstar;
            
            // Default to Rockstar if unknown
            return PlatformType.Rockstar;
        }

        private string GetPlatformName(PlatformType type)
        {
            switch (type)
            {
                case PlatformType.Steam:
                    return "Steam";
                case PlatformType.Rockstar:
                    return "Rockstar Games";
                case PlatformType.Epic:
                    return "Epic Games";
                default:
                    return "Unknown";
            }
        }

        private void LogMessage(string message)
        {
            try
            {
                string logPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "GTA5Launcher",
                    "logs.txt"
                );

                string logDir = Path.GetDirectoryName(logPath);
                if (!Directory.Exists(logDir))
                {
                    Directory.CreateDirectory(logDir);
                }

                string logEntry = $"[{DateTime.Now:yyyy-MM-dd HH:mm:ss}] {message}\n";
                File.AppendAllText(logPath, logEntry);
            }
            catch
            {
                // Silent fail for logging
            }
        }
    }
}
