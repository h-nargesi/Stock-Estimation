declare @Minsize int = 265

select sum(Size) as Size
from (
    select InstrumentID, COUNT(DateTimeEn) as Size
    from Trade
    group by InstrumentID
    having COUNT(DateTimeEn) >= @Minsize
) t
order by Size desc