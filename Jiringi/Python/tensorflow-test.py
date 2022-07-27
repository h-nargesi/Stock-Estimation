from keras.callbacks import ModelCheckpoint
import codes.handlers as hd
import codes.trade as trade
import solution_1.model as modeling

# Data
options = hd.LoadOptions(modeling.GetName(), "options.json")
loader = trade.TradeReader(*options, verbose=4)
loader.ReadData(ignore_existing=False)

x_training, y_training, x_testing, y_testing = hd.LoadData(modeling.GetName(), 1)

# Model
model = modeling.GetModel(x_training.shape[1:], y_training.shape[-1])
print("model: ", type(model))

check_point_path = "{}/model".format(modeling.GetName())

if hd.ModelExist(modeling.GetName()):
    model.load_weights(check_point_path)

# Test
score = model.evaluate(x_testing, y_testing, verbose=1)
accuracy = 100 * score[1]
print('Test accuracy: %.4f%%' % accuracy)

# Train
checkpointer = ModelCheckpoint(filepath=check_point_path, verbose=1, save_best_only=True)
checkpointer.load_weights_on_restart = True

hist = model.fit(x_training, y_training, batch_size=1024, epochs=10,
                 validation_split=0.2, callbacks=[checkpointer],
                 verbose=1, shuffle=True)