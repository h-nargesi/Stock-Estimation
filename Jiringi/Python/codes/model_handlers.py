import numpy as np
from rsa import sign

class ModelHandlers:

    FACTOR = None
    SHOW_FACTOR = None

    def __init__(self, factor: int):
        self.FACTOR = factor
        self.SHOW_FACTOR = 100 / factor
    
    def Prediction(self, predicted, y_testing):

        max_profit = np.max(predicted - 0.2, axis=1)
        max_profit = (max_profit + 0.2) * (max_profit > 0)
        max_profit = np.dot(max_profit.reshape([3, 1]), np.ones((1, 3)))
        # print("max_profit")
        # print(max_profit)

        predicted = (np.sign(predicted - max_profit) + 1) * max_profit
        # print("predicted")
        # print(predicted)

        investment_mask = np.sign(predicted)
        check_multies = np.sum(investment_mask, axis=1) - 1
        check_multies = check_multies * (check_multies > 0)
        check_multies = np.dot(check_multies.reshape([3, 1]), np.ones((1, 3))) * -1 + 1
        # print("check_multies")
        # print(check_multies)

        investment_mask = check_multies * investment_mask
        # print("investment_mask")
        # print(investment_mask)

        investment_result = y_testing * investment_mask
        # print("investment_result: {0:.2f}".format(np.sum(investment_result)))
        # print(investment_result)
        return np.sum(investment_result)

    def PrintResult(self, score, suffix='Result:', otype='.3f', prefix='%'):
        if type(score) != list and type(score) != tuple:
            score = [score]
        
        if prefix is None: prefix = ""
        else: prefix = " " + prefix

        text = ""
        for index in range(0, len(score)):
            text += ", {{{}:{}}}{}".format(index, otype, prefix)
            index += 1

        if self.SHOW_FACTOR != 1:
            score = (np.array(score) * self.SHOW_FACTOR).tolist()
        
        if len(text) > 0:
            print(suffix, text[2:].format(*score))
