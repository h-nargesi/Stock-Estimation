import math
from tensorflow.python.keras.utils import Sequence
from codes.handlers import Handlers

class FileProvider(Sequence):
    
    TotalBatchCount = None
    IndexRanges = None
    Handler: Handlers = None
    RecordLength = None

    __index = 0
    __x_data = None
    __y_data = None

    def __init__(self, handlers: Handlers, batch_size):
        if batch_size <= 0: raise "batch_size: out of range"

        self.Handler = handlers
        self.BATCH_SIZE = batch_size

    def __len__(self):
        if self.TotalBatchCount is None:
            self.IndexRanges = self.Handler.LoadFile("info")
            self.TotalBatchCount = 0
            self.IndexRanges = self.IndexRanges.tolist()
            prvsize = 0
            self.RecordLength = 0
            for i in range(0, len(self.IndexRanges)):
                self.RecordLength += self.IndexRanges[i]
                self.IndexRanges[i] = int(math.ceil(float(self.IndexRanges[i]) / self.BATCH_SIZE))
                self.TotalBatchCount += self.IndexRanges[i]
                self.IndexRanges[i] = (prvsize, self.TotalBatchCount)
                prvsize = self.TotalBatchCount
            self.__loaddata__()

        return self.TotalBatchCount
    
    def __getitem__(self, idx):
        idx = self.__findfile__(idx)
        idx *= self.BATCH_SIZE
        end = idx + self.BATCH_SIZE
        if len(self.__x_data) < end: end = len(self.__x_data)
        return (
            self.__x_data[idx:end],
            self.__y_data[idx:end])
        
    def __findfile__(self, idx):
        if idx < 0:
            raise "index is out of range: {}".format(new_data_index)
        
        new_data_index = self.__index
        cur_range = self.IndexRanges[new_data_index]

        while idx < cur_range[0] or idx >= cur_range[1]:
            new_data_index += 1
            if new_data_index >= len(self.IndexRanges):
                if idx >= cur_range[1]:
                    raise "index is out of range: {}".format(new_data_index)
                new_data_index = 0
            cur_range = self.IndexRanges[new_data_index]
        
        if new_data_index != self.__index:
            self.__index = new_data_index
            self.__loaddata__()
        
        if self.__index == 0: return idx
        else: return idx - cur_range[0]

    def __loaddata__(self):
        self.__x_data = self.Handler.LoadFile("trade-x", 1, self.__index + 1)
        self.__y_data = self.Handler.LoadFile("trade-y", 1, self.__index + 1)