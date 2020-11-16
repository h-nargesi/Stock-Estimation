using Photon.Persian;
using System;
using System.Collections.Generic;
using System.Text;

namespace Photon.Jiringi.DataCaching
{
    public class Cache
    {
        private Cache(ICache<StockTradeData>[] caches)
        {
            this.caches = caches;
        }

        private readonly ICache<StockTradeData>[] caches;

        public int Count { get; private set; }

        public bool InjectData(DateTime date, double value)
        {
            Count = 0;
            StockTradeData inserted_value = new StockTradeData(new Jalali(date).GetDate(), value);
            StockTradeData? cargo = inserted_value;
            foreach (var cache in caches)
                if (cargo.HasValue)
                {
                    cargo = cache.InjectData(inserted_value, cargo.Value);
                    Count += cache.Count;
                }
                else return false;

            return true;
        }
        public void FillBuffer(double[] buffer, ref int index)
        {
            foreach (var cache in caches)
                cache.FillBuffer(buffer, ref index);
        }
        public void Clear()
        {
            Count = 0;
            foreach (var cache in caches)
                cache.Clear();
        }

        public static Cache Build(int basical, int year_count)
        {
            var builder = new CacheBuilder<StockTradeData>()
                .AddCacherArray(new StockDataSizeChecker(basical))
                .AddCacherGap(new StockDataYearChecker(1))
                .AddCacherArray(new StockDataSizeChecker(60));

            for (int i = 2; i <= year_count; i++)
                builder
                    .AddCacherGap(new StockDataYearChecker(i))
                    .AddCacherAvragtorCollection(60 / i, new StockDataSizeChecker(i));

            return new Cache(builder.CacheArray());
        }
    }
}
