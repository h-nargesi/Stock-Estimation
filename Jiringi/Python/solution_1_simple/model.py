import keras
from codes.model_handlers import ModelHandlers

class Modelling(ModelHandlers):

    NAME = "solution_1_simple"

    def __init__(self, factor) -> None:
        super().__init__(factor)

    def GetModel(input_shape, output_size):
        print("Model: {} -> {}".format(input_shape, output_size))
        DROPOUPT_VALUE_CNN = 0.2
        DROPOUPT_VALUE_FC = 0.7

        model = keras.models.Sequential(name="Jiringi_" + Modelling.NAME)
        model.add(keras.Input(shape=input_shape))

        features = input_shape[2] * 20
        model.add(keras.layers.Conv2D(features, (30, 1), strides=(2, 1), activation='gelu'))
        model.add(keras.layers.Dropout(DROPOUPT_VALUE_CNN))

        features += input_shape[2] * 10
        model.add(keras.layers.Conv2D(features, (15, 1), strides=(3, 1), activation='gelu'))
        model.add(keras.layers.Dropout(DROPOUPT_VALUE_CNN))

        features += input_shape[2] * 10
        model.add(keras.layers.Conv2D(features, (10, 1), strides=(4, 1), activation='gelu'))
        model.add(keras.layers.Dropout(DROPOUPT_VALUE_CNN))

        model.add(keras.layers.Flatten())

        model.add(keras.layers.Dense(100, activation="gelu"))
        model.add(keras.layers.Dropout(DROPOUPT_VALUE_FC))
        model.add(keras.layers.Dense(50, activation="gelu"))
        model.add(keras.layers.Dropout(DROPOUPT_VALUE_FC))
        model.add(keras.layers.Dense(output_size, activation="linear"))

        model.compile(loss='mse', optimizer='adam', metrics=['mae'])

        model.summary()

        return model
