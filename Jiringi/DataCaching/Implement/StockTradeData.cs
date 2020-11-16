using System;
using System.Collections.Generic;
using System.Text;
using Photon.Persian;

namespace Photon.Jiringi.DataCaching
{
    public struct StockTradeData : ICacheData
    {
        public StockTradeData(IDateInfo date, double value)
        {
            Date = date ?? throw new ArgumentNullException(nameof(date));
            Value = value;
        }

        public double Value { get; }
        public IDateInfo Date { get; }
    }
}
