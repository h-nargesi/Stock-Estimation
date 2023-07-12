from codes.handlers import Handlers
from codes.trade import TradeReader

hd = Handlers('solution_1')

loader = TradeReader(hd, verbose=4)
loader.ReadData(ignore_existing=True)