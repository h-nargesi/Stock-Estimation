declare @MinSize int = 310

select ROW_NUMBER() OVER (order by InstrumentID, DateTimeEn) as Ranking
	, TradeNo - 1 as TradeNo
	, InstrumentID
	, RowCounts - 1 as RowCounts
    , CAST(CloseIncreasing AS FLOAT) as CloseIncreasing
from (
    select Trade.InstrumentID, RowCounts, DateTimeEn
        , ROW_NUMBER() OVER(partition by Trade.InstrumentID order by DateTimeEn) as TradeNo
        , 100 * ISNULL(ClosePriceChange / ClosePrice, 0) as CloseIncreasing
    from Trade
	join ActiveInstuments(@MinSize) ValidInstruments
	on ValidInstruments.InstrumentID = Trade.InstrumentID
) t
where t.TradeNo > 1
order by InstrumentID, DateTimeEn