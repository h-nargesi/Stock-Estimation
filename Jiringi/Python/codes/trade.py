import time
import codes.handlers as hd
from threading import Thread

class TradeReader:

    x_training = list()
    y_training = list()
    saving_tasks = list()
    file_index = 0
    message = None
    finished = None

    def __init__(self, input_size, output_size, batch_size, verbose = 2):
        self.INPUT_SIZE = input_size
        self.BUFFER_SIZE = input_size + output_size
        self.BATCH_SIZE = batch_size
        self.VERBOSE = verbose

    def ReadData(self, ignore_existing = False):

        if ignore_existing == True:
            if self.VERBOSE >= 1:
                print("The data already have read. deleting data ...")
            hd.ClearDataDirectory()
        
        elif ignore_existing == False and hd.DataExist():
            if self.VERBOSE >= 1:
                print("The data already have read.")
            return

        hd.SqlQueryExecute('trade-selection', (self.BUFFER_SIZE, ), self.__data_handler)
        
        self.__wait_all_tasks_finished()
        print()

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
                print("\rReading data: {}".format(row[0]), end='')
                if self.message is not None:
                    print("\t{}".format(self.message), end='')

            if len(buffer) < self.BUFFER_SIZE: continue

            self.x_training.append(buffer[:self.INPUT_SIZE])
            self.y_training.append(buffer[self.INPUT_SIZE:])

            buffer.pop()

        self.__save_and_reset()

        if self.VERBOSE >= 1:
            self.finished = "\nReading finished in {0:.2f} sec and {1} files".format(
                time.time() - dur, self.file_index)
            print(self.finished, end='')
            if self.message is not None:
                print("\t{}".format(self.message), end='')
    
    def __save_and_reset(self):
        self.file_index += 1

        if self.VERBOSE >= 4:
            self.message = None
            x_length = len(self.x_training)
            x_depth = len(self.x_training[0])
            y_length = len(self.y_training)
            y_depth = len(self.y_training[0])
            self.message = "saving files ({}): x={}, y={}".format(
                self.file_index, (x_length, x_depth), (y_length, y_depth))

        args = [
            { "name": "trade-x-{}".format(self.file_index), "data": self.x_training, "key": "x" },
            { "name": "trade-y-{}".format(self.file_index), "data": self.y_training, "key": "y" }
        ]
        saving_task = Thread(target=self.__convert_and_save, args=(args, self.file_index, ))
        self.saving_tasks.append(saving_task)
        saving_task.start()

        self.x_training = list()
        self.y_training = list()
        
    def __convert_and_save(self, files, index):
        for file in files:
            file["shape"] = hd.SaveFile(file["name"], file["data"])
        
        if self.VERBOSE >= 3:
            self.message = None
            files_info = ""
            for file in files:
                files_info += ", {}={}".format(file["key"], file["shape"])
            if len(files_info) > 0: files_info = files_info[2:]
            self.message = "files ({}) have been saved: {}".format(index, files_info)
    
    def __wait_all_tasks_finished(self):
        for task in self.saving_tasks:
            task.join()
            print(self.finished, end='')
            if self.message is not None:
                print("\t{}".format(self.message), end='')