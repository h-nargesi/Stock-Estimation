using System;
using System.Collections.Generic;
using System.Text;

namespace Photon.Jiringi.DataCaching
{
    class CacheBuilder<T> where T : struct, ICacheData
    {
        private readonly List<ICache<T>> caches = new List<ICache<T>>();

        public CacheBuilder<T> AddCacher(Cacher<T> cacher)
        {
            caches.Add(cacher);
            return this;
        }
        public CacheBuilder<T> AddCacherArray(IOverFlowCheck<T> checker)
        {
            caches.Add(new CacherArray<T>(checker));
            return this;
        }
        public CacheBuilder<T> AddCacherAvragtor(IOverFlowCheck<T> checker)
        {
            caches.Add(new CacherAvragtor<T>(checker));
            return this;
        }
        public CacheBuilder<T> AddCacherGap(IOverFlowCheck<T> checker)
        {
            caches.Add(new CacherGap<T>(checker));
            return this;
        }
        public CacheBuilder<T> AddCacherArrayCollection(int count, IOverFlowCheck<T> checker)
        {
            caches.Add(CacheCollection<T>.CreateMultiArray(count, checker));
            return this;
        }
        public CacheBuilder<T> AddCacherAvragtorCollection(int count, IOverFlowCheck<T> checker)
        {
            caches.Add(CacheCollection<T>.CreateMultiAvragtor(count, checker));
            return this;
        }
        public CacheBuilder<T> AddCacherGapCollection(int count, IOverFlowCheck<T> checker)
        {
            caches.Add(CacheCollection<T>.CreateMultiGap(count, checker));
            return this;
        }
        public CacheBuilder<T> AddCacheCollection(CacheCollection<T> checkes)
        {
            caches.Add(checkes);
            return this;
        }

        public ICache<T>[] CacheArray()
        {
            if (caches.Count < 1) throw new Exception("No cache is set.");
            else return caches.ToArray();
        }

        public ICache<T> CacheCollection()
        {
            if (caches.Count < 1) throw new Exception("No cache is set.");
            else if (caches.Count == 1) return caches[0];
            else return new CacheCollection<T>(caches.ToArray());
        }
    }
}
