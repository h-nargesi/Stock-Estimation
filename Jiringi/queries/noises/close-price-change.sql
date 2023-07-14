select * --COUNT(1)
from (
	select ClosePrice - LAG(ClosePrice) OVER(partition by t.InstrumentID order by DateTimeEn) as CalcClosePriceChange
		, ClosePriceChange
		, LAG(ClosePrice) OVER(partition by t.InstrumentID order by DateTimeEn) as PrvClosePrice
		, ClosePrice
		, LowPrice
		, HighPrice
	from Trade t
	join ActiveInstuments(1) v
	on v.InstrumentID = t.InstrumentID
) t
where ClosePriceChange is null or CalcClosePriceChange != ClosePriceChange