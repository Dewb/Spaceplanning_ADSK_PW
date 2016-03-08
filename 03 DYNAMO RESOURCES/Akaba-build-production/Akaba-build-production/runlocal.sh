#!/bin/bash

# Run the entire Akaba stack locally (except Groundhog and Reach) without Docker or AWS

# build and/or install dependencies for generators

pushd services/generators/dynamo; npm install; popd
#pushd services/generators/lsystem; mkdir build; cd build; cmake ..; make; popd
#pushd services/generators/stuffer; mkdir build; cd build; cmake ..; make; popd
#pushd services/generators/togo; python setup.py; popd

# start dynamo adapter

pushd services/generators/dynamo
node app.js -x -p 34571 &
PIDS[0]=$!
popd

# serve client files

python -m SimpleHTTPServer &
PIDS[1]=$!

# start C++ services

#services/generators/lsystem/build/AkabaGeneratorService -a 0.0.0.0 -p 34569 &
#PIDS[2]=$!
#services/generators/stuffer/build/ShellStufferService -a 0.0.0.0 -p 34570 &
#PIDS[3]=$!

# kill all spawned processes on CTRL-C

trap "kill ${PIDS[*]}"
wait

