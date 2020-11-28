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
    public abstract class ConfigHandler
    {
        private JObject setting;
        private readonly ReaderWriterLock locker;
        public ConfigHandler(string path)
        {
            locker = new ReaderWriterLock();
            LoadFrom(path);
            CurrentPath = Path.GetFileNameWithoutExtension(path);
        }
        protected ConfigHandler(ConfigHandler conf)
        {
            setting = conf?.setting ??
                throw new ArgumentNullException(nameof(setting));
            locker = conf?.locker ??
                throw new ArgumentNullException(nameof(locker));
            CurrentPath = conf.CurrentPath ??
                throw new ArgumentNullException(nameof(CurrentPath));
        }
        private ConfigHandler(JObject setting, ReaderWriterLock locker, string path)
        {
            this.setting = setting ??
                throw new ArgumentNullException(nameof(setting));
            this.locker = locker ??
                throw new ArgumentNullException(nameof(locker));
            CurrentPath = path ??
                throw new ArgumentNullException(nameof(path));
        }


        public event ConfigChangedEventHandler Changed;
        public string CurrentPath { get; }
        public int Count => setting.Count;

        public ConfigHandler GetConfig(string name, object default_value)
        {
            JObject obj;
            locker.AcquireWriterLock(-1);
            try
            {
                if (!setting.ContainsKey(name))
                {
                    if (default_value == null) obj = new JObject();
                    else obj = new JObject(default_value);
                    setting.Add(name, obj);
                }
                else obj = setting.Value<JObject>(name);
            }
            finally { locker.ReleaseWriterLock(); }

            return new ConfigHandlerPack(obj, locker, $"{CurrentPath}.{name}");
        }
        public ConfigHandler GetConfig(string name)
        {
            locker.AcquireReaderLock(-1);
            try
            {
                if (!setting.ContainsKey(name)) return null;
                return new ConfigHandlerPack(setting.Value<JObject>(name), locker, $"{CurrentPath}.{name}");
            }
            finally { locker.ReleaseReaderLock(); }
        }

        public T? GetSetting<T>(string name) where T : struct
        {
            locker.AcquireReaderLock(-1);
            try
            {
                if (!setting.ContainsKey(name)) return null;
                else return setting.Value<T>(name);
            }
            finally { locker.ReleaseReaderLock(); }
        }
        public string GetSetting(string name)
        {
            locker.AcquireReaderLock(-1);
            try
            {
                if (!setting.ContainsKey(name)) return null;
                else return setting.Value<string>(name);
            }
            finally { locker.ReleaseReaderLock(); }
        }
        public T GetSetting<T>(string name, T default_value)
        {
            locker.AcquireWriterLock(-1);
            try
            {
                T value;
                if (!setting.ContainsKey(name))
                {
                    value = default_value;
                    setting.Add(name, value != null ? JToken.FromObject(value) : null);
                }
                else value = setting.Value<T>(name);

                return value;
            }
            finally { locker.ReleaseWriterLock(); }
        }

        public T[] GetSettingArray<T>(string name, params T[] default_value)
        {
            locker.AcquireWriterLock(-1);
            try
            {
                T[] value;
                if (!setting.ContainsKey(name))
                {
                    value = default_value;
                    setting.Add(name, JArray.FromObject(value));
                }
                else
                {
                    var array = setting.Value<JArray>(name);
                    value = array.Select(jv => jv.Value<T>()).ToArray();
                }

                return value;
            }
            finally { locker.ReleaseWriterLock(); }
        }
        public T[] GetSettingArray<T>(string name)
        {
            locker.AcquireReaderLock(-1);
            try
            {
                if (!setting.ContainsKey(name)) return null;
                else
                {
                    var array = setting.Value<JArray>(name);

                    T[] value;
                    value = array.Select(jv => jv.Value<T>()).ToArray();
                    return value;
                }
            }
            finally { locker.ReleaseReaderLock(); }
        }

        public void SetSetting(string name, object value)
        {
            locker.AcquireWriterLock(-1);
            try
            {
                if (!setting.ContainsKey(name))
                    setting.Add(name, value != null ? JToken.FromObject(value) : null);
                else setting[name].Replace(value != null ? JToken.FromObject(value) : null);
            }
            finally { locker.ReleaseWriterLock(); }

            Changed?.Invoke(this, new ConfigChangedEventArg(this, $"{CurrentPath}.{name}"));
        }

        protected void LoadFrom(string path, Action<string> under_lock = null)
        {
            locker.AcquireWriterLock(-1);
            try
            {
                using var setting_file = File.Open(path, FileMode.OpenOrCreate);
                var buffer = new byte[setting_file.Length];
                setting_file.Read(buffer, 0, buffer.Length);

                var file_content = Encoding.UTF8.GetString(buffer);

                if (string.IsNullOrWhiteSpace(file_content))
                {
                    if (setting == null) setting = new JObject();
                    // else do not change
                }
                else setting = JObject.Parse(file_content);
            }
            catch (Exception ex)
            {
                setting = new JObject();
                under_lock?.Invoke($"setting reloading: {ex.Message}");
            }
            finally
            {
                under_lock?.Invoke($"The setting ({Count} node(s)) is reloaded.");
                locker.ReleaseWriterLock();
            }

            Changed?.Invoke(this, new ConfigChangedEventArg(this, CurrentPath));
        }
        protected void WriteTo(string path)
        {
            using StreamWriter file = File.CreateText(path);
            using JsonTextWriter writer = new JsonTextWriter(file)
            {
                Formatting = Formatting.Indented
            };

            locker.AcquireReaderLock(-1);
            try { setting.WriteTo(writer); }
            finally { locker.ReleaseReaderLock(); }
        }
        public override string ToString()
        {
            locker.AcquireReaderLock(-1);
            try { return setting.ToString(); }
            finally { locker.ReleaseReaderLock(); }
        }

        private class ConfigHandlerPack : ConfigHandler
        {
            public ConfigHandlerPack(JObject setting, ReaderWriterLock locker, string path)
                : base(setting, locker, path) { }
        }
    }
}
