using System;
using System.Collections.Generic;
using System.Text;
using Photon.Persian;

namespace Photon.Jiringi.DataCaching
{
    interface ICacheData
    {
        public double Value { get; }
        public uint Offset { get; }
    }
}
