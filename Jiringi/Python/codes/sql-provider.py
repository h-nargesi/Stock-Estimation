import numpy as np
from keras.utils import Sequence
from threading import Thread
from codes.handlers import Handlers as hd

class SqlProvider(Sequence):
    
    TotalCount = 0
    Solution = None
    QueryName = 'query'
    CountingName = '../queries/tools/trade-counting'

    __batches = dict()

    def __init__(self, solution, input_size, output_size, output_gaps, batch_size):
        if input_size <= 0: raise "input_size: out of range"
        if output_size <= 0: raise "output_size: out of range"
        if output_gaps < 0: raise "output_gaps: out of range"
        if batch_size <= 0: raise "batch_size: out of range"

        self.Solution = solution
        self.INPUT_SIZE = input_size
        self.OUTPUT_GAPS = output_gaps
        self.BUFFER_SIZE = input_size + output_size * (output_gaps + 1)
        self.BATCH_SIZE = batch_size
        
        self.__start_reading_data()

    def __len__(self):
        hd.SqlQueryExecute(self.Solution, self.CountingName, (self.BUFFER_SIZE, ), self.__fetch_count)
        if self.TotalCount < 1: raise "Invalid input-set's size"
        return self.TotalCount

    def __fetch_count(self, cursor):
        for row in cursor:
            self.TotalCount = row[1] - self.BUFFER_SIZE + 1
        self.TotalCount = int(np.ceil(self.TotalCount / self.BATCH_SIZE))
    
    def __start_reading_data(self):
        self.__saving_task = Thread(target=hd.SqlQueryExecute, args=(self.Solution, self.QueryName, (self.BUFFER_SIZE, ), self.__data_handler,))
        self.__saving_task.start()
    
    def __getitem__(self, idx):
        self.condition_obj.acquire()

        try:
            while idx not in self.__batches:
                value = self.condition_obj.wait(10)
                if value:
                    break
                else:
                    if self.__saving_task is None or not self.__saving_task.is_alive:
                        self.__start_reading_data()
                        continue
                    else: raise "Fetching data timeout (index: {})".format(idx)

            result = self.__batches.pop(idx)

        finally:
            self.condition_obj.release()

        return result

    def __data_handler(self, cursor):
        instrument = None
        buffer = list()
        x_batch = list()
        y_batch = list()
        
        for row in cursor:
            if instrument != row[1]:
                instrument = row[1]
                buffer.clear()

                if len(x_batch) >= self.BATCH_SIZE:
                    self.__save_and_reset(x_batch, y_batch)
                    x_batch = list()
                    y_batch = list()

            record = list()
            record.append(row[3:])

            buffer.append(record)
            if len(buffer) < self.BUFFER_SIZE: continue

            x_batch.append(buffer[:self.INPUT_SIZE])
            y_batch.append(self.__generate_output(buffer[self.INPUT_SIZE:]))

            buffer.pop(0)

        self.__save_and_reset(x_batch, y_batch)

    def __generate_output(self, data):
        output = list()
        acc = 0.0
        moves = 0
        for d in data:
            acc = acc + d[0][0] - acc * d[0][0]
            
            if moves >= self.OUTPUT_GAPS:
                output.append(acc)
                moves = 0
            else: moves += 1

        if moves > 0: output.append(acc)
        
        return output
    
    def __save_and_reset(self, x_batch, y_batch):
        self.condition_obj.acquire()
        try:
            self.__batches.append((np.array(x_batch), np.array(y_batch), ))
            self.condition_obj.notify()
        finally:
            self.condition_obj.release()   
