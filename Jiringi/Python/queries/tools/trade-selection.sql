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
	, @Factor * CAST(DollarCloseIncreasing AS FLOAT) as DollarCloseIncreasing
	, @Factor * CAST(DollarHighIncreasing AS FLOAT) as DollarHighIncreasing
	, @Factor * CAST(DollarLowIncreasing AS FLOAT) as DollarLowIncreasing
	, @Factor * CAST(DollarOpenIncreasing AS FLOAT) as DollarOpenIncreasing
	, DATEDIFF(DAY, '1921-03-21', DateTimeEn) as DollarDurationDays
	--, d.Code2 as IndustryCode
from (
	select InstrumentID, RowCounts, DateTimeEn, TradeNo
		, (ClosePrice - ClosePricePrv) / ClosePrice as CloseIncreasing
		, (LowPrice - LowPricePrv) / LowPrice as LowIncreasing
		, (HighPrice - HighPricePrv) / HighPrice as HighIncreasing
		, (OpenPrice - OpenPricePrv) / OpenPrice as OpenIncreasing
		, (DollarClosePrice - DollarClosePricePrv) / DollarClosePrice as DollarCloseIncreasing
		, (DollarLowPrice - DollarLowPricePrv) / DollarLowPrice as DollarLowIncreasing
		, (DollarHighPrice - DollarHighPricePrv) / DollarHighPrice as DollarHighIncreasing
		, (DollarOpenPrice - DollarOpenPricePrv) / DollarOpenPrice as DollarOpenIncreasing
	from (
		select Trade.InstrumentID, RowCounts, DateTimeEn
			, ROW_NUMBER() OVER(partition by Trade.InstrumentID order by DateTimeEn) as TradeNo
			, ClosePrice as ClosePrice
			, LowPrice as LowPrice
			, HighPrice as HighPrice
			, OpenPrice as OpenPrice
			, LAG(ClosePrice) OVER(partition by Trade.InstrumentID order by DateTimeEn) as ClosePricePrv
			, LAG(LowPrice) OVER(partition by Trade.InstrumentID order by DateTimeEn) as LowPricePrv
			, LAG(HighPrice) OVER(partition by Trade.InstrumentID order by DateTimeEn) as HighPricePrv
			, LAG(OpenPrice) OVER(partition by Trade.InstrumentID order by DateTimeEn) as OpenPricePrv
		from UniqueTrade Trade
		join ActiveInstuments(@MinSize) ValidInstruments
		on ValidInstruments.InstrumentID = Trade.InstrumentID
	) t
	left join (
		select DateTimeEn as StartDateTimeEn
			, DATEADD(DAY, -1, LEAD(DateTimeEn) OVER(order by DateTimeEn)) as EndDateTimeEn
			, ClosePrice as DollarClosePrice
			, LowPrice as DollarLowPrice
			, HighPrice as DollarHighPrice
			, OpenPrice as DollarOpenPrice
			, LAG(ClosePrice) OVER (order by DateTimeEn) as DollarClosePricePrv
			, LAG(LowPrice) OVER (order by DateTimeEn) as DollarLowPricePrv
			, LAG(HighPrice) OVER (order by DateTimeEn) as DollarHighPricePrv
			, LAG(OpenPrice) OVER (order by DateTimeEn) as DollarOpenPricePrv
		from UniqueTrade
		where InstrumentID = 17321
	) d
	on t.DateTimeEn between d.StartDateTimeEn and ISNULL(d.EndDateTimeEn, t.DateTimeEn)
) t
--join Instrument i on i.ID = t.InstrumentID
--join Company c on c.ID = i.CompanyID
--join (
--	select Code
--		, ROW_NUMBER() OVER (order by Code) as Code2
--	from Industry
--) d on d.Code = c.IndustryCode
where TradeNo > 1
order by InstrumentID, DateTimeEn