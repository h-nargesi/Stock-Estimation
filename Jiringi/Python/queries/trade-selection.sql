declare @Minsize int = 310

select ROW_NUMBER() OVER(order by t.InstrumentID, t.DateTimeEn) as Ranking
    , t.InstrumentID
    , c.Size
    , CAST(greatest(least(Increasing / 0.05, 10), -10) AS FLOAT) as Increasing
    --, CAST(Jalali / 10000 AS FLOAT) as Year
    --, CAST(Jalali / 100 % 100 AS FLOAT) as Month
    --, CAST(Jalali % 100 AS FLOAT) as Day
    --, CAST(Week AS FLOAT) as Week
from (
    select InstrumentID
        , ROW_NUMBER() OVER(partition by InstrumentID order by DateTimeEn) as Ranking
        , (ClosePrice - lag(ClosePrice) over(partition by InstrumentID order by DateTimeEn)) / ClosePrice as Increasing
        --, CASE
        --   WHEN Trade.DateTime IS NULL THEN dbo.jalali(DateTimeEn)
        --   ELSE CAST(REPLACE(Trade.DateTime, '/', '') AS INT)
        --   END as Jalali
        --, DATEPART(weekday, DateTimeEn) as Week
        , DateTimeEn
    from Trade
) t
join (
    select InstrumentID, COUNT(DateTimeEn) as Size
    from Trade
    group by InstrumentID
    having COUNT(DateTimeEn) >= @Minsize
) c on t.InstrumentID = c.InstrumentID
where t.Ranking > 1
order by InstrumentID, DateTimeEn