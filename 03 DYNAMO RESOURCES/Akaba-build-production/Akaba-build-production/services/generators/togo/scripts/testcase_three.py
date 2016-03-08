import togo
import random

size = (500, 500)

specifications = {
    "site": size,
    "circulation_width": 10,
    "usage": (
        {"name": "office", "percentage": .3, "quantity": 10},
        {"name": "meeting", "percentage": .2, "quantity": 2},
        {"name": "bathroom", "percentage": .1, "quantity": 2},
    ),
    "layout": (
        {"source": "office", "target": "bathroom", "maximum_distance": 20},
        {"source": "office", "target": "meeting", "maximum_distance": 50},
    ),
}

gv = togo.visualization.GraphViz()
opt = togo.Togo(specifications)
layout = togo.layout.Layout()
for room in opt.state.rooms:
    layout.add_node(room)
layout.randomize()
gv.render(layout, "layout")
layout.program_layout()
viz = togo.visualization.Visualization(scale=2)
viz.render(layout.keys(), opt.state.site)
