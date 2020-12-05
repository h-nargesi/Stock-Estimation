using System;
using MathNet.Numerics.LinearAlgebra;
using Photon.NeuralNetwork.Chista;
using Photon.NeuralNetwork.Chista.Implement;
using Photon.NeuralNetwork.Chista.Serializer;

namespace Photon.Jiringi.NetSpecifics
{
    [FunctionCode(code:0x8001, parameter_length:4)]
    public class ErrorStack : FunctionSerializer<ErrorStack>, IErrorFunction, ISerializableFunction
    {
        private readonly Vector<double> indexed;

        public ErrorStack(int output_count)
        {
            var ix = new double[output_count];
            if (output_count == 1) ix[0] = 1;
            else
            {
                var r = output_count - 1;
                var c = r / 2D;
                var a = r - (0.4D / r);
                for (int i = 0; i < output_count; i++)
                    ix[output_count - (i + 1)] = 1 + (i - c) / a;
            }

            indexed = Vector<double>.Build.DenseOfArray(ix);
        }

        public int IndexCount => indexed.Count;
        public override string Name => nameof(ErrorStack);

        public Vector<double> ErrorCalculation(Vector<double> output, Vector<double> values)
        {
            return indexed.PointwiseMultiply(values - output);
        }
        public double Accuracy(NeuralNetworkFlash prediction)
        {
            return 1 - prediction.ErrorAverage;
        }


        public override byte[] Serialize(ErrorStack func)
        {
            return BitConverter.GetBytes(func.indexed.Count);
        }
        public override ErrorStack Restore(byte[] parameters)
        {
            return new ErrorStack(BitConverter.ToInt32(parameters));
        }

        public override string ToString()
        {
            return "ErrorStack";
        }
    }
}