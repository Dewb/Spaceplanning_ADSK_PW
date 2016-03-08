define(function() {
   var opacityFilters = {};
   var visibilityFilters = {};
   var lastFilterId = 0;

   window.addVisibilityFilter = function(filterFunction) {
      lastFilterId++;
      visibilityFilters[lastFilterId] = filterFunction;

      updateAllElementsAppearance();

      return lastFilterId;
   };

   window.addWireframeFilter = function(filterFunction) {
      lastFilterId++;
      opacityFilters[lastFilterId] = filterFunction;

      updateAllElementsAppearance();

      return lastFilterId;
   };

   window.removeAppearanceFilter = function(filterId) {
      delete visibilityFilters[filterId];
      delete opacityFilters[filterId];

      updateAllElementsAppearance();
   };

   AppearanceOptions = {
      HIDDEN: 0,
      DIM: 1,
      VISIBLE: 2
   };

   determineElementAppearance = function(elem) {
      
      var clarityRequested = false;
      _.each(opacityFilters, function(filter) {
         if (filter(elem))
            clarityRequested = true;
      });

      var visibilityRequested = false;
      _.each(visibilityFilters, function(filter) {
         if (filter(elem))
            visibilityRequested = true;
      });

      // This logic is tailored to current usecases but can be made more flexible later.

      // If all visibility filters think it should be hidden...
      if (!_.isEmpty(visibilityFilters) && !visibilityRequested) {
         // But an opacityFilter thinks it should be clearly visible, show it but dim it
         if (!_.isEmpty(opacityFilters) && clarityRequested)
            return AppearanceOptions.DIM;

         // Otherwise hide it
         else
            return AppearanceOptions.HIDDEN;
      }

      // If it's visible but no opacityFilters think it should be clear, dim it
      else if (!_.isEmpty(opacityFilters) && !clarityRequested)
         return AppearanceOptions.DIM;

      return AppearanceOptions.VISIBLE;
   };

   window.updateElementAppearance = function(elem) {
      var geometry = elem.getElementGeometry();
      if (geometry == null)
         return;

      var appearance = determineElementAppearance(elem);
      var dimmedOpacity = 0.2;
      geometry.traverse(function (child) {
         child.visible = (appearance != AppearanceOptions.HIDDEN);

         var material = child.material;
         if (child.visible && material != null) {
            if (appearance == AppearanceOptions.DIM) {
               if (material.originalOpacity == null)
                  material.originalOpacity = material.opacity;
               material.opacity = dimmedOpacity * material.originalOpacity;
            }
            else if (appearance == AppearanceOptions.VISIBLE && material.originalOpacity != null) {
               material.opacity = material.originalOpacity;
               delete material.originalOpacity;
            }
         }
      });

      if (appearance == AppearanceOptions.VISIBLE)
         addPickableElement(geometry);
      else
         removePickableElement(geometry);

      requestRedraw();
   };

   updateAllElementsAppearance = function() {
      Groundhog.getEach("*", {passive: true}, function(elem) {
         updateElementAppearance(elem)
      });
   };

   var isolatedLevelFilterId;
   var isolatedLevel;
   window.isolateLevelVisiblity = function(newIsolatedLevel) {
      isolatedLevel = newIsolatedLevel;

      if (isolatedLevelFilterId != null)
         removeAppearanceFilter(isolatedLevelFilterId);

      if (isolatedLevel != null) {
         isolatedLevelFilterId = addVisibilityFilter(function(elem) {
            if (elem.get("type") == "Stairs")
               return true;

            return almostEqual(elem.getPosition()[2], (isolatedLevel-1)*3.5);
         });
      }

      $("body").trigger("levelIsolated", isolatedLevel);
   }

   window.getIsolatedLevel = function() { return isolatedLevel; };
});
