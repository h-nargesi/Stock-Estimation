import pymssql

connection = pymssql.connect(
    server='localhost', 
    user='SA', 
    password='s@lm0nElla', 
    database='RahavardNovin3')

cursor = connection.cursor()
cursor.execute('SELECT TOP 3 * FROM trade')

for row in cursor:
    print(row)