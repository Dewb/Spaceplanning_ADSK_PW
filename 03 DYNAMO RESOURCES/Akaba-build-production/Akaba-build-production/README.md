# Project Akaba

For more information, visit https://wiki.autodesk.com/display/GEN/Project+Akaba

# Akaba Quick Start

## Deploying on AWS

To deploy your own copy of the Akaba client + services stack on AWS:
- visit http://buildbot.akaba.io (username: akaba, password: buildit)
- click the Builders tab
- scroll down to the custom branch section
- enter a branch name (e.g. `build/testing`) and a hostname (e.g. `mike`)
- hit Force Build

This will use the common Groundhog database and Reach server, and your hostname's services might share containers with other deployments, if there are already running services identical to your branch. If you make changes in a branch, new containers will be created for your host.

## Deploying locally (without Docker)

- Make sure you have the following prerequisites:
 - Node 0.10.35 or later
 - npm 2.1.18 or later
 - Python 2.7
 - bash shell (git-bash on Windows)
- Run `./runlocal.sh`

This will run the adapter service and serve the web client on port 8000, sufficient for testing Dynamo graph generator workflows. The C++ generators are not built and run by this script, because there is no cross-platform package management for the Microsoft C++ REST SDK; if you can build it locally, you can uncomment the C++ services sections from `runlocal.sh`.

## Deploying locally (with Docker)

You can also deploy the *entire* stack on your local machine, including Reach, the Groundhog server, the C++ generator services, and the adaptor and client, using Docker. Docker is a containerization system that runs individual services inside isolated environments that talk to a Linux kernel. If your local host OS is not Linux, Docker will set up a Linux virtual machine to host these containers.

More here: [What is Docker?](https://docs.docker.com/engine/introduction/understanding-docker/)

It's not yet possible to deploy the entire stack with a single script; hopefully it will be soon. In the meantime, you can follow these steps:

1. Install the Docker toolset by following Step One of the Docker getting started guide:
  - [Windows](http://docs.docker.com/windows/step_one/)
  - [Mac OSX](http://docs.docker.com/mac/step_one/)
  - [Linux](http://docs.docker.com/linux/step_one/)
2. Run a Docker Quickstart Terminal if you aren't already in one. Note the IP of your Docker VM (e.g. 192.168.99.100)
- If you want to run local Groundhog and Reach servers, follow these instructions; otherwise you can skip this step. (Note that if you want your local stack to talk to the default AWS Reach deployment, you need to be on the Autodesk network or VPN.)
  a. Change the Akaba source to use your local VM for Groundhog & Reach servers:
    - in `index.html`, change the two instances of `54.172.22.83` to your Docker VM IP.
    - in `services/generators/dynamo/reachModule/Request.js`, change `52.20.23.28` to your Docker VM IP.
    - (This will be automated in the future.)
  b. Follow the Docker setup instructions for the Groundhog server:
    - [Deploying Groundhog locally](https://github.com/AutodeskBIG/Groundhog#deploying-groundhog-locally)
  c. Follow the Docker setup instructions for the Reach server:
    - [Reach and Docker](https://github.com/DynamoDS/Reach#reach-and-docker)
    - Until the .dyn-file endpoint work lands in upstream Reach, you'll need to pull code from [this fork](http://github.com/Dewb/Reach).
    - You'll also need to get `ShellTools.dll` into the Dynamo bin directory, and add the `ShellTools.dll` and `DSIronPython.dll` assemblies to `Reach.Rest.config`. Instructions on how to do this are forthcoming.
3. From your Docker Quickstart Terminal, run `./runlocal_docker.sh`.
  - This will take a while the first time, but subsequent deployments will be much faster.
4. Open the Akaba client in a web browser by going to your Docker VM IP, e.g. `http://192.168.99.100`.
  - If you forgot the IP, you can discover it by running `docker-machine ip default`.
5. Some useful things you can now do from the Docker terminal:
  - `docker ps` will list running containers.
    - The first column of this table is the container ID. Like Git commit IDs, you don't have to type the entire ID string when you want to use it in another command, you just need to type the first few characters until it's unique.
  - `docker logs -f <container id>` will watch the console of a container.
  - `docker restart <container id>` will restart a container.
  - `docker exec -it <container id> /bin/bash` will open a shell prompt inside a container.
  - `docker stop $(docker ps -a -q)` stop all containers (do this before running `./runlocal_docker.sh` again.)
  - If something really bad happens and the Docker host goes down, you can restart it with `docker-machine restart default`. If that doesn't work, try `docker-machine rm default` and reopen the Docker Quickstart Terminal to start over from scratch.

