declare @Minsize int = 310

select ROW_NUMBER() OVER(order by t.InstrumentID, t.DateTimeEn) as Ranking
    , t.InstrumentID
    , c.Size
    , CAST(CASE WHEN Increasing > 10 THEN 10 WHEN Increasing < -10 THEN -10 ELSE Increasing END AS FLOAT) as Increasing
    --, CAST(Jalali / 10000 AS FLOAT) as Year
    --, CAST(Jalali / 100 % 100 AS FLOAT) as Month
    --, CAST(Jalali % 100 AS FLOAT) as Day
    --, CAST(Week AS FLOAT) as Week
	--, t.BuyerCount
	--, t.OpenPrice
	--, t.LowPrice
	--, t.HighPrice
	--, t.ClosePrice
	--, t.RecordType
	--, t.TradeCount
	--, t.Volume
from (
    select InstrumentID, DateTimeEn
        , ROW_NUMBER() OVER(partition by InstrumentID order by DateTimeEn) as Ranking
        , isnull(ClosePriceChange / ClosePrice, 0) as Increasing
		--, Trade.BuyerCount
		--, Trade.OpenPrice
		--, Trade.LowPrice
		--, Trade.HighPrice
		--, Trade.ClosePrice
		--, Trade.RecordType
		--, Trade.TradeCount
		--, Trade.Volume
        --, CASE
        --   WHEN Trade.DateTime IS NULL THEN dbo.jalali(DateTimeEn)
        --   ELSE CAST(REPLACE(Trade.DateTime, '/', '') AS INT)
        --   END as Jalali
        --, DATEPART(weekday, DateTimeEn) as Week
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