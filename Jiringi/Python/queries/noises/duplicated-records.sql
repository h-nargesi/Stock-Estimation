WITH DupTrade as (
    select 'C' + CAST(ROW_NUMBER() OVER (partition by t.InstrumentID, t.DateTimeEn order by t.ID) as VARCHAR(MAX)) + '|' + Name as Name
        , RC
        , t.*
        , Name as Grouping
    from Trade t
    join (
        select InstrumentID, DateTimeEn, COUNT(*) as RC
            , CAST(InstrumentID as varchar(MAX)) + '|' + CAST(CAST(DateTimeEn as date) as varchar(max)) as Name
        from Trade
        group by InstrumentID, DateTimeEn
        having count(*) > 1
    ) er on er.InstrumentID = t.InstrumentID and er.DateTimeEn = t.DateTimeEn
    -- where er.InstrumentID = 20038 --and er.DateTimeEn = '2011-10-17'
)

select *
from (
    -- select 'X' + SUBSTRING(d.Name, 2, LEN(d.Name)) as Name, d.RC, t2.*, d.Grouping
    -- from Trade t2
    -- join Trade t1 on t2.PreviousID = t1.ID and t2.DateTimeEn > t1.DateTimeEn
    -- join DupTrade d on t1.PreviousID = d.ID and t1.DateTimeEn > d.DateTimeEn
    -- union
    select 'N' + SUBSTRING(d.Name, 2, LEN(d.Name)) as Name, d.RC, t.*, d.Grouping
    from Trade t
    join DupTrade d on t.PreviousID = d.ID --and t.DateTimeEn > d.DateTimeEn
    union
    select * from DupTrade
    union
    select 'P' + SUBSTRING(d.Name, 2, LEN(d.Name)) as Name, d.RC, t.*, d.Grouping
    from Trade t
    join DupTrade d on d.PreviousID = t.ID --and d.DateTimeEn > t.DateTimeEn
) t
order by t.Grouping, t.InstrumentID, t.DateTimeEn