define(['analysis/analyzers', 'scene'], function(analyzers, scene) {
   // The "boundaries" element used below is:
   //    a map from level number (1+)
   //       to a map keyed axis orientation ("x" or "y") 
   //          to a map keyed by the segment's coordinate on its perpendicular axis
   //             to an array of [start, end] points on the parallel axis.
   // E.g.: {2: {x: {5.5: [[-5, 1], [3, 4]]}}}

   var setBoundaryFromPerimeterLine = function(line, levelData) {
      var axisOrientation;
      var sharedCoordinate;
      var range;
      if (almostEqual(line[0][1], line[1][1])) {
         axisOrientation = "x";
         sharedCoordinate = line[0][1];
         range = [line[0][0], line[1][0]];
      }
      else {
         axisOrientation = "y";
         sharedCoordinate = line[0][0];
         range = [line[0][1], line[1][1]];
      }

      if (range[0] > range[1])
         range.reverse();

      var orientationData = levelData[axisOrientation];
      if (orientationData == null) {
         orientationData = {};
         levelData[axisOrientation] = orientationData;
      }
      
      var positionData =  orientationData[sharedCoordinate];
      if (positionData == null) {
         positionData = [];
         orientationData[sharedCoordinate] = positionData;
      }
      positionData.push(range);
   }

   var getBoundariesForElem = function(elem, boundaries) {
      var perimeterLines = analyzers.getSpacePerimeterLines(elem);
      var minLevel = elem.getLevel();
      var numLevels = 1;
      if (minLevel == "*") {
         minLevel = elem.displayedBaseLevel;
         numLevels = elem.displayedLevels;
      }
      else if (!_.isNumber(minLevel)) {
         console.log("Space " + elem.get("uniqueId") + " didn't return a numeric level or '*'. This function is designed to operate on single-level elements or stairs. It returned: " + minLevel);
         return;
      }

      for (var level = minLevel; level < minLevel + numLevels; level++) {
         var levelData = boundaries[level];
         if (levelData == null) {
            levelData = {};
            boundaries[level] = levelData;
         }
         _.each(perimeterLines, function(line) {
            setBoundaryFromPerimeterLine(line, levelData);
         });
      }
   }

   var mergeSegments = function(segments) {
      // Sort by their start point
      var sorted = _.sortBy(segments, function(seg) { return seg[0]; })

      var merged = [];
      var mergedStart;
      var potentialEnd;

      _.each(sorted, function(seg) {
         if (potentialEnd != null && seg[0] > potentialEnd) {
            merged.push([mergedStart, potentialEnd]);
            mergedStart = null;
            potentialEnd = null;
         }

         if (mergedStart == null)
            mergedStart = seg[0];

         if (potentialEnd == null || seg[1] > potentialEnd)
            potentialEnd = seg[1];
      });
      if (mergedStart != null)
         merged.push([mergedStart, potentialEnd]);

      return merged;
   }

   var mergeOverlappingBoundaries = function(overlapping) {
      var merged = [];

      _.each(overlapping, function(levelData, levelString) {
         var levelHeight = (parseFloat(levelString) - 1) * 3.5;
         _.each(levelData, function(orientationData, axis) {
            _.each(orientationData, function(segments, positionString) {
               var position = parseFloat(positionString);
               var mergedSegments = mergeSegments(segments);
               _.each(mergedSegments, function(segment) {
                  if (axis == "x")
                     merged.push([[segment[0], position, levelHeight], [segment[1], position, levelHeight]]);
                  else
                     merged.push([[position, segment[0], levelHeight], [position, segment[1], levelHeight]]);
               });
            })
         });
      });
      return merged;
   };

   var getAllBoundaries = function(callback) {
      Groundhog.getAll(".Space, .Hall, .Stairs", function(elems) {
         var boundaries = {};
         _.each(elems, function(elem) {
            if (elem.get("usageName") == "egress")
               return;
            getBoundariesForElem(elem, boundaries);
         });
         var merged = mergeOverlappingBoundaries(boundaries);
         callback(merged);
      });
   };

   var generateWalls = function(callback) {
      getAllBoundaries(callback);
   };

   var wallsVisible = false;
   var nonwallVisibilityFilterId;
   var wallGeometry = [];
   var toggleWallVisibility = function() {
      var activeScene = scene.getActiveScene();
      if (wallsVisible) {
         wallsVisible = false;
         removeAppearanceFilter(nonwallVisibilityFilterId);
         _.each(wallGeometry, function(geom) {
            activeScene.remove(geom);
         });
         wallGeometry = [];
         scene.requestRedraw();
      }
      else {
         wallsVisible = true;
         drawAdjacenciesForElemId(null);
         unselectAll();

         generateWalls(function(walls) {
            nonwallVisibilityFilterId = addVisibilityFilter(function(elem) { return false; });

            var wallWidth = 0.2;

            _.each(walls, function(wall) {
               var length = arrToVector3(wall[0]).distanceTo(arrToVector3(wall[1])) + wallWidth;

               var object = new THREE.Object3D();
               var geometry = new THREE.BoxGeometry(length, wallWidth, 3.5);
               var material = new THREE.MeshLambertMaterial({transparent: true, color: 0x999999});
               var mesh = new THREE.Mesh(geometry, material);
               mesh.position.z = 1.75;
               mesh.castShadow = true;
               object.add(mesh);

               var edges = new THREE.BoxHelper();
               edges.scale.set(length/2, wallWidth/2, 3.5/2);
               edges.material.color.setRGB(1, 1, 1);
               edges.material.transparent = true;
               edges.material.opacity = 1.0;
               edges.position.z = 1.75;
               object.add(edges);

               object.position = arrToVector3(wall[1]).lerp(arrToVector3(wall[0]), 0.5);

               if (almostEqual(wall[0][0], wall[1][0]))
                  object.rotation.z = Math.PI/2;

               activeScene.add(object);
               wallGeometry.push(object);
            });

            scene.requestRedraw();
         });
      }
   };

   window.toggleWallVisibility = toggleWallVisibility;

   return {
      generate: generateWalls,
      toggleVisibility: toggleWallVisibility
   }
});