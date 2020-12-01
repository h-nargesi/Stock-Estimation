using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using LiveCharts;
using LiveCharts.Defaults;
using Photon.Jiringi.DataCaching;
using Photon.Jiringi.DataProviding;
using Photon.NeuralNetwork.Chista;
using Photon.NeuralNetwork.Chista.Trainer;

namespace Photon.Jiringi
{
    public class NetProcess : Instructor, INotifyPropertyChanged
    {
        public NetProcess() : base(new DataProvider())
        {
            time_reporter = new TimeReporter[3];

            DataValues = new ChartValues<ObservableValue>();
            PredictedValues = new ChartValues<ObservableValue>();
            PredictedRecently = new ChartValues<ObservableValue>();

            App.Setting.Changed += Setting_Changed;
        }

        private void Setting_Changed(object sender, Config.ConfigChangedEventArg e)
        {
            if (Stopped) return;

            if (TextReportingVisibility != (App.Setting.Process.TextReporting ? Visibility.Visible : Visibility.Collapsed))
            {
                time_reporter[0].Clear();
                time_reporter[1].Clear();
                time_reporter[2].Clear();

                TextReportingVisibility = App.Setting.Process.TextReporting
                    ? Visibility.Visible : Visibility.Collapsed;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextReportingVisibility)));
            }

            if (GraphReportingVisibility != 
                (App.Setting.Process.GraphReporting ? Visibility.Visible : Visibility.Collapsed))
            {
                if (App.Setting.Process.GraphReporting)
                {
                    PredictedRecently.Clear();
                    PredictedValues.Clear();
                    DataValues.Clear();
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PredictedRecently)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PredictedValues)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DataValues)));
                }

                GraphReportingVisibility = App.Setting.Process.GraphReporting
                    ? Visibility.Visible : Visibility.Collapsed;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GraphReportingVisibility)));
            }
        }


        #region Status Properties

        public Brush StatusColor { get; private set; } = BLANK;
        public string StatusMessage { get; private set; }
        public string NetworkReport { get; private set; }
        public double ProgressBar { get; private set; }
        public string ProgressInfo { get; private set; }
        public ChartValues<ObservableValue> DataValues { get; }
        public ChartValues<ObservableValue> PredictedValues { get; }
        public ChartValues<ObservableValue> PredictedRecently { get; }
        public int MaxGraphPoints { get; set; } = DataProviding.DataProvider.RECORDS_PREVIOUS_ONE_YEAR;
        public Visibility TextReportingVisibility { get; private set; } = Visibility.Collapsed;
        public Visibility GraphReportingVisibility { get; private set; } = Visibility.Collapsed;
        #endregion


        #region Change Status Methods
        public void Networks_Report()
        {
            // prepare report info
            double accuracy = 0;
            foreach (var prc in Processes)
                accuracy = Math.Max(prc.Accuracy, accuracy);

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
        private void ChangeStatus(Brush state, string message)
        {
            StatusColor = state;
            StatusMessage = message;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusColor)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(StatusMessage)));
        }
        public void ChangeStatusWithLog(Brush state, string message)
        {
            ChangeStatus(state, message);
            App.Log(message);
        }
        public void ChangeStatusWithLog(Brush state, string message, string report)
        {
            ChangeStatus(state, message);
            App.Log(message, report);
        }

        public static readonly SolidColorBrush
            BLANK = new SolidColorBrush(Color.FromRgb(64, 64, 64)),
            INFO = new SolidColorBrush(Color.FromRgb(47, 117, 181)),
            ERROR = new SolidColorBrush(Color.FromRgb(150, 0, 50)),
            DONE = new SolidColorBrush(Color.FromRgb(0, 176, 80));
        #endregion


        #region Instructor Events

        private const int report_time_interval = 30 * 10000;
        private long last_time_text_reported;
        private int last_instrument_id = -1;
        private uint last_offset = 0, last_record_offset = 0;
        private readonly TimeReporter[] time_reporter;
        private BasicalMethodsTypes method_type = 0;

        protected override void OnInitialize()
        {
            App.Log("Process initializing");

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

            PredictedRecently.Clear();
            PredictedValues.Clear();
            DataValues.Clear();
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PredictedRecently)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PredictedValues)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DataValues)));

            TextReportingVisibility = App.Setting.Process.TextReporting ? Visibility.Visible : Visibility.Collapsed;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextReportingVisibility)));

            GraphReportingVisibility = App.Setting.Process.GraphReporting ? Visibility.Visible : Visibility.Collapsed;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GraphReportingVisibility)));

            method_type = ((DataProvider)DataProvider).Method;
        }
        protected override void ReflectFinished(Record record, long duration, int running_code)
        {
            var (current_instrument_id, current_offset, current_record_offset, result_price) =
                (ValueTuple<int, uint, uint, double>)record.extra;
            if (last_instrument_id != current_instrument_id)
            {
                if (last_instrument_id > -1)
                    App.Log($"End Section:\tinstrument-id({last_instrument_id})\toffset({last_offset})\trecord({last_record_offset})");
                App.Log($"Start Section:\tinstrument-id({current_instrument_id})\toffset({current_offset})\trecord({current_record_offset})");
            }
            last_instrument_id = current_instrument_id;
            last_offset = current_offset;
            last_record_offset = current_record_offset;

            if (TextReportingVisibility == Visibility.Visible)
            {
                var record_duration = time_reporter[0].GetNextAvg(record.duration ?? 0);
                duration = time_reporter[1].GetNextAvg(duration);
                var offset_interval = time_reporter[2].GetNextAvg();

                if (DateTime.Now.Ticks - last_time_text_reported > report_time_interval)
                {
                    long remain_count;
                    var done_count = Stage switch
                    {
                        TrainingStages.Training => DataProvider.TrainingCount,
                        TrainingStages.Validation => DataProvider.ValidationCount,
                        TrainingStages.Evaluation => DataProvider.EvaluationCount,
                        _ => throw new Exception("Invalid stage type"),
                    };
                    remain_count = done_count - Offset;

                    ProgressBar = Offset * 100D / done_count;
                    ProgressInfo =
@$"#{Epoch} {Stage} {PrintUnsign(ProgressBar, 3):R}% ID({current_instrument_id})";
                    string message =
@$"Data loading={GetDurationString(record_duration, 6)} Prediction={GetDurationString(duration, 6)} Left time={GetDurationString(offset_interval * (done_count - Offset), 3)}";
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProgressBar)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ProgressInfo)));

                    ChangeStatus(INFO, message);
                    Networks_Report();

                    last_time_text_reported = DateTime.Now.Ticks;
                }
            }


            if (GraphReportingVisibility == Visibility.Visible)
            {
                DataValues.Add(new ObservableValue(result_price));
                while (DataValues.Count > MaxGraphPoints) DataValues.RemoveAt(0);
                Task.Run(() =>
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DataValues))));

                // Find best
                double accuracy = 0; double data_delta = 0; int best = -1;
                if (running_code == (int)TrainingStages.Evaluation)
                {
                    for (var i = 0; i < OutOfLine.Count; i++)
                        if (accuracy < OutOfLine[i].Accuracy)
                        {
                            accuracy = OutOfLine[i].Accuracy;
                            data_delta = OutOfLine[i].LastPrediction.ResultSignals[0];
                            best = i;
                        }
                }
                else
                {
                    for (var i = 0; i < Processes.Count; i++)
                        if (accuracy < Processes[i].Accuracy)
                        {
                            accuracy = Processes[i].Accuracy;
                            data_delta = Processes[i].LastPrediction.ResultSignals[0];
                            best = i;
                        }
                }

                PredictedRecently.Add(new ObservableValue(0));
                double data_price, result_factor, data_factor;
                switch (method_type)
                {
                    case BasicalMethodsTypes.ChangeBased:
                        data_factor = result_factor = 1;
                        for (int i = 0; i < DataProviding.DataProvider.RESULT_COUNT; i++)
                            result_factor *= 1 + record.result[i] / 100D;
                        if (running_code == (int)TrainingStages.Evaluation)
                            for (int i = DataProviding.DataProvider.RESULT_COUNT - 1; i >= 0; i--)
                            {
                                data_factor *= 1 + OutOfLine[best].LastPrediction.ResultSignals[i] / 100D;
                                if (PredictedRecently.Count >= i + 1)
                                {
                                    data_price = result_price * (data_factor / result_factor);
                                    PredictedRecently[^(i + 1)].Value = data_price;
                                }
                            }
                        else
                            for (int i = DataProviding.DataProvider.RESULT_COUNT - 1; i >= 0; i--)
                            {
                                data_factor *= 1 + Processes[best].LastPrediction.ResultSignals[i] / 100D;
                                if (PredictedRecently.Count >= i + 1)
                                {
                                    data_price = result_price * (data_factor / result_factor);
                                    PredictedRecently[^(i + 1)].Value = data_price;
                                }
                            }
                        data_price = result_price * (data_factor / result_factor);
                        break;
                    case BasicalMethodsTypes.AngleBased:
                        result_factor = 1 + CacherRadian.K * Math.Tan(record.result[0]);
                        if (running_code == (int)TrainingStages.Evaluation)
                            for (int i = DataProviding.DataProvider.RESULT_COUNT - 1; i >= 0; i--)
                            {
                                data_factor = 1 + CacherRadian.K * Math.Tan(
                                    OutOfLine[best].LastPrediction.ResultSignals[i]);
                                if (PredictedRecently.Count >= i + 1)
                                {
                                    data_price = result_price * (data_factor / result_factor);
                                    PredictedRecently[^(i + 1)].Value = data_price;
                                }
                            }
                        else
                            for (int i = DataProviding.DataProvider.RESULT_COUNT - 1; i >= 0; i--)
                            {
                                data_factor = 1 + CacherRadian.K * Math.Tan(
                                    Processes[best].LastPrediction.ResultSignals[i]);
                                if (PredictedRecently.Count >= i + 1)
                                {
                                    data_price = result_price * (data_factor / result_factor);
                                    PredictedRecently[^(i + 1)].Value = data_price;
                                }
                            }
                        data_factor = 1 + CacherRadian.K * Math.Tan(data_delta);
                        data_price = result_price * (data_factor / result_factor);
                        break;
                    default: data_price = 0; break;
                }

                PredictedValues.Add(new ObservableValue(data_price));

                // it should not be greater than "DataValues"
                while (PredictedValues.Count > DataValues.Count) PredictedValues.RemoveAt(0);
                // it should not be greater than "DataValues"
                while (PredictedRecently.Count > DataValues.Count) PredictedRecently.RemoveAt(0);
                Task.Run(() =>
                {
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PredictedValues)));
                    PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PredictedRecently)));
                });

                // let the ui to change
                Thread.Sleep(1000);
            }
        }
        protected override void OnFinished()
        {
        }
        protected override void OnStopped()
        {
            App.Log($"End Process:\tinstrument-id({last_instrument_id})\toffset({last_offset})\trecord({last_record_offset})");
            last_instrument_id = -1;

            TextReportingVisibility = Visibility.Collapsed;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextReportingVisibility)));
        }
        protected override void OnError(Exception ex)
        {
            ChangeStatusWithLog(ERROR, ex.Message, ex.StackTrace);

            TextReportingVisibility = Visibility.Collapsed;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TextReportingVisibility)));

            GraphReportingVisibility = Visibility.Collapsed;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(GraphReportingVisibility)));
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
