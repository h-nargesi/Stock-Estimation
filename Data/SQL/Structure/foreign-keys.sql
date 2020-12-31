use RahavardNovin3
go

/*
ALTER TABLE [Asset] ADD FOREIGN KEY ([TypeId]) REFERENCES [AssetType]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Asset] ADD FOREIGN KEY ([ExchangeId]) REFERENCES [Exchange]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Asset] ADD FOREIGN KEY ([StateId]) REFERENCES [AssetState]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Asset] ADD FOREIGN KEY ([StockCompanyId]) REFERENCES [Company]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Asset] ADD FOREIGN KEY ([BondBondId]) REFERENCES [Bond]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Asset] ADD FOREIGN KEY ([FundFundId]) REFERENCES [Fund]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Asset] ADD FOREIGN KEY ([MortgageLoanMortgageLoanId]) REFERENCES [MortgageLoan]([ID]) ON DELETE NO ACTION;
--ALTER TABLE [Asset] ADD FOREIGN KEY ([OptionContractId]) REFERENCES [?]([ID]) ON DELETE NO ACTION;

ALTER TABLE [BalanceSheet] ADD FOREIGN KEY ([CompanyID]) REFERENCES [Company]([ID]) ON DELETE NO ACTION;
ALTER TABLE [BalanceSheet] ADD FOREIGN KEY ([MeetingID]) REFERENCES [Meeting]([ID]) ON DELETE NO ACTION;
ALTER TABLE [BalanceSheet] ADD FOREIGN KEY ([ReportID]) REFERENCES [Report]([ID]) ON DELETE NO ACTION;
ALTER TABLE [BalanceSheet] ADD FOREIGN KEY ([AnnouncementTypeID]) REFERENCES [AnnouncementType]([ID]) ON DELETE NO ACTION;
ALTER TABLE [BalanceSheet] ADD FOREIGN KEY ([FinancialViewTypeID]) REFERENCES [FinancialViewType]([ID]) ON DELETE NO ACTION;

ALTER TABLE [BalanceSheetDetail] ADD FOREIGN KEY ([BalanceSheetID]) REFERENCES [BalanceSheet]([ID]) ON DELETE CASCADE;
ALTER TABLE [BalanceSheetDetail] ADD FOREIGN KEY ([BalanceSheetFieldID]) REFERENCES [BalanceSheetField]([ID]) ON DELETE CASCADE;

ALTER TABLE [BidAskDetails] ADD FOREIGN KEY ([InstrumentID]) REFERENCES [Instrument]([ID]) ON DELETE NO ACTION;

ALTER TABLE [BoardOfDirectors] ADD FOREIGN KEY ([GroupID]) REFERENCES [BoardOfDirectorsGroup]([ID]) ON DELETE NO ACTION;
ALTER TABLE [BoardOfDirectors] ADD FOREIGN KEY ([CompanyID]) REFERENCES [Company]([ID]) ON DELETE NO ACTION;
ALTER TABLE [BoardOfDirectors] ADD FOREIGN KEY ([PersonID]) REFERENCES [Person]([ID]) ON DELETE NO ACTION;
ALTER TABLE [BoardOfDirectors] ADD FOREIGN KEY ([PositionID]) REFERENCES [PositionType]([ID]) ON DELETE NO ACTION;
ALTER TABLE [BoardOfDirectors] ADD FOREIGN KEY ([DirectorStatusID]) REFERENCES [DirectorStatus]([ID]) ON DELETE NO ACTION;

ALTER TABLE [BoardOfDirectorsGroup] ADD FOREIGN KEY ([CompanyID]) REFERENCES [Company]([ID]) ON DELETE NO ACTION;
	update [BoardOfDirectorsGroup] set [MeetingID] = null where [MeetingID] not in (select ID from [Meeting]);
ALTER TABLE [BoardOfDirectorsGroup] ADD FOREIGN KEY ([MeetingID]) REFERENCES [Meeting]([ID]) ON DELETE NO ACTION;
	delete BoardOfDirectors where [GroupID] in (select ID from [BoardOfDirectorsGroup] where [ReportID] not in (select ID from [Report]));
	delete [BoardOfDirectorsGroup] where [ReportID] not in (select ID from [Report]);
ALTER TABLE [BoardOfDirectorsGroup] ADD FOREIGN KEY ([ReportID]) REFERENCES [Report]([ID]) ON DELETE NO ACTION;

ALTER TABLE [Bond] ADD FOREIGN KEY ([TypeId]) REFERENCES [BondType]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Bond] ADD FOREIGN KEY ([ExchangeID]) REFERENCES [Exchange]([ID]) ON DELETE NO ACTION;

ALTER TABLE [BondInterestPayment] ADD FOREIGN KEY ([BondID]) REFERENCES [Bond]([ID]) ON DELETE NO ACTION;

ALTER TABLE [CapitalIncrease] ADD FOREIGN KEY ([CompanyID]) REFERENCES [Company]([ID]) ON DELETE NO ACTION;
	update [CapitalIncrease] set [MeetingID] = null where [MeetingID] not in (select ID from [Meeting]);
ALTER TABLE [CapitalIncrease] ADD FOREIGN KEY ([MeetingID]) REFERENCES [Meeting]([ID]) ON DELETE NO ACTION;
ALTER TABLE [CapitalIncrease] ADD FOREIGN KEY ([ReportID]) REFERENCES [Report]([ID]) ON DELETE NO ACTION;
ALTER TABLE [CapitalIncrease] ADD FOREIGN KEY ([UnderwritingEndReportID]) REFERENCES [Report]([ID]) ON DELETE NO ACTION;
ALTER TABLE [CapitalIncrease] ADD FOREIGN KEY ([RegistrationReportID]) REFERENCES [Report]([ID]) ON DELETE NO ACTION;

ALTER TABLE [CashFlow] ADD FOREIGN KEY ([CompanyID]) REFERENCES [Company]([ID]) ON DELETE NO ACTION;
ALTER TABLE [CashFlow] ADD FOREIGN KEY ([MeetingID]) REFERENCES [Meeting]([ID]) ON DELETE NO ACTION;
ALTER TABLE [CashFlow] ADD FOREIGN KEY ([ReportID]) REFERENCES [Report]([ID]) ON DELETE NO ACTION;
ALTER TABLE [CashFlow] ADD FOREIGN KEY ([FinancialViewTypeID]) REFERENCES [FinancialViewType]([ID]) ON DELETE NO ACTION;

ALTER TABLE [CashFlowDetail] ADD FOREIGN KEY ([CashFlowID]) REFERENCES [CashFlow]([ID]) ON DELETE CASCADE;
	delete from CashFlowDetail where CashFlowFieldID not in (select ID from CashFlowField);
ALTER TABLE [CashFlowDetail] ADD FOREIGN KEY ([CashFlowFieldID]) REFERENCES [CashFlowField]([ID]) ON DELETE CASCADE;

ALTER TABLE [Company] ADD FOREIGN KEY ([TypeID]) REFERENCES [CompanyType]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Company] ADD FOREIGN KEY ([StateID]) REFERENCES [CompanyState]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Company] ADD FOREIGN KEY ([ParentID]) REFERENCES [Company]([ID]) ON DELETE NO ACTION;
--ALTER TABLE [Company] ADD FOREIGN KEY ([BrokerID]) REFERENCES [?]([ID]) ON DELETE NO ACTION;
--ALTER TABLE [Company] ADD FOREIGN KEY ([BrokerTradeID]) REFERENCES [?]([ID]) ON DELETE NO ACTION;

ALTER TABLE [CompanyContactInfo] ADD FOREIGN KEY ([CompanyID]) REFERENCES [Company]([ID]) ON DELETE CASCADE;
ALTER TABLE [CompanyContactInfo] ADD FOREIGN KEY ([TypeID]) REFERENCES [ContactInfoType]([ID]) ON DELETE NO ACTION;

ALTER TABLE [CompanyHistory] ADD FOREIGN KEY ([ReportID]) REFERENCES [Report]([ID]) ON DELETE NO ACTION;
	delete from [CompanyHistory] where [CompanyID] not in (select [ID] from [Company]);
ALTER TABLE [CompanyHistory] ADD FOREIGN KEY ([CompanyID]) REFERENCES [Company]([ID]) ON DELETE NO ACTION;
--ALTER TABLE [CompanyHistory] ADD FOREIGN KEY ([StateID]) REFERENCES [?]([ID]) ON DELETE NO ACTION;

ALTER TABLE [Director] ADD FOREIGN KEY ([CompanyID]) REFERENCES [Company]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Director] ADD FOREIGN KEY ([MeetingID]) REFERENCES [Meeting]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Director] ADD FOREIGN KEY ([ReportID]) REFERENCES [Report]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Director] ADD FOREIGN KEY ([PersonID]) REFERENCES [Person]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Director] ADD FOREIGN KEY ([PositionID]) REFERENCES [PositionType]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Director] ADD FOREIGN KEY ([StatusID]) REFERENCES [DirectorStatus]([ID]) ON DELETE NO ACTION;

ALTER TABLE [DividentPayout] ADD FOREIGN KEY ([MeetingID]) REFERENCES [Meeting]([ID]) ON DELETE NO ACTION;
ALTER TABLE [DividentPayout] ADD FOREIGN KEY ([ReportID]) REFERENCES [Report]([ID]) ON DELETE NO ACTION;

ALTER TABLE [EPS] ADD FOREIGN KEY ([CompanyID]) REFERENCES [Company]([ID]) ON DELETE NO ACTION;
ALTER TABLE [EPS] ADD FOREIGN KEY ([MeetingID]) REFERENCES [Meeting]([ID]) ON DELETE NO ACTION;
ALTER TABLE [EPS] ADD FOREIGN KEY ([ReportID]) REFERENCES [Report]([ID]) ON DELETE NO ACTION;
ALTER TABLE [EPS] ADD FOREIGN KEY ([AnnouncementTypeID]) REFERENCES [AnnouncementType]([ID]) ON DELETE NO ACTION;

ALTER TABLE [FreeFloat] ADD FOREIGN KEY ([CompanyID]) REFERENCES [Company]([ID]) ON DELETE NO ACTION;

ALTER TABLE [FundPrice] ADD FOREIGN KEY ([FundID]) REFERENCES [Fund]([ID]) ON DELETE NO ACTION;

ALTER TABLE [IndexInstrument] ADD FOREIGN KEY ([IndexID]) REFERENCES [Index]([ID]) ON DELETE NO ACTION;
ALTER TABLE [IndexInstrument] ADD FOREIGN KEY ([InstrumentID]) REFERENCES [Instrument]([ID]) ON DELETE NO ACTION;

ALTER TABLE [IndexInstrumentEffect] ADD FOREIGN KEY ([IndexID]) REFERENCES [Index]([ID]) ON DELETE NO ACTION;
ALTER TABLE [IndexInstrumentEffect] ADD FOREIGN KEY ([InstrumentID]) REFERENCES [Instrument]([ID]) ON DELETE NO ACTION;

ALTER TABLE [IndexIntradayTradeSummary] ADD FOREIGN KEY ([IndexID]) REFERENCES [Index]([ID]) ON DELETE NO ACTION;
*/

