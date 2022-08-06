import sys
import os
sys.path.insert(1, os.path.abspath(os.getcwd()))

from codes.handlers import Handlers
from solution_1.model import Modelling

hd = Handlers(Modelling.NAME)
modeling = Modelling(hd)
model = modeling.GetModel(2)