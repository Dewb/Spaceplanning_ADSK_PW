var request = require('request');

var ReachApi = {
  'domain': 'http://52.20.23.28',
  'port': 8080,
  'path': '/run'
};

module.exports = computeWithReach;

/**
 * Attempts to connect and compute the dynamo graph with the Reach API
 * @param  {Sstring} path        The path to the graph json to be computed.
 * @param  {Object} requestBody The body of the client request.
 * @param  {Function} callback  Function that controls the flow of the reponse
 * to the Akaba API.
 */
function computeWithReach(path, requestBody, callback) {
  var graph = readDynamoFile(path);
  var finalNode = graph.workspace.nodes.filter(
    function(n) { return hasPrefix(n, 'akaba.result'); })[0];
  var finalNodeId = finalNode ? finalNode._id : null;

  if (!finalNodeId) {
    // The selected generator graph didn't have a Akaba.Result node, so we
    // don't need to compute it. Return with error.
    callback(true);
  }

  adjustInputs(graph.workspace.nodes, requestBody);
  graph.properties.nodesToSerialize = [finalNodeId];

  // console.log('REQUEST BODY==================================');
  // console.log(JSON.stringify(graph, null, 2));
  // console.log('REQUEST BODY END=================================');

  request.post(
    ReachApi.domain + ':' + ReachApi.port + ReachApi.path,
    {'form': JSON.stringify(graph)},
    function(error, response, body) {
      if (!error) {
        if (response.statusCode === 200) {
          // console.log(JSON.stringify(JSON.parse(body), null, 2));
          var res = findResult(JSON.parse(body), finalNodeId);
          // console.log(res);
          callback(null, res);
          return;
        }

        console.log('Failed to compute graph\n' +
                    'Reach error: ' + response.statusCode + body);
        callback(true, null);
        return;
      }

      console.log('Failed to connect to Reach\n' + error);
      callback(true, null);
      return;
    }
  );
}

/**
 * Computes the given file with Reach and returns the data of the output node.
 * @param  {string} path       The path to the JSON conversion of a .dyn file
 * with the desired graph.
 * @return {Object}            The data in the output node if it exists.
 */
var readDynamoFile = function readDynamoFile(path) {
  var graph = require(path);

  if (!graph) {
    console.log('Failed to read ' + path);
    throw new Error('File ' + path + ' could not be opened.');
  }

  if ((!graph.nodes) || (!graph.connections)) {
    console.log('Invalid dynamo graph.');
    throw new Error('Invalid dynamo graph. Needs nodes and ' +
                    'connections field.');
  }

  return {
    workspace: {
      guid: '00000000-0000-0000-0000-000000000000',
      name: 'Home',
      nodes: graph.nodes,
      connections: graph.connections
    },
    customNodes: [],
    properties: {sendGeometry: true}
  };
};

/**
 * Adjusts the given input nodes' values to the
 * @param  {Array}  nodes  Nodes to be updated.
 * @param  {Object} source Object contaning the values.
 */
function adjustInputs(nodes, source) {
  var stringToLower = function(s) { return s.toLowerCase(); };
  for (var i = 0, j = nodes.length; i < j; i++) {
    if (hasPrefix(nodes[i], 'akaba.input.')) {
      var properties = nodes[i].displayName
                               .replace(/[ \n\t\r]/g, '')
                               .split('.')
                               .slice(2)
                               .map(stringToLower);

      var newValue = getValue(source, properties);
      if (newValue) {
        updateValue(nodes[i], newValue);
      }
    }
  }
}

/**
 * Determines if the node's displayName starts with the given prefix.
 * @param  {Object}  node   Node to check the name for the prefix.
 * @param {String}   prefix Prefix to check for in the display name.
 * @return {Boolean} Whether the given Node's displayName starts with the
 * given prefix.
 */
var hasPrefix = function hasPrefix(node, prefix) {
  return node.displayName.slice(0, prefix.length).toLowerCase() === prefix.toLowerCase();
};

/**
 * Gets the value in the source's properties in properties. Returns null if
 * it is unable to find a certain property in the source object.
 * @param  {Object} source     Object to look for properties in.
 * @param  {Array}  properties String names of properties to look for.
 * @return {Object}            The value of the object.
 */
var getValue = function getValue(source, properties) {
  var propertiesToCheck = properties;
  var currentObject = source;

  while (propertiesToCheck.length > 0) {
    if (!(propertiesToCheck[0] in currentObject)) {
      console.log('There is no ' + propertiesToCheck[0] + ' in ' +
                  JSON.stringify(currentObject));
      return null;
    }
    currentObject = currentObject[propertiesToCheck[0]];
    propertiesToCheck = propertiesToCheck.slice(1);
  }

  return currentObject;
};

/**
 * Updates the value of the given node with the given value.
 * @param  {Object} node  The node to be updated.
 * @param  {Object} value The value to insert in the node.
 * @throws {Error(400)} If The node has a min or a max and the value is not within the
 * limits.
 * @throws {Error(500)} If The node has no extra.value field.
 */
var updateValue = function updateValue(node, value) {
  if ('min' in node.extra) {
    if (value < node.extra) {
      console.log('Value ' + value + ' too small. Min: ' +
                  node.extra.min);
      throw new Error(400);
    }
  }

  if ('max' in node.extra) {
    if (value > node.extra.max) {
      console.log('Value ' + value + ' too big. Max: ' + node.extra.max);
      throw new Error(400);
    }
  }

  if (!('value' in node.extra)) {
    console.log('Node ' + JSON.stringify(node) +
                ' does not have a value field');
    throw new Error(500);
  }

  // console.log('Updating ' + node.displayName + ' value to ' + value);
  node.extra.value = value;
  return;
};

/**
 * Finds the data of the node with the given id in the given response of
 * serialized nodes.
 * Returns null if the response is empty or the desired output node can't be
 * found.
 * @param  {Object} body         The body object of the Reach response.
 * @param  {string} outputNodeId The id of the output node.
 * @return {string}              The data in the output node.
 */
var findResult = function findResult(body, outputNodeId) {
  if (!body.nodes || (body.nodes.length === 0)) {
    console.log('Empty response from Reach.');
    return null;
  }

  if (!body.serializedNodes || (body.serializedNodes === 0)) {
    console.log('No serialized nodes returned.');
    return null;
  }

  for (var i = 0, j = body.serializedNodes.length; i < j; i++) {
    if (body.serializedNodes[i].nodeId === outputNodeId) {
      return body.serializedNodes[i].data;
    }
  }

  console.log('Node with the ID ' + outputNodeId + ' could not be found.');
  return null;
};
