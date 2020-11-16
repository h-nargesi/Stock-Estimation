using System;
using System.Collections.Generic;
using System.Text;

namespace Photon.Jiringi.DataCaching
{
    class Cache<T> where T : struct, ICacheData
    {
        public Cache(ICache<T>[] caches)
        {
            this.caches = caches;
        }

        private readonly ICache<T>[] caches;

        public bool InjectData(T value)
        {
            T? cargo = value;
            foreach (var cache in caches)
                if (cargo.HasValue)
                    cargo = cache.InjectData(value, cargo.Value);
                else if (cache.IsNecessary) return false;
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
            foreach (var cache in caches)
                cache.Clear();
        }

        public static CacheCollection<T> CreateMultiArray(
            int count, IOverFlowCheck<T> checker, bool is_necessary)
        {
            var result = new CacherArray<T>[count];
            for (var i = 0; i < count; i++)
                result[i] = new CacherArray<T>(checker, is_necessary);
            return new CacheCollection<T>(result, is_necessary);
        }
        public static CacheCollection<T> CreateMultiAvragtor(
            int count, IOverFlowCheck<T> checker, bool is_necessary)
        {
            var result = new CacherAvragtor<T>[count];
            for (var i = 0; i < count; i++)
                result[i] = new CacherAvragtor<T>(checker, is_necessary);
            return new CacheCollection<T>(result, is_necessary);
        }
        public static CacheCollection<T> CreateMultiGap(
            int count, IOverFlowCheck<T> checker, bool is_necessary)
        {
            var result = new CacherGap<T>[count];
            for (var i = 0; i < count; i++)
                result[i] = new CacherGap<T>(checker, is_necessary);
            return new CacheCollection<T>(result, is_necessary);
        }
    }
}
