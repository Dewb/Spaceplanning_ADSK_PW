from . graph import *
import random

class Topology(BidirectedGraph):
    def random_connect(self):
        nodes = self.keys()
        for node_a in self:
            if len(self[node_a]) >= 4:
                continue
            while True:
                node_b = random.choice(nodes)
                if node_a == node_b:
                    continue
                if len(self[node_b]) >= 4:
                    continue
                if not node_b.circulation:
                    continue
                break
            self.connect(node_a, node_b)