ALTER TABLE [IndexIntradayValue] ADD FOREIGN KEY ([IndexValueID]) REFERENCES [IndexValue]([ID]) ON DELETE NO ACTION;
ALTER TABLE [IndexIntradayValue] ADD FOREIGN KEY ([IndexValueID]) REFERENCES [IndexValue]([ID]) ON DELETE NO ACTION;

ALTER TABLE [IndexTradeSummary] ADD FOREIGN KEY ([IndexID]) REFERENCES [Index]([ID]) ON DELETE NO ACTION;

ALTER TABLE [IndexValue] ADD FOREIGN KEY ([IndexID]) REFERENCES [Index]([ID]) ON DELETE NO ACTION;
--ALTER TABLE [IndexValue] ADD FOREIGN KEY ([PreviousID]) REFERENCES [?]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Instrument] ADD FOREIGN KEY ([CompanyID]) REFERENCES [Company]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Instrument] ADD FOREIGN KEY ([IndexID]) REFERENCES [Index]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Instrument] ADD FOREIGN KEY ([TypeID]) REFERENCES [InstrumentType]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Instrument] ADD FOREIGN KEY ([MarketID]) REFERENCES [Market]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Instrument] ADD FOREIGN KEY ([BoardID]) REFERENCES [Board]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Instrument] ADD FOREIGN KEY ([GroupID]) REFERENCES [InstrumentGroup]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Instrument] ADD FOREIGN KEY ([ValueTypeID]) REFERENCES [InstrumentType]([ID]) ON DELETE NO ACTION;
--ALTER TABLE [Instrument] ADD FOREIGN KEY ([TseID]) REFERENCES [?]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Instrument] ADD FOREIGN KEY ([ExchangeID]) REFERENCES [Exchange]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Instrument] ADD FOREIGN KEY ([MortgageLoanID]) REFERENCES [MortgageLoan]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Instrument] ADD FOREIGN KEY ([FundID]) REFERENCES [Fund]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Instrument] ADD FOREIGN KEY ([BondID]) REFERENCES [Bond]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Instrument] ADD FOREIGN KEY ([AssetID]) REFERENCES [Asset]([ID]) ON DELETE NO ACTION;

	delete from InstrumentBaseVolume where InstrumentID not in (select ID from Instrument)
