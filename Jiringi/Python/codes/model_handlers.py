import math
from operator import mod
import numpy as np
import keras
from codes.handlers import Handlers

class ModelHandlers:

    Handler: Handlers = None

    def __init__(self, handler: Handlers):
        self.Handler = handler
    
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
            print("investment_result: sum={0:.2f}, avg={1:.2f}".format(np.sum(investment_result), np.average(investment_result)))
            print(investment_result)
        
        return { "sum": np.sum(investment_result), "average": np.average(investment_result) }

    def PrintModel(model: keras.Sequential):
        print('Model:', model.name, end='')
        print("""
=================================================================================
 Layer                 | Type  | Output Shape          | Params | Activation Func
---------------------------------------------------------------------------------""")

        ModelHandlers.__print_layer(model.input)
        parameters = 0
        for layer in model.layers:
            parameters += ModelHandlers.__print_layer(layer)
        ModelHandlers.__print_model(model, parameters)
        print("""
=================================================================================""")

    def __print_model(model, parameters):
        print("""---------------------------------------------------------------------------------""")
        text = model.loss if type(model.loss) == str else model.loss.__name__
        text = "loss: {}".format(text)
        print(text, end='')
        ModelHandlers.__print_tabs(len(text), 2, 0)

        text = "optimizer: {}".format(model.optimizer._name)
        print(text, end='')
        ModelHandlers.__print_tabs(len(text), 3, 2)

        text = [m if type(m) == str else m.__class__.__name__ for m in model.metrics]
        text = "metrics: {}".format(text)
        print(text)
        print('parameters:', parameters)
    
    def __print_layer(layer):
        parameters = 0
        print(layer.name, end='')
        ModelHandlers.__print_tabs(len(layer.name), 3, 0)
        
        text = "{}".format(layer.__class__.__name__)
        print(text, end='')
        ModelHandlers.__print_tabs(len(text), 1, 3)

        if type(layer) == keras.engine.keras_tensor.KerasTensor:
            print(layer.type_spec.shape)
        else:
            print(layer.output_shape, end='')
            text = "{}".format(layer.output_shape)
            ModelHandlers.__print_tabs(len(text), 3, 4)

            parameters = layer.count_params()
            text = "{}".format(parameters)
            print(text, end='')
            if hasattr(layer, "activation"):
                ModelHandlers.__print_tabs(len(text), 1, 7)
                print(' ', end='')
                print(layer.activation.__name__, end='')
            print()

        print()
        return parameters

    def __print_tabs(length, tabs, orgin):
        if length >= tabs * 8:
            print()
            length = orgin + tabs
        else:
            length = tabs - int(length / 8.0)
        print('\t' * length, end='')