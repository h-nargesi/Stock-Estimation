import keras
from codes.model_handlers import ModelHandlers

class Modelling(ModelHandlers):

    NAME = "solution_1"

    def GetModel(input_shape, output_size):
        print("Model: {} -> {}".format(input_shape, output_size))
        DROPOUPT_VALUE_FC = 0.5

        model = keras.models.Sequential(name="Jiringi_" + Modelling.NAME)
        model.add(keras.Input(shape=input_shape))

        features = input_shape[2] * 15
        model.add(keras.layers.Conv2D(features, (5, 1), strides=(1, 1), kernel_initializer='normal', activation='relu'))
        
        features += input_shape[2] * 10
        model.add(keras.layers.Conv2D(features, (19, 1), strides=(2, 1), kernel_initializer='normal', activation='relu'))
        
        features += input_shape[2] * 5
        model.add(keras.layers.Conv2D(features, (46, 1), strides=(4, 1), kernel_initializer='normal', activation='relu'))
        
        model.add(keras.layers.Flatten())

        model.add(keras.layers.Dense(90, kernel_initializer='normal', activation="relu"))
        model.add(keras.layers.Dropout(DROPOUPT_VALUE_FC))

        model.add(keras.layers.Dense(60, kernel_initializer='normal', activation="relu"))
        model.add(keras.layers.Dropout(DROPOUPT_VALUE_FC))

        model.add(keras.layers.Dense(output_size, activation="linear"))

        model.compile(loss='mae', optimizer='adam')

        model.summary()

        return model
