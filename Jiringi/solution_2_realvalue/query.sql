declare @Factor int = 100;

with UniqueTrade as (
	select InstrumentID, DateTimeEn, ClosePriceChange
		, ClosePrice, LowPrice, HighPrice, OpenPrice
	from (
		select InstrumentID, DateTimeEn, ClosePriceChange
			, ClosePrice, LowPrice, HighPrice, OpenPrice
			, ROW_NUMBER() OVER (partition by InstrumentID, DateTimeEn order by ID desc) as ranking
		from Trade
	) t
	where ranking = 1
)

select ROW_NUMBER() OVER (order by InstrumentID, DateTimeEn) as Ranking
	, TradeNo - 1 as TradeNo
	, InstrumentID
	, RowCounts - 1 as RowCounts
	, @Factor * CAST(CASE WHEN CloseIncreasing > 0.1 THEN 0.1 WHEN CloseIncreasing < -0.1 THEN -0.1 ELSE CloseIncreasing END AS FLOAT) as CloseIncreasing
from (
	select InstrumentID, RowCounts, DateTimeEn, TradeNo
		, (ClosePrice - ClosePricePrv) / ClosePrice as CloseIncreasing
	from (
		select Trade.InstrumentID, RowCounts, DateTimeEn
			, ROW_NUMBER() OVER(partition by Trade.InstrumentID order by DateTimeEn) as TradeNo
			, ClosePrice / DollarClosePrice as ClosePrice
			, LAG(ClosePrice) OVER(partition by Trade.InstrumentID order by DateTimeEn) / DollarClosePricePrv as ClosePricePrv
		from UniqueTrade Trade
		join ValidInstruments on ValidInstruments.InstrumentID = Trade.InstrumentID
		left join (
			select DateTimeEn as StartDateTimeEn
				, DATEADD(DAY, -1, LEAD(DateTimeEn) OVER(order by DateTimeEn)) as EndDateTimeEn
				, LAG(ClosePrice) OVER (order by DateTimeEn) as DollarClosePricePrv
				, ClosePrice as DollarClosePrice
			from UniqueTrade
			where InstrumentID = 17321
		) Dollar
		on Trade.DateTimeEn between Dollar.StartDateTimeEn and ISNULL(Dollar.EndDateTimeEn, Trade.DateTimeEn)
	) td
) t
join Instrument i on i.ID = t.InstrumentID
join Company c on c.ID = i.CompanyID
join (
	select Code
		, ROW_NUMBER() OVER (order by Code) as Code2
	from Industry
) d on d.Code = c.IndustryCode
where TradeNo > 1
order by InstrumentID, DateTimeEn