declare @Minsize int = 310

select SUM(RowCounts) AS Size
from (
    select top 80 percent dens.InstrumentID, RowCounts
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
                having COUNT(1) >= @Minsize

            ) ins
        ) dur
    ) dens
    order by Score desc
) scr