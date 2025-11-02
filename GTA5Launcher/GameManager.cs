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
        private const string GAME_EXE = "GTA5.exe";

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

            var exePath = Path.Combine(path, GAME_EXE);
            if (!File.Exists(exePath))
                return false;

            // Additional validation: check for other essential files
            // This helps avoid false positives with Legacy versions
            var essentialFiles = new[]
            {
                "GTA5.exe",
                "PlayGTAV.exe",
                "GTAVLauncher.exe"
            };

            int foundFiles = 0;
            foreach (var file in essentialFiles)
            {
                if (File.Exists(Path.Combine(path, file)))
                    foundFiles++;
            }

            // Need at least 2 of the 3 essential files
            return foundFiles >= 2;
        }

        private bool IsGameRunning()
        {
            var processes = Process.GetProcessesByName("GTA5");
            return processes.Length > 0;
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
