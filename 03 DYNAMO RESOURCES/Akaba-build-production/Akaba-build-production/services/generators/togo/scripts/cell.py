import togo
from togo.cellular import *
import random

def random_hallways(count, grid, width=2):
    cells = []
    for idx in range(count):
        while 1:
            x = random.randint(0, grid.size[0] - 1)
            y = random.randint(0, grid.size[0] - 1)
            if grid[(x, y)] == None:
                break
        name = "Hallway-%d" % idx
        cell = OmniCirculationCell(name=name, grid=grid, position=(x, y), radius=2)
        cells.append(cell)
    return cells

def random_rooms(count, grid):
    for idx in range(count):
        while 1:
            x = random.randint(0, grid.size[0] - 1)
            y = random.randint(0, grid.size[0] - 1)
            #x = 18
            #y = 10
            if grid[(x, y)] == None:
                break
        name = "Room-%d" % idx
        OmniSpace(name=name, grid=grid, position=(x, y))

def run(grid, phase, snapshot=False):
    viz = togo.visualization.CellularVisualization(scale=5)
    idx = 0
    filename = "%s_initial.svg" % phase
    viz.render(grid, filename=filename)
    while True:
        idx += 1
        if snapshot:
            filename = "%s_%04d.svg" % (phase, idx)
            viz.render(grid, filename=filename)
        if not grid.step():
            break
    filename = "%s_final.svg" % phase
    viz.render(grid, filename=filename)

size = (200, 200)
grid = Grid(size)
halls = random_hallways(4, grid)
run(grid, "hallways")
random_rooms(20, grid)
run(grid, "rooms", False)
