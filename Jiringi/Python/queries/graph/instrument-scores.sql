declare @Type varchar(max) = 'Score',
        @Count int = 20,
        @MinSize int = 320;

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
from ActiveInstuments(@MinSize) t
join Instrument i on i.ID = t.InstrumentID
order by 
    CASE @Type
        WHEN 'RowCounts' THEN RowCounts
        WHEN 'Density' THEN Density
        WHEN 'Duration' THEN Duration
        ELSE Score
    END desc
offset 0 rows fetch next @Count rows only