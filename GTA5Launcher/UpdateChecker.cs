using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using System.Diagnostics;

namespace GTA5Launcher
{
    public class GitHubRelease
    {
        public string tag_name { get; set; }
        public string name { get; set; }
        public string html_url { get; set; }
        public string body { get; set; }
        public bool prerelease { get; set; }
        public DateTime published_at { get; set; }
    }

    public class UpdateChecker
    {
        private const string GITHUB_API_URL = "https://api.github.com/repos/LightZirconite/GR-Mods/releases/latest";
        private const string CURRENT_VERSION = "0.0.1";

        public async Task<(bool hasUpdate, string latestVersion, string downloadUrl, string releaseNotes)> CheckForUpdatesAsync()
        {
            try
            {
                using (var client = new HttpClient())
                {
                    // GitHub API requires User-Agent header
                    client.DefaultRequestHeaders.Add("User-Agent", "GR-Mods-Updater");
                    client.Timeout = TimeSpan.FromSeconds(10);

                    var response = await client.GetAsync(GITHUB_API_URL);
                    
                    if (!response.IsSuccessStatusCode)
                        return (false, null, null, null);

                    var json = await response.Content.ReadAsStringAsync();
                    var release = JsonSerializer.Deserialize<GitHubRelease>(json);

                    if (release == null || string.IsNullOrEmpty(release.tag_name))
                        return (false, null, null, null);

                    // Remove 'v' prefix if present
                    string latestVersion = release.tag_name.TrimStart('v');
                    
                    // Compare versions
                    bool hasUpdate = IsNewerVersion(CURRENT_VERSION, latestVersion);

                    return (hasUpdate, latestVersion, release.html_url, release.body);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking for updates: {ex.Message}");
                return (false, null, null, null);
            }
        }

        private bool IsNewerVersion(string currentVersion, string latestVersion)
        {
            try
            {
                var current = Version.Parse(currentVersion);
                var latest = Version.Parse(latestVersion);
                
                return latest > current;
            }
            catch
            {
                // If version parsing fails, assume no update
                return false;
            }
        }

        public void OpenDownloadPage(string url)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error opening download page: {ex.Message}");
            }
        }
    }
}
