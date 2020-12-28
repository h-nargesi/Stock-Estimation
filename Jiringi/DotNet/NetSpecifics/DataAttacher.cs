using System;
using System.Collections.Generic;
using System.Text;
using Photon.NeuralNetwork.Chista.Implement;
using Photon.NeuralNetwork.Chista.Serializer;

namespace Photon.Jiringi.NetSpecifics
{
    [FunctionCode(code: 0x8002, parameter_length: 0)]
    internal class DataAttacher : FunctionSerializer<DataAttacher>, IDataCombiner
    {
        public override string Name => "DataAttacher";

        public double[] Combine(double[] output, double[] data)
        {
            var result = new double[output.Length + data.Length];

            int r = 0, i = 0;
            while(i < output.Length)
                result[r++] = output[i++];

            i = 0;
            while (i < data.Length)
                result[r++] = data[i++];

            return result;
        }

        public override DataAttacher Restore(byte[] _)
        {
            return this;
        }
        public override byte[] Serialize(DataAttacher func)
        {
            return null;
        }

        public override string ToString()
        {
            return "DataAttacher";
        }
    }
}
