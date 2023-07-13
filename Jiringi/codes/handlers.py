import pymssql as sql
import numpy as np
import pandas as pd
import datetime
import json
import re
import os
import shutil

class Handlers:

    SOLUTION = None
    OPTIONS = None
    SHOW_FACTOR = None

    def __init__(self, solution):
        self.SOLUTION = solution

    def PrintResult(self, score, suffix='Result:', otype='.3f', prefix='%'):
        names = None
        if type(score) != list and type(score) != tuple:
            if type(score) != dict: score = [score]
            else:
                temp = list()
                names = list()
                for k, v in score.items():
                    names.append(k)
                    temp.append(v)
                score = temp
        
        if prefix is None: prefix = ""
        else: prefix = " " + prefix

        text = ""
        for index in range(0, len(score)):
            name = names[index] + "=" if names is not None else ""
            text += ", {0}{{{1}:{2}}}{3}".format(name, index, otype, prefix)
            index += 1

        if self.SHOW_FACTOR is not None and self.SHOW_FACTOR != 1:
            score = (np.array(score) * self.SHOW_FACTOR).tolist()
        
        if len(text) > 0:
            print(suffix, text[2:].format(*score))

    def SaveFile(self, name, data):
        os.makedirs('{}/data/'.format(self.SOLUTION), exist_ok=True)
        data = np.array(data)
        np.save('{}/data/{}.npy'.format(self.SOLUTION, name), data)
        return data.shape

    def ClearDataDirectory(self):
        if self.DataExist():
            shutil.rmtree('{}/data/'.format(self.SOLUTION))

    def GetFilesCount(self):
        if not self.DataExist(): return 0

        dir_path = '{}/data/'.format(self.SOLUTION)
        count = 0
        for path in os.listdir(dir_path):
            if os.path.isfile(os.path.join(dir_path, path)):
                count += 1
        return count

    def ModelList(self):
        dir_path = '{}/model/'.format(self.SOLUTION)
        return os.listdir(dir_path)

    def ModelExist(self, name):
        os.makedirs('{}/model/'.format(self.SOLUTION), exist_ok=True)
        return os.path.isfile("{}/model/{}.h5".format(self.SOLUTION, name))

    def DataExist(self):
        return os.path.isdir('{}/data/'.format(self.SOLUTION))

    def LoadData(self, test_count):
        total = int(self.GetFilesCount() / 2)

        if total < 0: raise "Data have not read yet."

        x_training, y_training = self.LoadTrainData(test_count)
        x_testing, y_testing = self.LoadTestData(test_count)

        return (x_training, y_training, x_testing, y_testing)

    def LoadTrainData(self, test_count):
        total = int(self.GetFilesCount() / 2)

        if total < 0: raise "Data have not read yet."

        x_training = self.LoadFile('trade-x', total - test_count)
        y_training = self.LoadFile('trade-y', total - test_count)

        print("training.shapes: x{}, y{}".format(x_training.shape, y_training.shape))

        return (x_training, y_training)

    def LoadTestData(self, test_count):
        total = int(self.GetFilesCount() / 2)

        if total < 0: raise "Data have not read yet."

        x_testing = self.LoadFile('trade-x', test_count, total)
        y_testing = self.LoadFile('trade-y', test_count, total)

        print("testing.shapes: x{}, y{}".format(x_testing.shape, y_testing.shape))

        return (x_testing, y_testing)

    def LoadFile(self, name, pices = None, offset = 1):
        data = None
        
        if pices is None:
            print("Loading {}.npy".format(name), end='')
            data = np.load('{}/data/{}.npy'.format(self.SOLUTION, name))
            print("\r{}.npy was loaded".format(name))
            return data
        
        if offset < 1: offset = 1

        print("Loading files", end='')
        for i in range(offset, pices + offset):
            file = '{}/data/{}-{}.npy'.format(self.SOLUTION, name, i)
            print("\rLoading {}-{}.npy ".format(name, i), end='')
            if data is None: data = np.load(file)
            else:
                temp = np.load(file)
                print("\rAppending: data={}, file={}".format(data.shape, temp.shape), end='')
                data = np.append(data, temp, axis=0)
            print(data.shape, end='')
        print("\rLoading finished, shape =", data.shape if data is not None else "none")
        
        return data

    def SqlQueryExecute(self, file, parameters, job):
        
        path = "{}/{}.sql".format(self.SOLUTION, file)
        query, parameters = Handlers.LoadGuery(path, parameters)

        with Handlers.GetConnection() as connection:
            with connection.cursor() as cursor:
                cursor.execute(query, parameters)
                job(cursor)

    def ReadPanda(self, file, parameters=None):
        
        path = "{}/{}.sql".format(self.SOLUTION, file)
        query, parameters = Handlers.LoadGuery(path, parameters)

        with Handlers.GetConnection() as connection:
            return pd.read_sql(query, connection, params=parameters)

    def LoadOptions(self, file='options.json'):
        if self.OPTIONS is None:
            self.OPTIONS = Handlers.LoadSetting(self.SOLUTION, file)
            print("Options: ", self.OPTIONS)
            self.SHOW_FACTOR = self.OPTIONS['Factor']
        return self.OPTIONS

    def LoadGuery(path, parameters):
        
        with open(path, 'r') as content:
            query = content.read()

        i = 0
        params = { }
        matches = re.finditer('\@(\w+)\s+[\w\(\)]+\s*=\s*', query)
        for m in matches:
            params[m.group(1)] = parameters[i] if i < len(parameters) else None
            i += 1
        
        query = re.sub('(@(\w+)\s+[\w\(\)]+\s*=\s*)[^\r\n,;]*([,;]?)', '\\1%(\\2)d\\3', query)
        
        return (query, params)

    def GetConnection():

        info = Handlers.LoadSetting("queries", "setting.json")

        return sql.connect(
            server = info['server'],
            user = info['user'], 
            password = info['password'], 
            database = info['database'])

    def LoadSetting(solution, file):

        with open("{}/{}".format(solution, file), 'r') as content:
            info = json.load(content)
        
        return info

    def GetStringTime():
        return re.sub("[: ]", "-", str(datetime.datetime.now()))
