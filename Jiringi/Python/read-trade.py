import codes.trade as trade

loader = trade.TradeReader(300, 10, 5, verbose=4)
loader.ReadData(ignore_existing=True)
