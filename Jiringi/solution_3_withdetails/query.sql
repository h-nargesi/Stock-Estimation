declare @MinSize int = 310,
        @Count int = 10,
        @Factor int = 20;

with UniqueTrade as (
	select InstrumentID, DateTimeEn, ClosePrice
		--, LowPrice, HighPrice, OpenPrice--, ClosePriceChange
	from (
		select InstrumentID, DateTimeEn, ClosePrice
			--, LowPrice, HighPrice, OpenPrice--, ClosePriceChange
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
	-- , @Factor * CAST(HighIncreasing AS FLOAT) as HighIncreasing
	-- , @Factor * CAST(LowIncreasing AS FLOAT) as LowIncreasing
	-- , @Factor * CAST(OpenIncreasing AS FLOAT) as OpenIncreasing
from (
	select InstrumentID, RowCounts, DateTimeEn, TradeNo
		, (ClosePrice - ClosePricePrv) / ClosePrice as CloseIncreasing
		-- , (LowPrice - LowPricePrv) / LowPrice as LowIncreasing
		-- , (HighPrice - HighPricePrv) / HighPrice as HighIncreasing
		-- , (OpenPrice - OpenPricePrv) / OpenPrice as OpenIncreasing
	from (
		select Trade.InstrumentID, RowCounts, DateTimeEn
			, ROW_NUMBER() OVER(partition by Trade.InstrumentID order by DateTimeEn) as TradeNo
			, ClosePrice as ClosePrice
			-- , LowPrice as LowPrice
			-- , HighPrice as HighPrice
			-- , OpenPrice as OpenPrice
			, LAG(ClosePrice) OVER(partition by Trade.InstrumentID order by DateTimeEn) as ClosePricePrv
			-- , LAG(LowPrice) OVER(partition by Trade.InstrumentID order by DateTimeEn) as LowPricePrv
			-- , LAG(HighPrice) OVER(partition by Trade.InstrumentID order by DateTimeEn) as HighPricePrv
			-- , LAG(OpenPrice) OVER(partition by Trade.InstrumentID order by DateTimeEn) as OpenPricePrv
		from UniqueTrade Trade
		join ActiveInstuments(@MinSize, @Count) ValidInstruments on ValidInstruments.InstrumentID = Trade.InstrumentID
	) t
) t
where TradeNo > 1
order by InstrumentID, DateTimeEn