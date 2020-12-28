using System;
using System.Collections.Generic;
using System.Text;

namespace Photon.Jiringi.DataCaching
{
    class CacherAvragtor<T> : Cacher<T> where T : struct, ICacheData
    {
        public CacherAvragtor(IOverFlowCheck<T> checker) : base(checker) { }

        private double current_sum;

        public override uint OutputCount => 1;

        public override void InjectDataToFirst(T leader, LinkedList<T> cargo)
        {
            foreach (var input in cargo)
                current_sum += input.Value;

            base.InjectDataToFirst(leader, cargo);

            foreach (var output in cargo)
                current_sum -= output.Value;
        }
        public override void InjectDataToLast(T leader, LinkedList<T> cargo)
        {
            foreach (var input in cargo)
                current_sum += input.Value;

            base.InjectDataToLast(leader, cargo);

            foreach (var output in cargo)
                current_sum -= output.Value;
        }
        public override void FillBuffer(double[] buffer, ref int index)
        {
            if (cache.Count > 0)
                buffer[index] = current_sum / cache.Count;
            index++;
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


        public override string ToString()
        {
            return $"Avg: " + base.ToString();
        }
    }
}
