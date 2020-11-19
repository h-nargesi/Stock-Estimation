﻿using System;
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

        public bool OverFlow(IReadOnlyCollection<StockTradeData> cache,
            StockTradeData last_value, StockTradeData leader)
        {
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
