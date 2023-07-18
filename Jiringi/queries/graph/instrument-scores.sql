declare @Type varchar(max) = 'Score',
        @Count int = 20,
        @MinSize int = 320;

merge into ValidInstruments dst
using (select InstrumentID from ActiveInstuments(@MinSize, @Count)) src
on (dst.InstrumentID = src.InstrumentID)
when not matched by target then insert (InstrumentID) values (src.InstrumentID)
when not matched by source then delete;

select i.NameEn + ':' + CAST(i.ID as varchar(11)) as Instrument
    , RowCounts
    , Density
    , Duration
    , Score
    , CASE @Type
        WHEN 'RowCounts' THEN RowCounts
        WHEN 'Density' THEN Density
        WHEN 'Duration' THEN Duration
        ELSE Score
      END as Value
from ActiveInstuments(@MinSize, @Count) t
join Instrument i on i.ID = t.InstrumentID
left join Company c on i.CompanyID = c.ID
order by Value desc offset 0 rows fetch next 30 rows only -- limit for report