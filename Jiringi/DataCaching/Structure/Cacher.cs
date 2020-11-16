using System;
using System.Collections.Generic;
using System.Text;

namespace Photon.Jiringi.DataCaching
{
    abstract class Cacher<T> : ICache<T> where T : struct, ICacheData
    {
        public Cacher(IOverFlowCheck<T> checker, bool isnecessary)
        {
            cache = new Queue<T>();
            OverFlowChecker = checker ?? throw new ArgumentNullException(nameof(checker));
            IsNecessary = isnecessary;
        }

        protected readonly Queue<T> cache;

        public IOverFlowCheck<T> OverFlowChecker { get; }
        public bool IsNecessary { get; }

        public virtual bool InjectData(T index, ref T cargo)
        {
            bool overflow;
            if (OverFlowChecker.Check(cache, cache.Peek(), index))
            {
                cargo = cache.Dequeue();
                overflow = true;
            }
            else overflow = false;

            cache.Enqueue(cargo);

            return overflow;
        }
        public abstract void FillBuffer(double[] buffer, ref int index);
        public virtual void Clear()
        {
            cache.Clear();
        }
    }
}
