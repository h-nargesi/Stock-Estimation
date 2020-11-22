using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Defaults;
using Photon.Jiringi.DataProviding;
using Photon.NeuralNetwork.Chista;
using Photon.NeuralNetwork.Chista.Trainer;

namespace Photon.Jiringi
{
    public class NetProcess : Instructor, INotifyPropertyChanged
    {
        public NetProcess() : base(new DataProvider(App.Setting))
        {
            logs = new StringBuilder();
            time_reporter = new TimeReporter[3];

            DataValues = new ChartValues<ObservableValue>();
            PredictedValues = new ChartValues<ObservableValue>();
        }


        #region Status Properties
        private readonly StringBuilder logs;

        public Brush StatusColor { get; private set; } = BLANK;
        public string StatusMessage { get; private set; }
        public string NetworkReport { get; private set; }
        public double ProgressBar { get; private set; }
        public string ProgressInfo { get; private set; }
        public string Logs => logs.ToString();
        public ChartValues<ObservableValue> DataValues { get; }
        public ChartValues<ObservableValue> PredictedValues { get; }
        public int MaxGraphPoints { get; set; } = DataProviding.DataProvider.RECORDS_PREVIOUS_ONE_YEAR;
        public Visibility TextReporting { get; private set; } = Visibility.Collapsed;
        public Visibility GraphReporting { get; private set; } = Visibility.Collapsed;
        #endregion


