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
insert into ReportTitle
insert into [Report] (Date, DateEn, Number, 
	SourceID, TitleID, SubTitleID, CompanyID, EntryDate, EntryDateEn, FiscalYear, FiscalYearEn,
	SubCompanyId, Comments, CommentsEn, ReportImageID
)
select Date, DateEn, '0', (select top 1 ID from ReportTitle)
from [BoardOfDirectorsGroup] where [ReportID] not in (select ID from [Report]);
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
merge into CashFlowDetail dtl
using CashFlowField fld
on (dtl.CashFlowFieldID = fld.ID)
when not matched by source then delete;
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
--ALTER TABLE [InstrumentBaseVolume] ADD FOREIGN KEY ([InstrumentID]) REFERENCES [?]([ID]) ON DELETE NO CASCADE;
ALTER TABLE [InstrumentShareHolder] ADD FOREIGN KEY ([GroupID]) REFERENCES [InstrumentShareHolderGroup]([ID]) ON DELETE CASCADE;
--ALTER TABLE [InstrumentShareHolder] ADD FOREIGN KEY ([ShareHolderID]) REFERENCES [ShareHolder]([ID]) ON DELETE CASCADE;
ALTER TABLE [InstrumentShareHolderGroup] ADD FOREIGN KEY ([InstrumentID]) REFERENCES [Instrument]([ID]) ON DELETE CASCADE;
ALTER TABLE [InstrumentState] ADD FOREIGN KEY ([InstrumentID]) REFERENCES [Instrument]([ID]) ON DELETE CASCADE;
ALTER TABLE [InstrumentStaticThresholds] ADD FOREIGN KEY ([InstrumentID]) REFERENCES [Instrument]([ID]) ON DELETE CASCADE;
ALTER TABLE [IntradayTrade] ADD FOREIGN KEY ([TradeID]) REFERENCES [Trade]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Meeting] ADD FOREIGN KEY ([CompanyID]) REFERENCES [Company]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Meeting] ADD FOREIGN KEY ([ReportID]) REFERENCES [Report]([ID]) ON DELETE NO ACTION;
ALTER TABLE [Meeting] ADD FOREIGN KEY ([TypeID]) REFERENCES [MeetingType]([ID]) ON DELETE NO ACTION;
ALTER TABLE [MeetingInvite] ADD FOREIGN KEY ([CompanyID]) REFERENCES [Company]([ID]) ON DELETE NO ACTION;
--ALTER TABLE [MeetingInvite] ADD FOREIGN KEY ([ReportID]) REFERENCES [?]([ID]) ON DELETE NO ACTION;
ALTER TABLE [News] ADD FOREIGN KEY ([CompanyID]) REFERENCES [Company]([ID]) ON DELETE NO ACTION;
ALTER TABLE [News] ADD FOREIGN KEY ([SourceID]) REFERENCES [Source]([ID]) ON DELETE NO ACTION;
---------------------------------------------------------------------------------------------------------------------------------
---------------------------------------------------------------------------------------------------------------------------------
