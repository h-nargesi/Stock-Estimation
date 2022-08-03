import numpy as np

class ModelHandlers:

    FACTOR = None
    SHOW_FACTOR = None

    def __init__(self, factor: int):
        self.FACTOR = factor
        self.SHOW_FACTOR = 100 / factor
    
    def Prediction(self, predicted, y_testing):
        predicted = np.sign(np.sign(predicted - 0.02 * self.SHOW_FACTOR) + 1)
        predicted = np.multiply(predicted, y_testing)
        return np.average(predicted)

    def PrintPerentage(self, score, suffix='Result:', otype='.3f', prefix='%'):
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
