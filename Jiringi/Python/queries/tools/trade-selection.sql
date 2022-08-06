declare @MinSize int = 310,
        @Factor int = 100;

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
	, @Factor * CAST(CloseIncreasing AS FLOAT) as CloseIncreasing
	, @Factor * CAST(HighIncreasing AS FLOAT) as HighIncreasing
	, @Factor * CAST(LowIncreasing AS FLOAT) as LowIncreasing
	, @Factor * CAST(OpenIncreasing AS FLOAT) as OpenIncreasing
	, DATEDIFF(DAY, '1921-03-21', DateTimeEn) as DurationDays
	, d.Code2 as IndustryCode
from (
	select InstrumentID, RowCounts, DateTimeEn, TradeNo
		, (ClosePrice - ClosePricePrv) / ClosePrice as CloseIncreasing
		, (LowPrice - LowPricePrv) / LowPrice as LowIncreasing
		, (HighPrice - HighPricePrv) / HighPrice as HighIncreasing
		, (OpenPrice - OpenPricePrv) / OpenPrice as OpenIncreasing
	from (
		select Trade.InstrumentID, RowCounts, DateTimeEn
			, ROW_NUMBER() OVER(partition by Trade.InstrumentID order by DateTimeEn) as TradeNo
			, ClosePrice / DollarClosePrice as ClosePrice
			, LowPrice / DollarLowPrice as LowPrice
			, HighPrice / DollarHighPrice as HighPrice
			, OpenPrice / DollarOpenPrice as OpenPrice
			, LAG(ClosePrice) OVER(partition by Trade.InstrumentID order by DateTimeEn) as ClosePricePrv
			, LAG(LowPrice) OVER(partition by Trade.InstrumentID order by DateTimeEn) as LowPricePrv
			, LAG(HighPrice) OVER(partition by Trade.InstrumentID order by DateTimeEn) as HighPricePrv
			, LAG(OpenPrice) OVER(partition by Trade.InstrumentID order by DateTimeEn) as OpenPricePrv
		from UniqueTrade Trade
		join ActiveInstuments(@MinSize) ValidInstruments
		on ValidInstruments.InstrumentID = Trade.InstrumentID
		left join (
			select DateTimeEn as StartDateTimeEn
				, DATEADD(DAY, -1, LEAD(DateTimeEn) OVER(order by DateTimeEn)) as EndDateTimeEn
				, ClosePrice as DollarClosePrice
				, LowPrice as DollarLowPrice
				, HighPrice as DollarHighPrice
				, OpenPrice as DollarOpenPrice
				, LAG(ClosePrice) OVER (order by DateTimeEn) as DollarClosePricePrv
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