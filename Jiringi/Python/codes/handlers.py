import pymssql as sql
import numpy as np
import pandas as pd
import datetime
import json
import re
import os
import shutil

class Handlers:

    SOLUTION = ""

    def SaveFile(name, data):
        os.makedirs('{}/data/'.format(Handlers.SOLUTION), exist_ok=True)
        data = np.array(data)
        np.save('{}/data/{}.npy'.format(Handlers.SOLUTION, name), data)
        return data.shape

    def ClearDataDirectory():
        if Handlers.DataExist():
            shutil.rmtree('{}/data/'.format(Handlers.SOLUTION))

    def GetFilesCount():
        if not Handlers.DataExist(): return 0

        dir_path = '{}/data/'.format(Handlers.SOLUTION)
        count = 0
        for path in os.listdir(dir_path):
            if os.path.isfile(os.path.join(dir_path, path)):
                count += 1
        return count

    def ModelList():
        dir_path = '{}/model/'.format(Handlers.SOLUTION)
        return os.listdir(dir_path)

    def ModelExist(name):
        os.makedirs('{}/model/'.format(Handlers.SOLUTION), exist_ok=True)
        return os.path.isfile("{}/model/{}.hdf5".format(Handlers.SOLUTION, name))

    def DataExist():
        return os.path.isdir('{}/data/'.format(Handlers.SOLUTION))

    def LoadData(test_count):
        total = int(Handlers.GetFilesCount() / 2)

        if total < 0: raise "Data have not read yet."

        x_training, y_training = Handlers.LoadTrainData(test_count)
        x_testing, y_testing = Handlers.LoadTestData(test_count)

        return (x_training, y_training, x_testing, y_testing)

    def LoadTrainData(test_count):
        total = int(Handlers.GetFilesCount() / 2)

        if total < 0: raise "Data have not read yet."

        x_training = Handlers.LoadFile('trade-x', total - test_count)
        y_training = Handlers.LoadFile('trade-y', total - test_count)

        print("training.shapes: x{}, y{}".format(x_training.shape, y_training.shape))

        return (x_training, y_training)

    def LoadTestData(test_count):
        total = int(Handlers.GetFilesCount() / 2)

        if total < 0: raise "Data have not read yet."

        x_testing = Handlers.LoadFile('trade-x', test_count, total)
        y_testing = Handlers.LoadFile('trade-y', test_count, total)

        print("testing.shapes: x{}, y{}".format(x_testing.shape, y_testing.shape))

        return (x_testing, y_testing)

    def LoadFile(name, pices = None, offset = 1):
        data = None
        
        if pices is None:
            print("Loading {}.npy".format(name), end='')
            data = np.load('{}/data/{}.npy'.format(Handlers.SOLUTION, name))
            print("\r{}.npy was loaded".format(name))
            return data
        
        if offset < 1: offset = 1

        print("Loading files", end='')
        for i in range(offset, pices + offset):
            file = '{}/data/{}-{}.npy'.format(Handlers.SOLUTION, name, i)
            print("\rLoading {}-{}.npy ".format(name, i), end='')
            if data is None: data = np.load(file)
            else:
                temp = np.load(file)
                print("\rAppending: data={}, file={}".format(data.shape, temp.shape), end='')
                data = np.append(data, temp, axis=0)
            print(data.shape, end='')
        print("\rLoading finished, shape =", data.shape if data is not None else "none")
        
        return data

    def SqlQueryExecute(file, parameters, job):
        
        path = "{}/{}.sql".format(Handlers.SOLUTION, file)
        query, parameters = Handlers.LoadGuery(path, parameters)

        with Handlers.GetConnection() as connection:
            with connection.cursor() as cursor:
                cursor.execute(query, parameters)
                job(cursor)

    def ReadPanda(file, parameters=None):
        
        path = "{}/{}.sql".format(Handlers.SOLUTION, file)
        query, parameters = Handlers.LoadGuery(path, parameters)

        with Handlers.GetConnection() as connection:
            return pd.read_sql(query, connection, params=parameters)

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

    def LoadOptions(file):
        options = Handlers.LoadSetting(file)
        print("Options: ", options)
        return (Handlers.SOLUTION, options["Input Size"], options["Output Size"][0], options["Output Size"][1], options["Batch Count"])

    def LoadSetting(file, solution = None):

        if solution is None: solution = Handlers.SOLUTION

        with open("{}/{}".format(solution, file), 'r') as content:
            info = json.load(content)
        
        return info

    def GetConnection():

        info = Handlers.LoadSetting("setting.json", "queries")

        return sql.connect(
            server = info['server'],
            user = info['user'], 
            password = info['password'], 
            database = info['database'])

    def GetStringTime():
        return re.sub("[: ]", "-", str(datetime.datetime.now()))
