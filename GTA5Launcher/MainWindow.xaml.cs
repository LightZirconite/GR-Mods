using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;
using System.Windows.Interop;

namespace GTA5Launcher
{
    public partial class MainWindow : Window
    {
        private readonly GameManager gameManager;
        private CancellationTokenSource cancellationTokenSource;

        public MainWindow()
        {
            InitializeComponent();
            gameManager = new GameManager();
            Loaded += MainWindow_Loaded;
            
            // Load images properly
            LoadPlatformImages();
        }

        private void LoadPlatformImages()
        {
            try
            {
                string assetsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets");
                
                // Load Steam image
                string steamPath = Path.Combine(assetsPath, "steam.png");
                if (File.Exists(steamPath))
                {
                    SteamImage.Source = new BitmapImage(new Uri(steamPath, UriKind.Absolute));
                }
                else
                {
                    LogMessage($"Steam image not found at: {steamPath}");
                }
                
                // Load Rockstar image
                string rockstarPath = Path.Combine(assetsPath, "rockstar.png");
                if (File.Exists(rockstarPath))
                {
                    RockstarImage.Source = new BitmapImage(new Uri(rockstarPath, UriKind.Absolute));
                }
                else
                {
                    LogMessage($"Rockstar image not found at: {rockstarPath}");
                }
                
                // Load Epic image
                string epicPath = Path.Combine(assetsPath, "epic-games.png");
                if (File.Exists(epicPath))
                {
                    EpicImage.Source = new BitmapImage(new Uri(epicPath, UriKind.Absolute));
                }
                else
                {
                    LogMessage($"Epic image not found at: {epicPath}");
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Error loading images: {ex.Message}");
                MessageBox.Show(
                    $"Attention: Certaines images n'ont pas pu être chargées.\n\nErreur: {ex.Message}",
                    "Images manquantes",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
            }
        }

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            await DetectCurrentInstallation();
            
            // Check for updates in background
            _ = CheckForUpdatesAsync();
        }

        private async Task CheckForUpdatesAsync()
        {
            try
            {
                var updateChecker = new UpdateChecker();
                var (hasUpdate, latestVersion, downloadUrl, releaseNotes) = await updateChecker.CheckForUpdatesAsync();

                if (hasUpdate)
                {
                    var result = MessageBox.Show(
                        $"🎉 Une nouvelle version de GR Mods est disponible !\n\n" +
                        $"Version actuelle : 0.0.1\n" +
                        $"Nouvelle version : {latestVersion}\n\n" +
                        $"Voulez-vous télécharger la mise à jour ?",
                        "Mise à jour disponible",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Information);

                    if (result == MessageBoxResult.Yes)
                    {
                        updateChecker.OpenDownloadPage(downloadUrl);
                    }
                }
            }
            catch (Exception ex)
            {
                LogMessage($"Erreur lors de la vérification des mises à jour : {ex.Message}");
                // Don't show error to user, just log it
            }
        }

        private async Task DetectCurrentInstallation()
        {
            StatusText.Text = "Recherche de GTA V...";
            
            var allInstallations = await Task.Run(() => gameManager.DetectAllInstallations());
            
            if (allInstallations.Count > 0)
            {
                var currentPlatform = allInstallations[0];
                
                if (allInstallations.Count > 1)
                {
                    // Multiple installations found
                    var platforms = string.Join(", ", allInstallations.Select(p => p.Name));
                    CurrentLocationText.Text = $"⚠️ ATTENTION : {allInstallations.Count} installations détectées !\n" +
                                              $"Plateformes : {platforms}\n\n" +
                                              $"Installation principale : {currentPlatform.Name}\n" +
                                              $"Chemin : {currentPlatform.Path}\n\n" +
                                              $"Il est recommandé de n'avoir qu'une seule installation.";
                    StatusText.Text = $"⚠️ {allInstallations.Count} installations trouvées - Nettoyage recommandé";
                    
                    MessageBox.Show(
                        $"Plusieurs installations de GTA V ont été détectées :\n\n" +
                        string.Join("\n", allInstallations.Select(p => $"• {p.Name} : {p.Path}")) +
                        $"\n\nPour éviter les problèmes, il est recommandé de n'avoir qu'une seule installation.\n" +
                        $"Supprimez manuellement les copies inutiles avant d'utiliser ce launcher.",
                        "Plusieurs installations détectées",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
                else
                {
                    string modsInfo = "";
                    if (currentPlatform.HasMods && currentPlatform.DetectedMods.Count > 0)
                    {
                        modsInfo = $"\n⚠️ Mods détectés : {string.Join(", ", currentPlatform.DetectedMods.Take(3))}" +
                                  (currentPlatform.DetectedMods.Count > 3 ? $"... (+{currentPlatform.DetectedMods.Count - 3})" : "");
                    }
                    
                    CurrentLocationText.Text = $"✓ Plateforme : {currentPlatform.Name}\n" +
                                              $"📂 Chemin : {currentPlatform.Path}\n" +
                                              $"💾 Taille : {currentPlatform.GetSizeFormatted()}" + modsInfo;
                    StatusText.Text = $"✓ GTA V trouvé sur {currentPlatform.Name} ({currentPlatform.GetSizeFormatted()})";
                    
                    // Show warning if mods detected
                    if (currentPlatform.HasMods)
                    {
                        MessageBox.Show(
                            $"⚠️ Des mods ont été détectés dans votre installation GTA V :\n\n" +
                            string.Join("\n", currentPlatform.DetectedMods.Select(m => $"• {m}")) +
                            $"\n\nATTENTION : Les mods peuvent ne pas fonctionner correctement après un changement de plateforme.\n" +
                            $"Assurez-vous de sauvegarder vos mods avant de continuer.",
                            "Mods détectés",
                            MessageBoxButton.OK,
                            MessageBoxImage.Warning);
                    }
                }
                
                // Show active badge and disable current platform button
                ShowActivePlatform(currentPlatform.Type);
            }
            else
            {
                CurrentLocationText.Text = "❌ GTA V n'a pas été trouvé sur votre système.\nVeuillez vérifier que le jeu est installé.";
                StatusText.Text = "❌ Aucune installation détectée";
                DisableAllButtons();
            }
        }

        private void ShowActivePlatform(PlatformType type)
        {
            // Hide all badges first
            SteamActiveBadge.Visibility = Visibility.Collapsed;
            RockstarActiveBadge.Visibility = Visibility.Collapsed;
            EpicActiveBadge.Visibility = Visibility.Collapsed;
            
            // Enable all buttons
            SteamButton.IsEnabled = true;
            RockstarButton.IsEnabled = true;
            EpicButton.IsEnabled = true;
            
            // Show active badge and disable button for current platform
            switch (type)
            {
                case PlatformType.Steam:
                    SteamActiveBadge.Visibility = Visibility.Visible;
                    SteamButton.IsEnabled = false;
                    break;
                case PlatformType.Rockstar:
                    RockstarActiveBadge.Visibility = Visibility.Visible;
                    RockstarButton.IsEnabled = false;
                    break;
                case PlatformType.Epic:
                    EpicActiveBadge.Visibility = Visibility.Visible;
                    EpicButton.IsEnabled = false;
                    break;
            }
        }

        private void DisableAllButtons()
        {
            SteamButton.IsEnabled = false;
            RockstarButton.IsEnabled = false;
            EpicButton.IsEnabled = false;
        }

        private void EnableAllButtons()
        {
            SteamButton.IsEnabled = true;
            RockstarButton.IsEnabled = true;
            EpicButton.IsEnabled = true;
        }

        private async void SteamButton_Click(object sender, RoutedEventArgs e)
        {
            await MoveGameToPlatform(PlatformType.Steam, "Steam");
        }

        private async void RockstarButton_Click(object sender, RoutedEventArgs e)
        {
            await MoveGameToPlatform(PlatformType.Rockstar, "Rockstar Games");
        }

        private async void EpicButton_Click(object sender, RoutedEventArgs e)
        {
            await MoveGameToPlatform(PlatformType.Epic, "Epic Games");
        }

        private async Task MoveGameToPlatform(PlatformType targetPlatform, string platformName)
        {
            var result = MessageBox.Show(
                $"Êtes-vous sûr de vouloir déplacer GTA V vers {platformName} ?\n\n" +
                $"Cette opération peut prendre plusieurs minutes selon la taille du jeu.",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes)
                return;

            DisableAllButtons();
            ProgressBar.Visibility = Visibility.Visible;
            ProgressBar.IsIndeterminate = false;
            ProgressBar.Value = 0;
            StatusText.Text = $"🔄 Préparation du déplacement vers {platformName}...";

            cancellationTokenSource = new CancellationTokenSource();
            var progress = new Progress<ProgressInfo>(progressInfo =>
            {
                // Update UI on UI thread
                Dispatcher.Invoke(() =>
                {
                    ProgressBar.Value = progressInfo.PercentComplete;
                    
                    string speedText = progressInfo.SpeedMBps > 0 
                        ? $" • {progressInfo.SpeedMBps:F1} MB/s" 
                        : "";
                    
                    string etaText = progressInfo.EstimatedTimeRemaining.TotalSeconds > 0 
                        ? $" • Temps restant: {FormatTimeSpan(progressInfo.EstimatedTimeRemaining)}" 
                        : "";
                    
                    StatusText.Text = $"🔄 {progressInfo.PercentComplete:F1}% - {progressInfo.CurrentFile}{speedText}{etaText}";
                });
            });

            try
            {
                var success = await Task.Run(() => 
                    gameManager.MoveGameToPlatform(targetPlatform, progress, cancellationTokenSource.Token));

                if (success)
                {
                    // Show notification
                    var notificationService = new NotificationService();
                    notificationService.ShowNotification(
                        "GR Mods - Transfert terminé",
                        $"GTA V a été déplacé avec succès vers {platformName} !"
                    );
                    
                    // Flash window if not focused
                    var windowHandle = new WindowInteropHelper(this).Handle;
                    notificationService.FlashWindow(windowHandle);
                    
                    MessageBox.Show(
                        $"✓ GTA V a été déplacé avec succès vers {platformName} !",
                        "Succès",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);

                    await DetectCurrentInstallation();
                }
                else
                {
                    MessageBox.Show(
                        $"❌ Erreur lors du déplacement vers {platformName}.\n" +
                        $"Vérifiez les logs pour plus d'informations.",
                        "Erreur",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                    
                    EnableAllButtons();
                }
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show(
                    "⏸️ Opération annulée par l'utilisateur.",
                    "Annulé",
                    MessageBoxButton.OK,
                    MessageBoxImage.Warning);
                
                EnableAllButtons();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"❌ Une erreur est survenue :\n{ex.Message}",
                    "Erreur",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
                
                EnableAllButtons();
            }
            finally
            {
                ProgressBar.Visibility = Visibility.Collapsed;
                ProgressBar.IsIndeterminate = false;
                cancellationTokenSource?.Dispose();
                cancellationTokenSource = null;
            }
        }

        private string FormatTimeSpan(TimeSpan timeSpan)
        {
            if (timeSpan.TotalHours >= 1)
                return $"{(int)timeSpan.TotalHours}h {timeSpan.Minutes}m";
            else if (timeSpan.TotalMinutes >= 1)
                return $"{(int)timeSpan.TotalMinutes}m {timeSpan.Seconds}s";
            else
                return $"{timeSpan.Seconds}s";
        }

        private void LogsButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var logViewer = new LogViewerWindow();
                logViewer.ShowDialog();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erreur lors de l'ouverture du viewer de logs :\n{ex.Message}",
                    "Erreur",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
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
