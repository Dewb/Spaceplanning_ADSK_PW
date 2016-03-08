import math
from . shapes import *

Phi = (1.0 + math.sqrt(5)) / 2.0

class SpaceActual(Rect):
    Defaults = {
        "name": '',
        "specification": None,
        "serial_number": 0,
    }

    def get_circulatable(self):
        return self.specification.circulatable
    circulatable = property(get_circulatable)

    def get_center(self):
        cx = self.position[0] + self.size[0] / 2.0
        cy = self.position[1] + self.size[1] / 2.0
        return (cx, cy)
    center = property(get_center)

    def is_correct_geometry(self):
        # XXX: hack
        if self.circulatable:
            return True
        (width, height) = self.size
        try:
            if width > height:
                ratio = width / height
            else:
                ratio = height / width
        except ZeroDivisionError:
            return False
        error = abs(Phi - ratio)
        # XXX: threshold
        return error < 1

    def is_correct_size(self):
        # XXX: hack
        if self.circulatable:
            return True
        (width, height) = self.size
        area = width * height
        return area >= self.specification.individual_min_area and area <= self.specification.individual_max_area
