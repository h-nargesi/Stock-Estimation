declare @MinSize int = 310

select ROW_NUMBER() OVER (order by InstrumentID, DateTimeEn) as Ranking
	, TradeNo - 1 as TradeNo
	, InstrumentID
	, RowCounts - 1 as RowCounts
    , 100 * CAST(CASE WHEN CloseIncreasing > 0.1 THEN 0.1 WHEN CloseIncreasing < -0.1 THEN -0.1 ELSE CloseIncreasing END AS FLOAT) as CloseIncreasing
from (
    select Trade.InstrumentID, RowCounts, DateTimeEn
        , ROW_NUMBER() OVER(partition by Trade.InstrumentID order by DateTimeEn) as TradeNo
        , ISNULL(ClosePriceChange / ClosePrice, 0) as CloseIncreasing
    from Trade
	join ActiveInstuments(@MinSize) ValidInstruments
	on ValidInstruments.InstrumentID = Trade.InstrumentID
) t
where t.TradeNo > 1
order by InstrumentID, DateTimeEn