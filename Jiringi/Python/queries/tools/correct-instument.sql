drop function if exists ActiveInstuments
GO

create function ActiveInstuments(@MinSize int)
returns table as
return
    select top 80 percent InstrumentID, RowCounts, Duration, Density
        , ROW_NUMBER() over(order by Duration) + ROW_NUMBER() over(order by Density) * 10 as Score
    from (
        select InstrumentID, RowCounts, Duration
            , CASE Duration WHEN 0 THEN 0 ELSE CAST(RowCounts AS FLOAT) / CAST(Duration AS FLOAT) END AS Density
        from (
            select i.ID as InstrumentID, RowCounts
                , ISNULL(DATEDIFF(DAY, StartDateTime, EndDateTime), 0) as Duration
            from Instrument i join Company c on i.CompanyID = c.ID
            join (
                select InstrumentID
                    , MIN(DateTimeEn) as StartDateTime
                    , MAX(DateTimeEn) as EndDateTime
                    , COUNT(1) as RowCounts
                from Trade
                group by InstrumentID
                having COUNT(1) >= @MinSize
            ) t on t.InstrumentID = i.ID
            where i.TypeID = 1 and c.TypeID = 1 and c.StateID = 1
        ) dur
    ) dens
    order by Score desc
GO

select * from ActiveInstuments(320)