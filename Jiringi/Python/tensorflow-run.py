from keras.callbacks import ModelCheckpoint
from codes.handlers import Handlers as hd
from codes.trade import TradeReader
from solution_1.model import Modelling as modeling

print()
print("[{}]".format(modeling.NAME.replace('_', '-').capitalize()))

# Data
print("\n# Data\n")
options = hd.LoadOptions(modeling.NAME, "options.json")
loader = TradeReader(*options, verbose=4)
if hasattr(modeling, 'TradeReader') and callable(getattr(modeling, 'TradeReader')):
    modeling.TradeReader(loader)
loader.ReadData(ignore_existing=False)

x_training, y_training, x_testing, y_testing = hd.LoadData(modeling.NAME, 1)

# Modeling
print("\n# Modeling\n")
model = modeling.GetModel(x_training.shape[1:], y_training.shape[-1])

check_point_name = hd.GetStringTime()
check_point_path = "{}/model/{}.hdf5".format(modeling.NAME, check_point_name)

if hd.ModelExist(modeling.NAME, check_point_name):
    model.load_weights(check_point_path)

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

hist = model.fit(x_training, y_training, batch_size=1024, epochs=100,
                 validation_split=0.2, callbacks=[checkpointer],
                 verbose=1, shuffle=True)

# Evaluation
print("\n# Evaluation\n")
score = model.evaluate(x_testing, y_testing, verbose=1)
modeling.PrintResult(score)

# Prediction
print("\n# Prediction\n")
predicted = model.predict(x_testing)
average = modeling.Prediction(predicted, y_testing)
modeling.PrintResult(average)
