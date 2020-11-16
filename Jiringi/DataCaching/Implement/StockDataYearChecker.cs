using System;
using System.Collections.Generic;
using System.Text;
using Photon.Persian;

namespace Photon.Jiringi.DataCaching
{
    class StockDataYearChecker : IOverFlowCheck<StockTradeData>
    {
        public StockDataYearChecker(int year_diff)
        {
            this.year_diff = year_diff;
        }

        private readonly int year_diff;

        public bool Check(IReadOnlyCollection<StockTradeData> cache,
            StockTradeData last_value, StockTradeData index)
        {
            return year_diff >= index.Date.Year - last_value.Date.Year;
        }
    }
}
