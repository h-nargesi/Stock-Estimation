import math
from keras.utils import Sequence
from codes.handlers import Handlers as hd

class FileProvider(Sequence):
    
    Solution = None
    TotalCount = None
    Sizes = None

    __index = -1
    __data = None

    def __init__(self, solution, batch_size):
        if batch_size <= 0: raise "batch_size: out of range"

        self.Solution = solution
        self.BATCH_SIZE = batch_size

    def __len__(self):
        if self.TotalCount is None:
            self.Sizes = hd.LoadFile(self.Solution, "info")

            self.__index = 0

            self.TotalCount = 0
            self.Sizes = self.Sizes.tolist()
            for i in range(0, len(self.Sizes)):
                self.Sizes[i] = int(math.ceil(float(self.Sizes[i]) / self.BATCH_SIZE))
                self.Sizes[i] += self.TotalCount
                self.TotalCount = self.Sizes[i]

        return self.TotalCount
    
    def __getitem__(self, idx):
        idx = self.__findfile__(idx)
        idx *= self.BATCH_SIZE
        end = idx + self.BATCH_SIZE
        if len(self.__x_data) < end: end = len(self.__x_data)
        return (
            self.__x_data[idx:end],
            self.__y_data[idx:end])
        
    def __findfile__(self, idx):
        
        new_data_index = self.__index

        while idx >= self.Sizes[new_data_index] or idx < self.Sizes[new_data_index - 1]:
            new_data_index += 1
            if new_data_index >= len(self.Sizes):
                if idx >= self.Sizes[new_data_index - 1]:
                    raise "index is out of range: {}".format(new_data_index)
                new_data_index = 0
        
        if new_data_index != self.__index:
            self.__index = new_data_index
            self.__loaddata__()
        
        if self.__index == 0: return idx
        else: return idx - self.Sizes[self.__index]

    def __loaddata__(self):
        self.__x_data = hd.LoadFile(self.Solution, "trade-x", 1, self.__index + 1)
        self.__y_data = hd.LoadFile(self.Solution, "trade-y", 1, self.__index + 1)