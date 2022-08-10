WITH DupTrade as (
    select 'C' + CAST(ROW_NUMBER() OVER (partition by t.InstrumentID, t.DateTimeEn order by t.ID) as VARCHAR(MAX)) + '|' + Name as Name
        , RC
        , t.*
        , Name as Grouping
    from Trade t
    join (
        select InstrumentID, DateTimeEn, COUNT(*) as RC
            , CAST(InstrumentID as varchar(MAX)) + '|' + CAST(CAST(DateTimeEn as date) as varchar(MAX)) as Name
        from Trade
		join ActiveInstuments(@MinSize) v on v.InstrumentID = Trade.InstrumentID
        group by InstrumentID, DateTimeEn
        having count(*) > 1
    ) er on er.InstrumentID = t.InstrumentID and er.DateTimeEn = t.DateTimeEn
    where er.InstrumentID = 20038 --and er.DateTimeEn = '2011-10-17'
)

select d.*
from (
    select 'N' + SUBSTRING(d.Name, 2, LEN(d.Name)) as Name, d.RC, s.*, d.Grouping
    from Trade t
    join DupTrade d on t.PreviousID = d.ID and t.DateTimeEn > d.DateTimeEn
    join Trade s on t.InstrumentID = s.InstrumentID and s.DateTimeEn > d.DateTimeEn
      and s.DateTimeEn between DATEADD(DAY, -1, t.DateTimeEn) and DATEADD(DAY, +1, t.DateTimeEn)
    union
    select d.*
    from DupTrade d
    union
    select 'P' + SUBSTRING(d.Name, 2, LEN(d.Name)) as Name, d.RC, s.*, d.Grouping
    from Trade t
    join DupTrade d on d.PreviousID = t.ID and d.DateTimeEn > t.DateTimeEn
    join Trade s on t.InstrumentID = s.InstrumentID and d.DateTimeEn > s.DateTimeEn
      and s.DateTimeEn between DATEADD(DAY, -1, t.DateTimeEn) and DATEADD(DAY, +1, t.DateTimeEn)
) d
order by d.Grouping, d.InstrumentID, d.DateTimeEn