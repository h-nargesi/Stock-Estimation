using System;
using System.Collections.Generic;
using System.Text;
using Photon.Jiringi.DataProviding;
using Photon.NeuralNetwork.Chista;
using Photon.NeuralNetwork.Chista.Implement;

namespace Photon.Jiringi.NetSpecifics
{
    static class Tools
    {
        public static IConduction Conduction(string key)
        {
            return key switch
            {
                "sigmoind" => (IConduction)new Sigmoind(),
                "soft-relu" => new SoftReLU(),
                "relu" => new ReLU(),
                "soft-max" => new SoftMax(),
                _ => throw new Exception("invalid conduction function")
            };
        }

        public static IErrorFunction ErrorFunction(string key)
        {
            return key switch
            {
                "errorest" => (IErrorFunction)new Errorest(),
                "error-stack" => new ErrorStack(DataProvider.RESULT_COUNT),
                "classification" => new Classification(0),
                "tagging" => new Tagging(0.8, 0.4),
                _ => throw new Exception("invalid error function")
            };
        }
    }
}
