import pymssql as sql
import numpy as np
import pandas as pd
import _thread as thread
import json
import re

def SaveFile(data, name):
    args = ('data/{}.npy'.format(name), data,)
    thread.start_new_thread(np.save, args)

def LoadFile(name, splits):
    if splits is None:
        return np.load('data/{}.npy'.format(name))
    
    for i in range(i, splits):
        file = 'data/{}-{}.npy'.format(name, i)
        if data is None: data = np.load(file)
        else: data = np.append(data, np.load(file))
    
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