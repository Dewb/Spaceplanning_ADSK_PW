/* A node.js implementation of the Akaba generator REST interface.
The implementation does not do anything significant: regardless
of the requirements given, it will return a design with just one
space in it, with function 'core'.
*/

const express = require('express');
const bodyParser = require('body-parser');
const uuid = require('uuid');
const argv = require('optimist').argv;
const fs = require('fs');

const pjson = require('./package.json');
const Job = require('./Job.js');

var jobs = {};
var validGenerators = [];

var app = express();
app.use(bodyParser.json());
app.use(bodyParser.urlencoded({extended: true}));

// Global variables & constants.
const PORT = 34574;
const VERSION = pjson.version;
const defaultIterationCount = 0;
const defaultDesignCount = 3;

/**
 * Prints a help message in case the user asks in the options or the given
 * port is invalid.
 */
var printHelpMessage = function checkHelp() {
  var message = 'Akaba Generator Service: REST Listener for dynamo.\n' +
                'Options:\n' +
                '-p [--port] arg        Port to listen on (default ' + PORT + ').\n' +
                '-i [--iterations] arg  Number of iterations per design.\n' +
                '-h [--help]            Print this message.';
  console.log(message);
  return;
};

/**
* HTTP POST /generator
* Request must include 'Content-Type:application/json' in the headers.
* Body Param: the JSON of requirements
* Returns: 200 HTTP
* Error: 400 HTTP if there are no length, width and height requirements for the
* site.
* Error: 500 HTTP if there is any error while computing with Reach.
*/
app.post('/generator', function handleResult(req, res) {
  // Checking for space requirements.
  if (!hasSpaceRequirements(req.body)) {
    console.log('POST /generator request: 400');
    errorResponse(res, 400, 'No space type requirements provided.');
    return;
  }


  // Checking for a generator name.
  var generatorName = (req.body.settings &&
                       req.body.settings.generatorName) ?
                       req.body.settings.generatorName : null;

  if (!generatorName) {
    errorResponse(res, 400, 'No generator name provided.');
    return;
  }

  if (!isValidGeneratorName(generatorName)) {
    errorResponse(res, 400, 'No generators found with the name ' +
                            JSON.stringify(req.body.settings.generatorName) +
                            '. The available generator(s)  is(are): ' +
                            listGenerators());
    return;
  }

  var newId = uuid.v4();
  var newJob = new Job(newId, req.body.requirements);
  var handleGenResult = function handleGenResult(err) {
    if (err) {
      console.log(err);
      console.log('POST /generator request: 500');
      errorResponse(res, 500, 'Internal error when ' +
      'computing with Reach');
      return;
    }

    var sucessResponse = {
      'job': {'id': newId},
      'status': {'code': 200,
                 'message': 'Job ' + newId + ' created sucessfully.',
                 'version': VERSION
      }
    };

    console.log('Sucessful Reach request.');
    jobs[newId] = newJob;

    console.log('POST /generator request: 200');
    res.status(200).json(sucessResponse);
    return;
  };


  newJob.generate('./generatorGraphs/' + generatorName + '.json',
                  req.body,
                  handleGenResult,
                  defaultDesignCount);
});

/**
 * Lists the name of all the accepted generators into a string, without the
 * .json extension.
 * @return {String} Name of all generators separated by spaces.
 */
var listGenerators = function listGenerators() {
  var allGenerators = '';
  for (var i = 0, j = validGenerators.length; i < j; i++) {
    var curr = validGenerators[i];
    allGenerators += curr.slice(0, curr.length - 5);
    allGenerators += ' ';
  }
  return allGenerators;
};

/**
 * Reads the names of the files in ./generatorGraphs into the validGenerators
 * variable.
 */
var readGenerators = function readGenerators() {
  validGenerators = fs.readdirSync('./generatorGraphs');
};

/**
 * Checks if the given generator name is among the valid ones.
 * @param  {String}  generatorName A generator name
 * @return {Boolean}               Whether the generator name is valid.
 */
var isValidGeneratorName = function isValidGeneratorName(generatorName) {
  return validGenerators.indexOf(generatorName + '.json') > -1;
};

/**
 * Generates the data to go with an error response.
 * @param  {httpResponse} res     The client to respond to.
 * @param  {int} code    The code of the HTTP error.
 * @param  {string} message The message to go along with the response.
 */
var errorResponse = function errorResponse(res, code, message) {
  res.status(code).json({
    'status': {
      'code': code,
      'message': message,
      'version': VERSION
    }
  });
};

/**
* hasSpaceRequirements
* Param: reqBody is a JSON
* Returns: boolean if reqBody includes field requirements with site with width
*          and height.
*/
/**
 * Checks if the body of the request includes a requirements field with a site
 * field with height, width and length.
 * @param  {object}  reqBody The body of the request.
 * @return {Boolean}         Whether the request was valid.
 */
var hasSpaceRequirements = function hasSpaceRequirements(reqBody) {
  if (!('requirements' in reqBody)) { return false; }
  if (!('site' in reqBody.requirements)) { return false; }
  return ('width' in reqBody.requirements.site &&
          'height' in reqBody.requirements.site &&
          'length' in reqBody.requirements.site);
};

