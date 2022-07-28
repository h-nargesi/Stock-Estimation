declare @Minsize int = 310

select ROW_NUMBER() OVER(order by t.InstrumentID, t.DateTimeEn) as Ranking
    , t.InstrumentID
    , RowCounts as Size
    , CAST(CASE WHEN CloseIncreasing > 10 THEN 10 WHEN CloseIncreasing < -10 THEN -10 ELSE CloseIncreasing END AS FLOAT) as CloseIncreasing
    --, CAST(CASE WHEN OpenIncreasing > 10 THEN 10 WHEN OpenIncreasing < -10 THEN -10 ELSE OpenIncreasing END AS FLOAT) as OpenIncreasing
    , CAST(CASE WHEN LowIncreasing > 10 THEN 10 WHEN LowIncreasing < -10 THEN -10 ELSE LowIncreasing END AS FLOAT) as LowIncreasing
    , CAST(CASE WHEN HighIncreasing > 10 THEN 10 WHEN HighIncreasing < -10 THEN -10 ELSE HighIncreasing END AS FLOAT) as HighIncreasing
from (
    select Trade.InstrumentID, RowCounts, DateTimeEn
        , ROW_NUMBER() OVER(partition by Trade.InstrumentID order by DateTimeEn) as Ranking
        , isnull(ClosePriceChange / ClosePrice, 0) as CloseIncreasing
        , (LowPrice - lag(LowPrice) over(partition by Trade.InstrumentID order by DateTimeEn)) / LowPrice as LowIncreasing
        , (HighPrice - lag(HighPrice) over(partition by Trade.InstrumentID order by DateTimeEn)) / HighPrice as HighIncreasing
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
	join (
		select InstrumentID, RowCounts
			from (
			select top 80 percent dens.InstrumentID, RowCounts
				 , ROW_NUMBER() over(order by Duration) +
				   ROW_NUMBER() over(order by Density) * 10 as Score
			from (
				select InstrumentID, Duration, RowCounts
					 , CASE Duration WHEN 0 THEN 0 ELSE CAST(RowCounts AS FLOAT) / CAST(Duration AS FLOAT) END AS Density
				from (
					select InstrumentID
						 , ISNULL(DATEDIFF(DAY, StartDateTime, EndDateTime), 0) as Duration
						 , RowCounts
					from (
						select InstrumentID
							 , MIN(DateTimeEn) as StartDateTime
							 , MAX(DateTimeEn) as EndDateTime
							 , COUNT(1) as RowCounts
						from Trade
						where InstrumentID in (
							select InstrumentID from Instrument where TypeID in (1)
						)
						group by InstrumentID
						having COUNT(1) >= @Minsize

					) ins
				) dur
			) dens
			order by Score desc
		) scr
	) ValidInstruments
	on ValidInstruments.InstrumentID = Trade.InstrumentID
) t
order by InstrumentID, DateTimeEn