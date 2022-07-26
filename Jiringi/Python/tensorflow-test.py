# import tensorflow as tf
import codes.handlers as hd
import keras
from keras.callbacks import ModelCheckpoint

x_training = hd.LoadFile('trade-x', 4)
y_training = hd.LoadFile('trade-y', 4)

x_testing = hd.LoadFile('trade-x', 1, 5)
y_testing = hd.LoadFile('trade-y', 1, 5)

print("x_training.shape:", x_training.shape)
print("y_training.shape:", y_training.shape)

print("x_testing.shape:", x_testing.shape)
print("y_testing.shape:", y_testing.shape)

# # Create TensorFlow object called hello_constant
# hello_constant = tf.constant('Hello World!')

# with tf.Session() as sess:
#     # Run the tf.constant operation in the session
#     output = sess.run(hello_constant)
#     print(output)

INPUT_SIZE = 300
OUTPUT_SIZE = 10
DROPOUPT_VALUE = 0.2

model = keras.models.Sequential(name='logits')
model.add(keras.Input(shape=(INPUT_SIZE, 1, 1)))

model.add(keras.layers.Conv2D(3, (10, 1), strides=(2, 1), activation='gelu'))
model.add(keras.layers.Dropout(DROPOUPT_VALUE))
model.add(keras.layers.Conv2D(6, (10, 1), strides=(1, 1), activation='gelu'))
model.add(keras.layers.Dropout(DROPOUPT_VALUE))
model.add(keras.layers.Conv2D(9, (5, 1), strides=(1, 1), activation='gelu'))
model.add(keras.layers.Dropout(DROPOUPT_VALUE))

model.add(keras.layers.Flatten())

model.add(keras.layers.Dense(60, activation="gelu"))
model.add(keras.layers.Dropout(DROPOUPT_VALUE))
model.add(keras.layers.Dense(OUTPUT_SIZE, activation="sigmoid"))

model.compile(loss='categorical_crossentropy', optimizer='adam', 
              metrics=['accuracy'])

model.summary()

check_point_path = 'models/trade-ver-1'

# train the model
checkpointer = ModelCheckpoint(filepath=check_point_path, 
                               verbose=1, save_best_only=True)

hist = model.fit(x_training, y_training, batch_size=512, epochs=10,
                 validation_split=0.2, callbacks=[checkpointer],
                 verbose=1, shuffle=True)