/**
* HTTP GET /generator/job/[jobId]
* Param: jobId the unique job id being requested.
* Returns: 200 HTTP
* Error: 404 HTTP if there are no jobs with the given id.
*/
app.get('/generator/job/:jobId', function(req, res) {
  if (!(req.params.jobId in jobs)) {
    console.log('GET /generator/job/[jobId] request: 404');
    errorResponse(res, 404, 'Job ID ' + req.params.jobId.toString() +
                            ' not found.');
    return;
  }

  var sucessResponse = {
    'job': jobs[req.params.jobId].asJSON(),
    'status': {
      'code': 200,
      'message': 'Query sucessful.',
      'version': VERSION
    }
  };

  console.log('GET /generator/job/[jobId] request: 200');
  res.status(200).json(sucessResponse);
});

/**
* HTTP DELETE /generator/job/[jobId]
* Param: jobId the unique job id being requested.
* Returns: 200 HTTP
* Error: 404 HTTP if there are no jobs with the given id.
*/
app.delete('/generator/job/:jobId', function(req, res) {
  if (!(req.params.jobId in jobs)) {
    console.log('GET /generator/job/[jobId] request: 404');
    errorResponse(res, 404, 'Job ID ' + req.params.jobId.toString() +
                        ' not found.');
    return;
  }

  delete jobs[req.params.jobId];

  var sucessResponse = {
    'status': {
      'code': 200,
      'message': 'Job ID ' + req.params.jobId.toString() + ' deleted.',
      'version': VERSION
    }
  };
  res.status(200).json(sucessResponse);
});

/**
* HTTP GET /generator/job/[jobId]/design/[designIndex]
* Param: :jobId the unique job id being requested.
* Param: :designIndex the index to the design in the job (counts from 1)
* Returns: 200 HTTP
* Error: 404 HTTP if there are no jobs with the given id, if job is not
* Error: 404 HTTP if the requested job is not complete.
* Error: 404 HTTP if the designIndex is out of bounds.
*/
app.get('/generator/job/:jobId/design/:designIndex', function(req, res) {
  if (!(req.params.jobId in jobs)) {
    console.log('GET /generator/job/[jobId] request: 404');
    errorResponse(res, 404, 'Job ID ' + req.params.jobId.toString() +
                        ' not found.');
    return;
  }

  var jobRequested = jobs[req.params.jobId];

  if (!jobRequested.completed) {
    console.log('GET /generator/job/[jobId] request: 404');
    errorResponse(res, 404, 'Job ID ' + req.params.jobId.toString() +
                        ' not completed yet.');
    return;
  }

  var requestedDesignIndex = req.params.designIndex;

  if (requestedDesignIndex > jobRequested.designs.length ||
      requestedDesignIndex <= 0) {
    errorResponse(res, 404, 'Design ' + requestedDesignIndex.toString() +
                            ' not found in job ' +
                            req.params.jobId.toString() + '.');
    return;
  }

  var requestedDesign = jobRequested.designs[requestedDesignIndex - 1];
  var sucessResponse = {
    design: requestedDesign.asJSON(),
    status: {
      'code': 200,
      'message': 'Design ' + requestedDesignIndex + ' from job ' +
                  req.params.jobId + ' returned.',
      'version': VERSION
    }
  };
  res.status(200).json(sucessResponse);
});

/**
* HTTP GET /generator/job/[jobId]/designs
* Param: :jobId the unique job id being requested.
* Returns: 200 HTTP
* Error: 404 HTTP if there are no jobs with the given id, if job is not
* Error: 404 HTTP if the requested job is not complete.
*/
app.get('/generator/job/:jobId/designs', function(req, res) {
  if (!(req.params.jobId in jobs)) {
    console.log('GET /generator/job/[jobId] request: 404');
    errorResponse(res, 404, 'Job ID ' + req.params.jobId.toString() +
                        ' not found.');
    return;
  }

  var jobRequested = jobs[req.params.jobId];

  if (!jobRequested.completed) {
    console.log('GET /generator/job/[jobId] request: 404');
    errorResponse(res, 404, 'Job ID ' + req.params.jobId.toString() +
                        ' not completed yet.');
    return;
  }

  var sucessResponse = {
    designs: [],
    status: {
      'code': 200,
      'message': '' + jobRequested.designs.length +
                 ' designs returned from job ' + req.params.jobId + '.',
      'version': VERSION
    }
  };

  for (var i = 0, j = jobRequested.designs.length; i < j; i++) {
    sucessResponse.designs.push(jobRequested.designs[i].asJSON());
  }

  res.status(200).json(sucessResponse);
});

/**
 * Runs the service. Checks for command line options.
 */
var main = function main() {
  var port;
  if (argv.h || argv.help) {
    printHelpMessage();
    return;
  }

  // TODO: Add iterations when relevant.

  if (argv.p) {
    port = argv.p;
  } else if (argv.port) {
    port = argv.port;
  } else {
    port = PORT;
  }

  if (port === true || (port > 65535 || port < 1024)) {
    console.log('Invalid port.');
    printHelpMessage();
    return;
  }

  readGenerators();

  console.log('Listening on port ' + port);
  app.listen(process.env.PORT || port);
};

main();

