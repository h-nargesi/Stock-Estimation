import pymssql
import sqlalchemy
import pandas as pd

conn = pymssql.connect("localhost", "sa", "abc.123456", "RahavardNovin3")
cursor = conn.cursor(as_dict=True)

cursor.execute("SELECT 0 as id, 'OK' as name")
for row in cursor:
    print("ID=%d, Name=%s" % (row['id'], row['name']))

conn.close()

con_string = "mssql+pymssql://{user}:{password}@{server}/{database}".format(**{
    "server": 'localhost',
    "database": 'RahavardNovin3',
    "user": 'sa',
    "password": 'abc.123456'
})

engine = sqlalchemy.create_engine(con_string)

with engine.begin() as conn:
    df = pd.read_sql_query("SELECT 0 as id, 'OK' as name", conn)
