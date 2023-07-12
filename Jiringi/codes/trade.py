import time
import math
from codes.handlers import Handlers
from threading import Thread

class TradeReader:

    Shapes = None
    Handler: Handlers = None
    TotalCount = 0
    QueryName = 'query'
    CountingName = '../queries/tools/trade-counting'
    Parameters = ()

    __sizes = list()
    __x_training = list()
    __y_training = list()
    __saving_tasks = list()
    __file_index = 0
    __file_state = None
    __finished_state = None

    def __init__(self, handler: Handlers, verbose=3):
        self.Handler = handler
        self.VERBOSE = verbose

        options = handler.LoadOptions()

        self.INPUT_SIZE = options['Input Size']
        self.INPUT_LINE_SIZE = self.INPUT_SIZE[1]
        self.INPUT_SIZE = self.INPUT_SIZE[0]
        self.OUTPUT_GAPS = options['Output Size'][1]
        self.BUFFER_SIZE = self.INPUT_SIZE + options['Output Size'][0] * (self.OUTPUT_GAPS + 1)
        self.BATCH_COUNT = options['Batch Count']
        self.FACTOR = options['Factor']
        self.Parameters = (self.FACTOR, )

    def ReadData(self, ignore_existing = False):

        if self.Handler.DataExist():
            if ignore_existing == True:
                if self.VERBOSE >= 1:
                    print("The data already have read. deleting data ...")
                self.Handler.ClearDataDirectory()
            
            elif ignore_existing == False:
                if self.VERBOSE >= 1:
                    print("The data already have read.")
                return

        if self.VERBOSE >= 1:
            print("Reading data: ...", end='')

        self.Handler.SqlQueryExecute(self.CountingName, (self.BUFFER_SIZE, ), self.__fetch_count)
        self.Handler.SqlQueryExecute(self.QueryName, (self.BUFFER_SIZE, * self.Parameters ), self.__data_handler)
        
        self.__wait_all_tasks_finished()
        print()

    def __fetch_count(self, cursor):
        total_batch_size = 0
        for row in cursor:
            total_batch_size += row[1] - self.BUFFER_SIZE
            self.TotalCount += row[1] - 1
        self.BATCH_SIZE = int(math.ceil(self.TotalCount / self.BATCH_COUNT))
    
    def __data_handler(self, cursor):
        instrument = None
        buffer = list()
        
        dur = time.time()
        for row in cursor:
            if instrument != row[2]:
                instrument = row[2]
                buffer.clear()

                if len(self.__x_training) >= 0 and self.__file_index + 1 < self.BATCH_COUNT and \
                   len(self.__x_training) + row[3] >= self.BATCH_SIZE:
                    self.__save_and_reset()

            record = list()
            s = 4
            depth = int((len(row) - 4) / self.INPUT_LINE_SIZE)
            for _ in range(0, self.INPUT_LINE_SIZE):
                e = s + depth
                record.append(row[s:e])
                s = e

            buffer.append(record)
            if self.VERBOSE >= 2:
                percent = 100.0 * row[0] / self.TotalCount
                message = "\rReading data: {0:.2f}% ".format(percent)
                if self.__file_state is not None: message += self.__file_state
                print(message, end='')

            if len(buffer) < self.BUFFER_SIZE: continue

            self.__x_training.append(buffer[:self.INPUT_SIZE])
            self.__y_training.append(self.__generate_output(buffer[self.INPUT_SIZE:]))

            buffer.pop(0)

        self.__save_and_reset()
        self.__sizes.pop()
        self.Handler.SaveFile("info", self.__sizes)

        if self.VERBOSE >= 1:
            self.__finished_state = "\rReading finished in {0:.2f} sec and {1} files".format(
                time.time() - dur, self.__file_index)
            print("\n" + self.__finished_state, end='')
            if self.__file_state is not None: print(self.__file_state, end='')
    
    def __generate_output(self, data):
        output = list()
        acc = 0.0
        moves = 0
        for d in data:
            spot_change: float = d[0][0] / self.FACTOR
            acc = acc + spot_change - acc * spot_change
            
            if moves >= self.OUTPUT_GAPS:
                output.append(acc * self.FACTOR)
                moves = 0
            else: moves += 1

        if moves > 0: output.append(acc * self.FACTOR)
        
        return output
    
    def __save_and_reset(self):
        self.__file_index += 1
        self.__sizes.append(len(self.__x_training))

        if self.VERBOSE >= 4:
            x_shape = self.__get_shape(self.__x_training)
            y_shape = self.__get_shape(self.__y_training)
            self.__file_state = "\tsaving files ({}): x={}, y={}{}".format(
                self.__file_index, x_shape, y_shape, " " * 10)

        args = [
            { "name": "trade-x-{}".format(self.__file_index), "data": self.__x_training, "key": "x" },
            { "name": "trade-y-{}".format(self.__file_index), "data": self.__y_training, "key": "y" }
        ]
        saving_task = Thread(target=self.__convert_and_save, args=(args, self.__file_index, ))
        self.__saving_tasks.append(saving_task)
        saving_task.start()

        self.__x_training = list()
        self.__y_training = list()
        
    def __convert_and_save(self, files, index):
        loading_shapes = False
        if self.Shapes is None:
            loading_shapes = True
            self.Shapes = dict()

        for file in files:
            file["shape"] = self.Handler.SaveFile(file["name"], file["data"])
            if loading_shapes: self.Shapes[file["key"]] = file["shape"]
        
        if self.VERBOSE >= 3:
            files_info = ""
            for file in files:
                files_info += ", {}={}".format(file["key"], file["shape"])
            if len(files_info) > 0: files_info = files_info[2:]
            self.__file_state = "\tfiles ({}) have been saved: {}{}".format(index, files_info, "" * 2)
    
    def __wait_all_tasks_finished(self):
        leatest_file_state = self.__file_state
        for task in self.__saving_tasks:
            task.join()
            if leatest_file_state != self.__file_state:
                leatest_file_state = self.__file_state
                print(self.__finished_state, end='')
                if self.__file_state is not None: print(self.__file_state, end='')
    
    def __get_shape(slef, data):
        dims = list()
        
        while type(data) == list or type(data) == tuple:
            l = len(data)
            dims.append(l)
            if l > 0: data = data[0]
            else: break

        return tuple(dims)
