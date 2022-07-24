from asyncio import tasks
import asyncio
import time
import _thread as thread
import codes.handlers as hd

class TradeReader:

    x_training = list()
    y_training = list()
    file_index = 0
    VERBOSE = 0

    loop = None
    saving_tasks = set()

    def __init__(self, input_size, output_size, batch_size, verbose = 2):
        self.INPUT_SIZE = input_size
        self.BUFFER_SIZE = input_size + output_size
        self.BATCH_SIZE = batch_size
        self.VERBOSE = verbose

    def ReadData(self):
        self.loop = asyncio.new_event_loop()
        hd.SqlQueryExecute('trade-selection', (self.BUFFER_SIZE, ), self.__data_handler)
        for t in self.saving_tasks:
            self.loop.run_until_complete(t)

    def __data_handler(self, cursor):
        instrument = None
        buffer = list()
        
        if self.VERBOSE >= 1:
            print("Reading data: ...", end='')

        dur = time.time()
        for row in cursor:
            if instrument != row[1]:
                instrument = row[1]
                buffer.clear()

                if len(self.x_training) >= 0 and \
                   len(self.x_training) + row[2] >= self.BATCH_SIZE:
                    self.__save_and_reset()

            buffer.append(row[-1])
            if self.VERBOSE >= 2:
                print("\rReading data: {}".format(row[0], self.file_index), end='')

            if len(buffer) < self.BUFFER_SIZE: continue

            self.x_training.append(buffer[:self.INPUT_SIZE])
            self.y_training.append(buffer[self.INPUT_SIZE:])

            buffer.pop()

        self.__save_and_reset()

        if self.VERBOSE >= 1:
            print("\nReading finished in {0:.2f} sec and {1} files".format(
                time.time() - dur, self.file_index))
    
    def __save_and_reset(self):
        self.file_index += 1

        if self.VERBOSE >= 4:
            x_length = len(self.x_training)
            x_depth = len(self.x_training[0])
            y_length = len(self.y_training)
            y_depth = len(self.y_training[0])
            print("\nSaving file: {} with shape: x={}, y={}".format(
                self.file_index, (x_length, x_depth), (y_length, y_depth)))

        args = [
            { "name": "trade-x-{}".format(self.file_index), "data": self.x_training },
            { "name": "trade-y-{}".format(self.file_index), "data": self.y_training }
        ]
        saving_task = tasks.run_coroutine_threadsafe(self.__convert_and_save(args), self.loop)
        self.saving_tasks.add(saving_task)

        self.x_training = list()
        self.y_training = list()
        
    async def __convert_and_save(self, files):
        for file in files:
            file["shape"] = hd.SaveFile(file["name"], file["data"])
        
        if self.VERBOSE >= 3:
            message = ""
            for file in files:
                message += ", {}:{}".format(file["name"], file["shape"])
            if len(message) > 0: message = message[2:]
            print("\nFiles was saved:", message)