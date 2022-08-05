import numpy as np

class ModelHandlers:

    def Prediction(predicted, y_testing, verbose=0):

        OUTPUT_DATA_SIZE, OUTPUT_INV_LENGTH = predicted.shape

        max_profit = np.max(predicted - 0.2, axis=1)
        max_profit = (max_profit + 0.2) * (max_profit > 0)
        max_profit = np.dot(max_profit.reshape([OUTPUT_DATA_SIZE, 1]), np.ones((1, OUTPUT_INV_LENGTH)))

        predicted = (np.sign(predicted - max_profit) + 1) * max_profit

        investment_mask = np.sign(predicted)
        check_multies = np.sum(investment_mask, axis=1) - 1
        check_multies = check_multies * (check_multies > 0)
        check_multies = np.dot(check_multies.reshape([OUTPUT_DATA_SIZE, 1]), np.ones((1, OUTPUT_INV_LENGTH))) * -1 + 1

        investment_mask = check_multies * investment_mask

        investment_result = y_testing * investment_mask

        if verbose == 1:
            print("max_profit")
            print(max_profit)
            print("predicted")
            print(predicted)
            print("check_multies")
            print(check_multies)
            print("investment_mask")
            print(investment_mask)
            print("investment_result: {0:.2f}".format(np.sum(investment_result)))
            print(investment_result)
        
        return np.sum(investment_result)
