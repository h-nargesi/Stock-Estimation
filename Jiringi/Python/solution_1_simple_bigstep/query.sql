declare @Minsize int = 310

select ROW_NUMBER() OVER(order by t.InstrumentID, t.DateTimeEn) as Ranking
    , t.InstrumentID
    , RowCounts as Size
    , CAST(CASE WHEN CloseIncreasing > 10 THEN 10 WHEN CloseIncreasing < -10 THEN -10 ELSE CloseIncreasing END AS FLOAT) as CloseIncreasing
from (
    select Trade.InstrumentID, RowCounts, DateTimeEn
        , ROW_NUMBER() OVER(partition by Trade.InstrumentID order by DateTimeEn) as Ranking
        , ISNULL(ClosePriceChange / ClosePrice, 0) as CloseIncreasing
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
						--having COUNT(1) >= @Minsize

					) ins
				) dur
			) dens
			order by Score desc
		) scr
	) ValidInstruments
	on ValidInstruments.InstrumentID = Trade.InstrumentID
) t
where t.Ranking > 1
order by InstrumentID, DateTimeEn