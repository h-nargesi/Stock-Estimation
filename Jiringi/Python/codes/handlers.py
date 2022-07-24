import pymssql as sql
import numpy as np
import pandas as pd
import _thread as thread
import re

def SaveFile(data, name):
    args = ('data/{}.npy'.format(name), data,)
    thread.start_new_thread(np.save, args)

def SqlQueryExecute(file, parameters, job):

    with open('queries/{}.sql'.format(file), 'r') as content:
        query = content.read()

    query = re.sub('(@\w+\s+\w+\s*=\s*)[^\r\n,]*(,?)', '\\1%d\\2', query)

    with GetConnection() as connection:
        with connection.cursor() as cursor:
            cursor.execute(query, parameters)
            job(cursor)

def ReadPanda(file, parameters):

    with open('queries/{}.sql'.format(file), 'r') as content:
        query = content.read()

    query = re.sub('(@\w+\s+\w+\s*=\s*)[^\r\n,]*(,?)', '\\1?\\2', query)

    return pd.read_sql(query, GetConnection(), parameters)

def GetConnection():
    return sql.connect(
        server='localhost', 
        user='SA', 
        password='ph0t0n-X', 
        database='RahavardNovin3')