var sceneObjects = [];

var RenderState = {
   opaque:        0x0001,
   transparent:   0x0002,
   faces:         0x0004,
   edges:         0x0008,

   noInternal:    0x0010,
   internal:      0x0020,
   internalLines: 0x0040,
   externalLines: 0x0080,

   shaded:        0x0100,
   path:          0x0200,
   highlighted:   0x0400,
   unselected:    0x0800
};

var showAllInternal = false;

function toggleShowAllInternal() {
   showAllInternal = !showAllInternal;
   requestRedraw();
   
   return ((showAllInternal) ? "Showing" : "Not showing") + " internal edges through everything.";
}

function initViewCacheItem(item) {
   item.addEventListener('addedToScene', function(event) {
      sceneObjects.push(item);
   });
   item.addEventListener('removedFromScene', function(event) {
      var index = sceneObjects.indexOf(item);
      if (index !== -1)
         sceneObjects.splice(index, 1);
   });
}

var restoreForRender = function(object, state, filterHighlighted, filterSelected) {
   if ((state & RenderState.edges) && !(object.isEdges === true))
      return false;

   if ((state & RenderState.faces) && !(object.isFaces === true))
      return false;

   if ((state & RenderState.transparent) && !(object.isTransparent === true))
      return false;

   if ((state & RenderState.opaque) && object.isTransparent === true)
      return false;

   if (showAllInternal) {
      if (state & RenderState.noInternal)
         return false;
   } else {
      if ((state & RenderState.noInternal) && !(object.noInternal === true))
         return false;

      if ((state & RenderState.internal) && !(object.internal === true))
         return false;
   }


   if ((state & RenderState.internalLines) && !(object.internalLines === true))
      return false;

   if ((state & RenderState.externalLines) && !(object.externalLines === true))
      return false;


   if ((state & RenderState.shaded) && !(object.shaded === true))
      return false;

   if ((state & RenderState.shaded) === 0 && object.shaded === true)
      return false;

   if ((state & RenderState.path) && !(object.isPath === true))
      return false;

   if (filterHighlighted) {
      if ((state & RenderState.highlighted) && !(object.isHighlighted === true))
         return false;

      if ((state & RenderState.highlighted) === 0 && object.isHighlighted === true)
         return false;
   }

   // Filtering on selected items
   if (filterSelected) {
      
      // Specifically looking for selected items (i.e. normal items when filtering selected)
      if ((state & RenderState.unselected) === 0) {
         // undefined implicitly means not selected
         if (object.isSelected === undefined)
            return false;
         // false means explicitly not selected, <- is this used?
         if (object.isSelected === false)
            return false;
      }

      // Specifically looking for unselected items, either implicit or explicit
      if (state & RenderState.unselected) {
         // undefined implicitly means not selected
         if (object.isSelected !== undefined) {
            // false means explicitly not selected, <- is this used?
            if (object.isSelected !== false)
               return false;
         }
      } 
   }

   return true;
}

var setRenderState = function(state, filterHighlighted, filterSelected) {
   for (var index = 0; index < sceneObjects.length; ++index) {
      var object = sceneObjects[index];
      if (restoreForRender(object, state, filterHighlighted, filterSelected)) {
         // Restore item to it's original state
         if (object.wasVisible) {
            object.visible = object.wasVisible;
            delete object.wasVisible;
         }
      } else {
         // Hide item
         if (object.wasVisible == undefined)
            object.wasVisible = object.visible;
         object.visible = false;
      }
   }
}

var resetRenderState = function() {
   for (var index = 0; index < sceneObjects.length; ++index) {
      var object = sceneObjects[index];
      if (object.wasVisible != undefined) {
         object.visible = object.wasVisible;
         delete object.wasVisible;
      }
   }
}
