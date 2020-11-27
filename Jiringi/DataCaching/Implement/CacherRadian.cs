using System;
using System.Collections.Generic;
using System.Text;

namespace Photon.Jiringi.DataCaching
{
    class CacherRadian : Cacher<StockTradeData>
    {
        public CacherRadian(IOverFlowCheck<StockTradeData> checker) : base(checker)
        {
            if (overflow_checker.MaxLength < 0) OutputCount = 0;
            else OutputCount = (uint)overflow_checker.MaxLength;
        }

        private const double PI2 = Math.PI / 2;
        public override uint OutputCount { get; }

        public override void FillBuffer(double[] buffer, ref int index)
        {
            var start_index = index;
            if (cache.Count > 0)
            {
                double factor = 1;
                index += overflow_checker.MaxLength;
                foreach (var val in reverse_enumerator)
                {
                    index--;
                    // factor based on orginal point
                    factor *= 1 + val.Value;
                    // the tangent of angle finding
                    buffer[index] = factor / (overflow_checker.MaxLength - index);
                    // the angle (radian) finding
                    buffer[index] = (Math.Atan(buffer[index]) + PI2) / Math.PI;
                }
            }
            index = start_index + overflow_checker.MaxLength;
        }

        public static CacherRadian[] CreateMulti(int count, IOverFlowCheck<StockTradeData> checker)
        {
            var result = new CacherRadian[count];
            for (var i = 0; i < count; i++)
                result[i] = new CacherRadian(checker);
            return result;
        }


        public override string ToString()
        {
            return $"Radian: " + base.ToString();
        }
    }
}
