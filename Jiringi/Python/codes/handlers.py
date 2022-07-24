import pymssql as sql
import numpy as np
import pandas as pd
import json
import re

def SaveFile(name, data):
    data = np.array(data)
    np.save('data/{}.npy'.format(name), data)
    return data.shape

def LoadFile(name, splits = None):
    data = None
    
    if splits is None:
        print("Loading {}.npy".format(name), end='')
        data = np.load('data/{}.npy'.format(name))
        print("\r{}.npy was loaded".format(name))
        return data
    
    print("Loading files", end='')
    for i in range(1, splits):
        file = 'data/{}-{}.npy'.format(name, i)
        print("\rLoading {}-{}.npy".format(name, i), end='')
        if data is None: data = np.load(file)
        else: data = np.append(data, np.load(file))
    print("\nLoading finished")
    
    return data

def SqlQueryExecute(file, parameters, job):
    
    query, parameters = GetQuery(file, parameters)

    with GetConnection() as connection:
        with connection.cursor() as cursor:
            cursor.execute(query, parameters)
            job(cursor)

def ReadPanda(file, parameters):
    
    query, parameters = GetQuery(file, parameters)

    with GetConnection() as connection:
        return pd.read_sql(query, connection, params=parameters)

def GetQuery(file, parameters):
    
    with open('queries/{}.sql'.format(file), 'r') as content:
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

    with open('queries/setting.json', 'r') as content:
        info = json.load(content)

    return sql.connect(
        server = info['server'],
        user = info['user'], 
        password = info['password'], 
        database = info['database'])