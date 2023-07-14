declare @InstrumentIDs varchar(max) = '13',
		@Zoom int = 5,
        @Start datetime = null,
        @End datetime = null;

WITH BasicData as (
	select InstrumentID, DateTimeEn, RowCounts, Duration
		, CAST(RowCounts AS FLOAT) / CAST(Duration AS FLOAT) as Density
	from (
		select InstrumentID
			, COUNT(1) as RowCounts
			, MIN(t.DateTimeEn) as DateTimeEn
			, @Zoom as Duration
		from (
			select InstrumentID
				, t.DateTimeEn
				, ISNULL(DATEDIFF(DAY, t.DateTimeEn, t.GroupingBaseDate), 0) / @Zoom as GroupingValue
			from (
				select InstrumentID
					, t.DateTimeEn
					, MIN(t.DateTimeEn) OVER(PARTITION BY InstrumentID ORDER BY DateTimeEn) as GroupingBaseDate
				from Trade t
				where t.InstrumentID in (select CAST(VALUE AS INT) AS VALUE from STRING_SPLIT(@InstrumentIDs, ','))
				  and (@Start is null or t.DateTimeEn >= @Start)
				  and (@End is null or t.DateTimeEn <= @End)
			) t
		) t
		group by InstrumentID, GroupingValue
	) dur

), AllInstruments as (
	select i.NameEn, d.*
	from (
		select InstrumentID
			 , MIN(DateTimeEn) as StartDateTime
			 , MAX(DateTimeEn) as EndDateTime
		from BasicData
		group by InstrumentID
	) d
	join Instrument i on i.ID = d.InstrumentID

), InstrumentsFill as (
	select NameEn, InstrumentID, DateTimeEn
	from AllInstruments i cross join (select distinct DateTimeEn from BasicData) t
	where t.DateTimeEn between i.StartDateTime and i.EndDateTime
)

select Instrument, DateTimeEn
	, FIRST_VALUE(RowCounts) OVER (partition by InstrumentID, RowCountsDomains order by DateTimeEn) as RowCounts
	, FIRST_VALUE(Duration) OVER (partition by InstrumentID, DurationDomains order by DateTimeEn) as Duration
	, FIRST_VALUE(Density) OVER (partition by InstrumentID, DensityDomains order by DateTimeEn) as Density
from (
	select i.NameEn + ':' + CAST(i.InstrumentID as varchar(11)) as Instrument
		, i.InstrumentID
		, i.DateTimeEn
		, d.RowCounts, d.Duration, d.Density
		, SUM(CASE WHEN d.RowCounts IS NOT NULL THEN 1 ELSE 0 END) OVER (partition by i.InstrumentID order by i.DateTimeEn) AS RowCountsDomains
		, SUM(CASE WHEN d.Duration IS NOT NULL THEN 1 ELSE 0 END) OVER (partition by i.InstrumentID order by i.DateTimeEn) AS DurationDomains
		, SUM(CASE WHEN d.Density IS NOT NULL THEN 1 ELSE 0 END) OVER (partition by i.InstrumentID order by i.DateTimeEn) AS DensityDomains
	from InstrumentsFill i
	left join BasicData d on i.InstrumentID = d.InstrumentID and i.DateTimeEn = d.DateTimeEn
) d
