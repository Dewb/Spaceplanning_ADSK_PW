Groundhog.watchEach(".Stairs", function(elem) { addToRedrawList(elem.get("uniqueId")); });

var maxStairsLevel = 2;

window.standardStairsParameters = {
   width: 3,
   length: 5,
   stepsPerRun: 8,
   landingLengthRatio: (1/6) // The fraction of the length that's occupied by each landing
}

Stairs.prototype.updateGraphics = function(force) {
   var geom = this.getElementGeometry();

   // Only draw one level of stairs if the user has isolated a level
   if (!this.forceDrawUncropped)
      var isolatedLevel = getIsolatedLevel();

   var numLevels = maxStairsLevel;
   var baseLevel = 0;
   if ("extents" in this) {
      numLevels = this.extents[1] - this.extents[0] + 1;
      baseLevel = this.extents[0];
   }

   if (force || geom == null || this.displayedLevels != numLevels || this.displayedBaseLevel != baseLevel || 
       this.displayedMaxLevel != maxStairsLevel || isolatedLevel != this.isolatedLevel) {
      this.displayedLevels = numLevels;
      this.displayedBaseLevel = baseLevel;
      this.displayedMaxLevel = maxStairsLevel;
      this.isolatedLevel = isolatedLevel;

      // Whether the top floor of stairs should be empty
      // (which it usually should be except for graphical reasons, since you don't need to walk a floor above the roof)
      var drawEmptyTopRun = !(isolatedLevel != null && isolatedLevel < maxStairsLevel);

      geom = createStairsGeometry(isolatedLevel != null ? 1 : numLevels, drawEmptyTopRun);

      // Add guide lines to visualize where to drag spaces on unconnected levels to to connect them to this stairwell
      // TODO: modularize this 
      // TODO: only show guides when dragging this or another space
      if (isolatedLevel == null) {
         var lineMaterial = new THREE.LineBasicMaterial({ color: 0xCCCCCC });
         for (var i = 0; i < 4; i++) {
            var guides = new THREE.Geometry();
            var bottomZ = -(baseLevel - 1) * 3.5;
            var topZ = (maxStairsLevel - (baseLevel - 1)) * 3.5;
            var v1 = {
               x: (i < 2 ? 3 : -1*3) / 2, 
               y: (i % 2 ? 5 : -5) / 2, 
               z: bottomZ
            };
            var v2 = {
               x: v1.x, 
               y: v1.y, 
               z: topZ
            };
            geom.add(createLine(v1, v2, lineMaterial, false));

            var size = { x: 3, y: 5 };
            var pos = { x: 0, y: 0, z: bottomZ };
            geom.add(createFlatEdges(size, pos, lineMaterial, false));
            pos.z = topZ;
            geom.add(createFlatEdges(size, pos, lineMaterial, false));
         }
      }
      
      this.setElementGeometry(geom);
   }

   geom.position = arrToVector3(this.get("position"));
   if (isolatedLevel != null)
      geom.position.z = (isolatedLevel - 1) * 3.5;
   else
      geom.position.z = (baseLevel - 1) * 3.5;

   geom.rotation.z = this.get("rotation");
};

Stairs.prototype.getWorldDimensions = function() {
   var width = 3;
   var length = 5;
   return getRotatedDims(this.get("rotation"), width, length, 3.5);
};

Stairs.prototype.getLevel = function() {
   return "*";
}

Stairs.prototype.keepOnGround = true;

var levelNumberWatchers = [];
var maxBuildingLevel = 1;
watchNumberOfLevels = function(callback) {
   levelNumberWatchers.push(callback);
   callback(maxBuildingLevel);
}

getNumberOfLevels = function() { return maxBuildingLevel; };

Groundhog.watchAll(".Hall, .Space", {passive: true}, function(elems) {
   var newMaxLevel = 1; // Always have at least one level, even without anything on it
   for (var id in elems) {
      var elem = elems[id];
      var level = Math.round(elem.getPosition()[2] / 3.5) + 1;
      if (level > newMaxLevel)
         newMaxLevel = level;
   }

   if (maxBuildingLevel != newMaxLevel) {
      maxBuildingLevel = newMaxLevel;
      _.each(levelNumberWatchers, function(cb) { cb(maxBuildingLevel); });
   }
});

var addAllStairsToRedrawList = function() { Groundhog.getEach(".Stairs", {passive: true}, function(stairs) { addToRedrawList(stairs.get("uniqueId")); }) };

