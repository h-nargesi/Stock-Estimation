declare @InstrumentIDs varchar(max) = '17321',
		@Zoom int = 10,
        @Start datetime = null,
        @End datetime = null;

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
		select t.InstrumentID, t.DateTimeEn
			, ISNULL(BuyerCount, 0) AS BuyerCount
			, OpenPrice, LowPrice, HighPrice, ClosePrice
			, TradeCount, Volume
			, ROW_NUMBER() over(partition by InstrumentID order by DateTimeEn) / @Zoom as GroupingValue
		from Trade t
		where (@InstrumentIDs is not null and t.InstrumentID in (select CAST(VALUE AS INT) AS VALUE from STRING_SPLIT(@InstrumentIDs, ','))
			or @InstrumentIDs is null and t.InstrumentID in (select InstrumentID from ValidInstruments))
		  and (@Start is null or t.DateTimeEn >= @Start)
		  and (@End is null or t.DateTimeEn <= @End)
	) t
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

select Instrument, DateTimeEn, BuyerCount
	, FIRST_VALUE(OpenPrice) OVER (partition by InstrumentID, OpenPriceDomains order by DateTimeEn) as OpenPrice
	, FIRST_VALUE(LowPrice) OVER (partition by InstrumentID, LowPriceDomains order by DateTimeEn) as LowPrice
	, FIRST_VALUE(HighPrice) OVER (partition by InstrumentID, HighPriceDomains order by DateTimeEn) as HighPrice
	, FIRST_VALUE(ClosePrice) OVER (partition by InstrumentID, ClosePriceDomains order by DateTimeEn) as ClosePrice
	, FIRST_VALUE(TradeCount) OVER (partition by InstrumentID, TradeCountDomains order by DateTimeEn) as TradeCount
	, FIRST_VALUE(Volume) OVER (partition by InstrumentID, VolumeDomains order by DateTimeEn) as Volume
from (
	select i.NameEn + ':' + CAST(i.InstrumentID as varchar(11)) as Instrument
		, i.InstrumentID
		, i.DateTimeEn
		, ISNULL(d.BuyerCount, 0) as BuyerCount
		, d.OpenPrice
		, d.LowPrice
		, d.HighPrice
		, d.ClosePrice
		, d.TradeCount
		, d.Volume
		, SUM(CASE WHEN d.OpenPrice IS NOT NULL THEN 1 ELSE 0 END) OVER (partition by i.InstrumentID order by i.DateTimeEn) AS OpenPriceDomains
		, SUM(CASE WHEN d.LowPrice IS NOT NULL THEN 1 ELSE 0 END) OVER (partition by i.InstrumentID order by i.DateTimeEn) AS LowPriceDomains
		, SUM(CASE WHEN d.HighPrice IS NOT NULL THEN 1 ELSE 0 END) OVER (partition by i.InstrumentID order by i.DateTimeEn) AS HighPriceDomains
		, SUM(CASE WHEN d.ClosePrice IS NOT NULL THEN 1 ELSE 0 END) OVER (partition by i.InstrumentID order by i.DateTimeEn) AS ClosePriceDomains
		, SUM(CASE WHEN d.TradeCount IS NOT NULL THEN 1 ELSE 0 END) OVER (partition by i.InstrumentID order by i.DateTimeEn) AS TradeCountDomains
		, SUM(CASE WHEN d.Volume IS NOT NULL THEN 1 ELSE 0 END) OVER (partition by i.InstrumentID order by i.DateTimeEn) AS VolumeDomains
		, ROW_NUMBER() OVER (partition by i.InstrumentID order by i.DateTimeEn) as Ranking
	from InstrumentsFill i
	left join BasicData d on i.InstrumentID = d.InstrumentID and i.DateTimeEn = d.DateTimeEn
) d
