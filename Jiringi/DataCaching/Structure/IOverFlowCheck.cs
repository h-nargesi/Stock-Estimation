using System;
using System.Collections.Generic;
using System.Text;

namespace Photon.Jiringi.DataCaching
{
    interface IOverFlowCheck<T> where T : struct, ICacheData
    {
        int MaxLength { get; }
        bool OverFlow(IReadOnlyCollection<T> cache, T last_value, T leader);
    }
}
