﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Photon.Jiringi.DataCaching
{
    class StockDataSizeChecker : IOverFlowCheck<StockTradeData>
    {
        public StockDataSizeChecker(uint maximum_size)
        {
            if (maximum_size <= 1)
                throw new ArgumentOutOfRangeException(nameof(maximum_size), "Must be greater than one.");

            this.maximum_size = maximum_size;
        }

        private readonly uint maximum_size;

        public bool Check(IReadOnlyCollection<StockTradeData> cache, 
            StockTradeData last_value, StockTradeData index)
        {
            return cache.Count >= maximum_size;
        }
    }
}
