from codes.handlers import Handlers
from solution_1.model import Modelling

print()
print("[{}]".format(Modelling.NAME.replace('_', '-').capitalize()))
hd = Handlers(Modelling.NAME)

# Data
print("\n# Data\n")

x_testing, y_testing = hd.LoadTestData(1)

# Modeling
print("\n# Modeling\n")
modeling = Modelling(hd.LoadOptions()["Factor"])
model = modeling.GetModel(x_testing.shape[1:], y_testing.shape[-1])

models = hd.ModelList()
for check_point_path in models:
    print('\n[{}]'.format(check_point_path))
    model.load_weights("{}/model/{}".format(check_point_path))

    # Evaluation
    print("\n# Evaluation")
    score = model.evaluate(x_testing, y_testing, verbose=0)
    modeling.PrintResult(score)

    # Prediction
    print("\n# Prediction")
    predicted = model.predict(x_testing, verbose=0)
    average = modeling.Prediction(predicted, y_testing)
    modeling.PrintResult(average)