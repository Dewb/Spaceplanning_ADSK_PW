# Testing the Reach Set Up
This tool is for testing a Reach set up and is also an example on how to make requests to Reach.

## Preparation

### Installing Node
- Make sure you have [Node](http://nodejs.org) installed.
- After cloning this, run `$ npm install request minimist` in this directory.

### Updating the set up
- Edit the IP address in the `api` field for `test.js`. You may need to edit the port as well, depending on the Reach set up.
- Currently the Akaba team has been using 52.20.23.28

### Adding Files

If you wish to add a graph to be run against Reach in the tests, add an object to `files` in [test.js](./test.js).

The object should be set up with the following strategy:
```
{
  name: [A brief description of the graph]
  path: [Path to the JSON graph]
  expected: [Expected data if there is a node named Akaba.Result in the graph]
  hint: [(Optional) Description of what generally causes issues with this graph]
}
```

For the expected result, it will be compared agains the data in the node in the graph that has its `displayName` set to `Akaba.Result`.

To know if your JSON graph is properly set up, see the [Dynamo to JSON for Reach](# Dynamo to JSON for Reach) guide.



## Usage
To run a predefined set of graphs that test some of the most common issues the Akaba team faced when setting up Reach, run in this directory:

```$ node test.js```

To run a specific graph against Reach, run

```$ node run.js [-v] path/to/graph.json```

The graphs *must* be JSON. See [here](#Dynamo to JSON for Reach) on what a proper JSON graph should look like.

If you get the "ECONNREFUSED" error, it means that the Reach service is not running at the given ip address. If you are using the default one and it is not working, contact me at pedro.silva@autodesk.com





# Dynamo to JSON for Reach
---
The JSON for a Home graph will include the following keys:
```javascript
{
  "$type": "System.Dynamic.ExpandoObject, System.Core",
  "guid": "00000000-0000-0000-0000-000000000000",
  "name": "Home",
  "nodes": [Array of nodes],
  "connections":[Array of connections],
  "isCustomNode": false,
  "customNodeDescription": "",
  "customNodeCategory": "",
  "offset": [
    0.0,
    0.0
  ],
  "isCustomizer": true
}
```
Most of these are editable, however the `guid` and `name` fields must be the same as the example. The script will fix it if you input a graph any different.

### Node representation

Here is an example of the representation of a Node object in JSON, equivalent to a node in Dynamo:

```javascript
{
  "$type": "System.Dynamic.ExpandoObject, System.Core",
  "_id": "bf59c3e9-0a75-4cda-bc1e-2a009a5899bc",
  "name": "DefaultNodeName",
  "position": [
    618.48167539267,
    676.282722513089
  ],
  "typeName": "Number",
  "creationName": "Number",
  "displayName": "Akaba.Result",
  "selected": false,
  "visible": true,
  "ignoreDefaults": [],
  "replication": "applyDisabled",
  "extra": {
    "$type": "<>f__AnonymousType1`1[[System.String, mscorlib]], Reach",
    "value": "42"
  }
},
```
Note the following:
- `_id` must be a string of a valid GUID
- `position` is a two-number array for doubles representing the x and y positioning of the node on the graph
- `typeName` and `creationName` are used by Reach for the computation. The name that is editable by the user is `displayName`. Changing `displayname` is equivalent to changing the name of a Dynamo node in the desktop app.
- `ignoreDefaults` should be as long as the number of inputs that node takes. Some nodes will not run correctly if one of the elements is set to `true`, as the node needs to have its own default value.
- `extra` will contain the data for the node. In the above example, the value is the number 42 as a string. For other nodes, such as sliders, this may contain other information (for the sliders example, min and max).

### Connection representation

Here is an example of the representation of a Connection object in JSON, equivalent to a connection between two nodes in Dynamo.

```javascript
{
  "$type": "System.Dynamic.ExpandoObject, System.Core",
  "_id": "3aa3914e-3f4b-4540-b027-f42ebce19ac2",
  "kind": "addConnection",
  "startPortIndex": 0,
  "endPortIndex": 0,
  "startNodeId": "3f028193-6379-43b1-b133-e775bb06381e",
  "endNodeId": "36ea0c5f-8a0d-4e3f-8f08-44d60e6f04c0",
  "startProxy": false,
  "endProxy": false,
  "startProxyPosition": [
    0,
    1
  ],
  "endProxyPosition": [
    0,
    1
  ],
  "hidden": false
},
```
Note the following:
- `_id` must be a string of a unique GUID.
- `startPortIndex` is the index of the outputs of the source node
- `endPortIndex` is the index of the inputs of the destination node.
- `startNodeId` and `endNodeId` are the ids for the source and destination nodes, respectively.


## Automatic Converter

As of today, there is no particular program released by the Reach team to officially covert Dynamo graph into JSONs that Reach will accept. 

At the moment of this writing, the Akaba team has been using a [quick fix](http://github.com/dewb/Reach) from Michael Dewberry that includes command line options and has been working well so far (though we are not confident on the correctness of it). A more proper converter is under review from the Reach team.

To use that, simply clone his fork, build Reach as usual and look for `Reach.TestClient.exe` in `Dynamo/bin/AnyCPU/Debug`.

Example:

```$ [mono] Dynamo/bin/AnyCPU/Debug/Reach.TestClient.exe path/to/dynamoGraph.dyn > path/to/dynamoGraph.json```

#### Known Bugs with this Conversion :bug:
- Python nodes are not converted properly. [This was fixed](https://github.com/DynamoDS/Reach/commit/cc3b8aab1686778059bc6eb684253abc9f126233) but hasn't been merged into Dewb's conversion.