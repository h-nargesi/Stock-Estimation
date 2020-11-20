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

        public readonly int year_diff;
        public int MaxLength => -1;

        public int OverFlow(IReverseEnumerable<StockTradeData> cache, StockTradeData leader)
        {
            var criterion_year = leader.Date.Year - year_diff;
            var out_count = 0;

            foreach (var last in cache)
                if (last.NextDate.Year < criterion_year) out_count++;
                else if (last.NextDate.Year > criterion_year) break;
                else if (last.NextDate.Month < leader.Date.Month) out_count++;
                else if (last.NextDate.Month > leader.Date.Month) break;
                else if (last.NextDate.Day < leader.Date.Day) out_count++;
                else break;

            return out_count;
        }

        public override string ToString()
        {
            return $"date({year_diff})";
        }
    }
}
