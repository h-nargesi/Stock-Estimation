if OBJECT_ID(N'ValidInstruments', N'U') is null begin
	create table ValidInstruments
	(
		InstrumentID	int not null primary key
	)
end
go

drop function if exists ActiveInstuments
go

create function ActiveInstuments(@MinSize int, @Count int)
returns table as
return
    select InstrumentID, RowCounts, Duration, Density
        , ROW_NUMBER() over(order by Duration) + ROW_NUMBER() over(order by Density) * 10 as Score
    from (
        select InstrumentID, RowCounts, Duration
            , CASE Duration WHEN 0 THEN 0 ELSE CAST(RowCounts AS FLOAT) / CAST(Duration AS FLOAT) END AS Density
        from (
            select i.ID as InstrumentID, RowCounts
                , ISNULL(DATEDIFF(DAY, StartDateTime, EndDateTime), 0) as Duration
            from Instrument i
            join (
                select InstrumentID
                    , MIN(DateTimeEn) as StartDateTime
                    , MAX(DateTimeEn) as EndDateTime
                    , COUNT(1) as RowCounts
                from Trade
				where InstrumentID not in (
					select distinct t.InstrumentID
					from Trade t
					group by t.InstrumentID, DateTimeEn
					having count(*) > 1
				)
                group by InstrumentID
                having COUNT(1) >= @MinSize
            ) t on t.InstrumentID = i.ID
			left join Company c on i.CompanyID = c.ID
			where i.TypeID = 1 /*Share*/ and c.TypeID = 1 and c.StateID = 1
			   or i.TypeID = 19 /*Currency*/
			--where i.TypeID in (1 /*Share*/, 19 /*Currency*/)
			--  and (c.ID is null or c.TypeID = 1 and c.StateID = 1)
        ) dur
    ) dens
    order by Score desc offset 0 rows fetch next @Count rows only
GO

merge into ValidInstruments dst
using (select InstrumentID from ActiveInstuments(320, 10)) src
on (dst.InstrumentID = src.InstrumentID)
when not matched by target then insert (InstrumentID) values (src.InstrumentID)
when not matched by source then delete;

select * from ActiveInstuments(320, 10) order by Score desc