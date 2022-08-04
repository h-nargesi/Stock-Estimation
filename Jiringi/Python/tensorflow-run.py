from keras.callbacks import ModelCheckpoint
from codes.handlers import Handlers
from codes.trade import TradeReader
from solution_1_simple_10.model import Modelling

print()
print("[{}]".format(Modelling.NAME.replace('_', '-').capitalize()))
hd = Handlers(Modelling.NAME)

# Data
print("\n# Data\n")
loader = TradeReader(hd, verbose=4)
if hasattr(Modelling, 'TradeReader') and callable(getattr(Modelling, 'TradeReader')):
    Modelling.TradeReader(loader)
loader.ReadData(ignore_existing=False)

x_training, y_training, x_testing, y_testing = hd.LoadData(1)

# Modeling
print("\n# Modeling\n")
modeling = Modelling(hd.LoadOptions()["Factor"])
model = Modelling.GetModel(x_training.shape[1:], y_training.shape[-1])

check_point_name = Handlers.GetStringTime()
check_point_path = "{}/model/{}.hdf5".format(Modelling.NAME, check_point_name)

# Evaluation
print("\n# Evaluation\n")
score = model.evaluate(x_testing, y_testing, verbose=1)
modeling.PrintResult(score)

# Prediction
print("\n# Prediction\n")
predicted = model.predict(x_testing)
average = modeling.Prediction(predicted, y_testing)
modeling.PrintResult(average)

# Training
print("\n# Training\n")
checkpointer = ModelCheckpoint(filepath=check_point_path, verbose=1, save_best_only=True)
checkpointer.load_weights_on_restart = True

hist = model.fit(x_training, y_training, batch_size=1024, epochs=500,
                 validation_split=0.2, callbacks=[checkpointer],
                 verbose=1, shuffle=True)

model.load_weights(check_point_path)

model.save(
    filepath=check_point_path,
    overwrite=True,
    include_optimizer=True,
    save_format=None,
    signatures=None,
    options=None,
    save_traces=True,
)

# Evaluation
print("\n# Evaluation\n")
score = model.evaluate(x_testing, y_testing, verbose=1)
modeling.PrintResult(score)

# Prediction
print("\n# Prediction\n")
predicted = model.predict(x_testing)
average = modeling.Prediction(predicted, y_testing)
modeling.PrintResult(average)
