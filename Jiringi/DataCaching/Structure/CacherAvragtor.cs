using System;
using System.Collections.Generic;
using System.Text;

namespace Photon.Jiringi.DataCaching
{
    class CacherAvragtor<T> : Cacher<T> where T : struct, ICacheData
    {
        public CacherAvragtor(IOverFlowCheck<T> checker, bool is_necessary) : base(checker, is_necessary) { }

        private double current_sum;

        public override T? InjectData(T index, T passed)
        {
            var out_put_value = base.InjectData(index, passed);

            if (out_put_value.HasValue)
                current_sum -= out_put_value.Value.Value;
            current_sum += passed.Value;

            return out_put_value;
        }
        public override void FillBuffer(double[] buffer, ref int index)
        {
            buffer[index++] = current_sum / cache.Count;
        }
        public override void Clear()
        {
            base.Clear();
            current_sum = 0;
        }

        public static CacherAvragtor<T>[] CreateMulti(int count, IOverFlowCheck<T> checker, bool isnecessary)
        {
            var result = new CacherAvragtor<T>[count];
            for (var i = 0; i < count; i++)
                result[i] = new CacherAvragtor<T>(checker, isnecessary);
            return result;
        }
    }
}
