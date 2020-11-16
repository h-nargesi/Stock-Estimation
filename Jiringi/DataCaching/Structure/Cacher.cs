using System;
using System.Collections.Generic;
using System.Text;

namespace Photon.Jiringi.DataCaching
{
    abstract class Cacher<T> : ICache<T> where T : struct, ICacheData
    {
        public Cacher(IOverFlowCheck<T> checker)
        {
            cache = new Queue<T>();
            overflow_checker = checker ?? throw new ArgumentNullException(nameof(checker));
        }

        protected readonly IOverFlowCheck<T> overflow_checker;
        protected readonly Queue<T> cache;

        public abstract int Count { get; }

        public virtual T? InjectData(T leader, T input)
        {
            T? output;
            if (overflow_checker.Check(cache, cache.Peek(), leader))
                output = cache.Dequeue();
            else output = null;

            cache.Enqueue(input);

            return output;
        }
        public abstract void FillBuffer(double[] buffer, ref int index);
        public virtual void Clear()
        {
            cache.Clear();
        }
    }
}
