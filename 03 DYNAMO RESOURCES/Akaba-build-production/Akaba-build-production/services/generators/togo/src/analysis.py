from . space import *
from . graph import *

class SpaceLayout(object):
    def __init__(self, cell):
        self.cell = cell
        self.max_pos = (0, 0)
        self.min_pos = (float("inf"), float("inf"))

    def add_position(self, pos):
        self.min_pos = map(min, zip(self.min_pos, pos))
        self.max_pos = map(max, zip(self.max_pos, pos))

    def build_space_actual(self, **kw):
        kw = kw.copy()
        kw["position"] = self.min_pos
        kw["size"] = [maxpos - minpos for (maxpos, minpos) in zip(self.max_pos, self.min_pos)]
        return SpaceActual(**kw)

class TopologyGraph(BidirectedGraph):
    pass

class GridAnalysis(object):
    def __init__(self, grid):
        self.grid = grid
        self.spaces = {}
        self.scores = {}
        self.negative_space_count = 0
        self.topology = TopologyGraph()
        self.generate_spaces()
        self.generate_topology()

    def generate_spaces(self):
        space_map = {}
        for (pos, cell) in self.grid.iter_positions():
            if cell == None:
                self.negative_space_count += 1
                continue
            if cell.name not in space_map:
                space_map[cell.name] = SpaceLayout(cell)
            space_map[cell.name].add_position(pos)
        for (name, space_layout) in space_map.items():
            kw = {"name": name, "specification": space_layout.cell.specification, "serial_number": space_layout.cell.serial_number}
            self.spaces[name] = space_layout.build_space_actual(**kw)

    def generate_topology(self):
        for (pos, cell) in self.grid.iter_positions():
            if cell == None:
                continue
            space = self.spaces[cell.name]
            if space not in self.topology:
                self.topology.add_node(space)
            for ortho_pos in self.grid.neighbors(pos, orthogonal=True):
                neighbor_cell = self.grid[ortho_pos]
                if neighbor_cell == None:
                    continue
                neighbor_space = self.spaces[neighbor_cell.name]
                if neighbor_space == space:
                    continue
                if neighbor_space not in self.topology:
                    self.topology.add_node(neighbor_space)
                if space.circulatable or neighbor_space.circulatable:
                    self.topology.connect(space, neighbor_space)

    def score_topology(self):
        spaces = self.spaces.values()
        score = 0
        total = 0
        for (idx, space_a) in enumerate(spaces[:-1]):
            for space_b in spaces[idx+1:]:
                total += 1
                path = self.topology.path(space_a, space_b)
                if path == None:
                    score += 1
        return score / float(total)

    def score_room_usage(self):
        spaces = self.spaces.values()
        score = 0
        for space in spaces:
            if not space.is_correct_size():
                score += 1
        return score / float(len(spaces))

    def score_missing_spaces(self):
        audit = {}
        spaces = self.spaces.values()
        total = 0
        for space in spaces:
            if space.circulatable:
                continue
            total += 1
            if space.specification not in audit:
                audit[space.specification] = space.specification.quantity
            audit[space.specification] -= 1
        score = sum(audit.values()) / float(total)
        return score

    def score_room_geometry(self):
        spaces = self.spaces.values()
        score = 0
        for space in spaces:
            if not space.is_correct_geometry():
                score += 1
        return score / float(len(spaces))
    
    def negative_space(self):
        total_size = self.grid.size[0] * self.grid.size[1]
        return self.negative_space_count / float(total_size)

    def get_score(self):
        stack = [self.score_topology, self.score_missing_spaces, self.negative_space, self.score_room_usage, self.score_room_geometry]
        stack.reverse()
        total_score = 0
        self.scores["scores"] = []
        for (idx, func) in enumerate(stack):
            raw_score = func()
            scale = 10 ** (idx + 1)
            scaled_score = raw_score * scale
            info = {"name": func.__name__, "raw_score": raw_score, "scaled_score": scaled_score, "scale": scale}
            self.scores["scores"].append(info)
            total_score += scaled_score
        self.scores["total_score"] = total_score
        return total_score
