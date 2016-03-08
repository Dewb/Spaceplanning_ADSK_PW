import togo

specifications = {
    "site": (100, 100),
    "usage": (
        {"name": "bathroom", "percentage": .1, "quantity": 2},
        {"name": "bedroom", "percentage": .3, "quantity": 2},
        {"name": "living", "percentage": .4, "quantity": 3},
    )
}

opt = togo.Togo(specifications)
opt.run()
