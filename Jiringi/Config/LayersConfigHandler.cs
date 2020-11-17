﻿using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Photon.Jiringi.Config
{
    public class LayersConfigHandler : ConfigHandler
    {
        public LayersConfigHandler(JObject setting) : base(setting) { }

        public const string key = "layer";
        private const string nodes_count = "nodes-count";
        private const string model_conduction = "conduction";
        private const string model_output = "output";

        public int[] NodesCount
        {
            get { return GetSettingArray<int>(nodes_count); }
            set { SetSetting(nodes_count, value); }
        }

        public string ConductionDefault { get; set; } = "soft-relu";
        public string Conduction
        {
            get { return GetSetting(model_conduction, ConductionDefault); }
            set { SetSetting(model_conduction, value); }
        }

        public string OutputConductionDefault { get; set; } = "sigmoind";
        public string OutputConduction
        {
            get { return GetSetting(model_output, OutputConductionDefault); }
            set { SetSetting(model_output, value); }
        }
    }
}
