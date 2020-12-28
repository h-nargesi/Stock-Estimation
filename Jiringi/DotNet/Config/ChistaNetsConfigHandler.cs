using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Photon.Jiringi.Config
{
    public class ChistaNetsConfigHandler : ConfigHandler
    {
        public ChistaNetsConfigHandler(ConfigHandler setting) : base(setting) { }

        public const string key = "chista-nets";
        private const string model_hidden_count = "hidden-count";
        private const string model_conduction = "hidden-conduction";
        private const string model_output_func = "output-function";
        private const string model_error_func = "error-function";
        private const string model_output_count = "output-count";

        public int[] HiddenCount
        {
            get { return GetSettingArray(model_hidden_count, 0); }
            set { SetSetting(model_hidden_count, value); }
        }

        public string ConductionDefault { get; set; } = "soft-relu";
        public string Conduction
        {
            get { return GetSetting(model_conduction, ConductionDefault); }
            set { SetSetting(model_conduction, value); }
        }

        public string OutputFunctionDefault { get; set; } = "sigmoind";
        public string OutputFunction
        {
            get { return GetSetting(model_output_func, OutputFunctionDefault); }
            set { SetSetting(model_output_func, value); }
        }

        public string ErrorFunctionDefault { get; set; } = "error-stack";
        public string ErrorFunction
        {
            get { return GetSetting(model_error_func, ErrorFunctionDefault); }
            set { SetSetting(model_error_func, value); }
        }

        public int OutputCountDefault { get; set; } = 20;
        public int OutputCount
        {
            get { return GetSetting(model_output_count, OutputCountDefault); }
            set { SetSetting(model_output_count, value); }
        }

    }
}
