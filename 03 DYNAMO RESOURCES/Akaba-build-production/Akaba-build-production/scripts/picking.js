setPickCallback = function(cb) {
   unselectAll();
   pickCallback = cb;
}

setClickCallback = function(cb) {
   unselectAll();
   clickCallback = cb;
}

var elementPickingEnabled = true;
disableElementPicking = function() {
   elementPickingEnabled = false;
}
enableElementPicking = function() {
   elementPickingEnabled = true;
}

var pickableControls = [];
var workplane, pickableObjects = [];

var prehilightedGeometry = [], pickedElementId, pickedControlId;

var selection = {};
var selectionDeletionWatchers = [];

var raycaster, projector;

var mouse2d;

var pickCallback, clickCallback;

var selectionWatchers = [];

window.watchSelection = function(callback) {
   selectionWatchers.push(callback);
}

window.getSelection = function() { return _.keys(selection); }

define(["analysis/analyzers"], function(analyzers) {
   var doSnappedDrag = function(draggedElementId, proposedPosition, completionCallback) {
      var pos = _.clone(proposedPosition);
      Groundhog.getAll(".Space, .Hall, .Stairs", {passive: true}, function(elems) {
         var draggedElement = elems[draggedElementId];
         if (draggedElement == null)
            return;

         var draggedDims = draggedElement.getWorldDimensions();

         var snapping = false;
         _.each(elems, function(elem, id) {
            // Don't snap to self or exterior doors (egress will attempt to re-route)
            if (id == draggedElementId || elem.get("usageName") == "egress")
               return;

            if (!_.isNumber(analyzers.getLevelSpacesConnect(draggedElement, elem)))
               return;

            var otherPos = elem.getPosition();
            var otherDims = elem.getWorldDimensions();
            var foundOverlaps = 0;
            var suggestedPosition = []; // fixes in each dimension (choose one)
            for (var i=0; i<2; i++) {
               var centerOffset = otherPos[i] - pos[i];
               var requiredDist = 0.5 * (draggedDims[i] + otherDims[i]);
               if (requiredDist - Math.abs(centerOffset) < groundhogEpsilon)
                  continue;

               foundOverlaps++;
               if (centerOffset > 0)
                  suggestedPosition[i] = otherPos[i] - requiredDist;
               else
                  suggestedPosition[i] = otherPos[i] + requiredDist;
            }

            // Not overlapping
            if (foundOverlaps < 2)
               return;
            
            if (Math.abs(suggestedPosition[0] - pos[0]) < Math.abs(suggestedPosition[1] - pos[1]))
               pos[0] = suggestedPosition[0];
            else
               pos[1] = suggestedPosition[1];
         });
         draggedElement.moveTo(pos);
      });
   }

   // Temporary exposure for editors.js, until it's ported to RequireJS
   window.doSnappedDrag = doSnappedDrag;

   // Detect clicks but not drags
   var isDragging = false; // Not necessarily dragging anything specific
   var draggingElement = false; // Dragging something specific
   var draggingControl = false;
   var mouseDown = false;
   $("#container").mousedown(function() {
      isDragging = false;
      mouseDown = true;
   });
   $("#container").mousemove(function() {
      if (!isDragging && mouseDown && elementPickingEnabled) {
         isDragging = true;

         var startDraggingItem = function(item) {
            var isolatedLevel = getIsolatedLevel();
            if (isolatedLevel != null)
               workplane.position.z = (isolatedLevel-1)*3.5;
            else if (item.keepOnGround)
               workplane.position.z = 0;
            else {
               var startPos = item.getPosition();
               if (_.isArray(startPos))
                  workplane.position.z = startPos[2];
            }
            orbitControls.enabled = false;
         }

         if (pickedControlId) {
            var control = getControlFromId(pickedControlId);

            window.pretouchElementForTransaction(control.controlledElementId);
            draggingControl = true;

            startDraggingItem(control);

            pickCallback = function (pos) { control.moveTo(pos); };
         }
         else if (pickedElementId) {
            Groundhog.get(pickedElementId, {passive: true}, function (pickedElement) {
               window.pretouchElementForTransaction(pickedElementId);
               draggingElement = true;

               startDraggingItem(pickedElement);

               setPickCallback(function (pos) {
                  doSnappedDrag(pickedElementId, pos);
               });
            });
         }
      }
   });
   $("#container").mouseup(function(e) {
      if (!isDragging) {
         if (clickCallback) {
            clickCallback();
         }
         else if (!pickCallback && elementPickingEnabled && pickedControlId == null)
            selectPick();
      }
      else if (draggingElement || draggingControl) {
         orbitControls.enabled = true;
         pickCallback = null;
         window.endTransaction();

         if (pickedElementId != null)
            drawAdjacenciesForElemId(pickedElementId);
      }
      mouseDown = false;
   });

   return {
      addPickableControl: function(geometry) {
         if (!_.contains(pickableControls, geometry))
            pickableControls.push(geometry);
      },

      removePickableControl: function(geometry) {
         var index = pickableControls.indexOf(geometry);
         if (index > -1) {
            pickableControls.splice(index, 1);
         }
      },
   };
});

