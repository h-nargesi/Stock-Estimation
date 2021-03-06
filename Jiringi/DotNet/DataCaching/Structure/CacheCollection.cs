﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Photon.Jiringi.DataCaching
{
    class CacheCollection<T> : ICache<T> where T : struct, ICacheData
    {
        public CacheCollection(ICache<T>[] caches)
        {
            this.caches = caches ?? throw new ArgumentNullException(nameof(caches));
            for (var c = 0; c < caches.Length; c++)
                if (caches[c] == null)
                    throw new ArgumentNullException(nameof(caches), "caches index " + c);
                else OutputCount += caches[c].OutputCount;
        }

        private int first_empty_cache = 0;
        private readonly ICache<T>[] caches;

        public bool IsFull => caches[^1].IsFull;
        public uint OutputCount { get; }
        public uint RealDataCount { get; private set; }
        public T? FirstValue => caches[0].FirstValue;
        public T? LastValue { get; private set; }

        public void InjectDataToFirst(T leader, LinkedList<T> cargo)
        {
            LastValue = null;
            first_empty_cache = 0;
            RealDataCount = 0;

            int i = 0;
            foreach (var cache in caches)
            {
                cache.InjectDataToFirst(leader, cargo);
                RealDataCount += cache.RealDataCount;
                if (cache.LastValue.HasValue) LastValue = cache.LastValue;
                if (cache.RealDataCount > 0) first_empty_cache = i;
                i++;
            }
        }
        public void InjectDataToLast(T leader, LinkedList<T> cargo)
        {
            if (cargo.Count < 1) return;

            while (first_empty_cache < caches.Length)
            {
                RealDataCount -= caches[first_empty_cache].RealDataCount;
                caches[first_empty_cache].InjectDataToLast(leader, cargo);
                RealDataCount += caches[first_empty_cache].RealDataCount;
                LastValue = caches[first_empty_cache].LastValue;

                if (cargo.Count > 0) first_empty_cache++;
                else break;
            }
        }
        public void Clear()
        {
            first_empty_cache = 0;
            RealDataCount = 0;
            LastValue = null;
            foreach (var cache in caches)
                cache.Clear();
        }

        public void FillBuffer(double[] buffer, ref int index)
        {
            foreach (var cache in caches)
                cache.FillBuffer(buffer, ref index);
        }
        public void CheckOffsetSequence(ref uint previous_offset)
        {
            foreach (var cache in caches)
                cache.CheckOffsetSequence(ref previous_offset);
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

        public override string ToString()
        {
            var result = new StringBuilder();

            foreach (var cache in caches)
                result.Append(",").Append(cache.ToString());
            if (result.Length > 0) result.Remove(0, 1);

            return result.ToString();
        }
    }
}
