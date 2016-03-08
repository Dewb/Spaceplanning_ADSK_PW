#!/usr/bin/env python

import sys
import os
import shutil

try:
    script = sys.argv[1]
    name = sys.argv[2]
    runs = int(sys.argv[3])
except:
    print "Usage: %s [script path] [run name] [number of runs]" % sys.argv[0]
    sys.exit(-1)

python_exe = "/Users/ghall/code/Akaba/togo/env/bin/python"
if not os.path.exists(python_exe):
    print "pypy not avaialble, using python"
    python_exe = "python"

setup_path = "/Users/ghall/code/Akaba/togo/setup.py"
if os.path.exists(setup_path):
    print "Installing togo"
    cmd = "%s %s install" % (python_exe, setup_path)
    os.system(cmd)
else:
    print "Can't find setup.py, not installing togo"

script = os.path.abspath(script)

bakeoff_root = os.path.join(os.getcwd(), "bakeoff")
bakeoff_path = os.path.join(bakeoff_root, name)

if os.path.exists(bakeoff_path):
    shutil.rmtree(bakeoff_path)
os.mkdir(bakeoff_path)
cwd = os.getcwd()
for run in range(runs):
    root = os.path.join(bakeoff_path, str(run + 1))
    os.mkdir(root)
    os.chdir(root)
    cmd = "%s %s > out.txt 2> error.txt &" % (python_exe, script)
    os.system(cmd)
    os.chdir(cwd)
