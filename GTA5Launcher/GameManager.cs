using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Collections.Generic;

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
    }

    public class GameManager
    {
        private const string GAME_FOLDER_NAME = "Grand Theft Auto V";
        private const string GAME_EXE = "GTA5.exe";

        private readonly string[] steamPaths = new[]
        {
            @"C:\Program Files (x86)\Steam\steamapps\common\Grand Theft Auto V",
            @"D:\Steam\steamapps\common\Grand Theft Auto V",
            @"E:\Steam\steamapps\common\Grand Theft Auto V"
        };

        private readonly string[] rockstarPaths = new[]
        {
            @"C:\Program Files\Rockstar Games\Grand Theft Auto V",
            @"C:\Program Files (x86)\Rockstar Games\Grand Theft Auto V",
            @"D:\Rockstar Games\Grand Theft Auto V"
        };

        private readonly string[] epicPaths = new[]
        {
            @"C:\Program Files\Epic Games\GTAV",
            @"C:\Program Files (x86)\Epic Games\GTAV",
            @"D:\Epic Games\GTAV"
        };

        public List<PlatformInfo> DetectAllInstallations()
        {
            var installations = new List<PlatformInfo>();

            // Check Steam
            foreach (var path in steamPaths)
            {
                if (IsValidGTAInstallation(path))
                {
                    installations.Add(new PlatformInfo
                    {
                        Type = PlatformType.Steam,
                        Name = "Steam",
                        Path = path
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
                        Path = path
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
                        Path = path
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
            return File.Exists(exePath);
        }

        public bool MoveGameToPlatform(PlatformType targetPlatform)
        {
            var currentInstallation = DetectCurrentInstallation();
            
            if (currentInstallation == null)
            {
                throw new Exception("No GTA V installation found.");
            }

            if (currentInstallation.Type == targetPlatform)
            {
                throw new Exception("Game is already on this platform.");
            }

            string targetPath = GetTargetPath(targetPlatform);
            
            if (string.IsNullOrEmpty(targetPath))
            {
                throw new Exception("Could not determine target path.");
            }

            try
            {
                // Create target directory if it doesn't exist
                string targetDir = Path.GetDirectoryName(targetPath);
                if (!Directory.Exists(targetDir))
                {
                    Directory.CreateDirectory(targetDir);
                }

                // Move the directory
                LogMessage($"Moving from {currentInstallation.Path} to {targetPath}");
                
                if (Directory.Exists(targetPath))
                {
                    throw new Exception($"Target directory already exists: {targetPath}");
                }

                Directory.Move(currentInstallation.Path, targetPath);
                
                LogMessage("Move completed successfully");
                return true;
            }
            catch (Exception ex)
            {
                LogMessage($"Error during move: {ex.Message}");
                
                // Try to rollback if something went wrong
                if (Directory.Exists(targetPath) && !Directory.Exists(currentInstallation.Path))
                {
                    try
                    {
                        Directory.Move(targetPath, currentInstallation.Path);
                        LogMessage("Rollback successful");
                    }
                    catch (Exception rollbackEx)
                    {
                        LogMessage($"Rollback failed: {rollbackEx.Message}");
                    }
                }
                
                throw;
            }
        }

        private string GetTargetPath(PlatformType platform)
        {
            switch (platform)
            {
                case PlatformType.Steam:
                    return steamPaths[0];
                case PlatformType.Rockstar:
                    return rockstarPaths[0];
                case PlatformType.Epic:
                    return epicPaths[0];
                default:
                    return null;
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
