declare @InstrumentIDs varchar(max) = '17321, 17322',
		@Zoom int = 10,
        @Offset int = 0,
        @Count int = 400
		;

WITH BasicData as (
	select InstrumentID
		, MAX(t.DateTimeEn) as DateTimeEn
		, AVG(t.BuyerCount) as BuyerCount
		, AVG(t.OpenPrice) as OpenPrice
		, AVG(t.LowPrice) as LowPrice
		, AVG(t.HighPrice) as HighPrice
		, AVG(t.ClosePrice) as ClosePrice
		, AVG(t.TradeCount) as TradeCount
		, AVG(t.Volume) as Volume
	from (
		select t.InstrumentID
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
		where t.InstrumentID in (select CAST(VALUE AS INT) AS ID from STRING_SPLIT(@InstrumentIDs, ','))
	) t
	where GroupingValue between @Offset and @Offset + @Count
	group by InstrumentID, GroupingValue

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

select i.NameEn as Instrument
	, i.DateTimeEn
	, SUM(d.BuyerCount) OVER (partition by i.InstrumentID order by i.DateTimeEn) AS BuyerCount
	, d.BuyerCount
	, d.OpenPrice
	, d.LowPrice
	, d.HighPrice
	, d.ClosePrice
	, d.TradeCount
	, d.Volume
from InstrumentsFill i
left join BasicData d on i.InstrumentID = d.InstrumentID and i.DateTimeEn = d.DateTimeEn