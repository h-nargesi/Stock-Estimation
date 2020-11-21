using Photon.Persian;
using System;
using System.Collections.Generic;
using System.Text;

namespace Photon.Jiringi.DataCaching
{
    public class Cache
    {
        private Cache(CacheCollection<StockTradeData> caches)
        {
            this.caches = caches ?? throw new ArgumentNullException(nameof(caches));
        }

        private StockTradeData? last_injected_to_last;
        private readonly CacheCollection<StockTradeData> caches;

        public bool IsFull => caches.IsFull;
        public uint OutputCount => caches.OutputCount;
        public uint RealDataCount => caches.RealDataCount;
        public StockTradeData? FirstValue => caches.FirstValue;

        public bool InjectDataToFirst(uint offset, DateTime date, decimal price, char? type)
        {
            double change;
            if (!caches.FirstValue.HasValue) change = 0;
            else change = 100D * ((double)(price / caches.FirstValue.Value.Price) - 1D);
            if (double.IsNaN(change)) throw new Exception("Invalid NaN input data.");

            var leader = new StockTradeData(
                offset, new Jalali(date).GetDate(), price, change, type, caches.FirstValue.Value.Date);
            var cargo = new LinkedList<StockTradeData>();
            cargo.AddLast(leader);

            caches.InjectDataToFirst(leader, cargo);
            last_injected_to_last = null;

            return cargo.Count > 0;
        }
        public bool InjectDataToLast(uint offset, DateTime date, decimal price, char? type)
        {
            if (caches.IsFull) return true;

            // replace new injected values
            var new_value = last_injected_to_last;
            last_injected_to_last = new StockTradeData(
                offset, new Jalali(date).GetDate(), price, -1, type, null);

            if (new_value == null) return false;

            // calculate price-change
            var change = 100D * ((double)(new_value.Value.Price / last_injected_to_last.Value.Price) - 1D);
            if (double.IsNaN(change)) throw new Exception("Invalid NaN input data.");

            var cargo = new LinkedList<StockTradeData>();
            cargo.AddLast(new StockTradeData(
                new_value.Value.Offset, new_value.Value.Date, new_value.Value.Price,
                change, new_value.Value.RecordType, last_injected_to_last.Value.Date));
            var leader = caches.FirstValue ?? cargo.First.Value;

            caches.InjectDataToLast(leader, cargo);

            if (cargo.Count > 0)
            {
                last_injected_to_last = null;
                return true;
            }
            else return false;
        }
        public void Clear()
        {
            last_injected_to_last = null;
            caches.Clear();
        }

        public void FillBuffer(double[] buffer, ref int index)
        {
            caches.FillBuffer(buffer, ref index);
        }
        public void CheckOffsetSequence(uint previous_offset)
        {
            caches.CheckOffsetSequence(ref previous_offset);
        }

        public static Cache Build(int result_count, int this_year_signal_count, int years_count, int one_year_records_cout)
        {
            var builder = new CacheBuilder<StockTradeData>()
                .AddCacherArray(new StockDataSizeChecker(result_count))
                .AddCacherArray(new StockDataSizeYearChecker(this_year_signal_count, 1))
                .AddCacherGap(new StockDataYearChecker(1))
                .AddCacherArray(new StockDataSizeYearChecker(one_year_records_cout, 2));

            for (int i = 2; i <= years_count; i++)
                builder
                    .AddCacherGap(new StockDataYearChecker(i))
                    .AddCacherAvragtorCollection(one_year_records_cout / i, new StockDataSizeYearChecker(i, i + 1));

            return new Cache((CacheCollection<StockTradeData>)builder.CacheCollection());
        }

        public override string ToString()
        {
            return caches.ToString();
        }
    }
}
