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
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            data_provider = new DataProvider(App.Setting);
        }

        private readonly DataProvider data_provider;
        private Instructor instructor;
        private TimeReporter time_reporter;
        private const int report_time_interval = 30 * 10000;
        private long last_time_reported = 0;

        private void Initialize_Instructor()
        {
            instructor = new Instructor(data_provider);
            instructor.ReflectFinished += Instructor_ReflectFinished;
            instructor.OnError += Instructor_OnError;
        }
        private void Instructor_ReflectFinished(Instructor instructor, Record record, long duration)
        {
            var offset_interval = time_reporter.GetNextAvg();

            if (DateTime.Now.Ticks - last_time_reported > report_time_interval)
            {
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
                var progress = instructor.Offset * 100D / done_count;
                string process_info =
                    @$"#{instructor.Epoch} {instructor.Stage} {PrintUnsign(progress, 3):R}%";
                string message = @$"Data loading={Instructor.GetDurationString(record.duration.Value)} Prediction={Instructor.GetDurationString(duration)} Left time={Instructor.GetDurationString(offset_interval * remain_count)}";

                last_time_reported = DateTime.Now.Ticks;

                Dispatcher.Invoke(() =>
                {
                    ProgressInfo.Text = process_info;
                    ProgressBar.Value = progress;
                });

                ChangeStatus(INFO, message);
                Networks_Report();
            }
        }
        private void Instructor_OnError(Instructor sender, Exception ex)
        {
            ChangeStatusWithReport(ERROR, ex.Message, ex.StackTrace);
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
                ChangeStatusWithSave(ERROR, "The instructor is not inizialized.");
            else
            {
                Dispatcher.Invoke(() =>
                {
                    ProgressPanel.Visibility = Visibility.Visible;
                    ProgressBar.Maximum = 100;
                });

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
            if (instructor != null)
            {
                instructor.Stop();
                ChangeStatusWithSave(INFO, "The training process is stoped by user.");
            }
            Dispatcher.Invoke(() =>
            {
                ProgressPanel.Visibility = Visibility.Collapsed;
                ProgressInfo.Text = "";
                ProgressBar.Value = 0;
            });
        }

        private void Networks_Create(object sender, RoutedEventArgs e)
        {
            Training_Stop(sender, e);

            ChangeStatusWithSave(INFO, "New brain ...");

            var layers = App.Setting.Brain.Layers.NodesCount;
            if (layers == null || layers.Length == 0)
            {
                App.Setting.Brain.Layers.NodesCount = new int[0];
                ChangeStatusWithSave(ERROR, "The default layer's node count is not set.");
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

            Initialize_Instructor();

            foreach (var image in images)
                instructor.AddProgress(new Brain(image)
                {
                    LearningFactor = App.Setting.Brain.LearningFactor,
                    CertaintyFactor = App.Setting.Brain.CertaintyFactor,
                    DropoutFactor = App.Setting.Brain.DropoutFactor,
                });

            Networks_Report();
            ChangeStatusWithSave(DONE, "The neural networks created.");
        }
        private void Networks_Load(object sender, RoutedEventArgs e)
        {
            var openning = new OpenFileDialog
            {
                Filter = "Neural Net Process|*.nnp|Neural Net Image|*.nni"
            };
            if (openning.ShowDialog() != true) return;

            ChangeStatusWithSave(INFO, "Loading neural network process ...");
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

            ChangeStatusWithSave(DONE, "The data loaded.");
        }
        private void Networks_Save(object sender, RoutedEventArgs e)
        {
            if (instructor == null)
            {
                ChangeStatusWithSave(ERROR, "The neural network process is not loaded.");
                return;
            }
            else ChangeStatusWithSave(INFO, "Saving neural network process ...");

            var saving = new SaveFileDialog();
            if (saving.ShowDialog() != true) return;

            TrainProcessSerializer.Serialize(saving.FileName, instructor);
            App.Setting.Save();

            ChangeStatusWithSave(DONE, "The data saved.");
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (instructor != null)
                TrainProcessSerializer.Serialize("temp.nnp", instructor);
            File.AppendAllText("logs", logs.ToString());
            App.Setting.Save();
        }

        private readonly StringBuilder logs = new StringBuilder();
        private void Networks_Report()
        {
            // prepare report info
            double accuracy = 0;
            foreach (var prc in instructor.Processes)
                accuracy = Math.Max(prc.CurrentAccuracy, accuracy);

            string cur_proc = $"Training [{instructor.Processes.Count} net(s), " +
                $"best:{PrintUnsign(accuracy * 100, 4):R}]";

            if (instructor.OutOfLine.Count > 0)
            {
                accuracy = 0;
                foreach (var prc in instructor.OutOfLine)
                    accuracy = Math.Max(prc.accuracy, accuracy);

                cur_proc += $"Done [{instructor.OutOfLine.Count} net(s), " +
                    $"best:{PrintUnsign(accuracy * 100, 4):R}]";
            }

            Dispatcher.Invoke(() => StatusCurrentNets.Text = cur_proc);
        }
        private void ChangeStatus(Brush state, string message)
        {
            Dispatcher.Invoke(() =>
            {
                StatusPanel.Background = state;
                StatusMessage.Text = message;
            });
        }
        private void ChangeStatusWithSave(Brush state, string message)
        {
            logs.Append(message).Append("\r\n");

            Dispatcher.Invoke(() =>
            {
                StatusPanel.Background = state;
                StatusMessage.Text = message;
                Logs.Text = logs.ToString();
            });
        }
        private void ChangeStatusWithReport(Brush state, string message, string report)
        {
            logs.Append(report).Append("\r\n");

            Dispatcher.Invoke(() =>
            {
                StatusPanel.Background = state;
                StatusMessage.Text = message;
                Logs.Text = logs.ToString();
            });
        }

        public static readonly SolidColorBrush
            BLANK = new SolidColorBrush(Color.FromRgb(64, 64, 64)),
            INFO = new SolidColorBrush(Color.FromRgb(47, 117, 181)),
            ERROR = new SolidColorBrush(Color.FromRgb(150, 0, 50)),
            DONE = new SolidColorBrush(Color.FromRgb(0, 176, 80));
    }
}
