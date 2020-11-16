using System;
using System.Collections.Generic;
using System.Text;

namespace Photon.Jiringi.DataCaching
{
    class CacherArray<T> : Cacher<T> where T : struct, ICacheData
    {
        public CacherArray(IOverFlowCheck<T> checker) : base(checker) { }

        public override int Count => cache.Count;

        public override void FillBuffer(double[] buffer, ref int index)
        {
            foreach (var val in cache)
                buffer[index++] = val.Value;
        }

        public static CacherArray<T>[] CreateMulti(int count, IOverFlowCheck<T> checker)
        {
            var result = new CacherArray<T>[count];
            for (var i = 0; i < count; i++)
                result[i] = new CacherArray<T>(checker);
            return result;
        }
    }
}
