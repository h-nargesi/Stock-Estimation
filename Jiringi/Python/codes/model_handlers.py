import numpy as np

class ModelHandlers:
    
    def Prediction(predicted, y_testing, factor=1):
        predicted = np.sign(np.sign(predicted - 0.02 * factor) + 1)
        predicted = np.multiply(predicted, y_testing)
        return np.average(predicted)

    def PrintPerentage(suffix, obj, otype, prefix=None, factor=1):
        if type(obj) != list and type(obj) != tuple:
            obj = [obj]
        
        if prefix is None: prefix = ""
        else: prefix = " " + prefix

        text = ""
        for index in range(0, len(obj)):
            text += ", {{{}:{}}}{}".format(index, otype, prefix)
            index += 1

        if factor != 1:
            obj = (np.array(obj) * factor).tolist()
        
        if len(text) > 0:
            print(suffix, text[2:].format(*obj))
