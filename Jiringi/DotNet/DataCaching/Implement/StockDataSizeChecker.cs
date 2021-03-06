﻿using System;
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

        public int OverFlow(IReverseEnumerable<StockTradeData> cache, StockTradeData leader)
        {
            return Math.Max(0, cache.Count - MaxLength);
        }

        public override string ToString()
        {
            return $"max({MaxLength})";
        }
    }
}
