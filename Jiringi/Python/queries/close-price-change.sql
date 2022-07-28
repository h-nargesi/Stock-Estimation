select *
from (
	select ClosePrice - LAG(ClosePrice) OVER(partition by InstrumentID order by DateTimeEn) as CalcClosePriceChange
		, ClosePriceChange
		, LAG(ClosePrice) OVER(partition by InstrumentID order by DateTimeEn) as PrvClosePrice
		, ClosePrice
		, LowPrice
		, HighPrice
	from Trade
	where InstrumentID in (
		select InstrumentID from Instrument where TypeID in (1)
	)
) t
where CalcClosePriceChange is null and ClosePriceChange is not null
	or CalcClosePriceChange is not null and ClosePriceChange is null
	or CalcClosePriceChange != ClosePriceChange