using Photon.Persian;
using System;
using System.Collections.Generic;
using System.Text;

namespace Photon.Jiringi.DataCaching
{
    public static class StockCacheBuilder
    {
        public static ICache<StockTradeData> Build()
        {
            return new CacheBuilder<StockTradeData>()
                .AddCacherArray(new StockDataSizeChecker(183))
                .AddCacherGap(new StockDataYearChecker(1))
                .AddCacherArray(new StockDataSizeChecker(60))
                // .IsNotNecessaryFromHere() // not necessay from here //
                .AddCacherGap(new StockDataYearChecker(2))
                .AddCacherAvragtorCollection(30, new StockDataSizeChecker(2))
                .AddCacherGap(new StockDataYearChecker(3))
                .AddCacherAvragtorCollection(20, new StockDataSizeChecker(3))
                .CacheCollection();
        }
    }
}
