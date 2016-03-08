import random
from utils import *
from cellular import *

class AbstractSeed(StatefulObject):
    Defaults = {
        "position": [0, 0],
        "specification": None,
        "global_specifications": None,
        "name": '',
        "specification": None,
        "serial_number": None,
    }
    CellClass = Cell
    CellKeys = ["name", "specification", "serial_number"]

    def random_position(self):
        (width, height) = self.global_specifications.site.size
        x = random.randint(0, width - 1)
        y = random.randint(0, height - 1)
        return [x, y]

    def shift_position(self):
        (width, height) = self.global_specifications.site.size
        vectors = [[-1, 0], [1, 0], [0, -1], [0, 1]]
        while 1:
            vec = random.choice(vectors)
            (x, y) = map(sum, zip(self.position, vec))
            if x < 0 or y < 0 or x >= width or y >= height:
                continue
            break
        return [x, y]

    def initialize(self):
        self.position = self.random_position()

    def change_position(self):
        if coinflip():
            self.position = self.random_position()
        else:
            self.position = self.shift_position()

    def implant(self, grid):
        if grid[self.position] != None:
            return False
        kw = {key: getattr(self, key) for key in self.CellKeys}
        kw["grid"] = grid
        cell = self.CellClass(**kw)
        return True

class SpaceSeed(AbstractSeed):
    Defaults = {
        "name": '',
        "budget": None,
    }
    CellClass = OmniSpace
    CellKeys = ["name", "specification", "serial_number", "budget", "position"]

    def initialize(self):
        super(SpaceSeed, self).initialize()
        self.budget = self.specification.individual_max_area

    def change(self):
        self.change_position()

class CirculationSeed(AbstractSeed):
    Defaults = {
        "circulation_width": 1,
    }
    CellClass = OmniCirculationCell
    CellKeys = ["circulation_width", "name", "specification", "serial_number", "position"]

    def initialize(self):
        super(CirculationSeed, self).initialize()
        self.circulation_width = self.specification.circulation_width

    def change(self):
        self.change_position()

class TogoState(StatefulObject):
    Defaults = {
        "global_specifications": None,
        "space_seeds": [],
        "circulation_seeds": [],
    }

    def initialize(self):
        for space in self.global_specifications.spaces:
            for count in range(space.quantity):
                name = "%s-%s" % (space.name, count)
                seed = SpaceSeed(name=name, specification=space, serial_number=count, global_specifications=self.global_specifications)
                seed.initialize()
                self.space_seeds.append(seed)
        circulation = self.global_specifications.circulation
        circulation_count = (circulation.max_circulation - circulation.min_circulation) / 2 + circulation.min_circulation
        for count in range(circulation_count):
            name = "circulation-%d" % count
            seed = CirculationSeed(name=name, specification=circulation, serial_number=count, global_specifications=self.global_specifications)
            seed.initialize()
            self.circulation_seeds.append(seed)

    def change(self):
        if coinflip():
            # space
            seed = random.choice(self.space_seeds)
        else:
            seed = random.choice(self.circulation_seeds)
        seed.change()

    def implant_circulation(self, grid):
        for seed in self.circulation_seeds:
            seed.implant(grid)

    def implant_spaces(self, grid):
        for seed in self.space_seeds:
            seed.implant(grid)

