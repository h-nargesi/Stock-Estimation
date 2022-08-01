import numpy as np
import codes.handlers as hd
import solution_1_simple.model as modeling

print()
print("[{}]".format(modeling.GetName().replace('_', '-').capitalize()))

# Data
print("\n# Data\n")

x_testing, y_testing = hd.LoadTestData(modeling.GetName(), 1)

# Modeling
print("\n# Modeling\n")
model = modeling.GetModel(x_testing.shape[1:], y_testing.shape[-1])

models = hd.ModelList(modeling.GetName())
for check_point_path in models:
    print('\n[{}]'.format(check_point_path))
    model.load_weights("{}/model/{}".format(modeling.GetName(), check_point_path))

    # Evaluation
    print("\n# Evaluation")
    score = model.evaluate(x_testing, y_testing, verbose=0)
    modeling.PrintResult(score)

    # Prediction
    print("\n# Prediction")
    predicted = model.predict(x_testing, verbose=0)
    predicted = np.sign(np.sign(predicted - 0.02) + 1)
    predicted = np.multiply(predicted, y_testing)
    modeling.PrintResult(np.average(predicted))