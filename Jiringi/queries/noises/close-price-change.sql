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

select *
from (
	select t.DateTimeEn, t.ClosePrice, t.ClosePriceChange, p.DateTimeEn as PrvDateTimeEn, p.ClosePrice as PrvClosePrice
		, t.ClosePrice - p.ClosePrice as CalcClosePriceChange
	from Trade t
	join Trade p on t.PreviousID = p.ID
) t
where t.ClosePriceChange is null or t.CalcClosePriceChange != t.ClosePriceChange