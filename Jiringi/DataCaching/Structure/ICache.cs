using System;
using System.Collections.Generic;
using System.Text;

namespace Photon.Jiringi.DataCaching
{
    public interface ICache<T> where T : struct, ICacheData
    {
        public bool IsNecessary { get; }
        public bool InjectData(T index, ref T cargo);
        public void FillBuffer(double[] buffer, ref int index);
        public void Clear();
    }
}
