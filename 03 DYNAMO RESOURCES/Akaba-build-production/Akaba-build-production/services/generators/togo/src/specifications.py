from utils import *

class Specification(StatefulObject):
    def normalize(self, global_specifications):
        pass

class CirculationSpecification(Specification):
    Defaults = {
        "min_circulation": 1,
        "max_circulation": None,
        "circulation_width": 1,
        "circulatable": True,
    }

    def normalize(self, global_specifications):
        if self.max_circulation == None:
            self.max_circulation = self.min_circulation

class SpaceSpecification(Specification):
    Defaults = {
        "name": '',
        "total_min_area": None,
        "total_max_area": None,
        "total_area": None,
        "individual_min_area": None,
        "individual_max_area": None,
        "quantity": 1,
        "size": None,
        "circulatable": False,
        "percentage": None,
    }

    def normalize(self, global_specifications):
        (width, height) = global_specifications.site.size
        site_area = width * height
        if self.percentage != None:
            assert self.percentage <= 1
            self.total_area = int(site_area * self.percentage)
        if self.total_area == None:
            if self.individual_min_area != None or self.individual_max_area != None:
                area = self.individual_min_area or self.individual_max_area
                self.total_area = self.quantity * area
            elif self.total_min_area != None or self.total_max_area:
                area = self.total_min_area or self.total_max_area
                self.total_area = area
            else:
                raise RuntimeError, "Space '%s' has no area information" % self.name
        if self.total_min_area == None:
            self.total_min_area = self.total_area
        if self.total_max_area == None:
            self.total_max_area = self.total_area
        if self.individual_min_area == None:
            self.individual_min_area = int(self.total_area / self.quantity)
        if self.individual_max_area == None:
            self.individual_max_area = int(self.total_area / self.quantity)

class LayoutSpecification(Specification):
    Defaults = {
        "endpoints": None,
        "min_length": None,
        "max_length": None,
    }

class SiteSpecification(Specification):
    Defaults = {
        "size": (1, 1),
    }

class TogoSpecification(object):
    def __init__(self, specification):
        # site
        site_spec = specification.get("site", {})
        self.site = SiteSpecification(**site_spec)
        # spaces
        space_specs = specification.get("spaces", [])
        self.spaces = [SpaceSpecification(**spec) for spec in space_specs]
        # layout
        layout_spec = specification.get("layout", [])
        self.layout = [LayoutSpecification(**spec) for spec in layout_spec]
        # circulation
        circulation_spec = specification.get("circulation", {})
        self.circulation = CirculationSpecification(**circulation_spec)
        # normalize
        self.site.normalize(self)
        self.circulation.normalize(self)
        for space in self.spaces:
            space.normalize(self)
        for layout in self.layout:
            layout.normalize(self)