ALTER TABLE [InstrumentBaseVolume] ADD FOREIGN KEY ([InstrumentID]) REFERENCES [Instrument]([ID]) ON DELETE CASCADE;

ALTER TABLE [InstrumentShareHolder] ADD FOREIGN KEY ([GroupID]) REFERENCES [InstrumentShareHolderGroup]([ID]) ON DELETE CASCADE;
	delete from InstrumentShareHolder where ShareHolderID not in (select ID from ShareHolder)
ALTER TABLE [InstrumentShareHolder] ADD FOREIGN KEY ([ShareHolderID]) REFERENCES [ShareHolder]([ID]) ON DELETE CASCADE;

ALTER TABLE [InstrumentShareHolderGroup] ADD FOREIGN KEY ([InstrumentID]) REFERENCES [Instrument]([ID]) ON DELETE CASCADE;

ALTER TABLE [InstrumentState] ADD FOREIGN KEY ([InstrumentID]) REFERENCES [Instrument]([ID]) ON DELETE CASCADE;

ALTER TABLE [InstrumentStaticThresholds] ADD FOREIGN KEY ([InstrumentID]) REFERENCES [Instrument]([ID]) ON DELETE CASCADE;

ALTER TABLE [IntradayTrade] ADD FOREIGN KEY ([TradeID]) REFERENCES [Trade]([ID]) ON DELETE NO ACTION;

