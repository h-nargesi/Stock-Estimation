using System;
using System.Collections.Generic;
using System.Text;

namespace Photon.Jiringi.DataCaching
{
    class CacheBuilder<T> where T : struct, ICacheData
    {
        bool is_necessary = true;
        List<ICache<T>> caches = new List<ICache<T>>();

        public CacheBuilder<T> IsNotNecessaryFromHere()
        {
            is_necessary = false;
            return this;
        }
        public CacheBuilder<T> AddCacherArray(IOverFlowCheck<T> checker)
        {
            caches.Add(new CacherArray<T>(checker, is_necessary));
            return this;
        }
        public CacheBuilder<T> AddCacherAvragtor(IOverFlowCheck<T> checker)
        {
            caches.Add(new CacherAvragtor<T>(checker, is_necessary));
            return this;
        }
        public CacheBuilder<T> AddCacherGap(IOverFlowCheck<T> checker)
        {
            caches.Add(new CacherGap<T>(checker, is_necessary));
            return this;
        }
        public CacheBuilder<T> AddCacherArrayCollection(int count, IOverFlowCheck<T> checker)
        {
            caches.Add(CacheCollection<T>.CreateMultiArray(count, checker, is_necessary));
            return this;
        }
        public CacheBuilder<T> AddCacherAvragtorCollection(int count, IOverFlowCheck<T> checker)
        {
            caches.Add(CacheCollection<T>.CreateMultiAvragtor(count, checker, is_necessary));
            return this;
        }
        public CacheBuilder<T> AddCacherGapCollection(int count, IOverFlowCheck<T> checker)
        {
            caches.Add(CacheCollection<T>.CreateMultiGap(count, checker, is_necessary));
            return this;
        }

        public ICache<T> CacheCollection()
        {
            if (caches.Count < 1) throw new Exception("No cache is set.");
            else if (caches.Count == 1) return caches[0];
            else return new CacheCollection<T>(caches.ToArray(), false);
        }
    }
}
