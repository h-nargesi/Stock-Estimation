select dens.*
	 , ROW_NUMBER() over(order by Duration) +
	   ROW_NUMBER() over(order by Density) * 10 as Score
from (
	select dur.*
		 , CASE Duration WHEN 0 THEN 0 ELSE CAST(RowCounts AS FLOAT) / CAST(Duration AS FLOAT) END AS Density
	from (
		select InstrumentID
			 , StartDateTime
			 , EndDateTime
			 , ISNULL(DATEDIFF(DAY, StartDateTime, EndDateTime), 0) as Duration
			 , RowCounts
		from (
			select InstrumentID
				 , MIN(DateTimeEn) as StartDateTime
				 , MAX(DateTimeEn) as EndDateTime
				 , COUNT(1) as RowCounts
			from Trade
			group by InstrumentID
			having count(1) > 265
		) ins
	) dur
) dens
order by Score desc