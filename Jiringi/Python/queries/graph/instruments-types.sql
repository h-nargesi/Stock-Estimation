select it.*, ct.RowCounts
from InstrumentType it
left join (
    select i.TypeID, COUNT(t.ID) as RowCounts
    from Instrument i
    left join Trade t on t.InstrumentID = i.ID
    group by i.TypeID
) ct
on ct.TypeID = it.ID
order by RowCounts desc