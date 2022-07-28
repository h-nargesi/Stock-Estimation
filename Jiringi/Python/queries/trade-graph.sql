declare @InstrumentIDs varchar(max) = '13',
		@Zoom int = 10,
        @Offset int = 0,
        @Count int = 400

select Instrument
	 , MAX(t.DateTimeEn) as DateTimeEn
	 , AVG(t.BuyerCount) as BuyerCount
	 , AVG(t.OpenPrice) as OpenPrice
	 , AVG(t.LowPrice) as LowPrice
	 , AVG(t.HighPrice) as HighPrice
	 , AVG(t.ClosePrice) as ClosePrice
	 , AVG(t.TradeCount) as TradeCount
	 , AVG(t.Volume) as Volume
from (
	select i.NameEn as Instrument
		 , t.DateTimeEn
		 , ISNULL(BuyerCount, 0) AS BuyerCount
		 , ISNULL(OpenPrice, 0) AS OpenPrice
		 , ISNULL(LowPrice, 0) AS LowPrice
		 , ISNULL(HighPrice, 0) AS HighPrice
		 , ISNULL(ClosePrice, 0) AS ClosePrice
		 , ISNULL(TradeCount, 0) AS TradeCount
		 , ISNULL(Volume, 0) as Volume
		 , ROW_NUMBER() over(partition by InstrumentID order by DateTimeEn) / @Zoom as GroupingValue
	from Trade t
	join Instrument i on t.InstrumentID = i.ID
	where t.InstrumentID in (select CAST(VALUE AS INT) AS VALUE from STRING_SPLIT(@InstrumentIDs, ','))
) t
where GroupingValue between @Offset and @Offset + @Count
group by Instrument, GroupingValue