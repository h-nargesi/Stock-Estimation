declare @Minsize int = 310

select ROW_NUMBER() OVER(order by t.InstrumentID, t.DateTimeEn) as Ranking
    , t.InstrumentID
    , RowCounts as Size
    , CloseIncreasing
	, HighIncreasing
	, LowIncreasing
	, OpenIncreasing
	, BuyerCount
from (
    select InstrumentID, RowCounts, DateTimeEn, Ranking
		 , ISNULL(ClosePriceChange, CalcClosePriceChange) / ClosePrice as CloseIncreasing
		 , (LowPrice - LowPriceChange) / LowPrice as LowIncreasing
		 , (HighPrice - HighPriceChange) / HighPrice as HighIncreasing
		 , (OpenPrice - OpenPriceChange) / OpenPrice as OpenIncreasing
		 , BuyerCount
	from (
		select Trade.InstrumentID, RowCounts, DateTimeEn
			 , ROW_NUMBER() OVER(partition by Trade.InstrumentID order by DateTimeEn) as Ranking
			 , ClosePrice
			 , LowPrice
			 , HighPrice
			 , OpenPrice
			 , ClosePriceChange
			 , LAG(ClosePrice) OVER(partition by Trade.InstrumentID order by DateTimeEn) as CalcClosePriceChange
			 , LAG(LowPrice) OVER(partition by Trade.InstrumentID order by DateTimeEn) as LowPriceChange
			 , LAG(HighPrice) OVER(partition by Trade.InstrumentID order by DateTimeEn) as HighPriceChange
			 , LAG(OpenPrice) OVER(partition by Trade.InstrumentID order by DateTimeEn) as OpenPriceChange
			 , ISNULL(BuyerCount, 0) as BuyerCount
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
							group by InstrumentID
							having COUNT(1) >= @Minsize
						) ins
					) dur
				) dens
				order by Score desc
			) scr
		) ValidInstruments
		on ValidInstruments.InstrumentID = Trade.InstrumentID
	) td
) t
where t.Ranking > 1
order by InstrumentID, DateTimeEn