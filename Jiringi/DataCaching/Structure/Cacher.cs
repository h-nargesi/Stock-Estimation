using System;
using System.Collections.Generic;
using System.Text;

namespace Photon.Jiringi.DataCaching
{
    abstract class Cacher<T> : ICache<T> where T : struct, ICacheData
    {
        public Cacher(IOverFlowCheck<T> checker)
        {
            cache = new LinkedList<T>();
            overflow_checker = checker ?? throw new ArgumentNullException(nameof(checker));
        }

        protected readonly IOverFlowCheck<T> overflow_checker;
        protected readonly LinkedList<T> cache;

        public bool IsFull { get; private set; }
        public abstract uint OutputCount { get; }
        public uint RealDataCount => (uint)cache.Count;
        public T? FirstValue => cache.First?.Value;
        public T? LastValue => cache.Last?.Value;

        public virtual void InjectDataToFirst(T leader, LinkedList<T> cargo)
        {
            // inject the input values
            while (cargo.Count > 0)
            {
                cache.AddFirst(cargo.Last.Value);
                cargo.RemoveLast();
            }

            // check existing overflow
            while (cache.Count > 0 &&
                overflow_checker.OverFlow(cache, cache.Last.Value, leader))
            {
                cargo.AddFirst(cache.Last.Value);
                cache.RemoveLast();
            }

            // reset the is-full
            IsFull = cargo.Count > 0;
        }
        public virtual void InjectDataToLast(T leader, LinkedList<T> cargo)
        {
            while (cargo.Count > 0 &&
                !overflow_checker.OverFlow(cache, cargo.First.Value, leader))
            {
                cache.AddLast(cargo.First.Value);
                cargo.RemoveFirst();
            }

            // reset the is-full
            IsFull = cargo.Count > 0;
        }
        public virtual void Clear()
        {
            cache.Clear();
            IsFull = false;
        }

        public abstract void FillBuffer(double[] buffer, ref int index);
        public void CheckOffsetSequence(ref uint previous_offset)
        {
            foreach (var val in cache)
            {
                // offset sequence
                if (val.Offset != previous_offset++)
                    throw new Exception(
                        $"Bad data. the offset sequence is not valid. offset:({previous_offset}), value:{val}.");
            }
        }

    }
}
