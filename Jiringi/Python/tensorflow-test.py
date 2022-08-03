import numpy as np
import codes.handlers as hd
from solution_1.model import Modelling as modeling

print()
print("[{}]".format(modeling.NAME.replace('_', '-').capitalize()))

# Data
print("\n# Data\n")

x_testing, y_testing = hd.LoadTestData(modeling.NAME, 1)

# Modeling
print("\n# Modeling\n")
model = modeling.GetModel(x_testing.shape[1:], y_testing.shape[-1])

models = hd.ModelList(modeling.NAME)
for check_point_path in models:
    print('\n[{}]'.format(check_point_path))
    model.load_weights("{}/model/{}".format(modeling.NAME, check_point_path))

    # Evaluation
    print("\n# Evaluation")
    score = model.evaluate(x_testing, y_testing, verbose=0)
    modeling.PrintResult(score)

    # Prediction
    print("\n# Prediction")
    predicted = model.predict(x_testing, verbose=0)
    average = modeling.Prediction(predicted, y_testing)
    modeling.PrintResult(average)