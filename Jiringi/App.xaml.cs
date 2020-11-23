using Photon.Jiringi.Config;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Photon.Jiringi
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly static StringBuilder logs;
        public static RootConfigHandler Setting { get; private set; }
        public static event EventHandler FileSettingChanged;
        private readonly static FileSystemWatcher watcher;

        static App()
        {
            logs = new StringBuilder();
            Setting = new RootConfigHandler();

            try
            {
                // Create a new FileSystemWatcher and set its properties.
                watcher = new FileSystemWatcher
                {
                    Path = Path.GetDirectoryName(Path.GetFullPath(Setting.setting_file_name)),
                    /* Watch for changes in LastAccess and LastWrite times, and 
                       the renaming of files or directories. */
                    NotifyFilter = NotifyFilters.LastWrite |
                        NotifyFilters.FileName | NotifyFilters.DirectoryName,
                    // Only watch text files.
                    Filter = Path.GetFileName(Path.GetFullPath(Setting.setting_file_name))
                };

                // Add event handlers.
                watcher.Changed += File_Changed;
                watcher.Created += File_Changed;
                watcher.Renamed += File_Changed;

                // Begin watching.
                watcher.EnableRaisingEvents = true;
            }
            catch (Exception ex) { Log("file watcher error", ex.Message); }
        }
        private static void File_Changed(object sender, FileSystemEventArgs e)
        {
            Setting = new RootConfigHandler(e.FullPath);
            FileSettingChanged?.Invoke(Setting, new EventArgs());
        }

        public static void Log(string message, string report = null)
        {
            logs.Append(DateTime.Now).Append("\t").Append(message).Append("\r\n");
            if (report != null) logs.Append(report).Append("\r\n");
        }
        public static string Logs()
        {
            return logs.ToString();
        }
    }
}
