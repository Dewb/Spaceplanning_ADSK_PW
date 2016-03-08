define(['analysis/analyzers', 'views/requirementsEditor'], function(analyzers, req) {
   var oldPaths = [];
   var adjacencyResults = [];
   var displayingAdjacenciesFor = null;
   var displayedAdjacenciesMustOriginateInSpace = null; // Rather than all paths that happen to go through the space
   var adjacencyDisplayInvalidated = false;
   var opacityFilterId;
   var artificiallyExtendedStairs = [];
   var roomAlertLabels = [];
   var showingRoomAlertLabels = false;
   var overlaps;

   var resetPaths = function() {
      for (var pathId in oldPaths)
         scene.remove(oldPaths[pathId]);
      oldPaths = [];

      if (opacityFilterId)
         removeAppearanceFilter(opacityFilterId);
      opacityFilterId = null;

      Groundhog.get(artificiallyExtendedStairs.join(","), {passive: true}, function (stairs) {
         delete stairs.forceDrawUncropped;
         addToRedrawList(stairs.get("uniqueId"));
      });
      artificiallyExtendedStairs = [];
   }

   var drawPath = function(path, color) {
      var material = new THREE.MeshLambertMaterial({
         color: color
      });

      var points3 = _.map(path, function(pt){ return new THREE.Vector3(pt[0], pt[1], (pt[2]-1)*3.5 + 1); });

      var geometry;
      var segments;
      if (points3.length == 2) {
         segments = 1;
         geometry = new THREE.LineCurve3(points3[0], points3[1]);
      }
      else if (points3.length == 4) {
         segments = 25;
         geometry = new THREE.CubicBezierCurve3(points3[0], points3[1], points3[2], points3[3]);
      }
      else {
         segments = points3.length * 5;
         geometry = new THREE.SplineCurve3(points3);
      }

      var tube = new THREE.TubeGeometry(geometry, segments, 0.1, 6, false, false);
      var tubeMesh = new THREE.Mesh(tube, material);
      oldPaths.push(tubeMesh);
      scene.add(tubeMesh);
      tubeMesh.isPath = true;

      initViewCacheItem(tubeMesh);
   }

   window.drawAdjacenciesForElemId = function(id, originateInSpaceOnly) {
      if (!adjacencyDisplayInvalidated && displayingAdjacenciesFor == id && displayedAdjacenciesMustOriginateInSpace == !!originateInSpaceOnly)
         return;

      displayingAdjacenciesFor = id;
      displayedAdjacenciesMustOriginateInSpace = !!originateInSpaceOnly;
      adjacencyDisplayInvalidated = false;
      resetPaths();

      if (_.isNull(id) || _.isUndefined(id))
         return;

      var roomPaths = {};

      for (var i in adjacencyResults) {
         var result = adjacencyResults[i];
         if (!result.reachable)
            continue;

         var found = false;
         if (displayedAdjacenciesMustOriginateInSpace)
            found = (result.fromSpace.data.uniqueId == id);
         else {
            for (var j in result.pathRooms) {
               var room = result.pathRooms[j];
               if (room.spaceId == id) {
                  found = true;
                  break;
               }
            }
         }

         if (!found)
            continue;

         _.each(result.pathRooms, function(room) {
            var roomPathData = roomPaths[room.spaceId];
            if (roomPathData == null) {
               roomPathData = [];
               roomPaths[room.spaceId] = roomPathData;
            }

            var previousPath = _.find(roomPathData, function(old) { return _.isEqual(old.internalPath, room.internalPath) });
            if (previousPath == null)
               roomPathData.push({internalPath: room.internalPath, success: result.satisfied});

            // If we found a previous path that looked successful, and we're not, prefer marking it unsuccessful
            else if (!result.satisfied)
               previousPath.success = false;
         });
      }

      _.each(roomPaths, function(paths, roomId) {
         _.each(paths, function(path) {
            if (path.success)
               drawPath(path.internalPath, 0x00ff00);
            else
               drawPath(path.internalPath, 0xffff00);
         });
      });

      var relevantRooms = _.keys(roomPaths);
      if (relevantRooms.length > 0 && !window.disablePathDimming) {
         // Draw the un-cropped version of stairs when we draw their paths
         Groundhog.get(relevantRooms.join(","), {passive: true}, function(elem) {
            if (elem.get("type") == "Stairs") {
               elem.forceDrawUncropped = true;
               artificiallyExtendedStairs.push(elem.get("uniqueId"));
               addToRedrawList(elem.get("uniqueId"));
            }
         });

         opacityFilterId = addWireframeFilter(function(elem) { return _.contains(relevantRooms, elem.get("uniqueId"))});
      }
   }

   window.getAdjacencyResultsFromElemId = function(id) {
      return _.filter(adjacencyResults, function(adj) { return adj.fromSpace.get("uniqueId") == id } );
   }

   var unionSolidsToCSG = function(solids) {
      var fullGeometry;
      for (var id in solids) {
         if (solids[id].geometry.faces) {
            var elemCSG = new ThreeBSP(solids[id]);
            if (fullGeometry == null)
               fullGeometry = elemCSG;
            else
               fullGeometry = fullGeometry.union(elemCSG);
         }
      }
      return fullGeometry;
   }

   window.clearRoomAlertLabels = function() {
      _.each(roomAlertLabels, function(text) { removeText(text); });
      roomAlertLabels = [];
   }

   var drawRoomAlertLabels = function() {
      clearRoomAlertLabels();

      if (!showingRoomAlertLabels)
         return;

      var badSpaces = {};

      // Pre-filter space warnings to only draw each one once
      _.each(adjacencyResults, function(result) {
         if (!result.satisfied)
            badSpaces[result.fromSpace.get("uniqueId")] = (result.reachable ? 1 : 2);
      });

      // Put '!' over bad spaces, colored based on their badness
      var isolatedLevel = getIsolatedLevel();
      _.each(badSpaces, function(badness, id) {
         Groundhog.get(id, {passive: true}, function(elem) {
            var textPosition = arrToVector3(elem.getPosition());

            if (isolatedLevel != null && !almostEqual(textPosition.z, (isolatedLevel - 1) * 3.5))
               return;

            textPosition.z += 3.5;
            roomAlertLabels.push(createText("!", textPosition, 2.5, (badness == 2 ? 0xff0000 : 0xffff00)));
         });
      });
   }
   $("body").on("levelIsolated", drawRoomAlertLabels);

   window.showRoomAlertLabels = function() {
      showingRoomAlertLabels = true;
      drawRoomAlertLabels();
   }

   window.hideRoomAlertLabels = function() {
      showingRoomAlertLabels = false;
      clearRoomAlertLabels();
   }

   var overlapGeometry = [];

   var clearOverlaps = function() {
      for (var i in overlapGeometry)
         scene.remove(overlapGeometry[i]);
      overlapGeometry = [];
   }

   var drawOverlaps = function() {
      clearOverlaps();
      var material = new THREE.MeshPhongMaterial({ color: 0xff0000 });

      var isolatedLevel = getIsolatedLevel();
      if (isolatedLevel != null) {
         var levelVoidThree = new THREE.Mesh(new THREE.BoxGeometry(10000, 10000, 3.5), material);
         levelVoidThree.position.z = 3.5 * (isolatedLevel - 0.5);
         var levelVoidCSG = new ThreeBSP(levelVoidThree);
      }

      for (var i in overlaps) {
         var overlap = overlaps[i];

         Groundhog.getAll(overlap.join(","), function(elemMap) {
            var elems = _.values(elemMap);
            if (elems.length != 2) {
               console.log("Warning: found overlap of unavailable elements");
               return;
            }

            var geom1 = elems[0].cloneModelGeometry();
            var geom2 = elems[1].cloneModelGeometry();
            if (geom1 == null || geom2 == null) {
               console.log("Warning: found overlap of elements without solid geometry");
               return;
            }

            var union1 = unionSolidsToCSG(geom1);
            var union2 = unionSolidsToCSG(geom2);
            if (union1 && union2) {
               var intersection = union1.intersect(union2);

               if (levelVoidCSG != null)
                  intersection = intersection.intersect(levelVoidCSG);

               var intersectionMesh = intersection.toMesh(material);

               scene.add(intersectionMesh);
               overlapGeometry.push(intersectionMesh);
            }
         });
      }
   }
   $("body").on("levelIsolated", drawOverlaps);

   window.findUnnecessaryHallways = function(callback) {
      var onlyHallwaysAdjacentToAGivenSpace = [];
      Groundhog.getEach(".Space", { passive: true }, function (space) {
         var adjacentHalls = [];
         _.each(space.neighbors, function (n) {
            Groundhog.get(n.id, function (nElem) {
               if (nElem.data.type == "Hall") {
                  adjacentHalls.push(n.id);
               }
            });
         });
         if (adjacentHalls.length == 1) {
            onlyHallwaysAdjacentToAGivenSpace.push(adjacentHalls[0]);
         }
      });
      Groundhog.getAll(".Hall, .Stairs", function (elems) {
         var results = [];
         _.each(elems, function(elem){
            var necessary = _.any(adjacencyResults, function(result) {
               return _.any(result.pathRooms, function(room) { return room.spaceId == elem.data.uniqueId });
            });
            if (!necessary && !_.contains(onlyHallwaysAdjacentToAGivenSpace, elem.data.uniqueId))
               results.push(elem.data.uniqueId);
         });
         callback(results);
      });
   }

   var findUnnecessaryEgressSpaces = function(callback) {
      Groundhog.getAll(".Space", function(elems) {
         var results = [];
         _.each(elems, function (elem) {
            if (elem.data.type == "Space" && elem.data.usageName == "egress") {
               var necessary = _.any(adjacencyResults, function(result) {
               return _.any(result.pathRooms, function(room) { return room.spaceId == elem.data.uniqueId });
            });
            if (!necessary)
               results.push(elem.data.uniqueId);
            };
         });
         callback(results);
      });
   }

  var updateStairHeights = function() {
      Groundhog.getEach(".Stairs", function(stair) {
         if ("neighbors" in stair && stair.neighbors.length > 0) {
            var low = Number.MAX_VALUE;
            var high = -1;
            _.each(stair.neighbors, function(n) {
               low = Math.min(low, n.connectionPoint[2]);
               high = Math.max(high, n.connectionPoint[2]);
            });
            stair.extents = [ low, high ];
         } else {
            stair.extents = [ 1, 2 ];
         }
         stair.updateGraphics();
      });
   }

   var calculateAggregateSpaceAssignment = function(spaces) {
      var assignment = {};
      _.each(spaces, function(space) {
         var worldDims = space.getWorldDimensions();
         if (worldDims == null)
            return;

         var usageName = analyzers.getUsageName(space);
         if (assignment[usageName] == null)
            assignment[usageName] = { area: 0, quantity: 0 };
         assignment[usageName].area += worldDims[0]*worldDims[1];
         assignment[usageName].quantity += 1;
      });
      return assignment;
   }

   var storeSpaceAssignment = function(aggregateSpaceAssignment) {
      Groundhog.getAll(".SpaceAssignmentResults, .Space, .Hall, .Stairs", function(elems) {
         var groupedElems = _.groupBy(elems, function(elem) { return elem.data.type == "SpaceAssignmentResults" });
         var spaces = groupedElems[false] || [];

         var allResults = groupedElems[true] || [];
         if (allResults.length > 1)
            console.log("Warning: multiple potential space assignment results within a single design option");

         var results;
         if (allResults.length == 0) {
           var resultsData = new SpaceAssignmentResultsData(getNewId(), {});
           window.preAddElementForTransaction(resultsData.uniqueId);
           results = Groundhog.addElement(resultsData);
         }
         else
            results = allResults[0];

         if (!_.isEqual(aggregateSpaceAssignment, results.get("assignedSpace")))
            results.set("assignedSpace", aggregateSpaceAssignment);
      });
   };

   var storeFootprint = function(siteFitResults) {
      if (siteFitResults.actualSite == null)
         return;

      Groundhog.getAll(".FuzzySiteDimensions", function(elems) {
         // The goal and the analysis results use the same class, but the goal has the id "targetSiteDimensions"
         var analysisResultElem;
         _.each(elems, function(elem, id) {
            if (id != "targetSiteDimensions")
               analysisResultElem = elem;
         });

         if (analysisResultElem == null) {
            var data = new FuzzySiteDimensionsData(getNewId());
            window.preAddElementForTransaction(data.uniqueId);
            analysisResultElem = Groundhog.addElement(data);
         }

         analysisResultElem.set("width", siteFitResults.actualSite.width);
         analysisResultElem.set("height", siteFitResults.actualSite.height);
      });
   }

   window.storeOptionScore = function(analysisResults) {
      var programReqError = analyzers.getProgramReqsError(analysisResults.spaceAllocation);
      Groundhog.setDesignOptionMetadata("programScore", programReqError[0]);
      Groundhog.setDesignOptionMetadata("scorePerProgramReq", programReqError[1]);

      var adjacencyResults = analysisResults.layout;
      if (adjacencyResults.length>0)
         Groundhog.setDesignOptionMetadata("proximityScore", analyzers.getProximityReqsError(adjacencyResults));
      else
         Groundhog.setDesignOptionMetadata("proximityScore", null);

      var formattedResults = {};
      if (analysisResults.siteFit != null)
         formattedResults.siteFit = analyzers.getSiteFitError(analysisResults.siteFit);
      if (analysisResults.aggregateSpaceAssignment != null)
         formattedResults.billableSpace = analyzers.getBillableSpaceError(analysisResults.aggregateSpaceAssignment);
      if (analysisResults.customScores != null) {
         formattedResults.customScores = analysisResults.customScores;
      }

      if (!_.isEmpty(formattedResults))
         Groundhog.setDesignOptionMetadata("analysisResults", formattedResults);
   };

   var needAnalysisUpdate = false

   Groundhog.watchAll(".Space, .Hall, .Stairs", function (elems) {
      for (id in elems) {
         var elem = null
         Groundhog.getEach(id, { passive: true }, function(e) { elem = e; });
         if (elem.data.usageName != "egress" && !("neighbors" in elem)) {
            // This element has never been analyzed
            needAnalysisUpdate = true
            break;
         }
      }
   });

   window.setInterval(function() {
      if (needAnalysisUpdate) {
         needAnalysisUpdate = false;
         runAnalysis( function(analysisResults) {
            storeOptionScore(analysisResults);
         });
      }
   }, 250);

   window.runAnalysis = function(completionCallback, verbose) {
      window.getRequirements(function (requirements) {
         window.clearVirtualSpaces();
         Groundhog.getAll(".Space, .Hall, .Stairs", function (elems) {
            resetPaths();

            var unitsSquared = window.displayUnits + "²";

            var layout = _.values(elems);

            overlaps = analyzers.findOverlapsInLayout(layout);
            drawOverlaps();

            adjacencyResults = [];
            var adjacencySatisfied = analyzers.doesLayoutSatisfyAdjacencyRequirements(requirements, layout, adjacencyResults);

            if (showingRoomAlertLabels)
               drawRoomAlertLabels();

            if (displayingAdjacenciesFor) {
               drawAdjacenciesForElemId(displayingAdjacenciesFor);
               adjacencyDisplayInvalidated = true;
            }

            if (requirements.site != null) {
               var siteFit = {};
               analyzers.doesLayoutSatisfySiteRequirements(requirements.site, layout, siteFit);
               storeFootprint(siteFit);
            }

            if (verbose) console.log(adjacencySatisfied ? "-- Adjacency requirements satisfied." : "** Adjacency requirements NOT satisfied.");
            
            findUnnecessaryEgressSpaces(function (ids) { _.each(ids, function (id) { Groundhog.deleteElement(id); })});
            updateStairHeights();

            var aggregateSpaceAssignment = calculateAggregateSpaceAssignment(elems);
            storeSpaceAssignment(aggregateSpaceAssignment);
            
            if (_.isFunction(completionCallback)) {
               var analysisResults = {
                  spaceAllocation: [],
                  customScores: {},
                  aggregateSpaceAssignment: aggregateSpaceAssignment, // This is redundant with the previous one, but includes circulation. Should unify them.
                  layout: adjacencyResults
               };
               analyzers.doesLayoutSatisfySpaceRequirements(requirements, layout, analysisResults.spaceAllocation);

               // Run custom scripts
               _.each(requirements.scripts, function(script, id) {
                  var numResults = 0;
                  var resultSum = 0;
                  _.each(elems, function(elem) {
                     var result;
                     var setResult = function(newResult) {
                        result = newResult;
                     };
                     try {
                        // THIS IS NOT SANDBOXED - this code should never enter production
                        eval(script.code);
                     }
                     catch(e) {
                        console.log("Error running script \"" + script.name + "\": " + e.message);
                     }
                     if (result != null) {
                        numResults++;
                        resultSum += Math.max(Math.min(result, 1), 0); // Confine to [0,1]
                     }
                  });

                  if (numResults > 0)
                     analysisResults.customScores[id] = resultSum / numResults;
               });

               if (siteFit != null)
                  analysisResults.siteFit = siteFit;

               completionCallback(analysisResults);
            }
         });
      });
   }

   var analysisPendingCancellation = false;
   window.cancelAnalyzeDesignOptions = function() {
      analysisPendingCancellation = true;
   }

   window.currentlyAnalyzingDesignOptions = false;
   window.analyzeDesignOptions = function(projectId, optionIds, completionCallback) {
      var doNextAnalysis = function() {
         // Halting condition
         if (optionIds.length == 0 || analysisPendingCancellation) {
            currentlyAnalyzingDesignOptions = false;
            analysisPendingCancellation = false;
            if (completionCallback != null)
               completionCallback();
            return;
         }

         selectProject(projectId, optionIds.pop(), true);
         runAnalysis( function(analysisResults) {
            storeOptionScore(analysisResults);
            setTimeout(doNextAnalysis, 200);
         });
      };
      currentlyAnalyzingDesignOptions = true;
      analysisPendingCancellation = false;
      doNextAnalysis();
   };
})