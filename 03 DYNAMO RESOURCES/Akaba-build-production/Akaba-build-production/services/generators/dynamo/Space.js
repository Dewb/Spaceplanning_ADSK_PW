module.exports = Space;

/* Represents a space design.
 * NOTE: Should update in the future to include other keys and generators.
 */
function Space(dimensions, position, usage) {
  this.dimensions = dimensions;
  this.position = position;
  this.usage = usage;
  this.asJSON = function() {
    /* Change to more elaborate and to include other keys */
    return {
      'dimensions': dimensions,
      'position': position,
      'usageName': usage
    };
  };
}
