use RahavardNovin3;
go

declare @InstrumentID int = 13;
declare @Offset int = 2812;
declare @Count int = 960;

select row_number() over(order by DateTimeEn desc) - 1 as Offset
     , DateTimeEn, ClosePrice, RecordType
from Trade where InstrumentID = @InstrumentID
order by DateTimeEn desc offset @Offset rows fetch first @Count rows only
