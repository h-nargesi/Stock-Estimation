import time
from codes.handlers import Handlers
from codes.file_provider import FileProvider

hd = Handlers('solution_1')
fp = FileProvider(hd, 1024)

length = fp.__len__()
print("length:", length)

dur = time.time()
records_length = 0
for i in range(0, length):
    data = fp.__getitem__(i)
    print('data-{}:'.format(i), data[0].shape, data[1].shape)
    records_length += data[0].shape[0]

for i in range(0, length):
    data = fp.__getitem__(i)
    print('data-{}:'.format(i), data[0].shape, data[1].shape)
    records_length += data[0].shape[0]

if fp.RecordLength == records_length: print('OK')
else: print('FAILED')
print('{} = {} in {} s'.format(fp.RecordLength, records_length, time.time() - dur))