ALTER TABLE [Meeting] ADD FOREIGN KEY ([CompanyID]) REFERENCES [Company]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Meeting] ADD FOREIGN KEY ([ReportID]) REFERENCES [Report]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Meeting] ADD FOREIGN KEY ([TypeID]) REFERENCES [MeetingType]([ID]) ON DELETE NO ACTION;

ALTER TABLE [MeetingInvite] ADD FOREIGN KEY ([CompanyID]) REFERENCES [Company]([ID]) ON DELETE NO ACTION;
	delete from MeetingInvite where ReportID not in (select ID from Report)
ALTER TABLE [MeetingInvite] ADD FOREIGN KEY ([ReportID]) REFERENCES [Report]([ID]) ON DELETE NO ACTION;

ALTER TABLE [News] ADD FOREIGN KEY ([CompanyID]) REFERENCES [Company]([ID]) ON DELETE NO ACTION;
ALTER TABLE [News] ADD FOREIGN KEY ([SourceID]) REFERENCES [Source]([ID]) ON DELETE NO ACTION;
----------------------------------------------------------------------------------------------

	delete from PortfolioAsset where PortfolioGroupID not in (select ID from PortfolioGroup)
ALTER TABLE [PortfolioAsset] ADD FOREIGN KEY ([PortfolioGroupID]) REFERENCES [PortfolioGroup]([ID]) ON DELETE NO ACTION;
--ALTER TABLE [PortfolioAsset] ADD FOREIGN KEY ([StateID]) REFERENCES [?]([ID]) ON DELETE NO ACTION;
ALTER TABLE [PortfolioAsset] ADD FOREIGN KEY ([AssetTypeID]) REFERENCES [AssetType]([ID]) ON DELETE NO ACTION;
	delete from PortfolioAsset where AssetID not in (select ID from Asset)
