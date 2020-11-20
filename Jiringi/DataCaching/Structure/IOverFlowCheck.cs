using System;
using System.Collections.Generic;
using System.Text;

namespace Photon.Jiringi.DataCaching
{
    interface IOverFlowCheck<T> where T : struct, ICacheData
    {
        int MaxLength { get; }
        int OverFlow(IReverseEnumerable<T> cache, T leader);
    }
}
