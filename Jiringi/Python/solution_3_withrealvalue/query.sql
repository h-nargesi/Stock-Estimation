declare @MinSize int = 310,
        @Factor int = 100;

with UniqueTrade as (
	select InstrumentID, DateTimeEn, ClosePrice
	from (
		select InstrumentID, DateTimeEn, ClosePrice
			, ROW_NUMBER() OVER (partition by InstrumentID, DateTimeEn order by ID desc) as ranking
		from Trade
	) t
	where ranking = 1
)

select ROW_NUMBER() OVER (order by InstrumentID, DateTimeEn) as Ranking
	, TradeNo - 1 as TradeNo
	, InstrumentID
	, RowCounts - 1 as RowCounts
	, @Factor * CAST(CloseIncreasing AS FLOAT) as CloseIncreasing
	, @Factor * CAST(DollarCloseIncreasing AS FLOAT) as DollarCloseIncreasing
	--, d.Code2 as IndustryCode
from (
	select InstrumentID, RowCounts, DateTimeEn, TradeNo
		, (ClosePrice - ClosePricePrv) / ClosePrice as CloseIncreasing
		, (DollarClosePrice - DollarClosePricePrv) / DollarClosePrice as DollarCloseIncreasing
	from (
		select Trade.InstrumentID, RowCounts, DateTimeEn
			, ROW_NUMBER() OVER(partition by Trade.InstrumentID order by DateTimeEn) as TradeNo
			, ClosePrice as ClosePrice
			, LAG(ClosePrice) OVER(partition by Trade.InstrumentID order by DateTimeEn) as ClosePricePrv
		from UniqueTrade Trade
		join ActiveInstuments(@MinSize) ValidInstruments
		on ValidInstruments.InstrumentID = Trade.InstrumentID
	) t
	left join (
		select DateTimeEn as StartDateTimeEn
			, DATEADD(DAY, -1, LEAD(DateTimeEn) OVER(order by DateTimeEn)) as EndDateTimeEn
			, ClosePrice as DollarClosePrice
			, LAG(ClosePrice) OVER (order by DateTimeEn) as DollarClosePricePrv
		from UniqueTrade
		where InstrumentID = 17321
	) d
	on t.DateTimeEn between d.StartDateTimeEn and ISNULL(d.EndDateTimeEn, t.DateTimeEn)
) t
where TradeNo > 1
order by InstrumentID, DateTimeEn