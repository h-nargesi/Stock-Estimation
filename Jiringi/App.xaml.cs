using Photon.Jiringi.Config;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace Photon.Jiringi
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private readonly static FileSystemWatcher watcher;
        public static event EventHandler FileSettingChanged;
        public static RootConfigHandler Setting { get; private set; }

        static App()
        {
            Setting = new RootConfigHandler($"setting.json");


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
                    Filter = "*.json"
                };

                // Add event handlers.
                watcher.Changed += File_Changed;
                watcher.Created += File_Changed;
                watcher.Renamed += File_Changed;

                // Begin watching.
                watcher.EnableRaisingEvents = true;
            }
            catch (Exception) { }
        }
        private static void File_Changed(object sender, FileSystemEventArgs e)
        {
            Setting = new RootConfigHandler(e.FullPath);
            FileSettingChanged?.Invoke(Setting, new EventArgs());
        }

    }
}
