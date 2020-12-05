using System;
using System.Collections.Generic;
using System.Text;
using Photon.NeuralNetwork.Chista;
using Photon.NeuralNetwork.Chista.Trainer;

namespace Photon.Jiringi.NetSpecifics
{
    static class ErrorStackReplacement
    {
        public static INetProcess Replace(INetProcess net_process)
        {
            var running_image = net_process.RunningChistaNet.Image();
            var stable_image = net_process.StableImage;
            var replace = false;
            var prc_info = net_process.ProcessInfo();

            if(running_image is NeuralNetworkImage current_image_obj &&
                current_image_obj.error_fnc is NeuralNetwork.Chista.Deprecated.ErrorStack cdep)
            {
                prc_info.running_image = new NeuralNetworkImage(
                    current_image_obj.layers,
                    new ErrorStack(cdep.IndexCount),
                    current_image_obj.input_convertor,
                    current_image_obj.output_convertor,
                    current_image_obj.regularization);
                replace = true;
            }

            if (stable_image is NeuralNetworkImage best_image_obj &&
                best_image_obj.error_fnc is NeuralNetwork.Chista.Deprecated.ErrorStack bdep)
            {
                prc_info.stable_image = new NeuralNetworkImage(
                    best_image_obj.layers,
                    new ErrorStack(bdep.IndexCount),
                    best_image_obj.input_convertor,
                    best_image_obj.output_convertor,
                    best_image_obj.regularization);
                replace = true;
            }

            if (replace) return prc_info.TrainProcess();
            else return null;
        }
    }
}
