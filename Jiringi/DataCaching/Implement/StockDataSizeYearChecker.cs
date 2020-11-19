using System;
using System.Collections.Generic;
using System.Text;
using Photon.Persian;

namespace Photon.Jiringi.DataCaching
{
    class StockDataSizeYearChecker : IOverFlowCheck<StockTradeData>
    {
        public StockDataSizeYearChecker(int maximum_size, int year_diff)
        {
            this.MaxLength = maximum_size;
            this.year_diff = year_diff;
        }

        public readonly int year_diff;
        public int MaxLength { get; }

        public bool OverFlow(IReadOnlyCollection<StockTradeData> cache,
            StockTradeData last_value, StockTradeData leader)
        {
            if (cache.Count >= MaxLength) return true;

            var criterion_yesr = leader.Date.Year - year_diff;
            if (last_value.Date.Year < criterion_yesr) return true;
            else if (last_value.Date.Year > criterion_yesr) return false;
            else if (last_value.Date.Month < leader.Date.Month) return true;
            else if (last_value.Date.Month > leader.Date.Month) return false;
            else if (last_value.Date.Day <= leader.Date.Day) return true;
            else return false;
        }
    }
}
