using System;
using System.Collections.Generic;
using System.Text;

namespace Photon.Jiringi.DataCaching
{
    class CacherGap<T> : Cacher<T> where T : struct, ICacheData
    {
        public CacherGap(IOverFlowCheck<T> checker) : base(checker) { }

        public override void FillBuffer(double[] buffer, ref int index) { }

        public override uint OutputCount => 0;

        public static CacherGap<T>[] CreateMulti(int count, IOverFlowCheck<T> checker)
        {
            var result = new CacherGap<T>[count];
            for (var i = 0; i < count; i++)
                result[i] = new CacherGap<T>(checker);
            return result;
        }
    }
}
