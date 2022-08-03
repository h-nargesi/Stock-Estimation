declare @MinSize int = 310

select ROW_NUMBER() OVER (order by InstrumentID, DateTimeEn) as Ranking
	, TradeNo - 1 as TradeNo
	, InstrumentID
	, RowCounts - 1 as RowCounts
	, CAST(CloseIncreasing AS FLOAT) as CloseIncreasing
	, CAST(HighIncreasing AS FLOAT) as HighIncreasing
	, CAST(LowIncreasing AS FLOAT) as LowIncreasing
	, CAST(OpenIncreasing AS FLOAT) as OpenIncreasing
	, DATEDIFF(DAY, '1921-03-21', DateTimeEn) as DurationDays
	, d.Code2 as IndustryCode
from (
	select InstrumentID, RowCounts, DateTimeEn, TradeNo
		, ISNULL(ClosePriceChange, CalcClosePriceChange) / ClosePrice as CloseIncreasing
		, (LowPrice - LowPriceChange) / LowPrice as LowIncreasing
		, (HighPrice - HighPriceChange) / HighPrice as HighIncreasing
		, (OpenPrice - OpenPriceChange) / OpenPrice as OpenIncreasing
	from (
		select Trade.InstrumentID, RowCounts, DateTimeEn
			, ROW_NUMBER() OVER(partition by Trade.InstrumentID order by DateTimeEn) as TradeNo
			, ClosePrice
			, LowPrice
			, HighPrice
			, OpenPrice
			, ClosePriceChange
			, LAG(ClosePrice) OVER(partition by Trade.InstrumentID order by DateTimeEn) as CalcClosePriceChange
			, LAG(LowPrice) OVER(partition by Trade.InstrumentID order by DateTimeEn) as LowPriceChange
			, LAG(HighPrice) OVER(partition by Trade.InstrumentID order by DateTimeEn) as HighPriceChange
			, LAG(OpenPrice) OVER(partition by Trade.InstrumentID order by DateTimeEn) as OpenPriceChange
		from Trade
		join ActiveInstuments(@MinSize) ValidInstruments
		on ValidInstruments.InstrumentID = Trade.InstrumentID
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