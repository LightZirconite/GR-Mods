using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace GTA5Launcher
{
    public class NotificationService
    {
        [DllImport("user32.dll")]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }

        private const uint FLASHW_ALL = 3;
        private const uint FLASHW_TIMERNOFG = 12;

        public void ShowNotification(string title, string message)
        {
            try
            {
                // Try to use PowerShell to show a Windows 10 toast notification
                string script = $@"
                    [Windows.UI.Notifications.ToastNotificationManager, Windows.UI.Notifications, ContentType = WindowsRuntime] | Out-Null
                    [Windows.UI.Notifications.ToastNotification, Windows.UI.Notifications, ContentType = WindowsRuntime] | Out-Null
                    [Windows.Data.Xml.Dom.XmlDocument, Windows.Data.Xml.Dom.XmlDocument, ContentType = WindowsRuntime] | Out-Null

                    $template = @""
                    <toast>
                        <visual>
                            <binding template='ToastGeneric'>
                                <text>{title}</text>
                                <text>{message}</text>
                            </binding>
                        </visual>
                    </toast>
                    ""@

                    $xml = New-Object Windows.Data.Xml.Dom.XmlDocument
                    $xml.LoadXml($template)
                    $toast = New-Object Windows.UI.Notifications.ToastNotification $xml
                    [Windows.UI.Notifications.ToastNotificationManager]::CreateToastNotifier('GR Mods').Show($toast)
                ";

                var processInfo = new ProcessStartInfo
                {
                    FileName = "powershell.exe",
                    Arguments = $"-NoProfile -ExecutionPolicy Bypass -Command \"{script}\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden
                };

                var process = Process.Start(processInfo);
                process?.WaitForExit(5000); // Wait max 5 seconds
            }
            catch
            {
                // Fallback: just flash the window
                // Silent fail
            }
        }

        public void FlashWindow(IntPtr windowHandle)
        {
            try
            {
                FLASHWINFO fInfo = new FLASHWINFO
                {
                    cbSize = Convert.ToUInt32(Marshal.SizeOf<FLASHWINFO>()),
                    hwnd = windowHandle,
                    dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG,
                    uCount = 3,
                    dwTimeout = 0
                };

                FlashWindowEx(ref fInfo);
            }
            catch
            {
                // Silent fail
            }
        }
    }
}
