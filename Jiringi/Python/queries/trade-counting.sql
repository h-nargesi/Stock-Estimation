declare @Minsize int = 265

select sum(Size) as Size
from (
    select InstrumentID, COUNT(DateTimeEn) as Size
    from Trade
    where InstrumentID in (
        select InstrumentID from Instrument where TypeID = 2
    )
    group by InstrumentID
    having COUNT(DateTimeEn) >= @Minsize
) t
order by Size desc