using System;
using System.Collections.Generic;
using System.Text;

namespace Photon.Jiringi.Config
{
    public delegate void ConfigChangedEventHandler(object sender, ConfigChangedEventArg e);

    public class ConfigChangedEventArg
    {
        public readonly ConfigHandler container;
        public readonly string path;

        public ConfigChangedEventArg(ConfigHandler container, string path)
        {
            this.container = container;
            this.path = path;
        }
    }
}