// Update stair height for the number of levels in the building
watchNumberOfLevels(function(maxLevel) {
   // Stairs should never span less than two floors
   if (maxLevel < 2)
      maxLevel = 2;

   if (maxLevel != maxStairsLevel) {
      maxStairsLevel = maxLevel;

      // Force a redraw of all stairs to the new level
      addAllStairsToRedrawList();
   }
});

// Only draw one level of stairs if the user has isolated a level
$("body").on("levelIsolated", addAllStairsToRedrawList);

// Renders a stairwell and stairs
function createStairsGeometry(numLevels, drawEmptyTopRun) {
   var object = new THREE.Object3D();

   var stairsMaterial = new THREE.LineBasicMaterial({ color: 0x777777, transparent: true, opacity: 1 });
   var boundaryMaterial = new THREE.LineBasicMaterial({ color: 0xffffff, transparent: true, opacity: 1 });

   var length = standardStairsParameters.length;
   var width = standardStairsParameters.width;
   var levelHeight = 3.5;
   var numSteps = standardStairsParameters.stepsPerRun*2 + 2;
   var landingLength = length * standardStairsParameters.landingLengthRatio;

   var dims = [width, length, numLevels*3.5];
   var color = { r: 0.6, g: 0.6, b: 0.6 };
   object.add(createBox(dims, color, 0.3, true, false));
   object.add(createBox(dims, color, 0.3, true, true));
   object.add(createBoxEdges(dims));

   var numRuns = numLevels - 1;
   if (drawEmptyTopRun)
      var numRuns = numLevels - 1;
   else
      var numRuns = numLevels;

   var shaftSize = { x:width, y:length };
   var landingSize = { x: width, y: landingLength };
   var landingPos = { x: 0, y: (length - landingLength)/2, z: 0 };
   for (var i = 0; i < numRuns; i++) {
      // Landings
      landingPos.z = levelHeight*i;
      object.add(createFlatEdges(landingSize, landingPos, stairsMaterial, true));
      landingPos.y *= -1;
      landingPos.z += levelHeight/2;
      object.add(createFlatEdges(landingSize, landingPos, stairsMaterial, true));

      // Steps
      renderStairsRun(object, stairsMaterial, numSteps / 2, length - landingLength*2, width / 2, width / 4, 0, levelHeight * i, levelHeight / 2, false);
      renderStairsRun(object, stairsMaterial, numSteps / 2, length - landingLength*2, width / 2, -1 * width / 4, 0, levelHeight * i + levelHeight / 2, levelHeight / 2, true);

      // A ring around the boundary to indicate the level position
      landingPos.z += levelHeight/2;
      object.add(createFlatEdges(shaftSize, { x:0, y:0, z: landingPos.z }, boundaryMaterial, false));

      // Prepare for next level
      landingPos.y *= -1;
   }

   // Render one more landing on the top floor
   if (drawEmptyTopRun)
      object.add(createFlatEdges(landingSize, landingPos, stairsMaterial, true));

   return object;
}

function renderStairsRun(parentObject, material, numSteps, length, width, centerX, centerY, baseHeight, runHeight, flip)
{
   var stepHeight = runHeight / numSteps;
   var stepDepth = length / (numSteps - 1);

   for (var i = 1; i < numSteps; i++) {
      var stepOffset = length / 2 - stepDepth * i + stepDepth / 2;
      if (flip)
         stepOffset *= -1;
      var stepPosZ = baseHeight + i*stepHeight;

      parentObject.add(createFlatEdges({ x: width, y: stepDepth }, { x: centerX, y: centerY + stepOffset, z: stepPosZ }, material, true));

      // Render two little lines dropping from the front of each step
      // Hackish: for the top step, also render another two lines going up to the landing
      for (var j = 0; j < (i == numSteps - 1 ? 4 : 2); j++) {
         var riserOffset = stepDepth / 2;
         if (flip != j > 1)
            riserOffset *= -1;
         var v1 = {
            x: centerX + (j % 2 ? width : -1*width) / 2, 
            y: centerY + stepOffset + riserOffset, 
            z: stepPosZ
         };
         var v2 = {
            x: v1.x, 
            y: v1.y, 
            z: v1.z + (j > 1 ? stepHeight : -1 * stepHeight)
         };
         parentObject.add(createLine(v1, v2, material, true));
      }
   }
}