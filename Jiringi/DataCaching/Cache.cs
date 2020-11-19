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
            this.caches = caches ?? throw new ArgumentNullException(nameof(caches));
            for (var c = 0; c < caches.Length; c++)
                if (caches[c] == null)
                    throw new ArgumentNullException(nameof(caches), "caches index " + c);
                else OutputCount += caches[c].OutputCount;
        }

        private (uint offset, DateTime date, decimal price, char? type)? last_injected_to_last;
        private readonly ICache<StockTradeData>[] caches;

        public bool IsFull => caches[^1].IsFull;
        public uint OutputCount { get; }
        public uint RealDataCount { get; private set; }
        public StockTradeData? FirstValue => caches[0].FirstValue;

        public bool InjectDataToFirst(uint offset, DateTime date, decimal price, char? type)
        {
            RealDataCount = 0;

            double change;
            if (!caches[0].FirstValue.HasValue) change = 0;
            else change = (double)(price / caches[0].FirstValue.Value.Price) - 1D;
            if (double.IsNaN(change)) throw new Exception("Invalid NaN input data.");

            var leader = new StockTradeData(offset, new Jalali(date).GetDate(), price, change, type);
            var cargo = new LinkedList<StockTradeData>();
            cargo.AddLast(leader);

            foreach (var cache in caches)
                if (cargo.Count > 0)
                {
                    cache.InjectDataToFirst(leader, cargo);
                    RealDataCount += cache.RealDataCount;
                }
                else return false;

            return cargo.Count > 0;
        }
        public bool InjectDataToLast(uint offset, DateTime date, decimal price, char? type)
        {
            if (last_injected_to_last == null)
            {
                last_injected_to_last = (offset, date, price, type);
                return false;
            }

            RealDataCount = 0;

            // replace new injected values
            (uint offset, DateTime date, decimal price, char? type) new_last_value = (offset, date, price, type);
            (offset, date, price, type) = last_injected_to_last.Value;
            last_injected_to_last = new_last_value;

            var change = (double)(price / new_last_value.price) - 1D;
            if (double.IsNaN(change)) throw new Exception("Invalid NaN input data.");

            var cargo = new LinkedList<StockTradeData>();
            cargo.AddLast(new StockTradeData(offset, new Jalali(date).GetDate(), price, change, type));
            var leader = caches[0].FirstValue ?? cargo.First.Value;

            foreach (var cache in caches)
                if (cargo.Count > 0)
                {
                    cache.InjectDataToLast(leader, cargo);
                    RealDataCount += cache.RealDataCount;
                }
                else return false;

            return cargo.Count > 0;
        }
        public void Clear()
        {
            RealDataCount = 0;
            foreach (var cache in caches)
                cache.Clear();
            last_injected_to_last = null;
        }

        public void FillBuffer(double[] buffer, ref int index)
        {
            foreach (var cache in caches)
                cache.FillBuffer(buffer, ref index);
        }
        public void CheckOffsetSequence(uint previous_offset)
        {
            foreach (var cache in caches)
                cache.CheckOffsetSequence(ref previous_offset);
        }

        public static Cache Build(int this_year_records_count, int years_count, int one_year_records_cout)
        {
            var builder = new CacheBuilder<StockTradeData>()
                .AddCacherArray(new StockDataSizeYearChecker(this_year_records_count, 1))
                .AddCacherGap(new StockDataYearChecker(1))
                .AddCacherArray(new StockDataSizeYearChecker(one_year_records_cout, 2));

            for (int i = 2; i < years_count; i++)
                builder
                    .AddCacherGap(new StockDataYearChecker(i))
                    .AddCacherAvragtorCollection(one_year_records_cout / i, new StockDataSizeYearChecker(i, i + 1));

            builder
                .AddCacherGap(new StockDataYearChecker(years_count))
                .AddCacherAvragtorCollection(
                    one_year_records_cout / years_count, new StockDataSizeChecker(years_count));

            return new Cache(builder.CacheArray());
        }
    }
}
