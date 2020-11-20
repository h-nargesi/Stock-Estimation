use RahavardNovin3;

declare @DateLimit date = dbo.jparse(1369, 1, 1, 0, 0, 0);
declare @LastTradeDate date = dbo.jparse(1399, 1, 1, 0, 0, 0);

update Trade set RecordType = null;

update		Trade
set			RecordType = 'T'
where		DateTimeEn >= @DateLimit
		and	InstrumentID in (
				select		InstrumentID
				from		Trade
				where		DateTimeEn >= @DateLimit
						and InstrumentID in (select ID from Instrument where TypeID = 1 and ExchangeID in (1, 2))
				group by	InstrumentID
				having		max(DateTimeEn) >= @LastTradeDate and count(*) >= 960
				order by	count(*) desc offset 0 rows fetch first 300 rows only
		)
;

merge into Trade
using (
	select InstrumentID, dbo.jparsejl(30000 + dbo.jalali(DateTimeEn)) as Criterion
	from (
		select		InstrumentID, DateTimeEn, row_number() over (partition by InstrumentID order by DateTimeEn) Ranking
		from		Trade
		where		InstrumentID in (select distinct InstrumentID from Trade where RecordType is not null)
	) last_record
	where Ranking = 70
) last_record_info
on (Trade.InstrumentID = last_record_info.InstrumentID and Trade.DateTimeEn < last_record_info.Criterion)
when matched then update set RecordType = null
;

update Trade set RecordType = null where InstrumentID in (
select InstrumentID from Trade where RecordType is not null group by InstrumentID having count(*) < 960)
;

update 	Trade
set 	RecordType = case when rand(checksum(newid())) <= 0.5 then 'V' else 'E' end
where 	RecordType = 'T' and InstrumentID in (
	select		InstrumentID
	from (
		select		InstrumentID,
					sum(Quantity) over (order by newid()) as Stack
		from (
			select 		InstrumentID, count(*) as Quantity
			from 		Trade
			where 		RecordType = 'T'
			group by	InstrumentID
		) inst_q
	) ins_or
	where Stack <= (select count(*) * 0.2 from Trade where RecordType = 'T')
)
;