        #region Change Status Methods
        public void Networks_Report()
        {
            // prepare report info
            double accuracy = 0;
            foreach (var prc in Processes)
                accuracy = Math.Max(prc.CurrentAccuracy, accuracy);

            string cur_proc = $"Current [{Processes.Count} net(s), " +
                $"best:{PrintUnsign(accuracy * 100, 4):R}]";

            if (OutOfLine.Count > 0)
            {
                accuracy = 0;
                foreach (var prc in OutOfLine)
                    accuracy = Math.Max(prc.Accuracy, accuracy);

                cur_proc += $" Done [{OutOfLine.Count} net(s), " +
                    $"best:{PrintUnsign(accuracy * 100, 4):R}]";
            }

            NetworkReport = cur_proc;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(NetworkReport)));
        }
        public void ChangeStatus(Brush state, string message)
        {
            StatusColor = state;
            StatusMessage = message;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusColor)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusMessage)));
        }
        public void ChangeStatusWithSave(Brush state, string message)
        {
            ChangeStatus(state, message);
            logs.Append(DateTime.Now).Append("\t").Append(message).Append("\r\n");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Logs)));
        }
        public void ChangeStatusWithReport(Brush state, string message, string report)
        {
            ChangeStatus(state, message);
            logs.Append(DateTime.Now).Append("\t").Append(message).Append("\r\n");
            logs.Append(DateTime.Now).Append("\r\n").Append(report).Append("\r\n");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Logs)));
        }
        public void Log(string message)
        {
            logs.Append(DateTime.Now).Append("\t").Append(message).Append("\r\n");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Logs)));
        }

        public static readonly SolidColorBrush
            BLANK = new SolidColorBrush(Color.FromRgb(64, 64, 64)),
            INFO = new SolidColorBrush(Color.FromRgb(47, 117, 181)),
            ERROR = new SolidColorBrush(Color.FromRgb(150, 0, 50)),
            DONE = new SolidColorBrush(Color.FromRgb(0, 176, 80));
        #endregion


        #region Instructor Events

        private const int report_time_interval = 30 * 10000;
        private long last_time_text_reported, last_time_graph_reported;
        private int last_instrument_id = -1;
        private uint last_offset = 0, last_record_offset = 0;
        private readonly TimeReporter[] time_reporter;

        protected override void OnInitialize()
        {
            Log("Process initializing");

            // reset stage
            var stage = App.Setting.Process.Stage;
            if (stage != null)
            {
                Stage = stage.Value;
                App.Setting.Process.Stage = null;
            }

            // reset offsets
            var offset = App.Setting.Process.Offset;
            if (offset != null)
            {
                Offset = offset.Value;
                App.Setting.Process.Offset = null;
            }

            time_reporter[0] = new TimeReporter
            {
                MaxHistory = App.Setting.Process.LeftTimeEstimateLength / 100
            };
            time_reporter[1] = new TimeReporter
            {
                MaxHistory = App.Setting.Process.LeftTimeEstimateLength / 100
            };
            time_reporter[2] = new TimeReporter
            {
                MaxHistory = App.Setting.Process.LeftTimeEstimateLength
            };

            if (TextReporting == Visibility.Visible != App.Setting.Process.TextReporting)
                TextReporting = App.Setting.Process.TextReporting ? Visibility.Visible : Visibility.Collapsed;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextReporting)));

            PredictedValues.Clear();
            DataValues.Clear();
            if (GraphReporting == Visibility.Visible != App.Setting.Process.GraphReporting)
                GraphReporting = App.Setting.Process.GraphReporting ? Visibility.Visible : Visibility.Collapsed;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GraphReporting)));

        }
        protected override void ReflectFinished(Record record, long duration, int running_code)
        {
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

            if (DateTime.Now.Ticks - last_time_text_reported > report_time_interval)
                if (TextReporting == Visibility.Visible)
                {
                    var record_duration = time_reporter[0].GetNextAvg(record.duration ?? 0);
                    duration = time_reporter[1].GetNextAvg(duration);
                    var offset_interval = time_reporter[2].GetNextAvg();

                    Task.Run(() =>
                    {
                        uint done_count;
                        long remain_count;
                        switch (Stage)
                        {
                            case TraingingStages.Training:
                                done_count = DataProvider.TrainingCount;
                                remain_count = DataProvider.TrainingCount - Offset;
                                remain_count += DataProvider.ValidationCount;
                                remain_count += DataProvider.EvaluationCount;
                                break;
                            case TraingingStages.Validation:
                                done_count = DataProvider.ValidationCount;
                                remain_count = DataProvider.ValidationCount - Offset;
                                remain_count += DataProvider.EvaluationCount;
                                break;
                            case TraingingStages.Evaluation:
                                done_count = DataProvider.EvaluationCount;
                                remain_count = DataProvider.EvaluationCount - Offset;
                                break;
                            default: throw new Exception("Invalid stage type");
                        }

                        ProgressBar = Offset * 100D / done_count;
                        ProgressInfo =
    @$"#{Epoch} {Stage} {PrintUnsign(ProgressBar, 3):R}% ID({current_instrument_id})";
                        string message =
    @$"Data loading={GetDurationString(record_duration, 6)} Prediction={GetDurationString(duration, 6)} Left time={GetDurationString(offset_interval * remain_count, 3)}";
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProgressBar)));
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProgressInfo)));

                        ChangeStatus(INFO, message);
                        Networks_Report();
                    });

                    last_time_text_reported = DateTime.Now.Ticks;
                }


            if (DateTime.Now.Ticks - last_time_graph_reported > report_time_interval * 50)
                if (GraphReporting == Visibility.Visible)
                {
                    Task.Run(() =>
                    {
                        double accuracy = -1; double best = -1;
                        if (running_code == (int)TraingingStages.Evaluation)
                        {
                            for (var i = 0; i < OutOfLine.Count; i++)
                                if (accuracy < OutOfLine[i].Accuracy)
                                {
                                    accuracy = OutOfLine[i].Accuracy;
                                    best = OutOfLine[i].LastPrediction.ResultSignals[0];
                                }
                        }
                        else
                        {
                            for (var i = 0; i < Processes.Count; i++)
                                if (accuracy < Processes[i].CurrentAccuracy)
                                {
                                    accuracy = Processes[i].CurrentAccuracy;
                                    best = Processes[i].LastPredict.ResultSignals[0];
                                }
                        }

                        DataValues.Add(new ObservableValue(record.result[0]));
                        PredictedValues.Add(new ObservableValue(best));

                        while (DataValues.Count > MaxGraphPoints) DataValues.RemoveAt(0);
                        while (PredictedValues.Count > MaxGraphPoints) PredictedValues.RemoveAt(0);

                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DataValues)));
                        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PredictedValues)));
                    });

                    last_time_graph_reported = DateTime.Now.Ticks;
                }
        }
        protected override void OnStopped()
        {
            Log($"End Process:\tinstrument-id({last_instrument_id})\toffset({last_offset})\trecord({last_record_offset})");
            last_instrument_id = -1;

            TextReporting = Visibility.Collapsed;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextReporting)));
        }
        protected override void OnError(Exception ex)
        {
            ChangeStatusWithReport(ERROR, ex.Message, ex.StackTrace);

            TextReporting = Visibility.Collapsed;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextReporting)));

            GraphReporting = Visibility.Collapsed;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GraphReporting)));
        }

        private static string PrintUnsign(double val, int? digit)
        {
            if (digit.HasValue)
                val = Math.Round(val, digit.Value);
            return val.ToString("R");
        }
        #endregion

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
