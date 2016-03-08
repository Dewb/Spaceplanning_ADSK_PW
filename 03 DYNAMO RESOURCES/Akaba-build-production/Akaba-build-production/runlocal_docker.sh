#!/bin/bash

# Deploy the entire Akaba stack locally with Docker (except Groundhog and Reach, TBD)

docker build -t togo       services/generators/togo
docker build -t lsystem    services/generators/lsystem
docker build -t stuffer    services/generators/stuffer
docker build -t dynamo     services/generators/dynamo
docker build -t client-www .

docker run -d -p 34568:34568 -t togo
docker run -d -p 34569:34569 -t lsystem
docker run -d -p 34570:34570 -t stuffer
docker run -d -p 34571:34571 -t dynamo
docker run -d -p 80:80       -t client-www

echo "All containers started. Use the following command to stop them:"
echo "docker stop \$(docker ps -a -q)"