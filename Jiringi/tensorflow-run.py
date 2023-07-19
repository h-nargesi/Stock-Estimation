import os
import keras
from keras.callbacks import ModelCheckpoint
from codes.handlers import Handlers
from codes.trade import TradeReader
from solution_3_withdetails.model import Modelling

print()
print("[{}]".format(Modelling.NAME.replace('_', '-').capitalize()))
hd = Handlers(Modelling.NAME)
hd.LoadOptions()

# Data
print("\n# Data\n")
loader = TradeReader(hd, verbose=4)
if hasattr(Modelling, 'TradeReader') and callable(getattr(Modelling, 'TradeReader')):
    Modelling.TradeReader(loader)
loader.ReadData(ignore_existing=False)

x_training, y_training, x_testing, y_testing = hd.LoadData(1)

# Modeling
print("\n# Modeling\n")

check_point_path = "{}/model/{}.h5".format(Modelling.NAME, Modelling.TITLE)

if os.path.isfile(check_point_path):
    print("loading model\n")
    model = keras.models.load_model(check_point_path)
    # model.load_weights(check_point_path)
else:
    print("model creation\n")
    modeling = Modelling(hd)
    model = modeling.GetModel(x_training.shape[-1])

Modelling.PrintModel(model)

# Evaluation
print("\n# Evaluation\n")
score = model.evaluate(x_testing, y_testing, verbose=1)
hd.PrintResult(score)

# Prediction
print("\n# Prediction\n")
predicted = model.predict(x_testing)
prediction_result = Modelling.Prediction(predicted, y_testing)
hd.PrintResult(prediction_result)

# Training
print("\n# Training\n")
checkpointer = ModelCheckpoint(filepath=check_point_path, verbose=1, save_best_only=True)
checkpointer.load_weights_on_restart = True

hist = model.fit(x_training, y_training, batch_size=1024, epochs=50,
                 validation_split=0.2, callbacks=[checkpointer],
                 verbose=1, shuffle=True)

check_point_path = "{}/model/{}.junk.h5".format(Modelling.NAME, Modelling.TITLE) 
model.save(
    filepath=check_point_path,
    overwrite=True,
    include_optimizer=True,
    save_format=None,
    signatures=None,
    options=None,
    save_traces=True,
)

# reload best model to evaluation and prediction
model.load_weights(check_point_path)

# Evaluation
print("\n# Evaluation\n")
score = model.evaluate(x_testing, y_testing, verbose=1)
hd.PrintResult(score)

# Prediction
print("\n# Prediction\n")
predicted = model.predict(x_testing)
prediction_result = Modelling.Prediction(predicted, y_testing)
hd.PrintResult(prediction_result)
