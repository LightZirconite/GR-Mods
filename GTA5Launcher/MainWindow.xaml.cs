using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Threading.Tasks;

namespace GTA5Launcher
{
    public partial class MainWindow : Window
    {
        private readonly GameManager gameManager;

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
                    CurrentLocationText.Text = $"✓ Plateforme : {currentPlatform.Name}\n📂 Chemin : {currentPlatform.Path}";
                    StatusText.Text = $"✓ GTA V trouvé sur {currentPlatform.Name}";
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
            ProgressBar.IsIndeterminate = true;
            StatusText.Text = $"🔄 Déplacement vers {platformName} en cours...";

            try
            {
                var success = await Task.Run(() => gameManager.MoveGameToPlatform(targetPlatform));

                if (success)
                {
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
