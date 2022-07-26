declare @Minsize int = 310

select ROW_NUMBER() OVER(order by t.InstrumentID, t.DateTimeEn) as Ranking
    , t.InstrumentID
    , c.Size
    , CAST(CASE WHEN CloseIncreasing > 10 THEN 10 WHEN CloseIncreasing < -10 THEN -10 ELSE CloseIncreasing END AS FLOAT) as CloseIncreasing
    --, CAST(CASE WHEN OpenIncreasing > 10 THEN 10 WHEN OpenIncreasing < -10 THEN -10 ELSE OpenIncreasing END AS FLOAT) as OpenIncreasing
    , CAST(CASE WHEN LowIncreasing > 10 THEN 10 WHEN LowIncreasing < -10 THEN -10 ELSE LowIncreasing END AS FLOAT) as LowIncreasing
    , CAST(CASE WHEN HighIncreasing > 10 THEN 10 WHEN HighIncreasing < -10 THEN -10 ELSE HighIncreasing END AS FLOAT) as HighIncreasing
from (
    select InstrumentID, DateTimeEn
        , ROW_NUMBER() OVER(partition by InstrumentID order by DateTimeEn) as Ranking
        , isnull(ClosePriceChange / ClosePrice, 0) as CloseIncreasing
        , (LowPrice - lag(LowPrice) over(partition by InstrumentID order by DateTimeEn)) / LowPrice as LowIncreasing
        , (HighPrice - lag(HighPrice) over(partition by InstrumentID order by DateTimeEn)) / HighPrice as HighIncreasing
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