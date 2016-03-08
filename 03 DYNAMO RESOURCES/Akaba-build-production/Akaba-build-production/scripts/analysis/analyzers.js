define(function() {

if (typeof _ == "undefined") {
   _ = require("underscore")
}

function clone(obj, levels) {
   if (levels <= 0) return obj;

      if (null == obj || "object" != typeof obj) return obj;

      if (obj instanceof Date) {
            var copy = new Date();
            copy.setTime(obj.getTime());
            return copy;
      }

      if (obj instanceof Array) {
            var copy = [];
            for (var i = 0; i < obj.length; i++) {
                  copy[i] = clone(obj[i], levels - 1);
            }
            return copy;
      }

      if (obj instanceof Object) {
            var copy = {};
            for (var attr in obj) {
                  if (obj.hasOwnProperty(attr)) {
            copy[attr] = clone(obj[attr], levels - 1);
               }
            }
            return copy;
      }

      throw new Error("Unsupported type in clone()!");
}

function findOverlapsInLayout(spaces) {
      var overlaps = [];
   for (var ii = 0; ii < spaces.length - 1; ii++) {
      for (var jj = ii + 1; jj < spaces.length; jj++) {
         var space1 = spaces[ii];
         var space2 = spaces[jj];
         if (doSpacesOverlap(space1, space2)) {
            overlaps.push([space1.get("uniqueId"), space2.get("uniqueId")]);
         }
      }
   }
      if (overlaps.length == 0)
            return null;
   return overlaps;
}

function doSpacesOverlap(space1, space2) {
   return _.isNumber(getLevelSpacesConnect(space1, space2)) &&
         (Math.abs(space1.getPosition()[0] - space2.getPosition()[0]) * 2 < (space1.getWorldDimensions()[0] + space2.getWorldDimensions()[0])) &&
         (Math.abs(space1.getPosition()[1] - space2.getPosition()[1]) * 2 < (space1.getWorldDimensions()[1] + space2.getWorldDimensions()[1]));
}

function doesSpaceOverlap(space, spaces) {
   for (var ii = 0; ii < spaces.length; ii++) {
      if (spaces[ii] == space) {
         continue;
      } else if (doSpacesOverlap(space, spaces[ii])) {
         return true;
      }
   }
   return false;
}

// A quick fix to ensure that stairs are only entered through a landing
function entersOnValidFace(space, entryPoint) {
   if (space.data.type != "Stairs") {
      return true;
   }

   var rot = space.get("rotation");
   var entryFace = Math.round((rot / (Math.PI / 2))) % 4; // Only enter the stairwell on the landing (interface inellegance observed)

   var dim = (entryFace + 1) % 2;
   return (entryPoint[dim] == space.getPosition()[dim] + ((entryFace == 1 || entryFace == 2) ? -0.5 : 0.5) * space.getWorldDimensions()[dim]);
}

function whereDoSpacesConnect(requirements, space1, space2) {
    var connectionLevel = getLevelSpacesConnect(space1, space2);
   if (!_.isNumber(connectionLevel)) {
      return null;
   }

   // Do the two spaces touch, with enough overlap to fit a door?
   var d = requirements.code.minimumDoorWidth;
   var x = Math.abs(space1.getPosition()[0] - space2.getPosition()[0]) - 0.5 * (space1.getWorldDimensions()[0] + space2.getWorldDimensions()[0]);
   var y = Math.abs(space1.getPosition()[1] - space2.getPosition()[1]) - 0.5 * (space1.getWorldDimensions()[1] + space2.getWorldDimensions()[1]);

   // If not, no overlap
   if (!(x == 0 && y < -d) &&
       !(y == 0 && x < -d))
      return null;

   // Where do they touch (midpoint of overlap)?
   var connectionPoint = [];
   var matchedAxis = (x == 0 ? 0 : 1);
   var unmatchedAxis = (matchedAxis == 1 ? 0 : 1);
   connectionPoint[matchedAxis] = (space1.getPosition()[matchedAxis] < space2.getPosition()[matchedAxis] ? -0.5 : 0.5) * 
                                space2.getWorldDimensions()[matchedAxis] + space2.getPosition()[matchedAxis];

   // Place most doors at the centerpoint between spaces for now, but prefer placing stairwell doors directly in front of a run
   var positionOfDoor = 0.5;
   if (space1.data.type == "Stairs") {
      positionOfDoor = 0.75;
   } else if (space2.data.type == "Stairs") {
      positionOfDoor = 0.25;
   }

   var overlapEnd1, overlapEnd2;
   overlapEnd1 = Math.max(space1.getPosition()[unmatchedAxis] - space1.getWorldDimensions()[unmatchedAxis] / 2, 
                         space2.getPosition()[unmatchedAxis] - space2.getWorldDimensions()[unmatchedAxis] / 2);
   overlapEnd2 = Math.min(space1.getPosition()[unmatchedAxis] + space1.getWorldDimensions()[unmatchedAxis] / 2, 
                         space2.getPosition()[unmatchedAxis] + space2.getWorldDimensions()[unmatchedAxis] / 2);
   connectionPoint[unmatchedAxis] = overlapEnd1 + (overlapEnd2 - overlapEnd1) * positionOfDoor;

   connectionPoint[2] = connectionLevel;

   if (!entersOnValidFace(space1, connectionPoint) || !entersOnValidFace(space2, connectionPoint)) {
      return null;
   }

   var overlapPoint1 = [0, 0, connectionLevel];
   var overlapPoint2 = [0, 0, connectionLevel];
   overlapPoint1[matchedAxis] = connectionPoint[matchedAxis];
   overlapPoint1[unmatchedAxis] = overlapEnd1;
   overlapPoint2[matchedAxis] = connectionPoint[matchedAxis];
   overlapPoint2[unmatchedAxis] = overlapEnd2;

   return { connectionPoint: connectionPoint, overlapEndpoint1: overlapPoint1, overlapEndpoint2: overlapPoint2 };
}

function spaceIsCirculation(requirements, space) {
   if (space.data.type == "Hall" || space.data.type == "Stairs") {
      return true; 
   } else {
      return false;
   }
}

function getLevelSpacesConnect(space1, space2) {
   var space1NoLevel = (space1.getLevel() == null || space1.getLevel() == "*");
   var space2NoLevel = (space2.getLevel() == null || space2.getLevel() == "*");

   if (space1NoLevel && space2NoLevel) {
      return 1;
   }
   if (typeof !space1NoLevel && !space2NoLevel && space1.getLevel() == space2.getLevel()) {
      return space1.getLevel();
   }
   if (space1NoLevel && !_.isArray(space2.data.levels)){
      return space2.getLevel();
   }
   if (space2NoLevel && !_.isArray(space1.data.levels)){
      return space1.getLevel();
   }
   if (_.isArray(space1.data.levels) && !_.isArray(space2.data.levels)) {
      if (space1.data.levels.indexOf(space2.getLevel()) >= 0) {
         return space2.getLevel();
      } else {
          return null;
      }
   }
   if (_.isArray(space2.data.levels) && !_.isArray(space1.data.levels)) {
      if (space2.data.levels.indexOf(space1.getLevel()) >= 0) {
               return space1.getLevel();
      } else {
          return null;
      }
   }   
   if (_.isArray(space1.data.levels) && _.isArray(space2.data.levels)) {
      for (var ii = 0; ii < space1.data.levels.length; ii++) {
         if (space2.data.levels.indexOf(space1.data.levels[ii]) >= 0) {
                  // Return the bottom level of overlap
                  // This is probably right when a multi-story lobby touches a staircase,
                  // but probably not when two staircases touch each other (which I think is uncommon)
            return space1.data.levels[ii];
         }
      }
   }
   return null;
}

function getUsageName(space) {
   if (space.data.type == "Hall") {
      return "Hall";
   } else if (space.data.type == "Stairs") {
      return "Stair";
   } else {
      return space.data.usageName;
   }
}

var spaceNames = {}

function getName(space) {
   return spaceNames[space.get("uniqueId")]; 
}

function assignSpaceNames(spaces) {
   var usageCounts = {};
   for (var ii = 0; ii < spaces.length; ii++) {

      var usage = getUsageName(spaces[ii]);
      if (typeof usageCounts[usage] == 'undefined') {
         usageCounts[usage] = 0;
      }
      usageCounts[usage]++;
      spaceNames[spaces[ii].get("uniqueId")] = usage + " " + usageCounts[usage];
   }
}

function testNeighborship(requirements, space1, space2, data) {
   data = data || {
      nextDoorIndex: 0
   };
   var connectResult = whereDoSpacesConnect(requirements, space1, space2);
   var connectionPoint = connectResult == null ? null : connectResult.connectionPoint;
   if (_.isArray(connectionPoint)) {
      if (!_.isArray(space1.neighbors)) { space1.neighbors = []; }
      if (!_.isArray(space2.neighbors)) { space2.neighbors = []; }
      var n1 = { id: space2.data.uniqueId, connectionPoint: connectionPoint, neighborUsage: space2.data.usageName, overlapEndpoint1: connectResult.overlapEndpoint1, overlapEndpoint2: connectResult.overlapEndpoint2 };
      var n2 = { id: space1.data.uniqueId, connectionPoint: connectionPoint, neighborUsage: space1.data.usageName, overlapEndpoint1: connectResult.overlapEndpoint1, overlapEndpoint2: connectResult.overlapEndpoint2 };
      if (spaceIsCirculation(requirements, space1) || spaceIsCirculation(requirements, space2)) {
         n1.doorIndex = data.nextDoorIndex;
         n2.doorIndex = data.nextDoorIndex;
         data.nextDoorIndex++;
      }
      space1.neighbors.push(n1);
      space2.neighbors.push(n2);
   }
}

function updateNeighbors(requirements, spaces) {
   for (var ii = 0; ii < spaces.length; ii++) {
      var s = spaces[ii];
      if (_.isArray(s.neighbors)) {
         s.neighbors.splice(0, s.neighbors.length);
      } else if ("neighbors" in s) {
         delete s.neighbors;
         s.neighbors = [];
      } else {
         s.neighbors = [];
      }
   }
   var data = { nextDoorIndex: 0 };
   for (var ii = 0; ii < spaces.length - 1; ii++) {
      for (var jj = ii + 1; jj < spaces.length; jj++) {
            testNeighborship(requirements, spaces[ii], spaces[jj], data);
      }
   }
}

function getSpacesForUsage(spaces, usage) {
   var output = [];
   for (var ii = 0; ii < spaces.length; ii++) {
      if (spaces[ii].data.usageName == usage) {
         output.push(spaces[ii]);
      }
   }
   return output;
}

function linearDistance(pt1, pt2) {
    var xDist = pt2[0] - pt1[0];
    var yDist = pt2[1] - pt1[1];
    return Math.sqrt(xDist*xDist + yDist*yDist);
}

function getPathDistance(path) {
    var dist = 0;
    for (var i = 1; i < path.length; i++)
         dist += linearDistance(path[i], path[i-1]);
    return dist;
}

// Find a position inside the room where the pedestrian is likely to step on its way through the room from a door 
function stepIntoSpace(doorPoint, space) {
   var stepInDistance = 3;
   // Is on vertical wall
   if (Math.abs(doorPoint[0] - space.getPosition()[0]) == space.getWorldDimensions()[0] / 2)
      return [doorPoint[0] + (doorPoint[0] < space.getPosition()[0] ? stepInDistance : -stepInDistance), doorPoint[1], doorPoint[2]];
   else // horizontal
      return [doorPoint[0], doorPoint[1] + (doorPoint[1] < space.getPosition()[1] ? stepInDistance : -stepInDistance), doorPoint[2]];
}

function pathThroughSpace(space, entrance, exit) {
   if (!_.isArray(entrance))
      return [[space.getPosition()[0], space.getPosition()[1], exit[2]], exit];
   if (!_.isArray(exit))
      return [entrance, [space.getPosition()[0], space.getPosition()[1], entrance[2]]];

   // No level change
   if (entrance[2] == exit[2])
      return [entrance, stepIntoSpace(entrance, space), stepIntoSpace(exit, space), exit];

   else {
      var path = [];

      // Assume two-run stairs
      var maxLevel = Math.max(entrance[2], exit[2]);
      var minLevel = Math.min(entrance[2], exit[2]);

      var flatPath = [];
      flatPath.push([space.getPosition()[0]+space.getWorldDimensions()[0] * 0.3, space.getPosition()[1]+space.getWorldDimensions()[1] * 0.3]);
      flatPath.push([space.getPosition()[0]+space.getWorldDimensions()[0] * 0.3, space.getPosition()[1]-space.getWorldDimensions()[1] * 0.3]);
      flatPath.push([space.getPosition()[0]-space.getWorldDimensions()[0] * 0.3, space.getPosition()[1]-space.getWorldDimensions()[1] * 0.3]);
      flatPath.push([space.getPosition()[0]-space.getWorldDimensions()[0] * 0.3, space.getPosition()[1]+space.getWorldDimensions()[1] * 0.3]);

      if (linearDistance(flatPath[0], entrance) > linearDistance(flatPath[2], entrance)) {
         flatPath.push(flatPath.shift());
         flatPath.push(flatPath.shift());
      }

      if (space.getWorldDimensions()[0] > space.getWorldDimensions()[1])
         flatPath.push(flatPath.shift());

      for (var i = minLevel; i < maxLevel; i++) {
         for (var j = 0; j < 4; j++) {
            var height = i;
            if (j == 1 || j == 2)
               height += 0.5;
            else if (j == 3)
               height += 1;
            path.push([flatPath[j][0], flatPath[j][1], height]);
         }
      }

      if (entrance[2] > exit[2])
         path.reverse();

      path.unshift(entrance);
      path.push(exit);
      return path;
   }
}

function initializeN(N, v) {
   var m = [];
   for (var ii = 0; ii < N; ii++) {
      m.push(v);
   }
   return m;
}

function initializeNxN(N, v) {
   var m = [];
   for (var ii = 0; ii < N; ii++) {
      m[ii] = [];
      for (var jj = 0; jj < N; jj++) {
         m[ii].push(v);
      }
   }
   return m;
}

function buildGraph(spaces) {
   // Precalculate the number of vertices in our graph: one for each space, one for each doorway
   var V = spaces.length + 0.5 * _.reduce(spaces, function (a, s) { 
      if (!_.isArray(s.neighbors))
         return a;
      else 
         return a + _.reduce(s.neighbors, function (b, n) { 
            if (!("doorIndex" in n))
               return b;
            else
               return b + 1; 
         }, 0);
   }, 0);

   var Vertices = [];
   var VertexIsEndpointOnly = [];
   var DoorwayVertices = initializeN(V - spaces.length, null);
   var Edges = [];
   var EdgeWeights = initializeNxN(V, Number.MAX_VALUE);
   var SpaceCenterVertexMap = {};
   var EdgesToRoomsMap = initializeNxN(V, null);
   var EdgesToPathsMap = initializeNxN(V, null);

   for (var ii = 0; ii < spaces.length; ii++) {
      var from = spaces[ii];

      // Indices of all vertexes in this space
      var spaceVertices = [ Vertices.length ];
      
      // First vertex is the center of the space
      SpaceCenterVertexMap[from.data.uniqueId] = Vertices.length;
      var center = from.getPosition();
      center[2] += 1;
      Vertices.push(center);
      VertexIsEndpointOnly.push(true);

      if (!_.isArray(from.neighbors))
         continue;

      // Other vertices come from neighbor connection points
      for (var jj = 0; jj < from.neighbors.length; jj++) {
         var neighbor = from.neighbors[jj];
         if ("doorIndex" in neighbor) {         
            if (DoorwayVertices[neighbor.doorIndex] == null) {
               DoorwayVertices[neighbor.doorIndex] = Vertices.length;
               Vertices.push(neighbor.connectionPoint);
               VertexIsEndpointOnly.push(false);
            }
            spaceVertices.push(DoorwayVertices[neighbor.doorIndex]);
         }
      }

      // Create an edge between each pair of vertices in the room
      for (var jj = 0; jj < spaceVertices.length; jj++) {
         for (var kk = 0; kk < spaceVertices.length; kk++) {
            if (jj != kk) {
               var v1 = spaceVertices[jj];
               var v2 = spaceVertices[kk];
               var path = pathThroughSpace(from, jj == 0 ? null : Vertices[v1], kk == 0 ? null : Vertices[v2]);

               Edges.push([v1, v2]);
               EdgeWeights[v1][v2] = getPathDistance(path);
               EdgesToRoomsMap[v1][v2] = from;
               EdgesToPathsMap[v1][v2] = path;
            }
         }
      }
   }          
   return { 
      vertices: Vertices, 
      edges: Edges,
      edgeWeights: EdgeWeights,
      spaceCenterVertexMap: SpaceCenterVertexMap,
      edgesToRoomsMap: EdgesToRoomsMap,
      edgesToPathsMap: EdgesToPathsMap,
      vertexIsEndpointOnly: VertexIsEndpointOnly
   };
}

function computeShortestPaths(graph) {
   var V = graph.vertices.length;
   var dist = initializeNxN(V, Number.MAX_VALUE);
   var next = initializeNxN(V, null);

   _.each(graph.edges, function(e) {
      var u = e[0];
      var v = e[1];
      dist[u][v] = graph.edgeWeights[u][v];
      next[u][v] = v;
   });

   for (var k = 0; k < V; k++) {
      if (graph.vertexIsEndpointOnly[k])
         continue;
      for (var i = 0; i < V; i++) {
         for (var j = 0; j < V; j++) {
            if (dist[i][k] + dist[k][j] < dist[i][j]) {
               dist[i][j] = dist[i][k] + dist[k][j];
               next[i][j] = next[i][k];
            }
         }
      }
   }

   graph.dist = dist;
   graph.next = next;
}

function isSpaceReachable_graph(graph, from, to) {
   var result = {
      path: "",
      pathRooms: [],
      distance: 0,
      reachable: false
   };

   var u = graph.spaceCenterVertexMap[from.data.uniqueId];
   var v = graph.spaceCenterVertexMap[to.data.uniqueId];

   if (graph.next[u][v] != null) {
      result.reachable = true;
      result.distance = graph.dist[u][v];

      while (u != v) {
         var next = graph.next[u][v];
         var space = graph.edgesToRoomsMap[u][next];
         var path = graph.edgesToPathsMap[u][next];

         result.path += getName(space) + " > ";
         result.pathRooms.push({ spaceId: space.data.uniqueId, internalPath: path });
         u = next;
      }
   }

   return result;
}

function isSpaceReachable(spaces, requirements, from, to, entryPosition, data) {
   if (!_.isArray(from.neighbors) || from.neighbors.length == 0 || 
      !_.isArray(to.neighbors) || to.neighbors.length == 0) {
      return { reachable: false };
   }

   data = data || {
      alreadySearched: [],
      path: "",
      pathRooms: [],
      distance: 0,
      currentLevel: from.getLevel(),
      reachable: false
   };

   data.path += getName(from) + " > ";

   // Is the destination space an immediate neighbor?
   for (var ii = 0; ii < from.neighbors.length; ii++) {
      if (from.neighbors[ii].space === to.data.uniqueId) {
             var pathThroughFrom = pathThroughSpace(from, entryPosition, from.neighbors[ii].connectionPoint);
             var pathThroughTo = pathThroughSpace(to, from.neighbors[ii].connectionPoint, null);
             if (spaceIsCirculation(requirements, from))
                  data.distance += getPathDistance(pathThroughFrom);
             if (spaceIsCirculation(requirements, to))
                  data.distance += getPathDistance(pathThroughTo);
             data.pathRooms.push({spaceId: from.data.uniqueId, internalPath: pathThroughFrom});
             data.pathRooms.push({spaceId: to.data.uniqueId, internalPath: pathThroughTo});
         data.path += getName(to);
         data.reachable = true;
         return data;
      }
   }

   // Is the destination space an immediate neighbor of a circulation space that's connected to the origin space?
   var bestDistance = Number.MAX_VALUE;
   var bestResult = undefined;

   for (var ii = 0; ii < from.neighbors.length; ii++) {
      var neighborSpace = null;
      
      //Groundhog.get(from.neighbors[ii].space, { passive: true }, function (elem) { neighborSpace = elem; });
      spaces.forEach(function (s) { if (s.data.uniqueId == from.neighbors[ii].space) { neighborSpace = s; }});

      if (spaceIsCirculation(requirements, neighborSpace)) {
         var nextSpace = neighborSpace;

         var recurse = true;
         for (var aa = 0; aa < data.alreadySearched.length; aa++) {
            if (data.alreadySearched[aa] === nextSpace.data.uniqueId) {
               recurse = false;
               break;
            }
         }

         if (recurse) {
            var newData = clone(data, 1);

            // Two structures need deeper copies
            newData.pathRooms = clone(data.pathRooms, 1);
            newData.alreadySearched = clone(data.alreadySearched, 1);

            var pathThroughFrom = pathThroughSpace(from, entryPosition, from.neighbors[ii].connectionPoint);
            if (spaceIsCirculation(requirements, from)) {
               newData.distance += getPathDistance(pathThroughFrom);
               if (typeof nextSpace.getLevel() != "undefined") {
                  newData.currentLevel = nextSpace.getLevel();
               }
            }

            newData.pathRooms.push({spaceId: from.data.uniqueId, internalPath: pathThroughFrom});
            newData.alreadySearched.push(nextSpace.data.uniqueId);

            var result = isSpaceReachable(spaces, requirements, nextSpace, to, from.neighbors[ii].connectionPoint, newData);
            if (result && result.distance < bestDistance) {
               bestResult = result;
               bestDistance = result.distance;
            }
         }
      }
   }

   return (typeof bestResult == "undefined") ? { success: false } : bestResult;
}

window.clearVirtualSpaces = function() {
   Groundhog.getEach(".Space", { passive: true }, function (s) {
       if (s.data.usageName == "egress") { Groundhog.deleteElement(s.get("uniqueId")); }
   });
}

function createEgressSpaces(requirements, spaces) {
   /*var nextDoorIndex = 0;
   _.each(spaces, function (s) {
      if(_.isArray(s.neighbors)) {
         _.each(s.neighbors, function (n) {
            if (n.doorIndex >= nextDoorIndex)
               nextDoorIndex = n.doorIndex + 1;
         });
      }
   });*/
   _.each(spaces, function (s) {
      if (s.data.type == "Hall" && s.getLevel() == 1) {
         var lines = getSpacePerimeterLines(s);
         _.each(s.neighbors, function (n) {
            if (n.neighborUsage != "egress") 
               subtractLineFromPerimeter(lines, [n.overlapEndpoint1, n.overlapEndpoint2]);
         });
         _.each(lines, function (l) {
            if (linearDistance(l[0], l[1]) >= 1.5) {
               var pos, dims;
               var thickness = 0.25;
               var doorWidth = 1.5;
               var doorHeight = 2.5;
               if (l[0][0] == l[1][0]) {
                  var d = l[0][0] > s.getPosition()[0] ? thickness/2 : -thickness/2;
                  var pos = [l[0][0] + d, (l[0][1] + l[1][1])/2, 0];
                  var w = Math.abs(l[1][1] - l[0][1]);
                  var dims = [thickness, doorWidth, doorHeight];
               } else {
                  var d = l[0][1] > s.getPosition()[1] ? thickness/2 : -thickness/2;
                  var pos = [(l[0][0] + l[1][0])/2, l[0][1] + d, 0];
                  var w = Math.abs(l[1][0] - l[0][0])
                  var dims = [doorWidth, thickness, doorHeight];
               }
               var data = new SpaceData("", pos, dims, 0);
               data.usageName = "egress";
               data.uniqueId = getNewId();
               var egress = Groundhog.addElement(data);
               //testNeighborship(requirements, s, egress, { nextDoorIndex: nextDoorIndex }); // update neighbors graph
               spaces.push(egress);
            }
         });
      }
   });
}


function subtractLineFromPerimeter(lines, subline) {
   for (var ii = 0; ii < lines.length; ii++) {
      var line = lines[ii];
      if (line[0][0] == subline[0][0] && line[0][0] == subline[1][0] && line[0][0] == line[1][0]) {
         // collinear and parallel to y-axis
         var a = Math.min(line[0][1], line[1][1]);
         var b = Math.max(line[0][1], line[1][1]);
         var c = Math.min(subline[0][1], subline[1][1]);
         var d = Math.max(subline[0][1], subline[1][1]);
         if (b - c >= 0 && d - a >= 0) {
            lines.splice(ii, 1);
            var overlap = [ Math.min(Math.max(a, c), Math.min(b, d)), Math.max(Math.max(a, c), Math.min(b, d)) ];
            if (overlap[0] > a)
               lines.push([[line[0][0], a], [line[0][0], overlap[0]]]);
            if (overlap[1] < b)
               lines.push([[line[1][0], overlap[1]], [line[1][0], b]]);
            return;
         }
      } else if (line[0][1] == subline[0][1] && line[0][1] == subline[1][1] && line[0][1] == line[1][1]) {
         // collinear and parallel to x-axis
         var a = Math.min(line[0][0], line[1][0]);
         var b = Math.max(line[0][0], line[1][0]);
         var c = Math.min(subline[0][0], subline[1][0]);
         var d = Math.max(subline[0][0], subline[1][0]);
         if (b - c >= 0 && d - a >= 0) {
            lines.splice(ii, 1);
            var overlap = [ Math.min(Math.max(a, c), Math.min(b, d)), Math.max(Math.max(a, c), Math.min(b, d)) ];
            if (overlap[0] > a)
               lines.push([[a, line[0][1]], [overlap[0], line[0][1]]]);
            if (overlap[1] < b)
               lines.push([[overlap[1], line[1][1]], [b, line[1][1]]]);
            return;
         }
      }
   }
}

function getSpacePerimeterLines(space) {
   var pos = space.getPosition();
   var dims = space.getWorldDimensions();
   return [
      [[pos[0] - dims[0]/2, pos[1] - dims[1]/2], [pos[0] - dims[0]/2, pos[1] + dims[1]/2]],
      [[pos[0] - dims[0]/2, pos[1] + dims[1]/2], [pos[0] + dims[0]/2, pos[1] + dims[1]/2]],
      [[pos[0] + dims[0]/2, pos[1] + dims[1]/2], [pos[0] + dims[0]/2, pos[1] - dims[1]/2]],
      [[pos[0] + dims[0]/2, pos[1] - dims[1]/2], [pos[0] - dims[0]/2, pos[1] - dims[1]/2]]
   ]
}

function doesLayoutSatisfySpaceRequirements(requirements, spaces, results) {
   var overallSuccess = true;

   for (var ii = 0; ii < requirements.spaces.length; ii++) {
      var spaceReq = requirements.spaces[ii];
      if (spaceReq.minimumArea > 0 || spaceReq.minimumCount > 0) {
         var totalArea = 0;
         var count = 0;
         
         for (var jj = 0; jj < spaces.length; jj++) {
            if (spaces[jj].data.usageName == spaceReq.usage) {
               count++;
               totalArea += spaces[jj].data.dimensions[0] * spaces[jj].data.dimensions[1];
            }
         }

         if ((spaceReq.minimumArea != null && totalArea < spaceReq.minimumArea) || 
               (spaceReq.minimumCount != null && count < spaceReq.minimumCount))
            overallSuccess = false;

         if (_.isArray(results)) {
            results.push({
               area: totalArea,
               count: count,
               requirement: spaceReq,
            });
         }
      }
   }

   return overallSuccess;
}

//returns the arithmetic mean of the relative error of the unsatisfied program requirements
function getProgramReqsError(results) {
   var scorePerProgramReq = {};
   var sumScore = 0;
   var cap = 1; // Constrain to [0, cap]

   results.forEach(
      function(res) {
         var factors = 0;
         var score = 0;
         if (res.requirement.minimumCount > 0) {
            factors++;
            score += Math.max(Math.min(1 - res.count/res.requirement.minimumCount, cap), 0);
         }
         if (res.requirement.minimumArea > 0) {
            factors++;
            score += Math.max(Math.min(1 - res.area/res.requirement.minimumArea, cap), 0);
         }

         if (factors > 0)
            score /= factors;
         sumScore += score;
         scorePerProgramReq[res.requirement.usage] = score; 
      }
   );

   var score;
   if (results.length>0) {
      score = sumScore/results.length;
      score = Math.max(Math.min(score, cap), 0); 
      score = score/cap;
   }
   else { 
      score = 0;
   }

   return [score, scorePerProgramReq];
}

//returns the arithmetic mean of the unsatisfied proximity requirements
function getProximityReqsError(results) {
   var sumError = 0;
   var unsatisfiedReqsCount = 0;
   var cap = 300; // Constrain to [0, cap]

   results.forEach(
      function(res){ 
         if (!res.satisfied) {
            unsatisfiedReqsCount++;
            var relativeError;
            if (res.reachable)
               relativeError = Math.abs(res.requirement.maxDistance-res.distance)/res.requirement.maxDistance;
            else
               relativeError = 300;
            sumError += relativeError;
         }         
      }
   );
   var score;
   if (unsatisfiedReqsCount>0) {
      score = sumError/unsatisfiedReqsCount;
      score = Math.max(Math.min(score, cap), 0); 
      score = score/cap;
   }
   else
      score = 0;

   return score;
}

var getBillableSpaceError = function(aggregateSpaceAssignment) {
   var totalSpace = 0;
   var circulationSpace = 0;

   _.each(aggregateSpaceAssignment, function(assignment, usageName) {
      // Assume doors don't use space
      if (usageName == "egress")
         return;

      if (usageName == "Hall" || usageName == "Stair")
         circulationSpace += assignment.area;

      totalSpace += assignment.area;
   });

   if (almostEqual(totalSpace, 0))
      return Math.MAX_VALUE;

   var ratio = circulationSpace / totalSpace;

   // Treat 30% circulation as success, and 50% as failure
   var successGoal = 0.3;
   var failureGoal = 0.5;
   var score = (ratio - successGoal) / (failureGoal-successGoal);
   score = Math.max(Math.min(score, 1), 0); // Constrain to [0, 1]
   return score;
}

var getSiteFitError = function(siteFitResults) {
   var actual = siteFitResults.actualSite;
   var target = siteFitResults.intendedSite;
   if (actual == null || target == null)
      return Math.MAX_VALUE;

   // Returns [0,1]
   // Fitting in both dimensions returns 0, being more than twice the length in both dimensions is 1,
   //   being double in one dimension and fitting in another is 0.5, etc.
   var scoreDimensions = function(actualWidth, actualHeight, targetWidth, targetHeight) {
      var errorWidth = actualWidth / targetWidth - 1;
      var errorHeight = actualHeight / targetHeight - 1;

      errorWidth = Math.max(Math.min(errorWidth, 1), 0); // Constrain to [0,1]
      errorHeight = Math.max(Math.min(errorHeight, 1), 0); // Constrain to [0,1]

      return (errorWidth+errorHeight)/2;
   }

   var score = scoreDimensions(actual.width, actual.height, target.width, target.height);

   // Try the building rotated 90º if that improves the score
   var score2 = scoreDimensions(actual.height, actual.width, target.width, target.height);

   return Math.min(score, score2);
}

function doesLayoutSatisfyAdjacencyRequirements(requirements, spaces, results) {

   assignSpaceNames(spaces);
   updateNeighbors(requirements, spaces);
   createEgressSpaces(requirements, spaces);
   updateNeighbors(requirements, spaces); // todo: make this more efficient

   var overallSuccess = true;

   var graph = buildGraph(spaces);
   computeShortestPaths(graph);

   for (var ii = 0; ii < requirements.adjacencies.length; ii++) {
      var adjacencyReq = requirements.adjacencies[ii];
      var fromSpaces = getSpacesForUsage(spaces, adjacencyReq.from);
      var toSpaces    = getSpacesForUsage(spaces, adjacencyReq.to);
      
      for (var ff = 0; ff < fromSpaces.length; ff++) {
         var fromSpace = fromSpaces[ff];
         var toSpace = undefined;
         var bestResult = { reachable: false };

         for (var tt = 0; tt < toSpaces.length; tt++) {
            toSpace = toSpaces[tt];
            //var result = isSpaceReachable(spaces, requirements, fromSpace, toSpace);
            var result = isSpaceReachable_graph(graph, fromSpace, toSpace);

            if (!bestResult.reachable && result.reachable || (result.reachable && result.distance < bestResult.distance)) {
               bestResult = result;
            }
         }

         var satisfied = bestResult.reachable && bestResult.distance <= adjacencyReq.maxDistance;
         if (!bestResult.reachable || !satisfied) {
            overallSuccess = false;      
         } 

         if (_.isArray(results)) {
            bestResult.fromSpace = fromSpace;
            bestResult.toSpace = toSpace;
            bestResult.requirement = adjacencyReq;
            bestResult.satisfied = satisfied;
            results.push(bestResult);
         }
      }
   }

   return overallSuccess;
}

function doesLayoutSatisfySiteRequirements(siteRequirements, spaces, results) {
   var minPoint = [Number.MAX_VALUE, Number.MAX_VALUE];
   var maxPoint = [Number.MIN_VALUE, Number.MIN_VALUE];
   _.each(spaces, function(space) {
      var bbox = space.getBoundingBox();
      minPoint[0] = Math.min(minPoint[0], bbox[0][0]);
      minPoint[1] = Math.min(minPoint[1], bbox[0][1]);
      maxPoint[0] = Math.max(maxPoint[0], bbox[1][0]);
      maxPoint[1] = Math.max(maxPoint[1], bbox[1][1]);
   });

   if (minPoint[0] == Number.MAX_VALUE)
      return true;

   results.actualSite = {
      width: maxPoint[0] - minPoint[0],
      height: maxPoint[1] - minPoint[1]
   };

   if (typeof(siteRequirements) != 'object') {
      // No site requirement
      return true;
   }

   results.intendedSite = _.clone(siteRequirements);

   // Return whether it fits
   if (results.actualSite.width <= results.intendedSite.width && results.actualSite.height <= results.intendedSite.height)
      return true;

   // See if it fits when rotated 90º
   else if (results.actualSite.height <= results.intendedSite.width && results.actualSite.width <= results.intendedSite.height) {
      results.actualSite = {width: results.actualSite.height, height: results.actualSite.width};
      return true;
   }

   return false;
}

return {
   findOverlapsInLayout: findOverlapsInLayout,
   doesLayoutSatisfySpaceRequirements: doesLayoutSatisfySpaceRequirements,
   doesLayoutSatisfyAdjacencyRequirements: doesLayoutSatisfyAdjacencyRequirements,
   doesLayoutSatisfySiteRequirements: doesLayoutSatisfySiteRequirements,
   clone: clone,
   doSpacesOverlap: doSpacesOverlap,
   doesSpaceOverlap: doesSpaceOverlap,
   getLevelSpacesConnect: getLevelSpacesConnect,
   whereDoSpacesConnect: whereDoSpacesConnect,
   getSpacePerimeterLines: getSpacePerimeterLines,
   getProgramReqsError: getProgramReqsError,
   getProximityReqsError: getProximityReqsError,
   getBillableSpaceError: getBillableSpaceError,
   getSiteFitError: getSiteFitError,
   getRotatedDims: getRotatedDims,
   getUsageName: getUsageName
}

});


