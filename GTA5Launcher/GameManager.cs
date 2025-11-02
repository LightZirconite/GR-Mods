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

    public class PlatformInfo
    {
        public PlatformType Type { get; set; }
        public string Name { get; set; }
        public string Path { get; set; }
        public long SizeInBytes { get; set; }
        
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
                            var gtaPath = Path.Combine(steamPath, "steamapps", "common", "Grand Theft Auto V Enhanced");
                            if (!paths.Contains(gtaPath))
                                paths.Insert(0, gtaPath);
                        }
                    }
                }
            }
            catch { }

            return paths;
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

            // Check Steam (using dynamic detection)
            foreach (var path in GetSteamLibraryPaths())
            {
                if (IsValidGTAInstallation(path))
                {
                    installations.Add(new PlatformInfo
                    {
                        Type = PlatformType.Steam,
                        Name = "Steam",
                        Path = path,
                        SizeInBytes = GetDirectorySize(path)
                    });
                }
            }

            // Check Rockstar
            foreach (var path in rockstarPaths)
            {
                if (IsValidGTAInstallation(path))
                {
                    installations.Add(new PlatformInfo
                    {
                        Type = PlatformType.Rockstar,
                        Name = "Rockstar Games",
                        Path = path,
                        SizeInBytes = GetDirectorySize(path)
                    });
                }
            }

            // Check Epic Games
            foreach (var path in epicPaths)
            {
                if (IsValidGTAInstallation(path))
                {
                    installations.Add(new PlatformInfo
                    {
                        Type = PlatformType.Epic,
                        Name = "Epic Games",
                        Path = path,
                        SizeInBytes = GetDirectorySize(path)
                    });
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

        private bool IsGameRunning()
        {
            // Check both Enhanced and Legacy versions
            var processesEnhanced = Process.GetProcessesByName("GTA5_Enhanced");
            var processesLegacy = Process.GetProcessesByName("GTA5");
            return processesEnhanced.Length > 0 || processesLegacy.Length > 0;
        }

        public bool MoveGameToPlatform(PlatformType targetPlatform)
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

            string targetPath = GetTargetPath(targetPlatform);
            
            if (string.IsNullOrEmpty(targetPath))
            {
                throw new Exception("Impossible de déterminer le chemin cible.");
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

                // Try simple move first (works if same drive)
                try
                {
                    Directory.Move(currentInstallation.Path, targetPath);
                    LogMessage("Déplacement rapide réussi (même disque)");
                }
                catch (IOException)
                {
                    // If move fails (different drives), use copy + delete
                    LogMessage("Déplacement entre disques différents détecté, utilisation de copie...");
                    CopyDirectory(currentInstallation.Path, targetPath);
                    LogMessage("Copie terminée, suppression de l'ancien emplacement...");
                    Directory.Delete(currentInstallation.Path, true);
                    LogMessage("Déplacement entre disques terminé");
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

        private void CopyDirectory(string sourceDir, string destDir)
        {
            Directory.CreateDirectory(destDir);

            foreach (string file in Directory.GetFiles(sourceDir))
            {
                string destFile = Path.Combine(destDir, Path.GetFileName(file));
                File.Copy(file, destFile, true);
            }

            foreach (string dir in Directory.GetDirectories(sourceDir))
            {
                string destSubDir = Path.Combine(destDir, Path.GetFileName(dir));
                CopyDirectory(dir, destSubDir);
            }
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
                                        installations.Add(new PlatformInfo
                                        {
                                            Type = platformType,
                                            Name = platformName,
                                            Path = folder,
                                            SizeInBytes = GetDirectorySize(folder)
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