addPickableElement = function(geometry) {
   if (!_.contains(pickableObjects, geometry))
      pickableObjects.push(geometry);
}

removePickableElement = function(geometry) {
   var index = pickableObjects.indexOf(geometry);
   if (index > -1) {
      pickableObjects.splice(index, 1);
   }
}

snapScalar = function(val) {
   // Round to nearest half unit in each dimension
   return Math.round(val * 2) / 2;
}

onDocumentMouseMove = function(event) {
   mouse2d.x = (event.clientX / window.innerWidth) * 2 - 1;
   mouse2d.y = - (event.clientY / window.innerHeight) * 2 + 1;
}

userDataFromGeometry = function(geometry, fieldName) {
   while (geometry) {
      id = geometry.userData[fieldName];
      if (id)
         return id;
      geometry = geometry.parent;
   }
   return null;
}

incrementalPick = function(scene, camera) {
   if (pickCallback) {
      raycaster = projector.pickingRay(mouse2d.clone(), camera);
      var intersects = raycaster.intersectObject(workplane);
      if (intersects.length > 0) {
         var snappedPickPoint = intersects[0].point.clone();
         snappedPickPoint.x = snapScalar(snappedPickPoint.x);
         snappedPickPoint.y = snapScalar(snappedPickPoint.y);
         pickCallback(snappedPickPoint.toArray());
      }
   }
   // Prehilight intersected elements
   else {
      raycaster = projector.pickingRay(mouse2d.clone(), camera);

      var oldSelection = prehilightedGeometry;
      prehilightedGeometry = [];

      var controlIntersection = raycaster.intersectObjects(pickableControls, true);
      if (controlIntersection && controlIntersection.length > 0) {
         var obj = controlIntersection[0].object;
         pickedElementId = null;
         pickedControlId = userDataFromGeometry(obj, "controlId");
         prehilightedGeometry.push(obj);
         if (obj.originalOpacity == null) {
            obj.originalOpacity = obj.material.opacity;
            obj.material.opacity = 1;
         }
         requestRedraw();
      }
      else {
         var intersects = raycaster.intersectObjects(pickableObjects, true);

         pickedElementId = null;
         pickedControlId = null;

         for (var i in intersects) {
            var obj = intersects[i].object;

            if (!canSelect(obj))
               continue;

            var elemId = userDataFromGeometry(obj, "elemId");

            // Only highlight pieces of the first element we hit
            if (pickedElementId && elemId != pickedElementId)
               continue;
            pickedElementId = elemId;

            // Don't prehighlight selected elements
            if (selection[elemId])
               break;

            prehilightedGeometry.push(obj);

            // Already selected, don't change it
            if (_.contains(oldSelection, obj))
               continue;

            if (!window.useOIT()) {
               setPrehighlight(obj);
               requestRedraw();
            }
            else
               highlightGeometry(obj);
         }
      }

      var unselected = _.difference(oldSelection, prehilightedGeometry);
      for (var i in unselected) {
         var geom = unselected[i];

         setHighlight(geom, false);

         if (geom.originalOpacity != null) {
            geom.material.opacity = geom.originalOpacity;
            delete geom.originalOpacity;
         }

         requestRedraw();
      }
   }
}

