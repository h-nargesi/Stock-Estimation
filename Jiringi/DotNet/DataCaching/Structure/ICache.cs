using System;
using System.Collections.Generic;
using System.Text;

namespace Photon.Jiringi.DataCaching
{
    interface ICache<T> where T : struct, ICacheData
    {
        public bool IsFull { get; }
        public uint OutputCount { get; }
        public uint RealDataCount { get; }
        public T? FirstValue { get; }
        public T? LastValue { get; }

        public void InjectDataToFirst(T leader, LinkedList<T> cargo);
        public void InjectDataToLast(T leader, LinkedList<T> cargo);
        public void Clear();

        public void FillBuffer(double[] buffer, ref int index);
        public void CheckOffsetSequence(ref uint previous_offset);

    }
}
