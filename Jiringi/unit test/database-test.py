import pymssql

conn = pymssql.connect("localhost", "sa", "abc.123456", "RahavardNovin3")
cursor = conn.cursor(as_dict=True)

cursor.execute("SELECT 0 as id, 'OK' as name")
for row in cursor:
    print("ID=%d, Name=%s" % (row['id'], row['name']))

conn.close()