highlightGeometry = function(geom) {
   setHighlight(geom, true);
   requestRedraw();
}

unhighlightGeometry = function(geom) {
   setHighlight(geom, false);
   requestRedraw();
}

selectPick = function() {
   unselectAll(true);

   if (pickedElementId) {
      for (var i in prehilightedGeometry) {
         highlightGeometry(prehilightedGeometry[i]);
         prehilightedGeometry[i].isSelected = true;
      }
      selection[pickedElementId] = prehilightedGeometry;
      selectionDeletionWatchers.push(Groundhog.watchUnload(pickedElementId, function() { unselectAll() } ));
   }
   _.each(selectionWatchers, function(cb) { cb(_.keys(selection)); });
   drawAdjacenciesForElemId(pickedElementId);
}

unselectAll = function(dontTriggerWatchers) {
   for (var elemId in selection) {
      var allGeometry = selection[elemId];
      for (var i in allGeometry) {
         unhighlightGeometry(allGeometry[i]);
         delete allGeometry[i].isSelected;
      }
   }
   for (var i in selectionDeletionWatchers)
      Groundhog.removeWatch(selectionDeletionWatchers[i]);
   selectionDeletionWatchers = [];
   selection = {};

   if (!dontTriggerWatchers)
      _.each(selectionWatchers, function(cb) { cb(_.keys(selection)); });

   requestRedraw();
}

initializePicking = function(scene) {
   plane = new THREE.Mesh(new THREE.PlaneGeometry(10000, 10000), new THREE.MeshBasicMaterial());
   plane.visible = false;
   plane.material.side = THREE.DoubleSide;
   scene.add(plane);
   workplane = plane;
   projector = new THREE.Projector();

   mouse2d = new THREE.Vector3(0, 10000, 0.5);
   document.addEventListener('mousemove', onDocumentMouseMove, false);

   $(document).keydown(function(e) {
      var element = e.target.nodeName.toLowerCase();
      if (element == 'input' || element == 'textarea')
         return;

      var keyCode = e.keyCode || e.which;

      // Arrows change the altitude of the pick plane, or of the selected element
      if (e.keyCode == 38 || e.keyCode == 40) {
         // Don't move elements to a level that the user can't see
         if (getIsolatedLevel() != null)
            return false;

         var offset = (e.keyCode == 38 ? 3.5 : -3.5);

         if (e.shiftKey) {
            window.copySelectedElementsToBuffer();
            var newElems = window.pasteFromBufferOnNewLevel(offset);
            pickedElementId = newElems[0].data.uniqueId;
            selectPick();
            return;
         }

         if (_.isEmpty(selection)) {
            workplane.position.z += offset;
            if (workplane.position.z < 0)
               workplane.position.z = 0;
         }
         else {
            for (var elemId in selection) {
               Groundhog.get(elemId, {passive: true}, function (elem) {
                  if (elem.keepOnGround)
                     return;

                  window.pretouchElementForTransaction(elemId);
                  
                  var currentPos = elem.getPosition();
                  if (currentPos) {
                     currentPos[2] += offset;
                     if (currentPos[2] < 0)
                        currentPos[2] = 0;
                     elem.moveTo(currentPos);
                     window.endTransaction();
                  }
               });
            }
         }
         e.preventDefault();
         return false;
      }

      // Backspace or delete removes the selected elements
      else if (e.keyCode == 8 || e.keyCode == 46) {
         for (var elemId in selection) {
            window.pretouchElementForTransaction(elemId);
            Groundhog.deleteElement(elemId);
         }
         window.endTransaction();
         e.preventDefault();
         return false;
      }

      // Space rotates the selected elements
      else if (e.keyCode == 32) {
         for (var elemId in selection) {
            Groundhog.get(elemId, {passive: true}, function (elem) {
               window.pretouchElementForTransaction(elemId);
               elem.rotate();
            });
         }
         window.endTransaction();
         e.preventDefault();
         return false;
      }

      return true;
   });

   addRenderCallback(incrementalPick);
}