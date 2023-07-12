
select i.ID as InstrumentID, i.Name
    , (select COUNT(1) from Trade t where t.InstrumentID = i.ID) as TradeCount
    , c.Name, c.Board
    , (select s.Title from CompanyState s where s.ID = c.StateID) as State
    , (select i.Name from Industry i where i.Code = c.IndustryCode) as Industry
    , (select ct.Name from Category ct where ct.Code = c.CategoryCode and ct.IndustryCode = c.IndustryCode) as Category
from Company c
join Instrument i on i.CompanyID = c.ID
join ActiveInstuments(200) ac on ac.InstrumentID = i.ID
where i.TypeID in (1)
order by c.ID