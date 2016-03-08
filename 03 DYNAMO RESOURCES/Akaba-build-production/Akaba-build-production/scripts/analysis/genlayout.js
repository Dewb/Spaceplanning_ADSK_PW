define(["./analyzers"], function(analyzers) {

window.callRestGeneratorService = function(serviceURL, updateStatusCallback, completionCallback) {
   window.getRequirements(function (requirements) {
      $.ajax({
         url: serviceURL + "/generator",
         method: "POST",
         data: JSON.stringify({
            requirements: requirements,
            settings: {
               grid: 0.5,
               designs: 1
            }
         }),
         processData: false,
         dataType: "json",
         contentType: "application/json;charset=UTF-8"
      })
      .done(function(jobId, textStatus, jqXHR) {
         updateStatusCallback(serviceURL, jobId);
         pollForJobResults(serviceURL, jobId, completionCallback);
      })
      .fail(function(jqXHR, textStatus, error) {
         console.log("Request at: " + serviceURL + " -> FAILED: " + jqXHR.status);
         updateStatusCallback(serviceURL);
      });
   }, true);
}

//This array stores the serviceURL of the jobs selected to be cancelled.
//It is a temporary solution until the "DELETE" method is supported by all generators.
window.jobsToCancel = [];
window.cancelRestGeneratorService = function(serviceURL, jobId, completionCallback) {
   $.ajax ({
      url: serviceURL + "/generator/job/" + jobId,
      method: "DELETE"
   })
   .done(function(jqXHR, textStatus, error) {
      completionCallback;
      console.log("Cancel request succeeded: " + jobId);
   })
   .fail(function(jqXHR, textStatus, error) {
      //In the case of a failed "DELETE" request, the returned results
      //will just be ignored and not stored as a design option
      completionCallback;
      console.log("Cancel request failed: " + jobId);
   });
};

pollForJobResults = function(serviceURL, jobId, completionCallback) {
   $.ajax({
      url: serviceURL + "/generator/job/" + jobId,
      method: "GET"
   })
   .done(function(jobdata, jqXHR, textStatus, error) {         
      if (!_.contains(jobsToCancel, serviceURL+jobId)) {
         if (jobdata.status == "completed") {
            processJobResults(serviceURL, jobId, completionCallback);
         } else {
            setTimeout(function() { 
               pollForJobResults(serviceURL, jobId, completionCallback); }, 2000);
         }
      }
   })
   .fail(function(jqXHR, textStatus, error) {
      console.log("GET request failed: " + jobId);
   });
};

processJobResults = function(serviceURL, jobId, completionCallback) {
   $.ajax({
         url: serviceURL + "/generator/job/" + jobId + "/design/1",
         method: "GET"
   })
   .done(function(design) {

      window.clearRoomAlertLabels();
      
      Groundhog.createDesignOption(null, function(optionId, projectId) {
         completionCallback(jobId, optionId, serviceURL);
         selectProject(projectId, optionId, true);
         
         for (var j in design.spaces) {
            var json = design.spaces[j];
            if (json.usageName == "Hall") {
               var length, width, rot;
               if (json.dimensions[0] > json.dimensions[1]) {
                  length = json.dimensions[0];
                  width = json.dimensions[1];
                  rot = 0;
               } else {
                  length = json.dimensions[1];
                  width = json.dimensions[0];
                  rot = Math.PI/2;
               }
               var start = [json.position[0] - length / 2 + Hall.defaultWidth()/2, json.position[1], json.position[2]];
               var end = [json.position[0] + length / 2 - Hall.defaultWidth()/2, json.position[1], json.position[2]];
               var hallData = new HallData(getNewId(), start, end, rot);
               if (width != Hall.defaultWidth()) {
                  hallData.width = width;
               }
               var hall = Groundhog.addElement(hallData);
            } else if (json.usageName == "Stairs") {
               var x = json.position[0];
               var y = json.position[1];
               // todo: check existing stairs, only add new if no other floor defines a stair space in the same position
               var length, rot;
               if (json.dimensions[0] > json.dimensions[1]) {
                  rot = Math.PI/2;
               } else {
                  rot = 0;
               }
               var stairData = new StairsData(getNewId(), [x, y, 0], rot, 35);
               var stairs = Groundhog.addElement(stairData);
            } else { // regular space
               var spaceData = new SpaceData(getNewId(), 
                  [json.position[0], json.position[1], json.position[2]], 
                  [json.dimensions[0], json.dimensions[1], json.dimensions[2]], 0);
               spaceData.usageName = json.usageName;
               var space = Groundhog.addElement(spaceData);
            }
         }
         
         for (var j in design.meshes) {
            var json = design.meshes[j];

            var mesh = new MeshData(getNewId(), "shell");
            mesh.position = [0, 0, 0];
            mesh.dimensions = [1, 1, 1];
            mesh.inlineData = json.inlineData;
            mesh.rotation = 0;

            var mesh = Groundhog.addElement(mesh);
         }
         
         window.endTransaction();
      });   
   });
}

function headingToVector(heading) {
   return [Math.cos(heading * Math.PI / 180), Math.sin(heading * Math.PI / 180)];
}

function Ray(origin, direction) {
   this.origin = origin;
   this.direction = direction;
   this.inv_direction = [
      1 / direction[0], 
      1 / direction[1]
   ];
   this.sign = [
      this.inv_direction[0] < 0 ? 1 : 0,
      this.inv_direction[1] < 0 ? 1 : 0
   ];
}

function rayIntersectsSpace(ray, space) {
   // assume space is axis-aligned box
   var center = space.getPosition();
   var dims = space.getWorldDimensions();
   var bounds = [[center[0] - dims[0]/2, center[1] - dims[1]/2], 
              [center[0] + dims[0]/2, center[1] + dims[1]/2]];

   var txmin = (bounds[ray.sign[0]][0] - ray.origin[0]) * ray.inv_direction[0];
   var txmax = (bounds[1-ray.sign[0]][0] - ray.origin[0]) * ray.inv_direction[0];
   var tymin = (bounds[ray.sign[1]][1] - ray.origin[1]) * ray.inv_direction[1];
   var tymax = (bounds[1-ray.sign[1]][1] - ray.origin[1]) * ray.inv_direction[1];

   if (txmin > tymax || tymin > txmax) {
      return false;
   }

   return true;
}

function findAdjacentSpace(spaces, currentSpace, direction) {
   var adjacentSpace = undefined;
   var requirements = { code: { minimumDoorWidth: 0.5 } };
   var ray = new Ray(currentSpace.getPosition(), headingToVector(direction));
   spaces.forEach(function (space) {
      if (rayIntersectsSpace(ray, space) && typeof analyzers.whereDoSpacesConnect(requirements, currentSpace, space) != null) {
         adjacentSpace = space;
      }
   });

   return adjacentSpace;
}

function findSpaceInDirectionMatchingCondition(spaces, currentSpace, direction, testCondition) {
   var adjacentSpace = undefined;
   var requirements = { code: { minimumDoorWidth: 0.5 } };
   var ray = new Ray(currentSpace.getPosition(), headingToVector(direction));
   spaces.forEach(function (space) {
      if (rayIntersectsSpace(ray, space) && testCondition(space)) {
         adjacentSpace = space;
      }
   });

   return adjacentSpace;
}

function backtrack(spaces, currentSpace, moveDirection) {
   var adjacentSpace = findAdjacentSpace(spaces, currentSpace, moveDirection);
   if (typeof adjacentSpace != "undefined") {
      currentSpace = adjacentSpace;
   }

   return currentSpace;
}

function backtrackCirculation(spaces, currentSpace, moveDirection) {
   var adjacentSpace = findSpaceInDirectionMatchingCondition(spaces, currentSpace, moveDirection, spaceIsCirculation);
   if (typeof adjacentSpace != "undefined") {
      currentSpace = adjacentSpace;
   }

   return currentSpace;
}

function positionSpaceRelativeToOtherSpace(space, otherSpace, moveVec)  {
   maybeLog("moveVec: " + JSON.stringify(moveVec));
   var minX = (otherSpace.getWorldDimensions()[0] + space.getWorldDimensions()[0]) / 2;
   var minY = (otherSpace.getWorldDimensions()[1] + space.getWorldDimensions()[1]) / 2;
   var tx = Math.abs(minX / moveVec[0]);
   var ty = Math.abs(minY / moveVec[1]);
   var dx = moveVec[0] * Math.min(tx, ty);
   var dy = moveVec[1] * Math.min(tx, ty);

   space.setPosition([snapScalar(otherSpace.getPosition()[0] + dx), snapScalar(otherSpace.getPosition()[1] + dy), otherSpace.getPosition()[2]]);

   // do a little bit of edge nudging to fix hallway bends that meet at a corner
   if ((otherSpace.data.type == "Hall" && space.data.type == "Hall") &&
      (otherSpace.getWorldDimensions()[0] == space.getWorldDimensions()[1] || 
       otherSpace.getWorldDimensions()[1] == space.getWorldDimensions()[0]) &&
      analyzers.whereDoSpacesConnect({code: { minimumDoorWidth: 0}}, otherSpace, space) == null) {
      if (Math.abs(moveVec[0]) > Math.abs(moveVec[1])) {
         dx += otherSpace.getWorldDimensions()[0] * (moveVec[0] > 0 ? -1 : 1);
      } else {
         dy += otherSpace.getWorldDimensions()[1] * (moveVec[1] > 0 ? -1 : 1);
      }

      //maybeLog("Nudging new space " + JSON.stringify(newSpace) + " to align with " + JSON.stringify(currentSpace));
      space.setPosition([snapScalar(otherSpace.getPosition()[0] + dx), snapScalar(otherSpace.getPosition()[1] + dy), otherSpace.getPosition()[2]]);
   }
}

function slideSpace(space, previousSpace, allSpaces) {
   var delta = Math.random() > 0.5 ? 0.5 : -0.5;

   var axis = 0;
   var spaceBBox = space.getBoundingBox();
   var previousBBox = previousSpace.getBoundingBox();
   if (spaceBBox[0][0] == previousBBox[1][0] || spaceBBox[1][0] == previousBBox[0][0]) {
      axis = 1;
   }

   var p = space.getPosition();
   var goodp = _.clone(p);
   for (var i = 0; i < previousBBox[axis]/delta; i++) {
      if (analyzers.whereDoSpacesConnect({code: { minimumDoorWidth: 1.5 }}, space, previousSpace) != null &&
         !analyzers.doesSpaceOverlap(space, allSpaces)) {
         goodp = _.clone(p);
      }
      p[axis] += delta;
      space.setPosition(p);
   }
   // Finalize space at maximum slide that remains connected but does not overlap any other spaces
   space.setPosition(goodp);
}

function addSpace(spaces, currentSpace, moveVec, newSpaceInfo, rotation) {
   var newSpace = createSpaceFromData(newSpaceInfo.data);
   if (rotation) {
      newSpace.set("rotation", rotation);
   }

   positionSpaceRelativeToOtherSpace(newSpace, currentSpace, moveVec);
   spaces.push(newSpace);
   if (!spaceIsCirculation(newSpace)) {
      slideSpace(newSpace, currentSpace, spaces);
   }
   return newSpace;
}

function generateLayout(genesequence, spaceTypes, options, spaces) {

   spaces = spaces || [];

   var hints = {
      minimumLevelCount: 1
   };
   var siteArea = null;

   Groundhog.get("targetSiteDimensions", {passive: true}, function(target) {
      siteArea = target.get("width") * target.get("height");
   });

   if (siteArea != null) {
      Groundhog.get("program", {passive: true}, function(program) {
         var reqs = program.get("programReqs");
         var totalSpaceRequired = 0;
         _.each(reqs, function(r) {
            totalSpaceRequired += r.requiredArea;
         });

         hints.minimumLevelCount = totalSpaceRequired / siteArea;
      });
   }

   //for (var i = hints.minimumLevelCount - 1; i >= 0; i--) {
      //var h = 3.5 * i
      var h = 0
      var hallData = new HallData(getNewId(), [-8, 0, h], [8, 0, h], 0);
      spaces.push(Groundhog.addElement(hallData));
   //}

   var sequence = analyzers.clone(genesequence, 1);
   var currentSpace = spaces[spaces.length - 1];
   createStairsAdjacentToSpace(currentSpace);

   window.getRequirements(function (requirements) { 
      while (sequence.length >= 3) {
         currentSpace = generateLayoutOneStep(requirements, sequence.splice(0, 3), spaces, spaceTypes, currentSpace, options, hints);
      }
   });
}

function spaceIsCirculation(space) {
   return space.data.type == "Hall";
}

window.verboseGenerator = false;

function maybeLog(string) {
   if (window.verboseGenerator) {
      console.log(string);
   }
}

function createStairsAdjacentToSpace(space) {
   var xdir = Math.random() > 0.5 ? 1 : -1;
   var ydir = Math.random() > 0.5 ? 1 : -1;
   var hand = Math.random() > 0.5 ? 1 : -1;

   var rot = 0;
   if (xdir == -1 && hand == 1) {
      rot = 3 * Math.PI/2;
   } else if (ydir == 1 && hand == -1) {
      rot = Math.PI;
   } else if (xdir == 1 && hand == 1) {
      rot = Math.PI/2;
   }

   var stairDims = analyzers.getRotatedDims(rot, 3, 5);

   maybeLog("Adding stairs at rotation " + rot + " at x:" + xdir + " y:" + ydir + " hand:" + hand);

   var x = space.getPosition()[0] + xdir * (space.getWorldDimensions()[0] / 2 + hand * stairDims[0] / 2);
   var y = space.getPosition()[1] + ydir * (space.getWorldDimensions()[1] / 2 - hand * stairDims[1] / 2)
    var stairData = new StairsData(getNewId(), [x, y, 0], rot, 35);
    return Groundhog.addElement(stairData);
}

function createSpaceFromData(data) {
   var newData = _.clone(data);
   newData.uniqueId = getNewId();
   return Groundhog.addElement(newData);
}

function createCopyOfSpace(space) {
   return createSpaceFromData(space.data);
}

function generateLayoutOneStep(requirements, gene, spaces, spaceTypes, previousSpace, options) {

   var options = options || {
      moveDoNothingRate: 0.0,
      moveBacktrackRate: 0.0,
      spaceDoNothingRate: 0.0,
      overlapMoveUpRate: 0.5,
      overlapMoveDownRate: 0.5,
      constrainMoveAngle: 45,
      requireCirculation: true,
      enforceCounts: true
   };
   var startingSpaceCount = spaces.length;

   var currentSpace = previousSpace || spaces[spaces.length-1];

   var moveGene = gene[0];
   var spaceGene = gene[1];
   var overlapGene = gene[2];

   var newSpaceIndex = Math.floor(spaceGene / (1 - options.spaceDoNothingRate) * spaceTypes.length);

   // Do nothing
   if (moveGene < options.moveDoNothingRate) {

      return;

   // Backtrack   
   } else if (moveGene < options.moveDoNothingRate + options.moveBacktrackRate) {
      // backtrack
      var moveDirection = 360 * (moveGene - options.moveDoNothingRate) / options.moveBacktrackRate;

      if (options.requireCirculation) {
         currentSpace = backtrackCirculation(spaces, currentSpace, moveDirection);
      } else {
         currentSpace = backtrack(spaces, currentSpace, moveDirection);
      }
      maybeLog("Backtracking to " + analyzers.getUsageName(currentSpace) + " at " + JSON.stringify(moveDirection) + " degrees");
   
   // Add space
   } else {

      if (options.requireCirculation && !spaceIsCirculation(currentSpace)) {
         // would place new space, but need to be on circulation 
         // first try the move direction
         var moveDirection = 360 * (moveGene - options.moveDoNothingRate) / options.moveBacktrackRate;
         maybeLog("Looking for circulation at " + JSON.stringify(moveDirection) + " degrees");
         currentSpace = backtrack(spaces, currentSpace, moveDirection);
         // if that fails, get the last placed circulation space
         if (!spaceIsCirculation(currentSpace)) {
            maybeLog("Moving to last placed circulation space");
            for (var ii = spaces.length - 1; ii >=0; ii--) {
               currentSpace = spaces[ii];
               maybeLog("Trying " + analyzers.getUsageName(currentSpace));
               if (spaceIsCirculation(currentSpace)) {
                  break;
               }
            }
         }
         previousSpace = currentSpace;
      }

      // Potentially place new space
      var range = options.moveDoNothingRate + options.moveBacktrackRate;
      var moveDirection = Math.round((360/options.constrainMoveAngle) * (moveGene - range) / (1 - range)) * options.constrainMoveAngle;

      // Don't bother if space type has a count and we've already added enough spaces of this type
      if (options.enforceCounts && "count" in spaceTypes[newSpaceIndex]) {
         var existing = 0;
         spaces.forEach(function(s) { if (s.data.usageName == spaceTypes[newSpaceIndex].data.usageName) { existing++; }});
         if (existing >= spaceTypes[newSpaceIndex].count) {
            return currentSpace;
         }
      }

      var overlapFree = false;
      var numAdded = 0;

      var basicPlaceFn = function (moveVec, rotation) {
         maybeLog("Placing space");
         newSpace = addSpace(spaces, currentSpace, moveVec, spaceTypes[newSpaceIndex], rotation);
         numAdded++;
      }

      var reflectDirectionFn = function (heading) {
         maybeLog("Reflecting heading");
         var moveVec = headingToVector(heading);
         var reflectedVec = [0, 0];
         reflectedVec[0] = -moveVec[1];
         reflectedVec[1] = -moveVec[0];
         return moveVec;
      }

      var maybeMoveToNewLevelFn = function (moveVec, rotation) {
         newSpace = addSpace(spaces, currentSpace, moveVec, spaceTypes[newSpaceIndex], rotation);
         numAdded++;
         var levelShift = null;
         if (overlapGene < options.overlapMoveUpRate) {
            maybeLog("Moving space up");
            levelShift = 1;
         } else if (overlapGene < options.overlapMoveUpRate + options.overlapMoveDownRate) {
            maybeLog("Moving space down");
            levelShift = -1;
         }

         if (levelShift != null) {
            
            var pos = newSpace.getPosition();
            pos[2] = Math.max(pos[2] + levelShift * 3.5, 0);
            newSpace.setPosition(pos);      

            // Potentially add circulation to the new level
            if (options.requireCirculation && spaceIsCirculation(previousSpace)) {
               var newCirc = createCopyOfSpace(previousSpace, 2);
               var newCircPos = newCirc.getPosition();
               newCircPos[2] = Math.max(newCircPos[2] + levelShift * 3.5, 0);
               newCirc.setPosition(newCircPos);
               spaces.push(newCirc);
               numAdded += 1;
            
               // Is the new circulation reachable? If not, add stairs         
               if (analyzers.whereDoSpacesConnect(requirements, spaces[0], newCirc) == null) {
                  var stairs = createStairsAdjacentToSpace(newCirc);
                  spaces.push(stairs);
                  numAdded += 1;
               }
            }
         }
      }


      var strategies = [ function() { basicPlaceFn(headingToVector(moveDirection), 0)}, 
                         function() { basicPlaceFn(headingToVector(moveDirection), Math.PI * 0.5)},
                         function() { basicPlaceFn(reflectDirectionFn(moveDirection), 0)}, 
                         function() { basicPlaceFn(reflectDirectionFn(moveDirection), Math.PI * 0.5)}, 
                         function() { maybeMoveToNewLevelFn(headingToVector(moveDirection), 0)},
                         function() { maybeMoveToNewLevelFn(headingToVector(moveDirection), Math.PI * 0.5)},
                         function() { maybeMoveToNewLevelFn(reflectDirectionFn(moveDirection), 0)},
                         function() { maybeMoveToNewLevelFn(reflectDirectionFn(moveDirection), Math.PI * 0.5)} ];

      for (k in strategies)
      {
         numAdded = 0;
         strategies[k]();

         var overlapResult = analyzers.findOverlapsInLayout(spaces) == null;
         var siteResult = analyzers.doesLayoutSatisfySiteRequirements(requirements.site, spaces, {});
         if (overlapResult && siteResult) {
            overlapFree = true;
            break;
         } else {
            maybeLog("Removing " + numAdded + " added spaces");
            if (!overlapResult) maybeLog("Overlap test failed");
            if (!siteResult) maybeLog("Site test failed");
            spaces.splice(numAdded * -1, numAdded).forEach(function(deleted) { Groundhog.deleteElement(deleted.get("uniqueId")); });
         }
      }

      if (!overlapFree) {
         return currentSpace;
      } else {
         currentSpace = newSpace;
      }

      maybeLog("Adding new " + analyzers.getUsageName(currentSpace) + " at " + JSON.stringify(moveDirection) + " degrees");
   }
   if (spaces.length > startingSpaceCount) {
      var c = spaces.length - startingSpaceCount;
      maybeLog("Added " + c + " space(s)");
   }
   maybeLog("Current space is " + analyzers.getUsageName(currentSpace));

   return currentSpace;
}

return {
   generateLayout: generateLayout,
   generateLayoutOneStep: generateLayoutOneStep,
   createCopyOfSpace: createCopyOfSpace
}

});
