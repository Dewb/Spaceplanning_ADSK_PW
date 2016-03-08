Build instructions
======

Prerequisites:
* CMake version 3.x
* Recent Boost libraries installed where CMake can find them

## OSX:

```
mkdir release
cd release
cmake ..
make
```

## Windows:

TBD. Should be similar, hopefully?


Usage instructions
======

## Command line option overview

```
$ ./AkabaGeneratorService --help
AkabaGeneratorService: REST listener for L-systems generation
Options:
  -p [ --port ] arg       Port to listen on (default 34568)
  -i [ --iterations ] arg Number of L-systems iterations per design
  --one                   Generate one topology string then quit
  -h [ --help ]           Print this message
```

## Running the REST service

```
$ ./AkabaGeneratorService 
Listening for requests at: http://localhost:34568/generator
```

In another terminal, create a job, then request results for that job ID. Default 3 designs per job.
```
$ curl -X POST http://localhost:34568/generator
"1"
$ curl -X GET http://localhost:34568/generator/1
{"designs":[{"spaces":[{"dimensions":{"x":12.5,"y":5,"z":0},"isCirculation":true,"origin":{"x":0,"y":0,"z":0}....
```

See REST API Documentation.md for more.

## Using just the topology generator, no REST service, no space geometry:

```
$ ./AkabaGeneratorService --iterations 5 --one
E[CV][C[-F]+C[-F]+C[+F][-F]P_dS][[C[+F]C_n[+F]P_lS][C[+F]C_n[+F]P_lS]]
```

