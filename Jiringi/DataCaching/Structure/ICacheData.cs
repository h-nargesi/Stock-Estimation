﻿using System;
using System.Collections.Generic;
using System.Text;
using Photon.Persian;

namespace Photon.Jiringi.DataCaching
{
    public interface ICacheData
    {
        public double Value { get; }
        public IDateInfo Date { get; }
    }
}
