from . utils import *
import itertools

def translate(vector, position):
    return map(sum, zip(vector, position))

class Grid(object):
    def __init__(self, size):
        self.size = size
        (self.width, self.height) = size
        self.grid = [None for idx in range(self.width * self.height)]
        self.cells = []
        self.timestamp = 0

    def to_index(self, pos):
        (x, y) = pos
        return self.width * y + x

    def to_position(self, index):
        y = index / self.width
        x = index - (self.width * y)
        return (x, y)
    
    def step(self):
        changes = False
        self.timestamp += 1
        for cell in self.cells:
            changes |= cell.step()
        return changes

    def run(self):
        while True:
            if not self.step():
                break

    def __getitem__(self, idx):
        if type(idx) != int:
            idx = self.to_index(idx)
        return self.grid[idx]

    def __setitem__(self, idx, val):
        if type(idx) != int:
            idx = self.to_index(idx)
        self.grid[idx] = val

    def is_within(self, pos):
        if pos[0] < 0 or pos[0] >= self.width:
            return False
        if pos[1] < 0 or pos[1] >= self.height:
            return False
        return True

    def implant(self, cell):
        self.cells.append(cell)

    def iter_positions(self):
        for (idx, cell) in enumerate(self.grid):
            pos = self.to_position(idx)
            yield (pos, cell)

    def neighbors(self, position, orthogonal=False):
        if type(position) == int:
            position = self.to_position(position)
        if orthogonal:
            neighbors = [[-1, 0], [0, 1], [1, 0], [0, -1]]
        else:
            dirs = [1, 0, -1]
            neighbors = itertools.product(dirs, dirs)
        for vector in neighbors:
            if vector == (0, 0):
                continue
            pos = translate(vector, position)
            if not self.is_within(pos):
                continue
            yield pos

class Cell(StatefulObject):
    Defaults = {
        "name": '',
        "specification": None,
        "serial_number": 0,
        "grid": None,
        "position": [0, 0],
    }

    def __init__(self, **kw):
        kw["position"] = Point(*kw["position"])
        super(Cell, self).__init__(**kw)
        self.grid.implant(self)
        
    def clone(self, **kw):
        _kw = self.kw.copy()
        _kw.update(kw)
        return self.__class__(**_kw)

    def step(self):
        return False

class OmniCirculationCell(Cell):
    DefaultGrowth = [Point(-1, 0), Point(1, 0), Point(0, -1), Point(0, 1)]
    Defaults = {
        "growth": None,
        "radius": 1,
        "step_idx": 0,
    }

    def __init__(self, **kw):
        super(OmniCirculationCell, self).__init__(**kw)
        self.growth = self.DefaultGrowth[:]
        
        for vector in self.growth:
            while 1:
                margin = vector * self.radius
                margin = self.position + margin
                if self.grid.is_within(margin):
                    break
                self.position = self.position + (vector * -1)

        for x_offset in range(self.radius * 2 + 1):
            x_offset -= self.radius
            for y_offset in range(self.radius * 2 + 1):
                y_offset -= self.radius
                pos = self.position + (x_offset, y_offset)
                self.grid[pos] = self
                
    def step(self):
        changes = False
        self.step_idx += 1
        iteration = self.step_idx + self.radius
        for vector in self.growth[:]:
            pos_offset = vector * iteration + self.position
            along = vector[::-1] * -1
            poslist = []
            for offset in range(self.radius * 2 + 1):
                offset -= self.radius
                newpos = pos_offset + (along * offset)
                if self.grid.is_within(newpos) and self.grid[newpos] == None:
                    poslist.append(newpos)
                else:
                    poslist = None
                    break
            if poslist == None:
                del self.growth[self.growth.index(vector)]
            else:
                changes = True
                for pos in poslist:
                    name = self.name + "_" + str(vector)
                    self.grid[pos] = self.clone(name=name)
        return changes

class OmniSpace(Cell):
    Defaults = {
        "growth": [Point(-1, 0), Point(1, 0), Point(0, -1), Point(0, 1)],
    }

    def __init__(self, **kw):
        super(OmniSpace, self).__init__(**kw)
        self.growth_limit = {vector: 0 for vector in self.growth}
        self.grid[self.position] = self

    def line_iter(self, vector, offset):
        axis = 0 if vector[0] == 0 else 1
        offset_vector = vector * offset
        along_vector = vector[::-1]
        post_one = along_vector
        post_two = post_one * -1
        post_one = post_one * self.growth_limit[post_one]
        post_two = post_two * self.growth_limit[post_two]
        post_one = post_one[axis]
        post_two = post_two[axis]
        post_min = min(post_one, post_two)
        post_max = max(post_one, post_two)
        #print "\t", "range", range(post_min, post_max + 1)
        other_axis = int(not axis)
        for offset in range(post_min, post_max + 1):
            ret = [0, 0]
            ret[axis] = offset
            ret[other_axis] = offset_vector[other_axis]
            ret = Point(*ret)
            yield ret

    def step(self):
        changes = False
        poslist = {vector: [] for vector in self.growth}
        #print
        for vector in self.growth:
            self.growth_limit[vector] += 1
            #print "vector", vector, "position", self.position, "growth_limit", self.growth_limit
            for pos in self.line_iter(vector, self.growth_limit[vector]):
                #print "\t", pos, "->", pos + self.position
                newpos = pos + self.position
                if self.grid.is_within(newpos) and self.grid[newpos] in (None, self):
                    poslist[vector].append(newpos)
                else:
                    poslist[vector] = None
                    self.growth_limit[vector] -= 1
                    break
        #print "growth_limit", self.growth_limit
        for vector in poslist:
            if poslist[vector] != None:
                changes = True
                for pos in poslist[vector]:
                    self.grid[pos] = self
        return changes
