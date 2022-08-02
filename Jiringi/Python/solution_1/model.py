import keras
import codes.handlers as hd

def GetName(): return "solution_1"

def GetModel(input_shape, output_size):
    print("Model: {} -> {}".format(input_shape, output_size))
    DROPOUPT_VALUE_CNN = 0.1
    DROPOUPT_VALUE_FC = 0.7

    model = keras.models.Sequential(name=GetName())
    model.add(keras.Input(shape=input_shape))

    features = input_shape[2] * 20
    model.add(keras.layers.Conv2D(features, (40, 1), strides=(5, 1), activation='relu'))
    model.add(keras.layers.Dropout(DROPOUPT_VALUE_CNN))
    
    features += input_shape[2] * 20
    model.add(keras.layers.Conv2D(features, (10, 1), strides=(2, 1), activation='relu'))
    model.add(keras.layers.Dropout(DROPOUPT_VALUE_CNN))

    model.add(keras.layers.Flatten())

    model.add(keras.layers.Dense(240, activation="relu"))
    model.add(keras.layers.Dropout(DROPOUPT_VALUE_FC))

    model.add(keras.layers.Dense(240, activation="relu"))
    model.add(keras.layers.Dropout(DROPOUPT_VALUE_FC))

    model.add(keras.layers.Dense(output_size, activation="linear"))

    model.compile(loss='mse', optimizer='adam', metrics=['mae'])

    model.summary()

    return model

def PrintResult(score):
    hd.PrintPerentage('Result:', score, ".4f", "%", 100)