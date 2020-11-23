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
        private const int waiting = 100;
        private readonly ReaderWriterLock locker;
        protected readonly JObject setting;
        public ConfigHandler(JObject setting, ReaderWriterLock locker)
        {
            this.setting = setting ??
                throw new ArgumentNullException(nameof(setting));
            this.locker = locker ??
                throw new ArgumentNullException(nameof(setting));
        }

        public JObject GetConfig(string name, object default_value)
        {
            locker.AcquireReaderLock(waiting);
            try
            {
                if (!setting.ContainsKey(name))
                {
                    JObject obj;
                    if (default_value == null) obj = new JObject();
                    else obj = new JObject(default_value);
                    setting.Add(name, obj);
                    return obj;
                }
                else return setting.Value<JObject>(name);
            }
            finally { locker.ReleaseReaderLock(waiting); }
        }
        public JObject GetConfig(string name)
        {
            if (!setting.ContainsKey(name)) return null;
            else return setting.Value<JObject>(name);
        }
        public T? GetSetting<T>(string name) where T : struct
        {
            if (!setting.ContainsKey(name)) return null;
            else return setting.Value<T>(name);
        }
        public string GetSetting(string name)
        {
            if (!setting.ContainsKey(name)) return null;
            else return setting.Value<string>(name);
        }
        public T GetSetting<T>(string name, T default_value)
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
        public T[] GetSettingArray<T>(string name, params T[] default_value)
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
        public T[] GetSettingArray<T>(string name)
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
        public void SetSetting(string name, object value)
        {
            if (!setting.ContainsKey(name))
                setting.Add(name, value != null ? JToken.FromObject(value) : null);
            else setting[name].Replace(value != null ? JToken.FromObject(value) : null);
        }

        public override string ToString()
        {
            return setting.ToString();
        }
    }
}
