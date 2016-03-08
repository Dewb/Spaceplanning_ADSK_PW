const Design = require('./Design.js');
const computeWithReach = require('./computeWithReach.js');
const translateSpace = require('./translateSpace.js');

module.exports = Job;


/**
 * Creates a new Job with the given ID and requirements.
 * @param {string} id           The unique UUID for the Job.
 * @param {object} requirements The requirements of the Job.
 */
function Job(id, requirements) {
  console.log('New job ' + id + ' created.');
  this.jobId = id;
  this.timeSubmitted = new Date().toJSON();
  this.fail = false;
  this.failMessage = '';
  this.designs = [];
  this.requirements = requirements;
}

Job.prototype.getStatus = function() {
    if (this.fail) {
      return 'failed';
    }
    if (this.timeCompleted) {
      return 'completed';
    }
    if (this.timeStarted) {
      return 'in progress';
    }
    if (this.timeSubmitted) {
      return 'queued';
    }
    console.log('Couldn\'t get the Job status');
    return null;
  };
Job.prototype.asJSON = function() {
    var response = { id: this.jobId.toString(),
                     status: this.getStatus() };
    switch (response.status) {
    case 'failed':
      response.message = this.failMessage;
      break;
    case 'completed':
      response.designCount = this.designs.length;
      response.timeSubmitted = this.timeSubmitted;
      response.timeStarted = this.timeStarted;
      response.timeCompleted = this.timeCompleted;
      break;
    case 'in progress':
      response.timeSubmitted = this.timeSubmitted;
      response.timeStarted = this.timeStarted;
      break;
    case 'queued':
      response.timeSubmitted = this.timeSubmitted;
      break;
    default:
      console.log('Unrecognized job status');
      break;
    }

    return response;
  };

Job.prototype.generate = function(path, request, callbackToApp, designCounts) {
  this.timeStarted = new Date().toJSON();
  var that = this;
  var count = designCounts;

  var requestLoop = function requestLoop() {
    if (count > 0) {
      makeRequest(path, request, callbackToApp); // Make this a new process in the future.
      count--;
      return;
    }

    that.timeCompleted = new Date().toJSON();
    callbackToApp(false); // Return to app with no errors
    return;
  };

  var makeRequest = function makeRequest() {
    computeWithReach(
      path, request, function(err, res) {
        if (err) {
          // Problem is with Reach.
          console.log('There was an error in making a Reach request');
          that.fail = true;
          that.failMessage = 'There was an error in computing the graph.';
          callbackToApp(err);
          return;
        }

        if (!res || res === 'null') {
          // Problem is with generator.
          that.fail = true;
          that.failMessage = 'There was an error in running the generator';
          callbackToApp('Reach computation result was null');
          return;
        }

        createDesign(res);
        requestLoop();
        return;
      });
  };

  var createDesign = function createDesign(reachResult) {
    var newDesign = new Design(that.requirements);
    var newSpace = translateSpace(reachResult);
    newDesign.addSpaces([newSpace]);
    that.designs.push(newDesign);
    return;
  };


  requestLoop();
  return;
};
