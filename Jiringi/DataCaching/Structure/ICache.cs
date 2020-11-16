using System;
using System.Collections.Generic;
using System.Text;

namespace Photon.Jiringi.DataCaching
{
    public interface ICache<T> where T : struct, ICacheData
    {
        public int Count { get; }
        public T? InjectData(T leader, T input);
        public void FillBuffer(double[] buffer, ref int index);
        public void Clear();
    }
}
