using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Photon.Jiringi.Config
{
    public class RootConfigHandler : ConfigHandler
    {
        public RootConfigHandler(string path = null) : base(ValidPath(ref path))
        {
            FileName = path;
            StartWatching();
        }

        #region File Load and Save

        private const string default_setting_file_name = "setting.json";
        public string FileName { get; }
        private static string ValidPath(ref string path)
        {
            if (string.IsNullOrWhiteSpace(path))
                path = default_setting_file_name;
            path = Path.GetFullPath(path);
            return path;
        }
        public void Save()
        {
            var start_again = Watching;
            StopWatching();

            WriteTo(FileName);
            App.Log($"The setting ({Count} node(s)) is saved.");
            App.LogRecording();

            if (start_again) StartWatching();
        }
        #endregion


        #region File Watcher

        private FileSystemWatcher watcher;
        public bool Watching
        {
            get { lock (FileName) return watcher != null; }
            set
            {
                if (value) StartWatching();
                else StopWatching();
            }
        }
        private void StartWatching()
        {
            lock (FileName)
            {
                if (watcher != null) return;
                try
                {
                    // Create a new FileSystemWatcher and set its properties.
                    watcher = new FileSystemWatcher
                    {
                        Path = Path.GetDirectoryName(FileName),
                        /* Watch for changes in LastAccess and LastWrite times, and 
                           the renaming of files or directories. */
                        NotifyFilter = NotifyFilters.LastWrite,
                        // Only watch text files.
                        Filter = Path.GetFileName(FileName)
                    };

                    // Add event handlers.
                    watcher.Changed += new FileSystemEventHandler(File_Changed);

                    // Begin watching.
                    watcher.EnableRaisingEvents = true;
                }
                catch (Exception ex) { App.Log("file watcher error", ex.Message); }
            }
        }
        private void StopWatching()
        {
            lock (FileName)
            {
                if (watcher == null) return;

                watcher.EnableRaisingEvents = false;

                watcher.Dispose();
                watcher = null;
            }
        }
        private void File_Changed(object sender, FileSystemEventArgs e)
        {
            if (e.FullPath != FileName) return;
            Thread.Sleep(100); // sure file is closed by other editor
            LoadFrom(FileName, (log_message) =>
            {
                initial_nets_info_instance = null;
                process = null;
                App.Log($"{log_message} ({e.ChangeType})");
            });
        }
        #endregion


        private const string data_provider = "data-provider";
        public string DataProvider
        {
            get { return GetSetting(data_provider, ""); }
            set { SetSetting(data_provider, value); }
        }

        private InitialNetsInfoConfigHandler initial_nets_info_instance;
        public InitialNetsInfoConfigHandler InitialNetsInfo
        {
            get
            {
                if (initial_nets_info_instance == null)
                    initial_nets_info_instance = new InitialNetsInfoConfigHandler(
                        GetOrCreateConfig(InitialNetsInfoConfigHandler.key));
                return initial_nets_info_instance;
            }
        }

        private ProcessConfigHandler process;
        public ProcessConfigHandler Process
        {
            get
            {
                if (process == null)
                    process = new ProcessConfigHandler(GetOrCreateConfig(ProcessConfigHandler.key));
                return process;
            }
        }

    }
}
