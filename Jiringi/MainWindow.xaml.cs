﻿using System;
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
using Photon.NeuralNetwork.Chista;
using Photon.NeuralNetwork.Chista.Implement;
using Photon.NeuralNetwork.Chista.Serializer;
using Photon.NeuralNetwork.Chista.Trainer;
using LiveCharts;
using LiveCharts.Wpf;

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

                var layers = App.Setting.Brain.Layers.NodesCount;
                if (layers == null || layers.Length == 0)
                {
                    App.Setting.Brain.Layers.NodesCount = new int[0];
                    NetProcess.ChangeStatusWithLog(NetProcess.ERROR, "The default layer's node count is not set.");
                    return;
                }

                var images = new NeuralNetworkImage[App.Setting.Brain.ImagesCount];

                var conduction = App.Setting.Brain.Layers.Conduction == "soft-relu" ?
                    (IConduction)new SoftReLU() : new ReLU();
                var output = App.Setting.Brain.Layers.OutputConduction == "straight" ?
                    (IConduction)new Straight() : new Sigmoind();
                var range = App.Setting.Brain.Layers.OutputConduction == "straight" ? null : new DataRange(20, 10);

                for (int i = 0; i < images.Length; i++)
                    images[i] = new NeuralNetworkInitializer()
                        .SetInputSize(DataProvider.SIGNAL_COUNT)
                        .AddLayer(conduction, layers)
                        .AddLayer(output, DataProvider.RESULT_COUNT)
                        .SetCorrection(new ErrorStack(DataProvider.RESULT_COUNT), new RegularizationL2())
                        .SetDataConvertor(new DataRange(5, 0), range)
                        .Image();

                // reset all values
                NetProcess.LoadProgress(new ProcessInfo
                {
                    Epoch = 0,
                    Offset = 0,
                    Stage = TraingingStages.Training,
                });

                // add new processes
                foreach (var image in images)
                    NetProcess.AddProgress(new Brain(image)
                    {
                        LearningFactor = App.Setting.Brain.LearningFactor,
                        CertaintyFactor = App.Setting.Brain.CertaintyFactor,
                        DropoutFactor = App.Setting.Brain.DropoutFactor,
                    });

                NetProcess.Networks_Report();
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
                var file = GeneralFileRestore.Restore(openning.FileName);

                switch (file)
                {
                    case ProcessInfo process_info:
                        Stop_Process();
                        NetProcess.LoadProgress(process_info);
                        NetProcess.Networks_Report();
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
                TrainProcessSerializer.Serialize(saving.FileName, NetProcess);
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

        private void App_LogChanged(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() => LogBox.AppendText(App.UnseenLogs()));
        }
        private void Window_Closing(object sender, CancelEventArgs e)
        {
            Stop_Process();

            App.Log($"Closing");
            if (NetProcess.Processes.Count > 0 || NetProcess.OutOfLine.Count > 0)
                TrainProcessSerializer.Serialize("temp.nnp", NetProcess);

            App.Setting.Watching = false;
            App.Setting.Save();
        }
    }
}
