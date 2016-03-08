from utils import *

class Rect(StatefulObject):
    Defaults = {
        "position": [0, 0],
        "size": [1, 1]
    }

    def get_top(self):
        return self.position[1]
    def set_top(self, val):
        self.position[1] = val
    top = property(get_top, set_top)

    def get_bottom(self):
        return self.position[1] + self.size[1]
    def set_bottom(self, val):
        self.position[1] = val - self.size[1]
    bottom = property(get_bottom, set_bottom)

    def get_left(self):
        return self.position[0]
    def set_left(self, val):
        self.position[0] = val
    left = property(get_left, set_left)

    def get_right(self):
        return self.position[0] + self.size[0]
    def set_right(self, val):
        self.position[0] = val - self.size[0]
    right = property(get_right, set_right)

    def get_corners(self):
        bottom_right = tuple([pos + sz for (pos, sz) in zip(self.position, self.size)])
        return (self.position, bottom_right)
    corners = property(get_corners)

    def get_coordinates(self):
        (corner_1, corner_2) = self.corners
        coords = (corner_1, (corner_1[0], corner_2[1]), corner_2, (corner_2[0], corner_1[1]))
        return coords
    coordinates = property(get_coordinates)

    def intersects(self, other):
        for pos in self.coordinates:
            if other.within(pos):
                return True
        for pos in other.coordinates:
            if self.within(pos):
                return True
        return False

    def outside(self, other):
        for pos in other.coordinates:
            if not self.within(pos):
                return True
        return False

    def within(self, pos):
        (x, y) = pos
        return x >= self.left and x <= self.right and y <= self.bottom and y >= self.top
