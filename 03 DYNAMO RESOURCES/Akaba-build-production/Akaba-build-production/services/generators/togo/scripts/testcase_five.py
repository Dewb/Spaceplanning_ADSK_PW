#!/usr/bin/env python

import togo
import random

size = (100, 100)

specifications = {
    "site": {
        "size": size,
    },
    "circulation": {
        "width": 10,
        "max_circulation": 4,
    },
    "spaces": (
        {"name": "office", "percentage": .4, "quantity": 20},
        {"name": "meeting", "percentage": .2, "quantity": 10},
        {"name": "bathroom", "percentage": .1, "quantity": 2},
    ),
    "layout": (
        {"source": "office", "target": "bathroom", "maximum_distance": 20},
        {"source": "office", "target": "meeting", "maximum_distance": 50},
    ),
}

opt = togo.Togo(specifications)
opt.run()
