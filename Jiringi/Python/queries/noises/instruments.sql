declare @TypeIDs varchar(max) = '1'

select i.ID, i.Name, i.NameEn, i.ShortName
    , c.Name as Company
    , i.IndexID
    --, (select * from [RahavardNovin3].dbo.In ii where ii.ID = i.IndexID) as [Index]
    , (select t.Title from InstrumentType t where t.ID = i.TypeID) as Type
    , (select m.Title from Market m where m.ID = i.MarketID) as Market
    , (select b.Title from Board b where b.ID = i.BoardID) as Board
    , (select g.Title from InstrumentGroup g where g.ID = i.GroupID) as [Group]
    , (select v.Title from ValueType v where v.ID = i.ValueTypeID) as ValueType
    , i.BaseVolume, i.NominalPrice, i.PriceTick, i.TradeTick, i.PaymentDelay
    , i.TseID, i.ExportName
    , (select e.Title from Exchange e where e.ID = i.ExchangeID) as Exchange
    , (select d.Name from Industry d where d.Code = i.Code) as Industry
    , (select g.Name from Category g where g.Code = i.CategoryCode) as Category
    , i.MinPricePermit, i.MaxPricePermit, i.MaxVolumePermit, i.MinVolumePermit
    , (select m.Name from MortgageLoan m where m.ID = i.MortgageLoanID) as MortgageLoan
    , (select f.Name from Fund f where f.ID = i.FundID) as Fund
    , (select b.Name from Bond b where b.ID = i.BondID) as Bond
    , (select a.Name from Asset a where a.ID = i.AssetId) as Asset
from Instrument i
join Company c on c.ID = i.CompanyID
where i.TypeID in (select CAST(VALUE AS INT) AS ID from STRING_SPLIT(@TypeIDs, ','))