ALTER TABLE [PortfolioAsset] ADD FOREIGN KEY ([AssetID]) REFERENCES [Asset]([ID]) ON DELETE NO ACTION;
-------------------------------------------------------------------------------------------------------

ALTER TABLE [ProductionMaterialSale] ADD FOREIGN KEY ([ProductionSaleGroupID]) REFERENCES [ProductionSaleGroup]([ID]) ON DELETE NO ACTION;
--ALTER TABLE [ProductionMaterialSale] ADD FOREIGN KEY ([TypeID]) REFERENCES [?]([ID]) ON DELETE NO ACTION;

ALTER TABLE [ProductionMaterialSaleDetails] ADD FOREIGN KEY ([ProductionMaterialSaleID]) REFERENCES [ProductionMaterialSale]([ID]) ON DELETE NO ACTION;
ALTER TABLE [ProductionMaterialSaleDetails] ADD FOREIGN KEY ([UnitID]) REFERENCES [Unit]([ID]) ON DELETE NO ACTION;

	delete from ProductionSale where GroupID not in (select ID from ProductionSaleGroup)
ALTER TABLE [ProductionSale] ADD FOREIGN KEY ([GroupID]) REFERENCES [ProductionSaleGroup]([ID]) ON DELETE NO ACTION;
ALTER TABLE [ProductionSale] ADD FOREIGN KEY ([ProductID]) REFERENCES [Product]([ID]) ON DELETE NO ACTION;
ALTER TABLE [ProductionSale] ADD FOREIGN KEY ([UnitID]) REFERENCES [Unit]([ID]) ON DELETE NO ACTION;

ALTER TABLE [ProfitLoss] ADD FOREIGN KEY ([CompanyID]) REFERENCES [Company]([ID]) ON DELETE NO ACTION;
ALTER TABLE [ProfitLoss] ADD FOREIGN KEY ([MeetingID]) REFERENCES [Meeting]([ID]) ON DELETE NO ACTION;
ALTER TABLE [ProfitLoss] ADD FOREIGN KEY ([ReportID]) REFERENCES [Report]([ID]) ON DELETE NO ACTION;

ALTER TABLE [ProductionSaleGroup] ADD FOREIGN KEY ([CompanyID]) REFERENCES [Company]([ID]) ON DELETE NO ACTION;
ALTER TABLE [ProductionSaleGroup] ADD FOREIGN KEY ([MeetingID]) REFERENCES [Meeting]([ID]) ON DELETE NO ACTION;
	--delete from ProductionSaleGroup where ReportID not in (select ID from Report)
--ALTER TABLE [ProductionSaleGroup] ADD FOREIGN KEY ([ReportID]) REFERENCES [Report]([ID]) ON DELETE NO ACTION;

	delete from ProfitLossDetail where ProfitLossID not in (select ID from ProfitLoss)
ALTER TABLE [ProfitLossDetail] ADD FOREIGN KEY ([ProfitLossID]) REFERENCES [ProfitLoss]([ID]) ON DELETE NO ACTION;
ALTER TABLE [ProfitLossDetail] ADD FOREIGN KEY ([ProfitLossFieldID]) REFERENCES [ProfitLossField]([ID]) ON DELETE NO ACTION;

