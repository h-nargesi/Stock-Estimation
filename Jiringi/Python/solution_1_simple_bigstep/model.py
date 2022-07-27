import keras

def GetName(): return "solution_1_simple_bigstep"

def GetModel(input_shape, output_size):
    print("Model: {} -> {}".format(input_shape, output_size))
    DROPOUPT_VALUE = 0.66

    model = keras.models.Sequential(name=GetName())
    model.add(keras.Input(shape=input_shape))

    features = input_shape[2] * 20
    model.add(keras.layers.Conv2D(features, (30, 1), strides=(2, 1), activation='gelu'))
    model.add(keras.layers.Dropout(DROPOUPT_VALUE))

    features += input_shape[2] * 10
    model.add(keras.layers.Conv2D(features, (15, 1), strides=(2, 1), activation='gelu'))
    model.add(keras.layers.Dropout(DROPOUPT_VALUE))

    features += input_shape[2] * 5
    model.add(keras.layers.Conv2D(features, (10, 1), strides=(3, 1), activation='gelu'))
    model.add(keras.layers.Dropout(DROPOUPT_VALUE))

    model.add(keras.layers.Flatten())

    model.add(keras.layers.Dense(100, activation="gelu"))
    model.add(keras.layers.Dropout(DROPOUPT_VALUE))
    model.add(keras.layers.Dense(50, activation="gelu"))
    model.add(keras.layers.Dropout(DROPOUPT_VALUE))
    model.add(keras.layers.Dense(output_size, activation="linear"))

    model.compile(loss='mean_absolute_error', optimizer='adam', metrics=['accuracy'])

    model.summary()

    return model