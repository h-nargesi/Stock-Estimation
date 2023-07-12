select *
from TradeType
left join (
    select TypeID, COUNT(1) as Counting
    from Trade
    group by TypeID
) Trade
on TradeType.ID = Trade.TypeID
