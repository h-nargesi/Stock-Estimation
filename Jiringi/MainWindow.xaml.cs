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
        private Instructor process;
        private TimeReporter[] time_reporter;
        private const int report_time_interval = 30 * 10000;
        private long last_time_reported = 0;
        private int last_instrument_id = -1;
        private uint last_offset = 0, last_record_offset = 0;

        private void Initialize_Instructor()
        {
            process = new Instructor(data_provider);
            process.ReflectFinished += Instructor_ReflectFinished;
            process.OnError += Instructor_OnError;
        }
        private void Instructor_ReflectFinished(Instructor instructor, Record record, long duration)
        {
            // TODO: use time_reporter for data-time and predict-time
            var record_duration = time_reporter[0].GetNextAvg(record.duration ?? 0);
            duration = time_reporter[1].GetNextAvg(duration);
            var offset_interval = time_reporter[2].GetNextAvg();

            var (current_instrument_id, current_offset, current_record_offset) =
                (ValueTuple<int, uint, uint>)record.extra;
            if (last_instrument_id != current_instrument_id)
            {
                if (last_instrument_id > -1)
                    Log($"End Section:\tinstrument-id({last_instrument_id})\toffset({last_offset})\trecord({last_record_offset})");
                Log($"Start Section:\tinstrument-id({current_instrument_id})\toffset({current_offset})\trecord({current_record_offset})");
            }
            last_instrument_id = current_instrument_id;
            last_offset = current_offset;
            last_record_offset = current_record_offset;

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
@$"#{instructor.Epoch} {instructor.Stage} {PrintUnsign(progress, 3):R}% ID({current_instrument_id})";
                string message =
@$"Data loading={Instructor.GetDurationString(record_duration, 6)} Prediction={Instructor.GetDurationString(duration, 6)} Left time={Instructor.GetDurationString(offset_interval * remain_count, 3)}";

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

        private void Instruction_Process(object sender, RoutedEventArgs e)
        {
            if (process == null)
            {
                HideProcessBar();
                ChangeStatusWithSave(ERROR, "The process is not inizialized.");
            }
            else if (!process.Stopped)
                ChangeStatusWithSave(INFO, "The process already is running.");

            else Process_Start(null);
        }
        private void Evaluation_Process(object sender, RoutedEventArgs e)
        {
            if (process == null)
            {
                HideProcessBar();
                ChangeStatusWithSave(ERROR, "The process is not inizialized.");
            }
            else if (!process.Stopped)
                ChangeStatusWithSave(INFO, "The process already is running.");

            else
            {
                process.Epoch = 0;
                process.Offset = 0;
                Process_Start(TraingingStages.Evaluation);
            }
        }
        private void Process_Start(TraingingStages? filter)
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
                process.Stage = stage.Value;
                App.Setting.Process.Stage = null;
            }

            // reset offsets
            var offset = App.Setting.Process.Offset;
            if (offset != null)
            {
                process.Offset = offset.Value;
                App.Setting.Process.Offset = null;
            }

            time_reporter = new TimeReporter[]
            {
                new TimeReporter
                {
                    MaxHistory = App.Setting.Process.LeftTimeEstimateLength / 100
                },
                new TimeReporter
                {
                    MaxHistory = App.Setting.Process.LeftTimeEstimateLength / 100
                },
                new TimeReporter
                {
                    MaxHistory = App.Setting.Process.LeftTimeEstimateLength
                }
            };

            switch (filter)
            {
                case TraingingStages.Evaluation:
                    process.Evaluate();
                    break;
                default:
                    process.Start();
                    break;
            }
        }
        private void Process_Stop(object sender, RoutedEventArgs e)
        {
            if (process != null)
            {
                process.Stop();
                Log($"End Process:\tinstrument-id({last_instrument_id})\toffset({last_offset})\trecord({last_record_offset})");
                last_instrument_id = -1;
                ChangeStatusWithSave(INFO, "The training process is stoped by user.");
            }
            HideProcessBar();
        }
        private void HideProcessBar()
        {
            Dispatcher.Invoke(() =>
            {
                ProgressPanel.Visibility = Visibility.Collapsed;
                ProgressInfo.Text = "";
                ProgressBar.Value = 0;
            });
        }

        private void Networks_Create(object sender, RoutedEventArgs e)
        {
            Process_Stop(sender, e);

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
                process.AddProgress(new Brain(image)
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
                    if (process == null) Initialize_Instructor();
                    else Process_Stop(sender, e);
                    process.LoadProgress(process_info);
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
            if (process is Instructor instructor)
                ChangeStatusWithSave(INFO, "Saving neural network instructor ...");
            else
            {
                ChangeStatusWithSave(ERROR, "The neural network instructor is not loaded.");
                return;
            }

            var saving = new SaveFileDialog();
            if (saving.ShowDialog() != true) return;

            TrainProcessSerializer.Serialize(saving.FileName, instructor);
            App.Setting.Save();

            ChangeStatusWithSave(DONE, "The data saved.");
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (process != null)
            {
                // Training_Stop(sender, new RoutedEventArgs());
                Log($"End Process:\tinstrument-id({last_instrument_id})\toffset({last_offset})\trecord({last_record_offset})");
                if (process is Instructor instructor)
                    TrainProcessSerializer.Serialize("temp.nnp", instructor);
            }
            File.AppendAllText("logs", logs.ToString());
            App.Setting.Save();
        }

        private readonly StringBuilder logs = new StringBuilder();
        private void Networks_Report()
        {
            // prepare report info
            double accuracy = 0;
            foreach (var prc in process.Processes)
                accuracy = Math.Max(prc.CurrentAccuracy, accuracy);

            string cur_proc = $"Current [{process.Processes.Count} net(s), " +
                $"best:{PrintUnsign(accuracy * 100, 4):R}]";

            if (process is Instructor instructor && instructor.OutOfLine.Count > 0)
            {
                accuracy = 0;
                foreach (var prc in instructor.OutOfLine)
                    accuracy = Math.Max(prc.Accuracy, accuracy);

                cur_proc += $" Done [{instructor.OutOfLine.Count} net(s), " +
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
            logs.Append(DateTime.Now).Append("\t").Append(message).Append("\r\n");

            Dispatcher.Invoke(() =>
            {
                StatusPanel.Background = state;
                StatusMessage.Text = message;
                Logs.Text = logs.ToString();
            });
        }
        private void ChangeStatusWithReport(Brush state, string message, string report)
        {
            logs.Append(DateTime.Now).Append("\t").Append(message).Append("\r\n");
            logs.Append(DateTime.Now).Append("\r\n").Append(report).Append("\r\n");

            Dispatcher.Invoke(() =>
            {
                StatusPanel.Background = state;
                StatusMessage.Text = message;
                Logs.Text = logs.ToString();
            });
        }
        private void Log(string message)
        {
            logs.Append(DateTime.Now).Append("\t").Append(message).Append("\r\n");
            Dispatcher.Invoke(() => Logs.Text = logs.ToString());
        }

        public static readonly SolidColorBrush
            BLANK = new SolidColorBrush(Color.FromRgb(64, 64, 64)),
            INFO = new SolidColorBrush(Color.FromRgb(47, 117, 181)),
            ERROR = new SolidColorBrush(Color.FromRgb(150, 0, 50)),
            DONE = new SolidColorBrush(Color.FromRgb(0, 176, 80));
    }
}
