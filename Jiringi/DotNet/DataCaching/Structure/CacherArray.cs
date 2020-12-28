using System;
using System.Collections.Generic;
using System.Text;

namespace Photon.Jiringi.DataCaching
{
    class CacherArray<T> : Cacher<T> where T : struct, ICacheData
    {
        public CacherArray(IOverFlowCheck<T> checker) : base(checker)
        {
            if (overflow_checker.MaxLength < 0) OutputCount = 0;
            else OutputCount = (uint)overflow_checker.MaxLength;
        }

        public override uint OutputCount { get; }

        public override void FillBuffer(double[] buffer, ref int index)
        {
            var start_index = index;
            foreach (var val in cache)
                buffer[index++] = val.Value;
            if (overflow_checker.MaxLength > 0)
                index = start_index + overflow_checker.MaxLength;
        }

        public static CacherArray<T>[] CreateMulti(int count, IOverFlowCheck<T> checker)
        {
            var result = new CacherArray<T>[count];
            for (var i = 0; i < count; i++)
                result[i] = new CacherArray<T>(checker);
            return result;
        }


        public override string ToString()
        {
            return $"All: " + base.ToString();
        }
    }
}