ALTER TABLE [Report] ADD FOREIGN KEY ([SourceID]) REFERENCES [ReportSource]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Report] ADD FOREIGN KEY ([TitleID]) REFERENCES [ReportTitle]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Report] ADD FOREIGN KEY ([SubTitleID]) REFERENCES [ReportSubTitle]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Report] ADD FOREIGN KEY ([CompanyID]) REFERENCES [Company]([ID]) ON DELETE NO ACTION;
	update Report set SubCompanyID = null where SubCompanyID not in (select ID from Company);
ALTER TABLE [Report] ADD FOREIGN KEY ([SubCompanyID]) REFERENCES [Company]([ID]) ON DELETE NO ACTION;
--ALTER TABLE [Report] ADD FOREIGN KEY ([ReportImageID]) REFERENCES [ReportImage]([ID]) ON DELETE NO ACTION;

ALTER TABLE [ReportImage] ADD FOREIGN KEY ([ReportID]) REFERENCES [Report]([ID]) ON DELETE CASCADE;

ALTER TABLE [ReportReportSubtitle] ADD FOREIGN KEY ([ReportID]) REFERENCES [Report]([ID]) ON DELETE CASCADE;
ALTER TABLE [ReportReportSubtitle] ADD FOREIGN KEY ([ReportSubTitleID]) REFERENCES [ReportSubTitle]([ID]) ON DELETE CASCADE;

ALTER TABLE [ShareHolder] ADD FOREIGN KEY ([GroupID]) REFERENCES [ShareHolderGroup]([ID]) ON DELETE CASCADE;
ALTER TABLE [ShareHolder] ADD FOREIGN KEY ([CompanyID]) REFERENCES [Company]([ID]) ON DELETE CASCADE;
ALTER TABLE [ShareHolder] ADD FOREIGN KEY ([PersonID]) REFERENCES [Person]([ID]) ON DELETE CASCADE;

ALTER TABLE [ShareHolderGroup] ADD FOREIGN KEY ([CompanyID]) REFERENCES [Company]([ID]) ON DELETE CASCADE;
ALTER TABLE [ShareHolderGroup] ADD FOREIGN KEY ([ReportID]) REFERENCES [Report]([ID]) ON DELETE NO ACTION;

ALTER TABLE [ShareHolderInfo] ADD FOREIGN KEY ([ParentID]) REFERENCES [ShareHolderInfo]([ID]) ON DELETE NO ACTION;

ALTER TABLE [TheoreticalOpeningPrice] ADD FOREIGN KEY ([InstrumentID]) REFERENCES [Instrument]([ID]) ON DELETE NO ACTION;

ALTER TABLE [Trade] ADD FOREIGN KEY ([InstrumentID]) REFERENCES [Instrument]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Trade] ADD FOREIGN KEY ([TypeID]) REFERENCES [TradeType]([ID]) ON DELETE NO ACTION;
--ALTER TABLE [Trade] ADD FOREIGN KEY ([PreviousID]) REFERENCES [Trade]([ID]) ON DELETE NO ACTION;

	delete from TradeDetails where TradeID not in (select ID from Trade);
ALTER TABLE [TradeDetails] ADD FOREIGN KEY ([TradeID]) REFERENCES [Trade]([ID]) ON DELETE NO ACTION;
ALTER TABLE [TradeDetails] ADD FOREIGN KEY ([InstrumentID]) REFERENCES [Instrument]([ID]) ON DELETE NO ACTION;

	delete from TradeSummary where TradeID not in (select ID from Trade);
ALTER TABLE [TradeSummary] ADD FOREIGN KEY ([TradeID]) REFERENCES [Trade]([ID]) ON DELETE NO ACTION;


/*
	select count(*) as allr,
		sum(case when r.ID is null then 1 else 0 end) as inv,
		(100.0 / count(*)) *
		sum(case when r.ID is null then 1 else 0 end) as inv
	from Trade f left join Trade r
	on f.PreviousID = r.ID
	where f.PreviousID is not null
*/