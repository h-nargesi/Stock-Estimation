import pymssql

connection = pymssql.connect(
    server='localhost', 
    user='SA', 
    password='s@lm0nElla', 
    database='RahavardNovin3')

sql_trade = """
select row_number() over(order by DateTimeEn desc) - 1 as Offset
     , DateTimeEn, ClosePrice, RecordType
from Trade where InstrumentID = %d
order by DateTimeEn desc offset %d rows fetch first %d rows only"""
parameters = (13, 0, 100,)

cursor = connection.cursor()
cursor.execute(sql_trade, parameters)

for row in cursor:
    print(row)