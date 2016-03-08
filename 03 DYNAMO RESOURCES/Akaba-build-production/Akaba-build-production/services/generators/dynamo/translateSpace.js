const Space = require('./Space.js');

module.exports = function translateSpace(nodeData) {
  var position = [nodeData.p0.x, nodeData.p0.y, nodeData.p0.z];
  var dimensions = [nodeData.p1.x, nodeData.p1.y, nodeData.p1.z];
  var usage = nodeData.usage.replace(/\"/g, '');
  return new Space(dimensions, position, usage);
};
