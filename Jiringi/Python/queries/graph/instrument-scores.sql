declare @Type varchar(max) = 'Score',
        @Count int = 20

select i.NameEn as Instrument
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
from (
    select dens.*
        , ROW_NUMBER() over(order by Duration) +
            ROW_NUMBER() over(order by Density) * 10 as Score
    from (
        select InstrumentID, Duration, RowCounts
                , CASE Duration WHEN 0 THEN 0 ELSE CAST(RowCounts AS FLOAT) / CAST(Duration AS FLOAT) END AS Density
        from (
            select InstrumentID
                    , ISNULL(DATEDIFF(DAY, StartDateTime, EndDateTime), 0) as Duration
                    , RowCounts
            from (
                select InstrumentID
                        , MIN(DateTimeEn) as StartDateTime
                        , MAX(DateTimeEn) as EndDateTime
                        , COUNT(1) as RowCounts
                from Trade
                where InstrumentID in (
                    select InstrumentID from Instrument where TypeID in (1)
                )
                group by InstrumentID
            ) ins
        ) dur
    ) dens
) t
join Instrument i on i.ID = t.InstrumentID
order by 
    CASE @Type
        WHEN 'RowCounts' THEN RowCounts
        WHEN 'Density' THEN Density
        WHEN 'Duration' THEN Duration
        ELSE Score
    END desc
offset 0 rows fetch next @Count rows only