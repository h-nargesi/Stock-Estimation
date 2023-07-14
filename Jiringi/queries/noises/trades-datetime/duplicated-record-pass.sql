WITH DupTrade as (
    select 'C' + CAST(ROW_NUMBER() OVER (partition by t.InstrumentID, t.DateTimeEn order by t.ID) as VARCHAR(MAX)) + '|' + Name as Name
        , RC
        , t.*
        , Name as Grouping
    from Trade t
    join (
        select t.InstrumentID, DateTimeEn, COUNT(*) as RC
            , CAST(t.InstrumentID as varchar(MAX)) + '|' + CAST(CAST(DateTimeEn as date) as varchar(max)) as Name
        from Trade t
		join ActiveInstuments(320) v on v.InstrumentID = t.InstrumentID
        group by t.InstrumentID, DateTimeEn
        having count(*) > 1
    ) er on er.InstrumentID = t.InstrumentID and er.DateTimeEn = t.DateTimeEn
    --where er.InstrumentID = 17314 --and er.DateTimeEn = '2011-10-17'

), ValidRecords as (
    select d.*
    from DupTrade d join Trade t on t.PreviousID = d.ID and t.DateTimeEn > d.DateTimeEn
)

select InstrumentID, DateTimeEn, COUNT(*) as RC
from ValidRecords
group by InstrumentID, DateTimeEn
having count(*) > 1
