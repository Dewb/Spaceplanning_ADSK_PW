#!/usr/bin/env python
# Copyright (c) 2009, Richard J. Wagner <wagnerr@umich.edu>
# modified by Giles Hall, Autodesk

import math
import time
import random
import json
import logging as logger

def round_figures(x, n):
    # Returns x rounded to n significant figures.
    return round(x, int(n - math.ceil(math.log10(abs(x)))))

class AnnealingLogBook(dict):
    def __init__(self, report_cycle=None):
        self.report_cycle = report_cycle
        super(AnnealingLogBook, self).__init__()

    def __call__(self, task="default", **kw):
        if task not in self:
            self[task] = {}
        for (key, val) in kw.items():
            if key not in self[task]:
                self[task][key] = []
            self[task][key].append(val)
        if self.report_cycle != None and (kw["current_step"] % self.report_cycle) == 0:
            self.report(task=task, **kw)

    def save(self, fn=None):
        if fn == None:
            fn = "logbook.txt"
        f = open(fn, 'w')
        f.write(json.dumps(self))

    def load(self, fn=None):
        if fn == None:
            fn = "logbook.txt"
        f = open(fn)
        super(AnnealingLogBook, self).__init__(json.loads(f.read()))

    def report(self, **kw):
        macros = kw.copy()
        macros["acceptance"] = kw["accepts"] / float(kw["current_step"]) * 100
        macros["improvement"] = kw["improves"] / float(kw["current_step"]) * 100
        msg = "(%(task)s) Temperature: %(current_temperature).2f, Energy: %(current_energy).2f, Accept: %(acceptance).2f%%, Improve: %(improvement).2f%%" % macros
        print msg
        
class AnnealingEnvironment(object):
    def copy_state(self):
        pass

    def set_state(self, state):
        pass

    def change_state(self):
        pass

    def revert_state(self):
        pass

    def evaluate(self):
        pass

class Annealer(object):
    Defaults = {
        "logbook": None,
        "temperature_min": 1,
        "temperature_max": 10,
        "steps": 100,
    }

    def __init__(self, environment, **kw):
        _kw = self.Defaults.copy()
        _kw.update(kw)
        self.logbook = _kw["logbook"]
        self.temperature_min = _kw["temperature_min"]
        self.temperature_max = _kw["temperature_max"]
        self.steps = _kw["steps"]
        self.environment = environment
        # reality checks
        if self.temperature_min <= 0.0:
            raise ValueError, "Exponential cooling requires a minimum temperature greater than zero."

    def get_acceptance(self):
        return self.accepts / float(self.current_step)
    acceptance = property(get_acceptance)

    def get_improvement(self):
        return self.improves / float(self.current_step)
    improvement = property(get_improvement)

    def get_current_temperature(self):
        return self.temperature_max * math.exp(self.temperature_coefficient * self.current_step / self.steps)
    current_temperature = property(get_current_temperature)

    def set_constant_temperature(self, val):
        # used to fix a temperature
        self.temperature_min = self.temperature_max = val

    def get_runtime(self):
        now = time.time()
        return now - self.timestamp_start
    runtime = property(get_runtime)

    def log_data(self, task):
        if self.logbook == None:
            return
        info = {
            "task": task,
            "current_energy": self.current_energy,
            "current_temperature": self.current_temperature,
            "accepts": self.accepts,
            "improves": self.improves,
            "current_step": self.current_step,
            "runtime": self.runtime,
        }
        self.logbook(**info)
        
    def step(self):
        self.current_step += 1
        self.environment.change_state()
        self.current_energy = self.environment.evaluate()
        delta_energy = self.current_energy - self.previous_energy
        if delta_energy > 0.0 and math.exp(-delta_energy / self.current_temperature) < random.random():
            # Restore previous state
            self.environment.revert_state()
            self.current_energy = self.previous_energy
        else:
            # Accept new state and compare to best state
            self.accepts += 1
            if delta_energy < 0.0:
                self.improves += 1
            self.previous_energy = self.current_energy
            if self.current_energy < self.best_energy:
                self.best_state = self.environment.copy_state()
                self.best_energy = self.current_energy

    def reset(self):
        self.current_step = 0
        self.current_energy = self.environment.evaluate()
        self.previous_energy = self.current_energy
        self.best_state = self.environment.copy_state()
        self.best_energy = self.current_energy
        self.accepts = 0
        self.improves = 0
        self.temperature_coefficient = -math.log(float(self.temperature_max) / self.temperature_min)

    def run(self, task=None):
        if task == None:
            task = "Anneal"
        self.reset()
        self.timestamp_start = time.time()
        while self.current_step < self.steps:
            self.step()
            self.log_data(task)
        self.timestamp_stop = time.time()
        # Return best state and energy
        self.environment.set_state(self.best_state)
        return self.best_energy
        
    def autoconf(self, seconds, steps=100):
        # original state
        original_state = self.environment.copy_state()
        step = 0
        start = time.time()

        # Find an initial guess for temperature
        initial_temperature = 0.0
        initial_energy = self.environment.evaluate()
        self.task = "Exploration"
        while initial_temperature == 0.0:
            step += 1
            self.environment.change_state()
            initial_temperature = abs(self.environment.evaluate() - initial_energy)
        msg = "(autoconf) Initial tempature %.2f configured in %d steps" % (initial_temperature, step)
        
        # Search for temperature_max - a temperature that gives 98% acceptance
        self.set_constant_temperature(initial_temperature)
        self.steps = steps
        self.run(task="Exploration")
        step += steps
        while self.acceptance > 0.98:
            self.set_constant_temperature(round_figures(self.current_temperature / 1.5, 2))
            self.run(task="Exploration")
            step += steps
        while self.acceptance < 0.98:
            self.set_constant_temperature(round_figures(self.current_temperature * 1.5, 2))
            self.run(task="Exploration")
            step += steps
        temperature_max = self.current_temperature
        msg = "(autoconf) Maximum temperture %.2f configured in %d steps" % (temperature_max, step)
        print msg
        
        # Search for temperature_min - a temperature that gives 0% improvement
        while self.improvement > 0.0:
            self.set_constant_temperature(round_figures(self.current_temperature / 1.5, 2))
            self.run(task="Exploration")
            step += steps
        temperature_min = self.current_temperature
        msg = "(autoconf) Minimum temperture %.2f configured in %d steps" % (temperature_min, step)
        print msg
        
        # Calculate anneal duration
        elapsed = time.time() - start
        steps = int(seconds / (elapsed / float(step)))
        # run the anneal
        self.environment.set_state(original_state)
        self.temperature_max = temperature_max
        self.temperature_min = temperature_min
        self.steps = steps
