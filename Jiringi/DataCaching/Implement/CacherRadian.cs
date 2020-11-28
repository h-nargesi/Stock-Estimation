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

        public const double K = 0.05;
        public override uint OutputCount { get; }

        public override void FillBuffer(double[] buffer, ref int index)
        {
            index += overflow_checker.MaxLength;
            if (cache.Count < 1) return;

            double factor = 1;
            int i = index;
            foreach (var val in reverse_enumerator)
            {
                i--;
                // cumulative factor
                factor *= 1 + val.Change / 100D;
                // price changes from base price
                // scaled price changes
                buffer[i] = (factor - 1) / K;
                // convert price chnages to angle changes
                buffer[i] = Math.Atan(buffer[i]);
            }
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
