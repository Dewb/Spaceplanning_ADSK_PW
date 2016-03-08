var massVisible = false, massWatchId, massGeometry, massEdges, redrawTimer = null;
var nonmassVisibilityFilterId;

var deleteMassGeometry = function() {
   if (massGeometry != null)
      scene.remove(massGeometry);
   massGeometry = null;
   if (massEdges != null)
      scene.remove(massEdges);
   massEdges = null;
}

toggleMassVisibility = function() {
   if (massVisible) {
      removeAppearanceFilter(nonmassVisibilityFilterId);

      deleteMassGeometry();
      requestRedraw();
      
      Groundhog.removeWatch(massWatchId);
      massWatchId = null;
      if (redrawTimer) {
         clearTimeout(redrawTimer)
      }

      massVisible = false;
   }
   else {
      nonmassVisibilityFilterId = addVisibilityFilter(function(elem) { return false; });
      drawAdjacenciesForElemId(null);

      var selector = ".Space, .Stairs, .Hall";

      var redrawMass = function () {
         redrawTimer = null;
         Groundhog.getAll(selector, function(elems) {
            var fullGeometry = null;
            for (var elemId in elems) {
               var elem = elems[elemId];
               var geom = elem.cloneModelGeometry();
               if (geom == null)
                  continue;
               for (var id in geom) {
                  var elemCSG = new ThreeBSP(geom[id]);
                  if (fullGeometry == null)
                     fullGeometry = elemCSG;
                  else
                     fullGeometry = fullGeometry.union(elemCSG);
               }
            }

            requestRedraw();
            deleteMassGeometry();

            if (fullGeometry == null)
               return;

            var fullMesh = fullGeometry.toMesh( new THREE.MeshPhongMaterial({ color: 0x999999 }) );
            massGeometry = fullMesh;
            massGeometry.castShadow = true;
            massGeometry.receiveShadow = true;
            scene.add(massGeometry);

            massEdges = new THREE.EdgesHelper(massGeometry, 0x000000);
            massEdges.material.linewidth = 2;
            scene.add(massEdges);
         });
      };

      unselectAll();

      massWatchId = Groundhog.watchAll(selector, function(elems) {
         if (redrawTimer == null)
            redrawTimer = setTimeout(redrawMass, 1);
      });

      massVisible = true;
   }
   return massVisible;
}