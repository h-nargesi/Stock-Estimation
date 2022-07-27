import keras

def GetName(): return "solution_1_simple"

def GetModel(input_shape, output_size):
    print("Model: {} -> {}".format(input_shape, output_size))
    DROPOUPT_VALUE = 0.3

    model = keras.models.Sequential(name=GetName())
    model.add(keras.Input(shape=input_shape))

    model.add(keras.layers.Conv2D(10, (20, 1), strides=(1, 1), activation='gelu'))
    model.add(keras.layers.Dropout(DROPOUPT_VALUE))
    model.add(keras.layers.Conv2D(20, (15, 1), strides=(1, 1), activation='gelu'))
    model.add(keras.layers.Dropout(DROPOUPT_VALUE))
    model.add(keras.layers.Conv2D(30, (10, 1), strides=(1, 1), activation='gelu'))
    model.add(keras.layers.Dropout(DROPOUPT_VALUE))
    model.add(keras.layers.Conv2D(40, (5, 1), strides=(1, 1), activation='gelu'))
    model.add(keras.layers.Dropout(DROPOUPT_VALUE))

    model.add(keras.layers.Flatten())

    model.add(keras.layers.Dense(500, activation="gelu"))
    model.add(keras.layers.Dropout(DROPOUPT_VALUE))
    model.add(keras.layers.Dense(250, activation="gelu"))
    model.add(keras.layers.Dropout(DROPOUPT_VALUE))
    model.add(keras.layers.Dense(output_size, activation="linear"))

    model.compile(loss='mean_absolute_error', optimizer='adam', metrics=['accuracy'])

    model.summary()

    return model