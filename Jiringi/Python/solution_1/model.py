import keras

def GetName(): return "solution_1"

def GetModel(input_shape, output_size):
    print("Model: {} -> {}".format(input_shape, output_size))
    DROPOUPT_VALUE_CNN = 0.1
    DROPOUPT_VALUE_FC = 0.7

    model = keras.models.Sequential(name=GetName())
    model.add(keras.Input(shape=input_shape))

    features = input_shape[2] * 10
    model.add(keras.layers.Conv2D(10, (30, 1), strides=(2, 1), activation='gelu'))
    model.add(keras.layers.Dropout(DROPOUPT_VALUE_CNN))
    
    features += input_shape[2] * 10
    model.add(keras.layers.Conv2D(20, (20, 1), strides=(1, 1), activation='gelu'))
    model.add(keras.layers.Dropout(DROPOUPT_VALUE_CNN))
    
    features += input_shape[2] * 10
    model.add(keras.layers.Conv2D(30, (10, 1), strides=(1, 1), activation='gelu'))
    model.add(keras.layers.Dropout(DROPOUPT_VALUE_CNN))
    
    features += input_shape[2] * 10
    model.add(keras.layers.Conv2D(40, (10, 1), strides=(1, 1), activation='gelu'))
    model.add(keras.layers.Dropout(DROPOUPT_VALUE_CNN))
    
    features += input_shape[2] * 10
    model.add(keras.layers.Conv2D(50, (5, 1), strides=(1, 1), activation='gelu'))
    model.add(keras.layers.Dropout(DROPOUPT_VALUE_CNN))

    model.add(keras.layers.Flatten())

    model.add(keras.layers.Dense(240, activation="gelu"))
    model.add(keras.layers.Dropout(DROPOUPT_VALUE_FC))

    model.add(keras.layers.Dense(240, activation="gelu"))
    model.add(keras.layers.Dropout(DROPOUPT_VALUE_FC))
    
    model.add(keras.layers.Dense(240, activation="gelu"))
    model.add(keras.layers.Dropout(DROPOUPT_VALUE_FC))

    model.add(keras.layers.Dense(output_size, activation="linear"))

    model.compile(loss='mean_absolute_error', optimizer='adam', metrics=['accuracy'])

    model.summary()

    return model

def PrintResult(score):
    hd.PrintPerentage('Result:', score, ".3f", "%", 100)