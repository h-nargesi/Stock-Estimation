declare @InstrumentIDs varchar(max) = '13',
		@Zoom int = 5,
        @Offset int = 0,
        @Count int = 1000;

WITH BasicData as (
	select InstrumentID, DateTimeEn, RowCounts, Duration
		, CASE Duration WHEN 0 THEN 0 ELSE CAST(RowCounts AS FLOAT) / CAST(Duration AS FLOAT) END AS Density
		, ROW_NUMBER() over(partition by InstrumentID order by DateTimeEn) as GroupingValue
	from (
		select InstrumentID
			, COUNT(1) AS RowCounts
			, MIN(t.DateTimeEn) as DateTimeEn
			, ISNULL(DATEDIFF(DAY, MIN(t.DateTimeEn), MAX(t.DateTimeEn)), 0) as Duration
		from (
			select InstrumentID
				, t.DateTimeEn
				, ROW_NUMBER() over(partition by InstrumentID order by DateTimeEn) / @Zoom as GroupingValue
			from Trade t
			where t.InstrumentID in (select CAST(VALUE AS INT) AS VALUE from STRING_SPLIT(@InstrumentIDs, ','))
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
	select i.NameEn as Instrument
		, i.InstrumentID
		, i.DateTimeEn
		, d.RowCounts, d.Duration, d.Density
		, SUM(CASE WHEN d.RowCounts IS NOT NULL THEN 1 ELSE 0 END) OVER (partition by i.InstrumentID order by i.DateTimeEn) AS RowCountsDomains
		, SUM(CASE WHEN d.Duration IS NOT NULL THEN 1 ELSE 0 END) OVER (partition by i.InstrumentID order by i.DateTimeEn) AS DurationDomains
		, SUM(CASE WHEN d.Density IS NOT NULL THEN 1 ELSE 0 END) OVER (partition by i.InstrumentID order by i.DateTimeEn) AS DensityDomains
		, ROW_NUMBER() OVER (partition by i.InstrumentID order by i.DateTimeEn) as Ranking
	from InstrumentsFill i
	left join BasicData d on i.InstrumentID = d.InstrumentID and i.DateTimeEn = d.DateTimeEn
) d
where (@Offset is null or Ranking >= @Offset)
  and (@Count is null or Ranking <= ISNULL(@Offset, 0) + @Count)