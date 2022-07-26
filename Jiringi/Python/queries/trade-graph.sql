declare @InstrumentID int = 13,
        @Offset int = 0,
        @Count int = 1000

select t.DateTimeEn
     , t.BuyerCount
     , t.OpenPrice
     , t.LowPrice
     , t.HighPrice
     , t.ClosePrice
     , t.RecordType
     , t.TradeCount
     , isnull(t.Volume, 0) as Volume
from Trade t where t.InstrumentID = @InstrumentID
order by t.DateTimeEn offset @Offset rows fetch next @Count rows only