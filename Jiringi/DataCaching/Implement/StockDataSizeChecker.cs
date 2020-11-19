using System;
using System.Collections.Generic;
using System.Text;

namespace Photon.Jiringi.DataCaching
{
    class StockDataSizeChecker : IOverFlowCheck<StockTradeData>
    {
        public StockDataSizeChecker(int maximum_size)
        {
            if (maximum_size <= 1)
                throw new ArgumentOutOfRangeException(nameof(maximum_size), "Must be greater than one.");

            MaxLength = maximum_size;
        }

        public int MaxLength { get; }

        public bool OverFlow(IReadOnlyCollection<StockTradeData> cache,
            StockTradeData last_value, StockTradeData leader)
        {
            return cache.Count >= MaxLength;
        }
    }
}
