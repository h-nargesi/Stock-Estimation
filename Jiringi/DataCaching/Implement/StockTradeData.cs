using System;
using System.Collections.Generic;
using System.Text;
using Photon.Persian;

namespace Photon.Jiringi.DataCaching
{
    public struct StockTradeData : ICacheData
    {
        public StockTradeData(uint offset, IDateInfo date, decimal price, 
            double change, char? type, IDateInfo next_date)
        {
            Date = date ?? throw new ArgumentNullException(nameof(date));
            Offset = offset;
            Price = price;
            Change = change;
            RecordType = type;
            NextDate = next_date;
        }

        public uint Offset { get; }
        public IDateInfo Date { get; }
        public decimal Price { get; }
        public double Change { get; }
        public double Value => Change;
        public char? RecordType { get; }
        public IDateInfo NextDate { get; }

        public override string ToString()
        {
            return $"#{Offset}[{Date},{Change}]";
        }
    }
}
