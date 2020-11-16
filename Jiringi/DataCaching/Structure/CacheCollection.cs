using System;
using System.Collections.Generic;
using System.Text;

namespace Photon.Jiringi.DataCaching
{
    class CacheCollection<T> : ICache<T> where T : struct, ICacheData
    {
        public CacheCollection(ICache<T>[] caches)
        {
            this.caches = caches ?? throw new ArgumentNullException(nameof(caches));
        }

        private readonly ICache<T>[] caches;

        public int Count { get; private set; }

        public T? InjectData(T leader, T input)
        {
            Count = 0;
            T? cargo = input;
            foreach (var cache in caches)
                if (cargo.HasValue)
                {
                    cargo = cache.InjectData(leader, cargo.Value);
                    Count += cache.Count;
                }
                else break;

            return cargo;
        }
        public void FillBuffer(double[] buffer, ref int index)
        {
            foreach (var cache in caches)
                cache.FillBuffer(buffer, ref index);
        }
        public void Clear()
        {
            Count = 0;
            foreach (var cache in caches)
                cache.Clear();
        }

        public static CacheCollection<T> CreateMultiArray(
            int count, IOverFlowCheck<T> checker)
        {
            var result = new CacherArray<T>[count];
            for (var i = 0; i < count; i++)
                result[i] = new CacherArray<T>(checker);
            return new CacheCollection<T>(result);
        }
        public static CacheCollection<T> CreateMultiAvragtor(
            int count, IOverFlowCheck<T> checker)
        {
            var result = new CacherAvragtor<T>[count];
            for (var i = 0; i < count; i++)
                result[i] = new CacherAvragtor<T>(checker);
            return new CacheCollection<T>(result);
        }
        public static CacheCollection<T> CreateMultiGap(
            int count, IOverFlowCheck<T> checker)
        {
            var result = new CacherGap<T>[count];
            for (var i = 0; i < count; i++)
                result[i] = new CacherGap<T>(checker);
            return new CacheCollection<T>(result);
        }
    }
}
