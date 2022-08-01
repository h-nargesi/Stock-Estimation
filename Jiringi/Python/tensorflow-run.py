from keras.callbacks import ModelCheckpoint
import codes.handlers as hd
import codes.trade as trade
import solution_1_simple.model as modeling

print()
print("[{}]".format(modeling.GetName().replace('_', '-').capitalize()))

# Data
print("\n# Data\n")
options = hd.LoadOptions(modeling.GetName(), "options.json")
loader = trade.TradeReader(*options, verbose=4)
loader.ReadData(ignore_existing=False)

x_training, y_training, x_testing, y_testing = hd.LoadData(modeling.GetName(), 1)

# Modeling
print("\n# Modeling\n")
model = modeling.GetModel(x_training.shape[1:], y_training.shape[-1])

check_point_name = hd.GetStringTime()
check_point_path = "{}/model/{}.hdf5".format(modeling.GetName(), check_point_name)

if hd.ModelExist(modeling.GetName(), check_point_name):
    model.load_weights(check_point_path)

# Evaluation
print("\n# Evaluation\n")
score = model.evaluate(x_testing, y_testing, verbose=1)
hd.PrintPerentage('Test accuracy:', score, ".3f", "%")

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
hd.PrintPerentage('Test accuracy:', score, ".3f", "%")
