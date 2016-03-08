from . graph import *
import graphviz
import colorsys
import hashlib

try:
    from pylab import *
    pylab_available = True
except ImportError:
    print "pylab unavailable, charts produced by matplotlib disabled."
    pylab_available = False

class SVG(object):
    def __init__(self, scale=5):
        self._scale = scale

    def render(self, tags=(), filename=None, *args, **kw):
        tags = [self.render_tag(**tag) for tag in tags]
        svg = str.join('\n', tags)
        svg = '<svg width="100%%" height="100%%" xmlns="http://www.w3.org/2000/svg" xmlns:xlink="http://www.w3.org/1999/xlink">\n%s\n</svg>\n' % svg
        if filename != None:
            f = open(filename, 'w')
            f.write(svg)
        else:
            return svg

    def scale(self, thing):
        if type(thing) in (int, float):
            return thing * self._scale
        return [x * self._scale for x in thing]

    def render_tag(self, tagname=None, attrs=None, style=None, body=None):
        if style:
            style = self.render_style(style)
            attrs.update(style)
        attrs = ['%s="%s"' % kv for kv in attrs.items()]
        attrs = str.join(' ', attrs)
        if body:
            tag = '<%s %s>%s</%s>' % (tagname, attrs, body, tagname)
        else:
            tag = '<%s %s />' % (tagname, attrs)
        return tag

    def render_style(self, style):
        style = ["%s:%s" % kv for kv in style.items()]
        style = str.join(';', style)
        return {"style": style}

class CellularVisualization(SVG):
    def render(self, grid, *args, **kw):
        tags = []
        for (pos, cell) in grid.iter_positions():
            tags.extend(self.draw_cell(pos, cell))
        return super(CellularVisualization, self).render(tags, *args, **kw)

    def draw_cell(self, pos, cell, **kw):
        # body
        (x, y) = self.scale(pos)
        (width, height) = self.scale((1, 1))
        if cell == None:
            color = "black"
        else:
            maxval = 2 ** 32
            hashfunc = hashlib.new("md5")
            hashfunc.update(cell.name)
            hashval = sum([ord(ch) << (idx * 8) for (idx, ch) in enumerate(hashfunc.digest())])
            hue = hashval % maxval
            hue = float(hue) / maxval
            color = [int(0xff * val) for val in colorsys.hsv_to_rgb(hue, 1, 1)]
            color = "rgb(%s)" % str.join(',', map(str, color))
        style = {"fill": color, "stroke-width": 1, "stroke": "white"}
        style.update(kw)
        attrs = {"x": x, "y": y, "width": width, "height": height}
        body = {"tagname": "rect", "attrs": attrs, "style": style}
        return [body]
    
class FloorplanVisualization(SVG):
    def render(self, spaces=None, site=None, *args, **kw):
        tags = []
        if site != None:
            tags.extend(self.draw_space(site, stroke="black", fill="none"))
        if spaces != None:
            for space in spaces:
                tags.extend(self.draw_space(space))
        super(FloorplanVisualization, self).render(tags, *args, **kw)

    def draw_space(self, space, **kw):
        # body
        style = {"fill": "none", "stroke-width": 1, "stroke": "black"}
        style.update(kw)
        (x, y) = self.scale(space.position)
        (width, height) = self.scale(space.size)
        attrs = {"x": x, "y": y, "width": width, "height": height}
        body = {"tagname": "rect", "attrs": attrs, "style": style}
        # label
        ret = [body]
        if not space.name.lower().startswith("circulation"):
            style["font-size"] = "22px"
            (x, y) = self.scale([(offset + sz / 2.0) for (offset, sz) in zip(space.position, space.size)])
            attrs = {"x": x, "y": y, "text-anchor": "middle"}
            msg = space.name
            label = {"tagname": "text", "attrs": attrs, "style": style, "body": msg}
            ret.append(label)
        return ret
    
class ReportGraph(object):
    def __init__(self, logbook):
        self.logbook = logbook
    
    def render(self, fn=None):
        if not pylab_available:
            return
        if fn == None:
            fn = "statistics.png"
        for task in self.logbook:
            self.graph_energy(task, fn)

    def graph_energy(self, task, fn):
        fn = "%s_energy_%s" % (task, fn)
        energy_data = self.logbook[task]["current_energy"]
        accept_data = [accept / float(step) * 100 for (accept, step) in zip(self.logbook[task]["accepts"], self.logbook[task]["current_step"])]
        improve_data = [improve / float(step) * 100 for (improve, step) in zip(self.logbook[task]["improves"], self.logbook[task]["current_step"])]
        temperature_data = self.logbook[task]["current_temperature"]
        clf()
        width = 8
        height = width * 2
        plt.style.use("ggplot")
        fig, axes = plt.subplots(nrows=3, ncols=1, figsize=(width, height))
        temperature = axes[0]
        energy = axes[1]
        rates = axes[2]
        axes[-1].set_xlabel('Step')
        #
        temperature.plot(temperature_data)
        temperature.set_ylabel('Temperature')
        temperature.set_title('Simulated Annealing Temperature')
        temperature.grid(True)
        #
        energy.plot(energy_data)
        energy.set_ylabel('Energy')
        energy.set_title('Simulated Annealing Energy')
        energy.grid(True)
        #
        rates.plot(zip(accept_data, improve_data))
        rates.set_ylabel('Percent')
        rates.set_title('Simulated Annealing Acceptance / Improvement')
        rates.legend(["Acceptance", "Improvement"])
        rates.grid(True)
        #
        savefig(fn)

class GraphViz(object):
    def render(self, graph, filename="graph_dot"):
        directed = isinstance(graph, DirectedGraph)
        dot = graphviz.Graph(format="png")
        visited = set()
        for node in graph:
            dot.node(str(id(node)), label=node.name)
        for node in graph:
            for n_node in graph[node]:
                if directed or (n_node, node) not in visited:
                    dot.edge(str(id(node)), str(id(n_node)), label=str(graph[node][n_node]))
                    visited.add((node, n_node))
        dot.render(filename)
