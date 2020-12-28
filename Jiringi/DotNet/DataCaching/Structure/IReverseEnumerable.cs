using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace Photon.Jiringi.DataCaching
{
    interface IReverseEnumerable<T> : IEnumerable<T>, IEnumerable, IReadOnlyCollection<T> { }
}
