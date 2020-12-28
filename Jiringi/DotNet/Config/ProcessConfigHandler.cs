using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;
using Photon.NeuralNetwork.Chista.Trainer;

namespace Photon.Jiringi.Config
{
    public class ProcessConfigHandler : ConfigHandler
    {
        public ProcessConfigHandler(ConfigHandler setting) : base(setting) { }

        public const string key = "process";
        private const string process_stage = "stage";
        private const string process_offset = "offset";
        private const string process_left_time = "left-time-estimate-length";
        private const string process_graph_reporting = "graph-reporting";
        private const string process_text_reporting = "text-reporting";

        public TrainingStages? Stage
        {
            get
            {
                var str = GetSetting<string>(process_stage, null);
                if (str == null) return null;

                str = str.ToLower();
                str = char.ToUpper(str[0]) + str.Substring(1);
                return (TrainingStages)Enum.Parse(typeof(TrainingStages), str);
            }
            set { SetSetting(process_stage, value?.ToString().ToLower()); }
        }

        public uint? Offset
        {
            get { return GetSetting<uint?>(process_offset, null); }
            set { SetSetting(process_offset, value); }
        }

        public uint LeftTimeEstimateLength
        {
            get { return GetSetting<uint>(process_left_time, 1000); }
            set { SetSetting(process_left_time, value); }
        }

        public bool TextReportingDefault { get; set; } = true;
        public bool TextReporting
        {
            get { return GetSetting(process_text_reporting, TextReportingDefault); }
            set { SetSetting(process_text_reporting, value); }
        }
        public string TextReportingPath
        {
            get { return $"{CurrentPath}.{process_text_reporting}"; }
        }

        public bool GraphReportingDefault { get; set; } = true;
        public bool GraphReporting
        {
            get { return GetSetting(process_graph_reporting, GraphReportingDefault); }
            set { SetSetting(process_graph_reporting, value); }
        }
        public string GraphReportingPath
        {
            get { return $"{CurrentPath}.{process_graph_reporting}"; }
        }

    }
}
