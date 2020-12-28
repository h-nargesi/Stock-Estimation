using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json.Linq;

namespace Photon.Jiringi.Config
{
    public class InitialNetsInfoConfigHandler : ConfigHandler
    {
        public InitialNetsInfoConfigHandler(ConfigHandler setting) : base(setting) { }

        public const string key = "initial-nets-info";
        private const string images_path = "images-path";
        private const string images_count = "images-count";
        private const string learning_factor = "learning-factor";
        private const string certainty_factor = "certainty-factor";
        private const string dropout_factor = "dropout-factor";
        private const string basical_method = "basical-method";

        public string ImagesPathDefault { get; set; } = "";
        public string ImagesPath
        {
            get { return GetSetting(images_path, ImagesPathDefault); }
            set { SetSetting(images_path, value); }
        }

        public int ImagesCountDefault { get; set; } = 10;
        public int ImagesCount
        {
            get { return GetSetting(images_count, ImagesCountDefault); }
            set { SetSetting(images_count, value); }
        }

        public double LearningFactorDefault { get; set; } = 0.01;
        public double LearningFactor
        {
            get { return GetSetting(learning_factor, LearningFactorDefault); }
            set { SetSetting(learning_factor, value); }
        }

        public double CertaintyFactorDefault { get; set; } = 0;
        public double CertaintyFactor
        {
            get { return GetSetting(certainty_factor, CertaintyFactorDefault); }
            set { SetSetting(certainty_factor, value); }
        }

        public double DropoutFactorDefault { get; set; } = 0;
        public double DropoutFactor
        {
            get { return GetSetting(dropout_factor, DropoutFactorDefault); }
            set { SetSetting(dropout_factor, value); }
        }

        public BasicalMethodsTypes BasicalMethodDefault { get; set; } = BasicalMethodsTypes.AngleBased;
        public BasicalMethodsTypes BasicalMethod
        {
            get
            {
                var str = GetSetting(basical_method, BasicalMethodDefault.ToString());
                return (BasicalMethodsTypes)Enum.Parse(typeof(BasicalMethodsTypes), str);
            }
            set { SetSetting(basical_method, value.ToString()); }
        }


        private ChistaNetsConfigHandler[] chista_nets_instance;
        public ChistaNetsConfigHandler[] ChistaNets
        {
            get
            {
                if (chista_nets_instance == null)
                {
                    var layers = GetOrCreateConfigArray(ChistaNetsConfigHandler.key);
                    chista_nets_instance = new ChistaNetsConfigHandler[layers.Length];
                    for (int i = 0; i < layers.Length; i++)
                        chista_nets_instance[i] = new ChistaNetsConfigHandler(layers[i]);
                }

                return chista_nets_instance;
            }
        }

    }
}
