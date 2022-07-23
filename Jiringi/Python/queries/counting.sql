declare @Minsize int = 310

select InstrumentID, Size
    -- , SUM(Size) over(order by Size desc rows between unbounded preceding and current row) as Total
from (
    select InstrumentID, COUNT(DateTimeEn) as Size
    from Trade
    group by InstrumentID
    having COUNT(DateTimeEn) >= @Minsize
) t
order by Size desc