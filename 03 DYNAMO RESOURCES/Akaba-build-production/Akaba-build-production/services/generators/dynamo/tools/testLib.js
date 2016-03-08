const request = require('request');
const fs = require('fs');
var _api = {};
var _fileindex = 0;
var _files = [];
var _options = {};

// Uncomment this line for more detailed request tracing
// To get the most accurate picture of what the request is doing, however, it's best
// to mock the server endpoint with netcat, e.g.:
// $ nc -l 8080
// request.debug = true;

exports.test_multiple = function(files, api, options) {
  console.log('Starting tests...');
  _api = api;
  _files = files;
  fileloop(options);
};

exports.test_one = function(file, api, options) {
  console.log('Starting test...');
  _api = api;
  _files = [{path: file}];
  _options = options;
  fileloop();
};

// To loop through each file.
var fileloop = function fileloop() {
  if (_files.length > _fileindex) {
    console.log('Files length: ' + _files.length);
    var curr = _files[_fileindex];
    console.log('-----------------------------------------');
    var name = curr.name || curr.path;
    console.log('Running test ' + name);
    fs.readFile(curr.path, 'utf-8', function(e, f) { filehandle(e, f, curr); });
    _fileindex++;
  }
  return;
};

var filehandle = function filehandle(err, filecontent, file) {
  if (err) {
    console.log('ERROR: Failed to read file.');
    return;
  }

  makerequest(JSON.parse(filecontent), file);
  return;
};

var makerequest = function makerequest(graph, file) {
  var resultid = graph.nodes.filter(isresult)[0] ? graph.nodes.filter(isresult)[0]._id : null;
  var requestbody;
  if (resultid) {
    requestbody = makebody(graph, [resultid]);
  } else {
    requestbody = makebody(graph, []);
  }


  if (_options.v) {
    if (resultid) {
      console.log('Result id: ' + resultid);
    }
    console.log(JSON.stringify(requestbody, null, 2));
  }

  var requesthandler = function requesthandler(err, res, body) {
    if (err) {
      console.log('ERROR: Failed to make request to Reach.');
      console.log(err);
      return;
    }

    if (res.statusCode !== 200) {
      console.log('ERROR: Failed to compute graph.');
      if (_options.v) { console.log(JSON.stringify(res, null, 2)); }
      console.log(err);
      return;
    }

    var resbody = JSON.parse(body);

    if (_options.v) { console.log(JSON.stringify(resbody, null, 2)); }
    if (lengthtest(resbody.nodes.length)) {
      if (file.hint) { printhint(file.hint); }
      return;
    }

    // console.log(JSON.stringify(resbody.nodes, null, 2));

    var inactives = resbody.nodes.filter(function(n) {return n.state !== 'Active'; });
    if (activetest(inactives.length)) {
      if (file.hint) { printhint(file.hint); }
      return;
    }

    // There is an Akaba.Result node.
    if (resultid && file.expected) {
      var resresult = resbody.nodes.filter(function(n) { return n.nodeId === resultid; })[0];
      if (resulttest(resresult.data, file.expected)) {
        if (file.hint) { printhint(file.hint); }
        return;
      }
    }

    console.log('Success.');
    return;
  };

  var resulttest = function resulttest(actual, expected) {
    if (actual !== expected) {
      console.log('Test failed.');
      console.log('Expected................... ' + expected);
      console.log('Actual..................... ' + actual);
      return true;
    }
    return false;
  };

  var lengthtest = function lengthtest(resbodysize) {
    // Not all nodes came back
    if (graph.nodes.length !== resbodysize) {
      console.log('Test failed. Not all nodes came back.');
      console.log('Number of nodes sent....... ' + graph.nodes.length);
      console.log('Number of nodes returned... ' + resbodysize);
      console.log('This error is usually caused by Reach not recognizing a certain library that was used in developing the graph.');
      console.log('Make sure the libraries you are using are properly loaded to Reach');
      return true;
    }
    return false;
  };

  var activetest = function activetest(inactives) {
    if (inactives > 0) {
      console.log('Test failed. ' + inactives.length + ' nodes came back inactive.');
      console.log('This error is usually caused by Reach failing to run a node.');
      // TODO: Add printing of Warning and Dead nodes.
      return true;
    }
    return false;
  };

  var printhint = function printhint(hint) {
    if (hint) { console.log('HINT: ' + hint); }
  };

  request({
    'url': _api.domain + ':' + _api.port + _api.path,
    'method': 'POST',
    'body': JSON.stringify(requestbody),
    'headers': {
      'Content-type': 'application/javascript'
    }
  },
    function(e, r, b) {
      requesthandler(e, r, b);
      fileloop();
    }
  );

  return;
};

/**
 * Prepares the body for the Reach request.
 * @param  {Object} graph         Graph in a JSON
 * @param  {Array} serializables  String ids of nodes to serialize
 * @return {Object}               Reach request workspace.
 */
var makebody = function makebody(graph, serializables) {
  // Makes the body Reach-style.
  return {
    workspace: {
      guid: '00000000-0000-0000-0000-000000000000',
      name: 'Home',
      nodes: graph.nodes,
      connections: graph.connections
    },
    customNodes: [],
    properties: {
      sendGeometry: true,
      sendNodeArrayItems: true,
      nodesToSerialize: serializables ? serializables : []
    }
  };
};

var isresult = function isresult(node) {
  var prefix = 'akaba.result';
  return node.displayName.slice(0, prefix.length).toLowerCase() === prefix;
};
