//var define = require('requirejs');
define(['./analyzers', './genlayout', 'views/requirementsEditor'],
function (analyzers, gen, req) {

createSpaceType = function (usageName, dimensions) {
   var d = new SpaceData("", [0, 0, 0], dimensions, 0);
   d.usageName = usageName;
   return d;
}

window.testGen = function (numGenes, callback) {
   var geneSequence = [];
   numGenes = numGenes || 400;

   for (var ii = 0; ii < numGenes * 3; ii++) {
      geneSequence.push(Math.random());
   }

   beginNewLayout(function() {
      window.prepareSpaceStandardsForGenerator(function (spaceTypes) {
         window.GeneratedLayout = gen.generateLayout(geneSequence, spaceTypes);
         
         render();
         finalizeGeneratedLayout(callback);
      });
   });
}

function finalizeGeneratedLayout(callback) {
   window.runAnalysis(function () {
      window.findUnnecessaryHallways(function(unnecessary) {
         unnecessary.forEach(function(id) { Groundhog.deleteElement(id); })
         window.endTransaction();
         if (_.isFunction(callback))
            callback();
      });
   });
}

function beginNewLayout(layoutCreatedCallback) {
   window.clearRoomAlertLabels();
   Groundhog.createDesignOption(null, function(optionId, projectId) {
      selectProject(projectId, optionId, true);
      layoutCreatedCallback(projectId, optionId);
   })
}

window.generationPendingCancellation = false;
window.cancelGeneration = function() {
   generationPendingCancellation = true;
}

window.currentlyAutogenerating = false;

window.generateAndScore = function (candidateCount, numGenes, completionCallback) {
   generationPendingCancellation = false;
   currentlyAutogenerating = true;

   numGenes = numGenes || 400;
   var bestScore = Number.MAX_VALUE;
   var bestLayoutProjectId = null;
   var bestLayoutOptionId = null;

   var ii = 0;

   var statusDisplay = $('#generatorStatusDisplay');
   statusDisplay.show();

   var findNewLeader = function() {
      if (ii >= candidateCount || generationPendingCancellation) {
         generationPendingCancellation = false;
         currentlyAutogenerating = false;
         statusDisplay.hide();

         // reconstitute the best layout
         if (bestLayoutOptionId != null) {
            selectProject(bestLayoutProjectId, bestLayoutOptionId);
         }

         if (completionCallback != null)
            completionCallback();

         return;
      }

      var geneSequence = [];
      for (var jj = 0; jj < numGenes * 3; jj++) {
         geneSequence.push(Math.random());
      }

      window.prepareSpaceStandardsForGenerator(function (spaceTypes) {
            
         if (!Array.isArray(spaceTypes) || spaceTypes.length == 0) {
            cancelGeneration();
            findNewLeader();
            return;
         } 

         window.getRequirements(function (requirements) {
         
            beginNewLayout(function (projectId, optionId) {      

               var layout = []

               gen.generateLayout(geneSequence, spaceTypes, undefined, layout);

               var absoluteRequirements = [
                  function() { return analyzers.doesLayoutSatisfySpaceRequirements(requirements, layout, []); },
                  function() { return analyzers.doesLayoutSatisfyAdjacencyRequirements(requirements, layout, []); },
               ];

               var scoredRequirements = [
                  function() {
                     var totalArea = 0;
                     layout.forEach(function(space) { var dims = space.getWorldDimensions(); totalArea += dims[0] * dims[1]; });
                     return totalArea;
                  }
               ];

               var satisfied = true;
               absoluteRequirements.forEach(function(f) { if (!f()) { satisfied = false; }});
               var score = 0;
               scoredRequirements.forEach(function(f) { score += f(); });

               if (satisfied && score < bestScore) {
                  console.log("Layout #" + ii + " meets requirements, score of " + score + " is the new leader");
                  bestLayoutProjectId = projectId;
                  bestLayoutOptionId = optionId;
                  bestScore = score;
               }
               ii++;
               statusDisplay.html(ii + "/" + candidateCount);

               finalizeGeneratedLayout(function() { setTimeout(findNewLeader, 200); });
            });
         });   
      });
   };

   findNewLeader();

}


});
