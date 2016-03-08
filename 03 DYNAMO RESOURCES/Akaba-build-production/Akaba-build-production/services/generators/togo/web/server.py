#!/usr/bin/env python

from flask import Flask, make_response, request, url_for
from flask.ext.mako import MakoTemplates
import flask.ext.mako
import mako
from werkzeug.routing import BaseConverter

import pprint
import json
import math
import random
import togo
import os
import re


class RegexConverter(BaseConverter):
    def __init__(self, url_map, *items):
        super(RegexConverter, self).__init__(url_map)
        self.regex = items[0]

ROOT_PATH = os.path.abspath(os.path.split(__file__)[0])

class Config(object):
    DEBUG = True
    TESTING = True
    MAKO_TRANSLATE_EXCEPTIONS = False
    template_folder = os.path.join(ROOT_PATH, "templates")

def bootstrap_server():
    app = Flask("togo")
    config = Config()
    app.config.from_object(config)
    app.template_folder = config.template_folder
    app.bakeoff_path = os.path.join(ROOT_PATH, "..", "bakeoff")
    mako_templates = MakoTemplates(app)
    app.url_map.converters['regex'] = RegexConverter
    globals()["app"] = app
    globals()["mako_templates"] = mako_templates

bootstrap_server()

size = (50, 50)

specification = {
    "site": {
        "size": size,
    },
    "circulation": {
        "width": 10,
        "max_circulation": 4,
    },
    "spaces": (
        {"name": "office", "percentage": .4, "individual_max_square": 25, "quantity": 10},
        {"name": "meeting", "percentage": .2, "quantity": 2},
        {"name": "bathroom", "percentage": .1, "quantity": 2},
    ),
    "layout": (
        {"source": "office", "target": "bathroom", "maximum_distance": 20},
        {"source": "office", "target": "meeting", "maximum_distance": 50},
    ),
}

class ApplicationState(object):
    def __init__(self, specification):
        self.specification = specification
        self.global_specifications = togo.specifications.TogoSpecification(self.specification)
        self.state = togo.state.TogoState(global_specifications=self.global_specifications)
        self.state.initialize()
        self.phase = 0
        self.reset()

    def get_svg(self):
        svg = togo.visualization.CellularVisualization(scale=10)
        svg = svg.render(self.grid)
        return svg

    def step(self):
        change = self.grid.step()
        if not change:
            self.phase += 1
            if self.phase == 1:
                self.state.implant_spaces(self.grid)
            else:
                self.change()

    def change(self):
        self.state.change()
        self.reset()

    def reset(self):
        self.phase = 0
        self.grid = togo.cellular.Grid(self.global_specifications.site.size)
        self.state.implant_circulation(self.grid)

state = ApplicationState(specification)

def render_template(template, **context):
    try:
        return flask.ext.mako.render_template(template, **context)
    except:
        return mako.exceptions.html_error_template().render()

@app.route('/draw')
def draw():
    return render_template("draw.html")

@app.route('/bakeoff/')
def bakeoff():
    context = {
        "bakeoff_path": app.bakeoff_path,
        "last_path": "",
    }
    return render_template("bakeoff.html", **context)

@app.route('/bakeoff/<dirname>/')
def bakeoff_dirname(dirname):
    context = {
        "bakeoff_path": os.path.join(app.bakeoff_path, dirname),
        "last_path": '',
    }
    return render_template("bakeoff.html", **context)

def make_resource_url(path):
    if path.endswith(".svg"):
        path = path[:-4] + ".png"
    url = "resource/%s" % path
    return url

def convert_svg(target):
    source = target[:-4] + ".svg"
    cmd = "rsvg-convert %s -o %s" % (source, target)
    os.system(cmd)

def bakeoff_report_assets(bakeoff_path):
    def sort_fun(a, b):
        return cmp(a[0], b[0])

    globmap = {
        "grid": re.compile("(.+)_grid.svg"),
        "floorplan": re.compile("(.+)_floorplan.svg"),
        "topology": re.compile("(.+)_topology.png"),
        "scores": re.compile("(.+)_scores.txt"),
    }
    graphics = ["grid", "floorplan", "topology", "scores"]
    assets = {}

    for fn in os.listdir(bakeoff_path):
        for key in graphics:
            m = globmap[key].match(fn)
            if m:
                break
        else:
            continue
        url = make_resource_url(fn)
        (_fn, ext) = os.path.splitext(fn)
        fn_parts = _fn.split('_')
        (serial_number, key) = fn_parts
        try:
            serial_number = int(serial_number)
        except ValueError:
            pass
        if key not in graphics:
            continue
        if key not in assets:
            assets[key] = []
        if key == "scores":
            f = open(os.path.join(bakeoff_path, fn))
            scores = f.read()
            scores = eval(scores)
            assets[key].append((serial_number, scores))
        else:
            assets[key].append((serial_number, url))
    for key in assets:
        assets[key].sort(sort_fun)
        print key, [val[0] for val in assets[key]]
        assets[key] = [val[1] for val in assets[key]]
    assets = zip(*[assets[key] for key in graphics])
    return assets
        
@app.route('/bakeoff/<dirname>/<run_number>/resource/<resource>')
def get_resource(dirname, run_number, resource):
    path = os.path.join(app.bakeoff_path, dirname, run_number, resource)
    print path
    if not os.path.exists(path):
        convert_svg(path)
    f = open(path)
    content = f.read()
    response = make_response(content)
    response.content_type = 'image/png'
    return response

@app.route('/bakeoff/<dirname>/<run_number>/')
def bakeoff_report(dirname, run_number):
    bakeoff_path = os.path.join("bakeoff", app.bakeoff_path, dirname, run_number)
    context = {
        "bakeoff_path": bakeoff_path,
        "assets": bakeoff_report_assets(bakeoff_path)
    }
    return render_template("bakeoff_report.html", **context)

@app.route('/change')
def change():
    state.change()
    return draw_svg()

@app.route('/step')
def step():
    state.step()
    return draw_svg()

@app.route('/reset')
def reset():
    state.reset()
    return draw_svg()

@app.route('/grid.svg')
def draw_svg():
    svg = state.get_svg()
    response = make_response(svg)
    response.content_type = 'image/svg+xml'
    return response

if __name__ == "__main__":
    app._static_folder = os.path.join(ROOT_PATH, "static")
    app.run(host="0.0.0.0")
