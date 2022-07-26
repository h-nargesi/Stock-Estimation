# import tensorflow as tf
import codes.handlers as hd

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