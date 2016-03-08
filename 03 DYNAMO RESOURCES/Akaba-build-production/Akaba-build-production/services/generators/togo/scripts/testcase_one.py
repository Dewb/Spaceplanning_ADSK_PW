import togo

specifications = {
    "size": (100, 100),
    "rooms": (
        {"name": "bathroom", "size": (10, 10)},
        {"name": "bedroom", "size": (20, 10)},
        {"name": "living room", "size": (15, 20)},
        {"name": "dining room", "size": (20, 20)},
    )
}

opt = togo.Togo(specifications)
opt.run()
