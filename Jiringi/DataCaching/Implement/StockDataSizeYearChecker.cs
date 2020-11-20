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
            if (maximum_size < 0)
                throw new ArgumentOutOfRangeException(nameof(maximum_size), "Must be greater than and equal zero.");

            MaxLength = maximum_size;
            this.year_diff = year_diff;
        }

        public readonly int year_diff;
        public int MaxLength { get; }

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

            return Math.Max(out_count, cache.Count - MaxLength);
        }
        public override string ToString()
        {
            return $"max({MaxLength})|date({year_diff})";
        }
    }
}
