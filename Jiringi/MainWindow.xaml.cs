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
        }


        private void Instruction_Process(object sender, RoutedEventArgs e)
        {
            if (NetProcess.Processes.Count < 1)
                NetProcess.ChangeStatusWithSave(NetProcess.ERROR, "The process is not inizialized.");
            else if (!NetProcess.Stopped)
                NetProcess.ChangeStatusWithSave(NetProcess.INFO, "The process already is running.");

            else NetProcess.Start();
        }
        private void Evaluation_Process(object sender, RoutedEventArgs e)
        {
            if (NetProcess.Processes.Count < 1)
                NetProcess.ChangeStatusWithSave(NetProcess.ERROR, "The process is not inizialized.");
            else if (!NetProcess.Stopped)
                NetProcess.ChangeStatusWithSave(NetProcess.INFO, "The process already is running.");

            else
            {
                NetProcess.Epoch = 0;
                NetProcess.Offset = 0;
                NetProcess.Evaluate();
            }
        }
        private void Process_Stop(object sender, RoutedEventArgs e)
        {
            if (!NetProcess.Stopped)
            {
                NetProcess.ChangeStatusWithSave(NetProcess.INFO, "The training process is stoped by user.");
                NetProcess.Stop();
            }
        }


        private void Networks_Create(object sender, RoutedEventArgs e)
        {
            Process_Stop(sender, e);
            NetProcess.ChangeStatusWithSave(NetProcess.INFO, "New brain ...");

            var layers = App.Setting.Brain.Layers.NodesCount;
            if (layers == null || layers.Length == 0)
            {
                App.Setting.Brain.Layers.NodesCount = new int[0];
                NetProcess.ChangeStatusWithSave(NetProcess.ERROR, "The default layer's node count is not set.");
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
            NetProcess.ChangeStatusWithSave(NetProcess.DONE, "The neural networks created.");
        }
        private void Networks_Load(object sender, RoutedEventArgs e)
        {
            var openning = new OpenFileDialog
            {
                Filter = "Neural Net Process|*.nnp|Neural Net Image|*.nni"
            };
            if (openning.ShowDialog() != true) return;

            NetProcess.ChangeStatusWithSave(NetProcess.INFO, "Loading neural network process ...");
            var file = GeneralFileRestore.Restore(openning.FileName);

            switch (file)
            {
                case ProcessInfo process_info:
                    Process_Stop(sender, e);
                    NetProcess.LoadProgress(process_info);
                    NetProcess.Networks_Report();
                    break;

                default:
                    throw new ArgumentException(
                        nameof(file), "this type of file is not supported.");
            }

            NetProcess.ChangeStatusWithSave(NetProcess.DONE, "The data loaded.");
        }
        private void Networks_Save(object sender, RoutedEventArgs e)
        {
            NetProcess.ChangeStatusWithSave(NetProcess.INFO, "Saving neural network instructor ...");

            var saving = new SaveFileDialog();
            if (saving.ShowDialog() != true) return;

            TrainProcessSerializer.Serialize(saving.FileName, NetProcess);
            App.Setting.Save();

            NetProcess.ChangeStatusWithSave(NetProcess.DONE, "The data saved.");
        }


        private void Window_Closing(object sender, CancelEventArgs e)
        {
            NetProcess.Log($"Closing");
            TrainProcessSerializer.Serialize("temp.nnp", NetProcess);
            File.AppendAllText("logs", NetProcess.Logs);
            App.Setting.Save();
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            //new PanelExample().Show();
        }
    }
}
