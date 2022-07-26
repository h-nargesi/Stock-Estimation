import pymssql as sql
import numpy as np
import pandas as pd
import json
import re
import os
import shutil

def ModelExist(solution):
    return os.path.isdir('{}/model/'.format(solution))

def DataExist(solution):
    return os.path.isdir('{}/data/'.format(solution))

def ClearDataDirectory(solution):
    if DataExist(solution):
        shutil.rmtree('{}/data/'.format(solution))

def SaveFile(solution, name, data):
    os.makedirs('{}/data/'.format(solution), exist_ok=True)
    data = np.array(data)
    np.save('{}/data/{}.npy'.format(solution, name), data)
    return data.shape

def GetFilesCount(solution):
    if not DataExist(solution): return 0

    dir_path = '{}/data/'.format(solution)
    count = 0
    for path in os.listdir(dir_path):
        if os.path.isfile(os.path.join(dir_path, path)):
            count += 1
    return count

def LoadFile(solution, name, pices = None, offset = 1):
    data = None
    
    if pices is None:
        print("Loading {}.npy".format(name), end='')
        data = np.load('{}/data/{}.npy'.format(solution, name))
        print("\r{}.npy was loaded".format(name))
        return data
    
    if offset < 1: offset = 1

    print("Loading files", end='')
    for i in range(offset, pices + offset):
        file = '{}/data/{}-{}.npy'.format(solution, name, i)
        print("\rLoading {}-{}.npy".format(name, i), end='')
        if data is None: data = np.load(file)
        else: data = np.append(data, np.load(file), axis=0)
    print("\nLoading finished")
    
    return data

def LoadData(solution, test_count):
    total = GetFilesCount()

    if total < 0: raise "Data have not read yet."

    x_training = LoadFile(solution, 'trade-x', total - test_count)
    y_training = LoadFile(solution, 'trade-y', total - test_count)

    x_testing = LoadFile(solution, 'trade-x', test_count, total)
    y_testing = LoadFile(solution, 'trade-y', test_count, total)

    print("x_training.shape:", x_training.shape)
    print("y_training.shape:", y_training.shape)

    print("x_testing.shape:", x_testing.shape)
    print("y_testing.shape:", y_testing.shape)

    return (x_training, y_training, x_testing, y_testing)

def SqlQueryExecute(solution, file, parameters, job):
    
    query, parameters = GetQuery(solution, file, parameters)

    with GetConnection() as connection:
        with connection.cursor() as cursor:
            cursor.execute(query, parameters)
            job(cursor)

def ReadPanda(file, parameters):
    
    query, parameters = GetQuery("queries", file, parameters)

    with GetConnection() as connection:
        return pd.read_sql(query, connection, params=parameters)

def GetQuery(solution, file, parameters):
    
    with open('{}/{}.sql'.format(solution, file), 'r') as content:
        query = content.read()

    i = 0
    params = { }
    matches = re.finditer('\@(\w+)\s+\w+\s*=\s*', query)
    for m in matches:
        params[m.group(1)] = parameters[i]
        i += 1
    
    query = re.sub('(@(\w+)\s+\w+\s*=\s*)[^\r\n,]*(,?)', '\\1%(\\2)d\\3', query)
    
    return (query, params)

def GetConnection():

    info = LoadSetting("queries", "setting.json")

    return sql.connect(
        server = info['server'],
        user = info['user'], 
        password = info['password'], 
        database = info['database'])

def LoadOptions(solution, file):
    options = LoadSetting(solution, file)
    print("Options: ", options)
    return (solution, options["Input Size"], options["Output Size"][0], options["Output Size"][1], options["Batch Count"])

def LoadSetting(solution, file):

    with open("{}/{}".format(solution, file), 'r') as content:
        info = json.load(content)
    
    return info
