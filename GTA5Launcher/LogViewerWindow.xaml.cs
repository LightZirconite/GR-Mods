using System;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace GTA5Launcher
{
    public partial class LogViewerWindow : Window
    {
        private readonly string logFilePath;

        public LogViewerWindow()
        {
            InitializeComponent();
            
            logFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "GTA5Launcher",
                "logs.txt"
            );

            LoadLogs();
        }

        private void LoadLogs()
        {
            try
            {
                if (File.Exists(logFilePath))
                {
                    string logs = File.ReadAllText(logFilePath);
                    LogTextBlock.Text = string.IsNullOrWhiteSpace(logs) 
                        ? "Aucun log disponible." 
                        : logs;
                    
                    // Auto-scroll to bottom
                    LogScrollViewer.ScrollToEnd();
                }
                else
                {
                    LogTextBlock.Text = "Fichier de logs introuvable.\n\n" +
                                       $"Chemin attendu : {logFilePath}";
                }
            }
            catch (Exception ex)
            {
                LogTextBlock.Text = $"Erreur lors du chargement des logs :\n{ex.Message}";
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadLogs();
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Êtes-vous sûr de vouloir effacer tous les logs ?\n\n" +
                "Cette action est irréversible.",
                "Confirmation",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    if (File.Exists(logFilePath))
                    {
                        File.WriteAllText(logFilePath, string.Empty);
                        LoadLogs();
                        MessageBox.Show(
                            "Logs effacés avec succès.",
                            "Succès",
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(
                        $"Erreur lors de l'effacement des logs :\n{ex.Message}",
                        "Erreur",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error);
                }
            }
        }

        private void CopyButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(LogTextBlock.Text))
                {
                    Clipboard.SetText(LogTextBlock.Text);
                    MessageBox.Show(
                        "Logs copiés dans le presse-papiers !",
                        "Succès",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erreur lors de la copie :\n{ex.Message}",
                    "Erreur",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }

        private void OpenFileButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (File.Exists(logFilePath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = logFilePath,
                        UseShellExecute = true
                    });
                }
                else
                {
                    MessageBox.Show(
                        "Le fichier de logs n'existe pas encore.",
                        "Fichier introuvable",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    $"Erreur lors de l'ouverture du fichier :\n{ex.Message}",
                    "Erreur",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            }
        }
    }
}
