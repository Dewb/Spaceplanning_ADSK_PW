import math
import random
import copy
import os

def which(program):
    def is_qualified_exe(fpath):
        return len(os.path.split(fpath)[0]) and os.path.isfile(fpath) and os.access(fpath, os.X_OK)
    if is_qualified_exe(program):
        return program
    for path in os.environ["PATH"].split(os.pathsep):
        path = path.strip('"')
        best_guess = os.path.join(path, program)
        if is_qualified_exe(best_guess):
            return best_guess
    return None

def coinflip():
    return bool(random.randint(0, 1))

def euclidean_distance(pos1, pos2):
    (x1, y1) = pos1
    (x2, y2) = pos2
    distance = (x2 - x1) ** 2 + (y1 - y2) ** 2
    return math.sqrt(distance)

class StatefulObject(object):
    Defaults = {}

    def __init__(self, **kw):
        _kw = self.get_defaults()
        _kw.update(kw)
        self.kw = _kw
        for key in self.kw:
            setattr(self, key, self.kw[key])

    @classmethod
    def get_defaults(cls):
        defaults = {}
        for _cls in cls.__mro__:
            defs = getattr(_cls, "Defaults", {})
            defaults.update(defs)
        return defaults
    
    def copy(self):
        return copy.deepcopy(self)

    def __repr__(self):
        keys = self.get_defaults().keys()
        kwstr = str.join(", ", ["%s=%r" % (key, getattr(self, key)) for key in keys])
        msg = "%s(%s)" % (self.__class__.__name__, kwstr)
        return msg

class Point(list):
    def __init__(self, *args, **kw):
        vals = [0, 0]
        vals[0] = kw.get('x', 0)
        vals[1] = kw.get('y', 0)
        if len(args) > 0:
            vals[0] = args[0]
        if len(args) > 1:
            vals[1] = args[1]
        super(Point, self).__init__(vals)

    def norm(self):
        return math.sqrt(self.x ** 2 + self.y ** 2)
    
    def __sub__(self, other):
        if type(other) in (int, float):
            return Point(self[0] - other, self[1] - other)
        else:
            return Point(self[0] - other[0], self[1] - other[1])

    def __add__(self, other):
        if type(other) in (int, float):
            return Point(self[0] + other, self[1] + other)
        else:
            return Point(self[0] + other[0], self[1] + other[1])

    def __mul__(self, other):
        if type(other) in (int, float):
            ret = Point(self[0] * other, self[1] * other)
        else:
            ret = Point(self[0] * other[0], self[1] * other[1])
        return ret

    def __div__(self, other):
        if type(other) in (int, float):
            return Point(self[0] / other, self[1] / other)
        else:
            return Point(self[0] / other[0], self[1] / other[1])

    def __repr__(self):
        msg = "%s(*%s)" % (self.__class__.__name__, super(Point, self).__repr__())
        return msg

    def get_x(self):
        return self[0]
    def set_x(self, x):
        self[0] = x
    x = property(get_x, set_x)

    def get_y(self):
        return self[1]
    def set_y(self, y):
        self[1] = y
    y = property(get_y, set_y)

    def __getitem__(self, *args):
        if len(args) == 1 and type(args[0]) == int:
            return super(Point, self).__getitem__(args[0])
        res = super(Point, self).__getitem__(*args)
        return self.__class__(*res)
    
    def __hash__(self):
        return hash(tuple(self))
