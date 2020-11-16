using System;
using System.Collections.Generic;
using System.Text;

namespace Photon.Jiringi.DataCaching
{
    class CacherAvragtor<T> : Cacher<T> where T : struct, ICacheData
    {
        public CacherAvragtor(IOverFlowCheck<T> checker) : base(checker) { }

        private double current_sum;

        public override int Count => cache.Count > 0 ? 1 : 0;

        public override T? InjectData(T leader, T input)
        {
            var out_put_value = base.InjectData(leader, input);

            if (out_put_value.HasValue)
                current_sum -= out_put_value.Value.Value;
            current_sum += input.Value;

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

        public static CacherAvragtor<T>[] CreateMulti(int count, IOverFlowCheck<T> checker)
        {
            var result = new CacherAvragtor<T>[count];
            for (var i = 0; i < count; i++)
                result[i] = new CacherAvragtor<T>(checker);
            return result;
        }
    }
}
