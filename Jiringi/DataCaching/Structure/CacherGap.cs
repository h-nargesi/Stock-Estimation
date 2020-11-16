using System;
using System.Collections.Generic;
using System.Text;

namespace Photon.Jiringi.DataCaching
{
    class CacherGap<T> : Cacher<T> where T : struct, ICacheData
    {
        public CacherGap(IOverFlowCheck<T> checker, bool is_necessary) : base(checker, is_necessary) { }

        public override void FillBuffer(double[] buffer, ref int index) { }

        public static CacherGap<T>[] CreateMulti(int count, IOverFlowCheck<T> checker, bool isnecessary)
        {
            var result = new CacherGap<T>[count];
            for (var i = 0; i < count; i++)
                result[i] = new CacherGap<T>(checker, isnecessary);
            return result;
        }
    }
}
