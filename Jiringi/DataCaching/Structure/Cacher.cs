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
            reverse_enumerator = new ReverseLinkedListEnumerable<T>(cache);
            overflow_checker = checker ?? throw new ArgumentNullException(nameof(checker));
        }

        private readonly ReverseLinkedListEnumerable<T> reverse_enumerator;
        protected readonly IOverFlowCheck<T> overflow_checker;
        protected readonly LinkedList<T> cache;

        public bool IsFull { get; private set; }
        public abstract uint OutputCount { get; }
        public uint RealDataCount => (uint)cache.Count;
        public T? FirstValue => cache.First?.Value;
        public T? LastValue => cache.Last?.Value;

        public virtual void InjectDataToFirst(T leader, LinkedList<T> cargo)
        {
            if (cache.Count < 1)
            {
                if (cargo.Count < 1) return;

                // we assume the cargo pushed in cache
                // check cargo overflow
                var out_count = overflow_checker.OverFlow(new ReverseLinkedListEnumerable<T>(cargo), leader);
                // set the is-full
                IsFull = out_count > 0;

                out_count = cargo.Count - out_count;
                while (out_count-- > 0)
                {
                    // the first value from cargo stay in cache 
                    // and remind will out of cache
                    cache.AddLast(cargo.First.Value);
                    cargo.RemoveFirst();
                }
            }
            else
            {
                // inject the input values
                while (cargo.Count > 0)
                {
                    cache.AddFirst(cargo.Last.Value);
                    cargo.RemoveLast();
                }

                // check existing cache overflow
                var out_count = overflow_checker.OverFlow(reverse_enumerator, leader);
                // set the is-full
                IsFull = out_count > 0;

                // remove overflow
                while (cache.Count > 0 && out_count-- > 0)
                {
                    cargo.AddFirst(cache.Last.Value);
                    cache.RemoveLast();
                }
            }
        }
        public virtual void InjectDataToLast(T leader, LinkedList<T> cargo)
        {
            if (cargo.Count < 1) return;

            // inject the input values
            while (cargo.Count > 0)
            {
                cache.AddLast(cargo.First.Value);
                cargo.RemoveFirst();
            }

            // check existing cache overflow
            var out_count = overflow_checker.OverFlow(reverse_enumerator, leader);
            // set the is-full
            IsFull = out_count > 0;

            // remove overflow
            while (cache.Count > 0 && out_count-- > 0)
            {
                cargo.AddFirst(cache.Last.Value);
                cache.RemoveLast();
            }
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

        public override string ToString()
        {
            return $"({RealDataCount})to-{overflow_checker}";
        }
    }
}
