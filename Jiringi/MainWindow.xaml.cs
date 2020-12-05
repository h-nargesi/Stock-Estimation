using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using Photon.Jiringi.Config;
using Photon.Jiringi.DataProviding;
using Photon.Jiringi.NetSpecifics;
using Photon.NeuralNetwork.Chista;
using Photon.NeuralNetwork.Chista.Implement;
using Photon.NeuralNetwork.Chista.Serializer;
using Photon.NeuralNetwork.Chista.Trainer;

namespace Photon.Jiringi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            App.LogChanged += App_LogChanged;
        }


        private void Instruction_Process(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NetProcess.Processes.Count < 1)
                    NetProcess.ChangeStatusWithLog(NetProcess.ERROR, "The process is not inizialized.");
                else if (!NetProcess.Stopped)
                    NetProcess.ChangeStatusWithLog(NetProcess.INFO, "The process already is running.");

                else NetProcess.Start();
            }
            catch (Exception ex)
            {
                NetProcess.ChangeStatusWithLog(NetProcess.ERROR, ex.Message, ex.StackTrace);
            }
        }
        private void Evaluation_Process(object sender, RoutedEventArgs e)
        {
            try
            {
                if (NetProcess.Processes.Count < 1)
                    NetProcess.ChangeStatusWithLog(NetProcess.ERROR, "The process is not inizialized.");
                else if (!NetProcess.Stopped)
                    NetProcess.ChangeStatusWithLog(NetProcess.INFO, "The process already is running.");

                else
                {
                    NetProcess.Epoch = 0;
                    NetProcess.Offset = 0;
                    NetProcess.Evaluate();
                }
            }
            catch (Exception ex)
            {
                NetProcess.ChangeStatusWithLog(NetProcess.ERROR, ex.Message, ex.StackTrace);
            }
        }
        private void Stop_Process(object sender, RoutedEventArgs e)
        {
            try
            {
                Stop_Process();
            }
            catch (Exception ex)
            {
                NetProcess.ChangeStatusWithLog(NetProcess.ERROR, ex.Message, ex.StackTrace);
            }
            finally
            {
                App.Setting.Save();
            }
        }
        private void Stop_Process()
        {
            if (!NetProcess.Stopped)
            {
                NetProcess.ChangeStatusWithLog(NetProcess.INFO, "The training process is stoped by user.");
                NetProcess.Stop();
            }
        }

        private void Networks_Create(object sender, RoutedEventArgs e)
        {
            try
            {
                Stop_Process();
                NetProcess.ChangeStatusWithLog(NetProcess.INFO, "New brain ...");

                if ((App.Setting.InitialNetsInfo.ChistaNets?.Length ?? 0) == 0)
                    throw new Exception("No chista-net is set.");

                // reset all values
                NetProcess.LoadProgress(new LearningProcessInfo
                {
                    Epoch = 0,
                    Offset = 0,
                    Stage = TrainingStages.Training,
                });

                ((DataProvider)NetProcess.DataProvider).Method = App.Setting.InitialNetsInfo.BasicalMethod;

                var leayers = App.Setting.InitialNetsInfo.ChistaNets;
                var out_data_range = 
                    App.Setting.InitialNetsInfo.BasicalMethod == BasicalMethodsTypes.AngleBased ?
                    (IDataConvertor)new DataRangeDouble(Math.PI / 2, 0) : new DataRange(20, 0);

                for (int i = 0; i < App.Setting.InitialNetsInfo.ImagesCount; i++)
                {
                    var initalizer = new NeuralNetworkInitializer();
                    var input_convertor = new DataRange(5, 0);
                    var input_size = DataProvider.SIGNAL_COUNT;

                    for (int l = 0; l < leayers.Length; l++)
                    {
                        var layers = leayers[l].HiddenCount;
                        if (layers == null || layers.Length == 0)
                            throw new Exception("The default layer's node count is not set.");

                        var conduction = Tools.Conduction(leayers[l].Conduction);
                        var output_func = Tools.Conduction(leayers[l].OutputFunction);
                        var error_func = Tools.ErrorFunction(leayers[l].ErrorFunction);

                        initalizer
                            .SetInputSize(input_size)
                            .AddLayer(conduction, layers);

                        if (l < leayers.Length - 1)
                        {
                            initalizer
                                .AddLayer(output_func, leayers[l].OutputCount)
                                .SetCorrection(error_func, new RegularizationL2())
                                .SetDataConvertor(input_convertor, null)
                                .SetDataCombiner(new DataAttacher());

                            input_size += leayers[l].OutputCount;
                            input_convertor = null;
                        }
                        else
                        {
                            initalizer
                                .AddLayer(output_func, DataProvider.RESULT_COUNT)
                                .SetCorrection(error_func, new RegularizationL2())
                                .SetDataConvertor(input_convertor, out_data_range);
                        }
                    }

                    NetProcess.AddRunningProgress(
                        initalizer.ChistaNet(
                            App.Setting.InitialNetsInfo.LearningFactor,
                            App.Setting.InitialNetsInfo.CertaintyFactor,
                            App.Setting.InitialNetsInfo.DropoutFactor));
                }

                NetProcess.Networks_Report();
                App.Log(NetProcess.PrintInfo());
                NetProcess.ChangeStatusWithLog(NetProcess.DONE, "The neural networks created.");
            }
            catch (Exception ex)
            {
                NetProcess.ChangeStatusWithLog(NetProcess.ERROR, ex.Message, ex.StackTrace);
            }
            finally
            {
                App.Setting.Save();
            }
        }
        private void Networks_Load(object sender, RoutedEventArgs e)
        {
            var openning = new OpenFileDialog
            {
                Filter = "Neural Net Process|*.nnp|Neural Net Image|*.nni"
            };
            if (openning.ShowDialog() != true) return;

            try
            {
                NetProcess.ChangeStatusWithLog(NetProcess.INFO, "Loading neural network process ...");
                var file = GeneralFileRestore.Restore(openning.FileName, out string method);

                BasicalMethodsTypes method_type;
                try
                {
                    method_type = (BasicalMethodsTypes)Enum.Parse(typeof(BasicalMethodsTypes), method);
                }
                catch (Exception ex)
                {
                    App.Log(ex.Message, ex.StackTrace);
                    method_type = BasicalMethodsTypes.ChangeBased;
                }

                switch (file)
                {
                    case LearningProcessInfo inst_process_info:

                        for (var p = 0; p < inst_process_info.Processes.Count; p++)
                        {
                            var net_process = ErrorStackReplacement.Replace(inst_process_info.Processes[p]);
                            if (net_process != null) inst_process_info.Processes[p] = net_process;
                        }
                        for (var o = 0; o < inst_process_info.OutOfLines.Count; o++)
                        {
                            var net_process = ErrorStackReplacement.Replace(inst_process_info.OutOfLines[o]);
                            if (net_process != null) inst_process_info.OutOfLines[o] = net_process;
                        }

                        Stop_Process();
                        NetProcess.LoadProgress(inst_process_info);
                        ((DataProvider)NetProcess.DataProvider).Method = method_type;
                        NetProcess.Networks_Report();
                        App.Log(NetProcess.PrintInfo());
                        break;

                    default:
                        throw new ArgumentException(
                            nameof(file), "this type of file is not supported.");
                }

                NetProcess.ChangeStatusWithLog(NetProcess.DONE, "The data loaded.");
            }
            catch (Exception ex)
            {
                NetProcess.ChangeStatusWithLog(NetProcess.ERROR, ex.Message, ex.StackTrace);
            }
            finally
            {
                App.Setting.Save();
            }
        }
        private void Networks_Save(object sender, RoutedEventArgs e)
        {
            var saving = new SaveFileDialog
            {
                Filter = "Neural Net Process|*.nnp"
            };
            if (saving.ShowDialog() != true) return;

            try
            {
                NetProcess.ChangeStatusWithLog(NetProcess.INFO, "Saving neural network instructor ...");
                LearningProcessSerializer.Serialize(saving.FileName, NetProcess,
                    ((DataProvider)NetProcess.DataProvider).Method.ToString());
                NetProcess.ChangeStatusWithLog(NetProcess.DONE, "The data saved.");
            }
            catch (Exception ex)
            {
                NetProcess.ChangeStatusWithLog(NetProcess.ERROR, ex.Message, ex.StackTrace);
            }
            finally
            {
                App.Setting.Save();
            }
        }
        private void MenuItem_PrintInfo(object sender, RoutedEventArgs e)
        {
            try
            {
                NetProcess.Networks_Report();
                App.Log(NetProcess.PrintInfo());
            }
            catch (Exception ex)
            {
                NetProcess.ChangeStatusWithLog(NetProcess.ERROR, ex.Message, ex.StackTrace);
            }
        }

        private void App_LogChanged(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                LogBox.AppendText(App.UnseenLogs());
                if (LogBox.Visibility == Visibility.Visible)
                {
                    LogBox.Focus();
                    LogBox.CaretIndex = LogBox.Text.Length;
                    LogBox.ScrollToEnd();
                }
            });
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Stop_Process();

            App.Log($"Closing");
            if (NetProcess.Processes.Count > 0 || NetProcess.OutOfLines.Count > 0)
                LearningProcessSerializer.Serialize("temp.nnp", NetProcess,
                    ((DataProvider)NetProcess.DataProvider).Method.ToString());

            App.Setting.Watching = false;
            App.Setting.Save();
        }
    }
}
