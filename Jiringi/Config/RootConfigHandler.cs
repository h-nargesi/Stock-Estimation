using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Photon.Jiringi.Config
{
    public class RootConfigHandler : ConfigHandler
    {
        public RootConfigHandler(string path = null) : base(Read(ref path))
        {
            setting_file_name = path;
        }

        private const string default_setting_file_name = "setting.json";
        public readonly string setting_file_name = null;
        private static JObject Read(ref string path)
        {
            if (!string.IsNullOrEmpty(path))
                path = default_setting_file_name;
            path = Path.GetFullPath(path);

            try
            {
                using var setting_file = File.Open(path, FileMode.OpenOrCreate);
                var buffer = new byte[setting_file.Length];
                setting_file.Read(buffer, 0, buffer.Length);

                var setting_text = Encoding.UTF8.GetString(buffer);

                App.Log("setting loading:", setting_text);

                return JObject.Parse(setting_text);
            }
            catch { return new JObject(); }
        }
        public void Save()
        {
            App.Log("setting saving:", ToString());

            using StreamWriter file = File.CreateText(setting_file_name);
            using JsonTextWriter writer = new JsonTextWriter(file)
            {
                Formatting = Formatting.Indented
            };
            setting.WriteTo(writer);
        }


        private const string data_provider = "data-provider";
        public string DataProvider
        {
            get { return GetSetting(data_provider, ""); }
            set { SetSetting(data_provider, value); }
        }

        private BrainConfigHandler brain_instance;
        public BrainConfigHandler Brain
        {
            get
            {
                if (brain_instance == null)
                    brain_instance = new BrainConfigHandler(GetConfig(BrainConfigHandler.key, null));
                return brain_instance;
            }
        }

        private ProcessConfigHandler process;
        public ProcessConfigHandler Process
        {
            get
            {
                if (process == null)
                    process = new ProcessConfigHandler(GetConfig(ProcessConfigHandler.key, null));
                return process;
            }
        }

    }
}
