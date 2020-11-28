using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Photon.Jiringi.Config;
using Photon.Jiringi.NetSpecifics;
using Photon.NeuralNetwork.Chista.Implement;

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
            logs = new StringBuilder();
            Setting = new RootConfigHandler();

            new ErrorStack(0).Register();
        }


        #region Log Handler
        private static int first_unsaved_index = 0, first_unseen_index = 0;
        private readonly static StringBuilder logs;
        public static event EventHandler LogChanged;
        public static void Log(string message, string report = null)
        {
            lock (logs)
            {
                logs.Append(DateTime.Now.ToString("yyyy/MM/dd HH:mm:ss.fff")).Append("\t")
                    .Append(message).Append("\r\n");
                if (report != null) logs.Append(report).Append("\r\n");
            }
            LogChanged?.Invoke(logs, new EventArgs());
        }
        public static string UnseenLogs()
        {
            lock (logs)
                if (logs.Length > first_unseen_index)
                {
                    var result = logs.ToString(first_unseen_index, logs.Length - first_unseen_index);
                    first_unseen_index = logs.Length;
                    return result;
                }
                else return null;
        }
        public static void LogRecording()
        {
            lock (logs)
                if (logs.Length > first_unsaved_index)
                {
                    File.AppendAllText(
                        System.Reflection.Assembly.GetExecutingAssembly().GetName().Name + ".log",
                        logs.ToString(first_unsaved_index, logs.Length - first_unsaved_index));
                    first_unsaved_index = logs.Length;
                }
        }
        #endregion
    }
}
