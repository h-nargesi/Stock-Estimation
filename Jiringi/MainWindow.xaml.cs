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
using Photon.Jiringi.Tools;
using Photon.NeuralNetwork.Chista;
using Photon.NeuralNetwork.Chista.Implement;
using Photon.NeuralNetwork.Chista.Serializer;
using Photon.NeuralNetwork.Chista.Trainer;

namespace Photon.Jiringi
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        public MainWindow()
        {
            InitializeComponent();

            data_provider = new DataProvider(App.Setting);
        }

        private readonly DataProvider data_provider;
        private Instructor instructor;
        private TimeReporter time_reporter;

        public event PropertyChangedEventHandler PropertyChanged;
        public AppState AppState { get; private set; }
        public string CurrentProcess { get; private set; }

        private void Initialize_Instructor()
        {
            instructor = new Instructor(data_provider);
            instructor.ReflectFinished += Instructor_ReflectFinished;
            instructor.OnError += Instructor_OnError;
        }
        private void Instructor_ReflectFinished(Instructor instructor, Record record, long duration)
        {
            var offset_interval = time_reporter.GetNextAvg();

            Networks_Report();

            uint done_count;
            long remain_count;
            switch (instructor.Stage)
            {
                case TraingingStages.Training:
                    done_count = data_provider.TrainingCount;
                    remain_count = data_provider.TrainingCount - instructor.Offset;
                    remain_count += data_provider.ValidationCount;
                    remain_count += data_provider.EvaluationCount;
                    break;
                case TraingingStages.Validation:
                    done_count = data_provider.ValidationCount;
                    remain_count = data_provider.ValidationCount - instructor.Offset;
                    remain_count += data_provider.EvaluationCount;
                    break;
                case TraingingStages.Evaluation:
                    done_count = data_provider.EvaluationCount;
                    remain_count = data_provider.EvaluationCount - instructor.Offset;
                    break;
                default: throw new Exception("Invalid stage type");
            }

            // prepare report string
            string message =
@$"#{instructor.Epoch} progress={instructor.Stage.ToString().ToLower()},{PrintUnsign(instructor.Offset * 100D / done_count, 3):R}% data loading={Instructor.GetDurationString(record.duration.Value)} prediction={Instructor.GetDurationString(duration)} left-time={Instructor.GetDurationString(offset_interval * remain_count)}";

            AppState.ChangeStatus(MessageState.Info, message);
        }
        private void Instructor_OnError(Instructor sender, Exception ex)
        {
            AppState.ChangeStatusWithSave(MessageState.Error, ex.Message, ex.StackTrace);
        }

        private static string Print(double val, int? digit)
        {
            if (digit.HasValue)
                val = Math.Round(val, digit.Value);
            return (val >= 0 ? "+" : "") + val.ToString("R");
        }
        private static string PrintUnsign(double val, int? digit)
        {
            if (digit.HasValue)
                val = Math.Round(val, digit.Value);
            return val.ToString("R");
        }

        private void Training_Start(object sender, RoutedEventArgs e)
        {
            if (instructor == null)
                AppState.ChangeStatusWithSave(MessageState.Error, "The instructor is not inizialized.");
            else
            {
                // reset stage
                var stage = App.Setting.Process.Stage;
                if (stage != null)
                {
                    instructor.Stage = stage.Value;
                    App.Setting.Process.Stage = null;
                }

                // reset offsets
                var offset = App.Setting.Process.Offset;
                if (offset != null)
                {
                    instructor.Offset = offset.Value;
                    App.Setting.Process.Offset = null;
                }


                time_reporter = new TimeReporter
                {
                    MaxHistory = App.Setting.Process.LeftTimeEstimateLength
                };

                instructor.Start();
            }
        }
        private void Training_Stop(object sender, RoutedEventArgs e)
        {
            instructor?.Stop();
        }

        private void Networks_Report()
        {
            // prepare report info
            double accuracy = 0;
            foreach (var prc in instructor.Processes)
                accuracy = Math.Max(prc.CurrentAccuracy, accuracy);

            string cur_proc = $"Training ({instructor.Processes.Count} net(s) | " +
                $"best accuracy={PrintUnsign(accuracy * 100, 4):R})";

            if (instructor.OutOfLine.Count > 0)
            {
                accuracy = 0;
                foreach (var prc in instructor.OutOfLine)
                    accuracy = Math.Max(prc.accuracy, accuracy);

                cur_proc += $"Done ({instructor.OutOfLine.Count} net(s) | " +
                    $"best accuracy={PrintUnsign(accuracy * 100, 4):R})";
            }

            CurrentProcess = cur_proc;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(CurrentProcess)));
        }
        private void Networks_Create(object sender, RoutedEventArgs e)
        {
            Training_Stop(sender, e);

            AppState.ChangeStatusWithSave(MessageState.Info, "New brain ...");

            var layers = App.Setting.Brain.Layers.NodesCount;
            if (layers == null || layers.Length == 0)
            {
                App.Setting.Brain.Layers.NodesCount = new int[0];
                AppState.ChangeStatusWithSave(MessageState.Error, "The default layer's node count is not set.");
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
                    .SetInputSize(DataProvider.SIGNAL_COUNT_TOTAL)
                    .AddLayer(conduction, layers)
                    .AddLayer(output, DataProvider.RESULT_COUNT)
                    .SetCorrection(new ErrorStack(DataProvider.RESULT_COUNT), new RegularizationL2())
                    .SetDataConvertor(new DataRange(5, 0), range)
                    .Image();

            Initialize_Instructor();

            foreach (var image in images)
                instructor.AddProgress(new Brain(image)
                {
                    LearningFactor = App.Setting.Brain.LearningFactor,
                    CertaintyFactor = App.Setting.Brain.CertaintyFactor,
                    DropoutFactor = App.Setting.Brain.DropoutFactor,
                });

            Networks_Report();
        }
        private void Networks_Load(object sender, RoutedEventArgs e)
        {
            var openning = new OpenFileDialog
            {
                Filter = "Neural Net Process|*.nnp|Neural Net Image|*.nni"
            };
            if (openning.ShowDialog() != true) return;

            AppState.ChangeStatusWithSave(MessageState.Info, "Loading neural network process ...");
            var file = GeneralFileRestore.Restore(openning.FileName);

            switch (file)
            {
                case ProcessInfo process_info:
                    if (instructor == null) Initialize_Instructor();
                    else Training_Stop(sender, e);
                    instructor.LoadProgress(process_info);
                    Networks_Report();
                    break;

                default:
                    throw new ArgumentException(
                        nameof(file), "this type of file is not supported.");
            }

        }
        private void Networks_Save(object sender, RoutedEventArgs e)
        {
            if (instructor == null)
            {
                AppState.ChangeStatusWithSave(MessageState.Error, "Loading neural network process ...");
                return;
            }

            var saving = new SaveFileDialog();
            if (saving.ShowDialog() != true) return;

            AppState.ChangeStatusWithSave(MessageState.Info, "Saving neural network process ...");
            TrainProcessSerializer.Serialize(saving.FileName, instructor);
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (instructor != null)
                TrainProcessSerializer.Serialize("temp.nnp", instructor);
            File.AppendAllText("logs", AppState.FullInfo);
        }
    }
}
