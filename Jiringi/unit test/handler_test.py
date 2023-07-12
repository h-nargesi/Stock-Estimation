import sys
import os
sys.path.insert(1, os.path.abspath(os.getcwd()))

from codes.handlers import Handlers

hd = Handlers('solution_1')

hd.PrintResult({ "name1": 0, "name2": 2 })
