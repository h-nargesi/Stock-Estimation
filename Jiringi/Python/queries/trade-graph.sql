declare @InstrumentID int = 13,
        @Offset int = 0,
        @Count int = 1000

select t.ClosePrice, t.DateTimeEn
from Trade t where t.InstrumentID = @InstrumentID
order by t.DateTimeEn offset @Offset rows fetch next @Count rows only