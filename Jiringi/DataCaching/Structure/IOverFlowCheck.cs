using System;
using System.Collections.Generic;
using System.Text;

namespace Photon.Jiringi.DataCaching
{
    interface IOverFlowCheck<T> where T : struct, ICacheData
    {
        bool Check(IReadOnlyCollection<T> cache, T last_value, T index);
    }
}
