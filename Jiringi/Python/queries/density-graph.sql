declare @InstrumentIDs varchar(max) = '13',
		@Zoom int = 5,
        @Offset int = 0,
        @Count int = 1000

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
	where GroupingValue between @Offset and @Offset + @Count
	group by InstrumentID, GroupingValue
) dur
