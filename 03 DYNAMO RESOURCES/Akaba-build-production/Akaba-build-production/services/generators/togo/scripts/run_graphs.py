#!/usr/bin/env python

import sys
import togo

if len(sys.argv) <= 1:
    fn = "logbook.txt"
else:
    fn = sys.argv[1]

logbook = togo.anneal.AnnealingLogBook()
logbook.load(fn)
graph = togo.visualization.ReportGraph(logbook)
graph.render()

