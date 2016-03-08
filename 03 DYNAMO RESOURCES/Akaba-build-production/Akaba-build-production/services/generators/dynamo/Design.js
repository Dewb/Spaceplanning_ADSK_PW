module.exports = Design;

/* Design representation.
 * - designSpaces is an Array of Spaces generated in this design.
 * - requirements is an Object with the requested requirements.
 */
/**
 * Creates a Design instance.
 * @param {Object} requirements The requirements for the design.
 * @param {Array}  spaces       Exisiting spaces to initialize the Design with.
 */
function Design(requirements) {
  this.requirements = requirements;
  this.designSpaces = [];
  this.asJSON = function() {
    return {'spaces': this.designSpaces.map(function(s) { return s.asJSON(); })};
  };
}

/**
 * Adds spaces to this.designSpaces
 * @param {Array} newSpaces Adds spaces to the array of spaces.
 */
Design.prototype.addSpaces = function(newSpaces) {
  for (var i = 0, j = newSpaces.length; i < j; i++) {
    this.designSpaces.push(newSpaces[i]);
  }
};
