import codes.trade as trade

loader = trade.TradeReader(300, 10, 280000, verbose=4)
loader.ReadData(ignore_existing=True)

# data = list()
# data.append([10, 20])
# data.append([10, 30])
# print("data=", data)
# print("data=", np.array(data).shape)