using Photon.Jiringi.Config;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Threading.Tasks;
using System.Windows;

namespace Photon.Jiringi
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static RootConfigHandler Setting { get; }

        static App()
        {
            Setting = new RootConfigHandler($"setting.json");
        }
    }
}
