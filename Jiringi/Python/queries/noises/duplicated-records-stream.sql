WITH RankTrade as (
    select ROW_NUMBER() OVER (partition by t.InstrumentID order by t.DateTimeEn, t.ID) as RowNumber
        , (select count(1) from Trade p where t.InstrumentID = p.InstrumentID and t.PreviousID = p.ID) as HasPrev
        , t.PreviousID, t.ID
        , (select count(1) from Trade n where t.InstrumentID = n.InstrumentID and n.PreviousID = t.ID) as NextCount
        -- , (select count(1) from Trade n where t.InstrumentID = n.InstrumentID and n.PreviousID = t.ID and n.DateTimeEn >= t.DateTimeEn) as TodayNextCount
        , t.InstrumentID, t.DateTimeEn
        , t.OpenPrice, t.HighPrice, t.LowPrice, t.ClosePrice, t.ClosePriceChange
        , t.RealClosePrice, t.RealClosePriceChange, t.BuyerCount, t.TradeCount, t.ShareCount, t.Volume
        , CAST(t.ID as varchar(MAX)) as ID_STR
    from Trade t

), DupTrade as (
    select CASE WHEN t.DateTimeEn = er.DateTimeEn THEN 'C'
            WHEN CHARINDEX(t.ID_STR, PreviousIDs) > 0 THEN 'PR'
            WHEN t.DateTimeEn < er.DateTimeEn THEN 'P'
            WHEN t.DateTimeEn > er.DateTimeEn THEN 'N'
            ELSE NULL END as Type
        , Name
        , RC
        , t.*
        , PreviousIDs
    from RankTrade t
    join (
        select InstrumentID, DateTimeEn, COUNT(*) as RC
            , CAST(InstrumentID as varchar(MAX)) + '|' + CAST(CAST(DateTimeEn as date) as varchar(MAX)) as Name
            , MAX(RowNumber) as MaxRowNumber
            , MIN(RowNumber) as MinRowNumber
            , STRING_AGG(CAST(PreviousID as varchar(MAX)), '-') as PreviousIDs
        from RankTrade
        group by InstrumentID, DateTimeEn
        having count(*) > 1
    ) er on er.InstrumentID = t.InstrumentID and (t.RowNumber between er.MinRowNumber - 3 and er.MaxRowNumber + 3
        or CHARINDEX(t.ID_STR, PreviousIDs) > 0
        )
    where er.InstrumentID = 20038 --and er.DateTimeEn = '2011-10-17'
)

select *
from DupTrade
order by Name, DateTimeEn, ID