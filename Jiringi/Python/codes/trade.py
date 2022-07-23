import time
import codes.handlers as hd

def read_trade(input_size, output_size, batch_size, verbose = 1):
    loader = TradeReader(input_size, output_size, batch_size, verbose)
    loader.ReadData()

class TradeReader:

    x_training = list()
    y_training = list()
    file_index = 0
    VERBOSE = 0

    def __init__(self, input_size, output_size, batch_size, verbose = 1):
        self.INPUT_SIZE = input_size
        self.BUFFER_SIZE = input_size + output_size
        self.BATCH_SIZE = batch_size
        self.VERBOSE = verbose

    def ReadData(self):
        hd.SqlQueryExecute('trade-selection', (self.BUFFER_SIZE, ), self.__data_handler)

    def __data_handler(self, cursor):
        instrument = None
        buffer = list()
        
        if self.VERBOSE > 0:
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
            if self.VERBOSE > 0:
                print("\rReading data: {}".format(row[0], self.file_index), end='')

            if len(buffer) < self.BUFFER_SIZE: continue

            self.x_training.append(buffer[:self.INPUT_SIZE])
            self.y_training.append(buffer[self.INPUT_SIZE:])

            buffer.pop()

        self.__save_and_reset()

        if self.VERBOSE > 0:
            print("\nReading finished in {0:.2f} sec and {1} files".format(
                time.time() - dur, self.file_index))
    
    def __save_and_reset(self):
        self.file_index += 1
        if self.VERBOSE > 1:
            print("\nSaving file: {} with size: {}".format(self.file_index, len(self.x_training)))
        hd.SaveFile(self.x_training, 'trade-x-{}'.format(self.file_index))
        hd.SaveFile(self.y_training, 'trade-y-{}'.format(self.file_index))
        self.x_training = list()
        self.y_training = list()
    
    def ReadRaw(self, query_name, parameters):
        hd.SqlQueryExecute(query_name, parameters, self.__graph_handler)
    
    def __graph_handler(self, cursor):
        self.x_training = [row for row in cursor]