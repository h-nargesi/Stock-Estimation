select RowCounts, t.*, p.*
from Trade t
join (
    select InstrumentID, DateTimeEn, count(*) as RowCounts
    from Trade
    group by InstrumentID, DateTimeEn
    having count(*) > 1
) er on er.InstrumentID = t.InstrumentID and er.DateTimeEn = t.DateTimeEn
left join Trade p on t.PreviousID = p.ID
order by t.InstrumentID, t.DateTimeEn