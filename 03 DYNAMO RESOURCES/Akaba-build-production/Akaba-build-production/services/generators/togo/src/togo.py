from . specifications import *
from . state import *
from . anneal import *
from . visualization import *
from . analysis import *
import math
import random
import pprint
import pickle

class Togo(AnnealingEnvironment):
    def __init__(self, global_specifications, visualization_step=0, anneal_logbook_filename=None):
        self.global_specifications = TogoSpecification(global_specifications)
        self.last_state = None
        self.state = TogoState(global_specifications=self.global_specifications)
        self.state.initialize()
        self.last_state = None
        self.grid = None
        self.analysis = None
        self.iteration = 0
        self.visualization_step = visualization_step
        self.anneal_logbook_filename = anneal_logbook_filename

    def run(self):
        if self.anneal_logbook_filename:
            self.anneal_logbook = AnnealingLogBook(report_cycle=10)
        else:
            self.anneal_logbook = None
        self.annealer = Annealer(self, logbook=self.anneal_logbook)
        self.annealer.temperature_min = .1
        self.annealer.temperature_max = 1000.00
        self.annealer.steps = 1000
        #self.annealer.autoconf(100, steps=20)
        self.annealer.run()
        # statistics report
        if self.anneal_logbook:
            self.logbook.save(self.anneal_logbook_filename)
            graph = ReportGraph(self.anneal_logbook)
            graph.render()
        if self.visualization_step:
            self.render_visualizations(self.grid, self.analysis, "final")
        return self.analysis.spaces

    def render_visualizations(self, grid, analysis, stem=None):
        if stem == None:
            stem = "%04d" % self.iteration
        # cellular grid
        viz = CellularVisualization(scale=10)
        filename = "%s_grid.svg" % stem
        viz.render(grid, filename=filename)
        # floorplan
        viz = FloorplanVisualization()
        filename = "%s_floorplan.svg" % stem
        viz.render(spaces=analysis.spaces.values(), filename=filename)
        # topology
        viz = GraphViz()
        filename = "%s_topology" % stem
        viz.render(analysis.topology, filename=filename)
        # scores
        filename = "%s_scores.txt" % stem
        scores = pprint.pformat(analysis.scores)
        f = open(filename, 'w')
        f.write(scores)
        # pickle
        filename = "%s_state.pickle" % stem
        pf = open(filename, 'wb')
        state = {"state": self.state, "analysis": analysis, "grid": grid}
        pickle.dump(state, pf)

    def run_grid(self):
        self.iteration += 1
        grid = Grid(self.global_specifications.site.size)
        self.state.implant_circulation(grid)
        grid.run()
        self.state.implant_spaces(grid)
        grid.run()
        return grid
    
    def evaluate(self):
        self.grid = self.run_grid()
        self.analysis = GridAnalysis(self.grid)
        score = self.analysis.get_score()
        if self.visualization_step and (self.iteration % self.visualization_step) == 0:
            self.render_visualizations(self.grid, self.analysis)
        return score

    def revert_state(self):
        assert self.last_state != None
        self.state = self.last_state
        self.evaluate()
        self.last_state = None

    def change_state(self):
        self.last_state = self.state.copy()
        self.state.change()
