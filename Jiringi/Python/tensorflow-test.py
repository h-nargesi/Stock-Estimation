from keras.models import load_model
from codes.handlers import Handlers
from solution_1_simple_10.model import Modelling

print()
print("[{}]".format(Modelling.NAME.replace('_', '-').capitalize()))
hd = Handlers(Modelling.NAME)

# Data
print("\n# Data\n")

x_testing, y_testing = hd.LoadTestData(1)

# Loading Models

models = hd.ModelList()
for check_point_path in models:
    print('\n[{}]'.format(check_point_path))
    model = load_model("{}/model/{}".format(Modelling.NAME, check_point_path))

    # Evaluation
    print("\n# Evaluation")
    score = model.evaluate(x_testing, y_testing, verbose=0)
    model.PrintResult(score)

    # Prediction
    print("\n# Prediction")
    predicted = model.predict(x_testing, verbose=0)
    average = model.Prediction(predicted, y_testing)
    model.PrintResult(average)
