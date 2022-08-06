import keras
from codes.model_handlers import ModelHandlers
from codes.handlers import Handlers

class Modelling(ModelHandlers):

    NAME = "solution_1"
    TITLE = "mse_downside_3cnn"

    def __init__(self, handler: Handlers) -> None:
        super().__init__(handler)

    def GetModel(self, input_depth=1):
        
        DROPOUPT_VALUE_FC = 0.5
        options = self.Handler.LoadOptions()
        input_size = options["Input Size"]
        output_size = options["Output Size"]
        input_shape = (input_size, 1, input_depth)
        print("Model: {} -> {}".format(input_shape, output_size[0]))

        leayer_count = 1
        model = keras.models.Sequential(name="jiringi_{}_{}".format(Modelling.NAME, Modelling.TITLE))
        model.add(keras.Input(shape=input_shape, name='{}/input/{}/factor:{}'.format(leayer_count, input_size, options["Factor"])))
        
        leayer_count += 1
        features = input_shape[2] * 15
        model.add(keras.layers.Conv2D(features, (5, 1), strides=(1, 1), kernel_initializer='normal', activation='relu', name='{}/normal_kinit'.format(leayer_count)))
        
        leayer_count += 1
        features += input_shape[2] * 10
        model.add(keras.layers.Conv2D(features, (19, 1), strides=(2, 1), kernel_initializer='normal', activation='relu', name='{}/normal_kinit'.format(leayer_count)))
        
        leayer_count += 1
        features += input_shape[2] * 5
        model.add(keras.layers.Conv2D(features, (46, 1), strides=(4, 1), kernel_initializer='normal', activation='relu', name='{}/normal_kinit'.format(leayer_count)))
        
        model.add(keras.layers.Flatten())

        leayer_count += 1
        model.add(keras.layers.Dense(90, kernel_initializer='normal', activation="relu", name='{}/normal_kinit'.format(leayer_count)))
        model.add(keras.layers.Dropout(DROPOUPT_VALUE_FC))

        leayer_count += 1
        model.add(keras.layers.Dense(60, kernel_initializer='normal', activation="relu", name='{}/normal_kinit'.format(leayer_count)))
        model.add(keras.layers.Dropout(DROPOUPT_VALUE_FC))

        leayer_count += 1
        model.add(keras.layers.Dense(output_size[0], activation="linear", name='{}/output/{}-{}'.format(leayer_count, output_size[0], output_size[1])))

        model.compile(loss='mse', optimizer='adam', metrics=['mae'])

        ModelHandlers.PrintModel(model)

        return model
