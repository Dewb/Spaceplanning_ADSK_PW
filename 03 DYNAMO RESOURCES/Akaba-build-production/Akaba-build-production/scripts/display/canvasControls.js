define(['picking', 'scene'], function(picking, scene) {

   var allControls = {};
   var controlsByElemId = {};
   var watchers = [];
   var lastControlId = 0;

   var isElemRotated = function(elem) { return (Math.round(elem.get("rotation") / (Math.PI / 2)) % 2 != 0) };
   var transposeRotatedDims = function(dims) { return [dims[1], dims[0], dims[2]] };

   var dimsFromElem = function(elem) {
      if (elem.get("type") == "Hall")
         return [elem.length(), 4, 3.5];
      else
         return elem.get("dimensions");
   };

   var positionHall = function(hall, position, dims) {
      var isRotated = isElemRotated(hall);
      var length = dims[0] - 4;

      var centerOffset = arrToVector3(isRotated ? [0, length, 0] : [length, 0, 0]).multiplyScalar(0.5);
      hall.set("position1", arrToVector3(position).add(centerOffset).toArray());
      hall.set("position2", arrToVector3(position).sub(centerOffset).toArray());
   }

   window.getControlFromId = function(controlId) {
      return {
         controlledElementId: allControls[controlId].elementId,
         moveTo: function(position) {
            var control = allControls[controlId];
            Groundhog.get(control.elementId, function(elem) {
               var controlDirection = arrToVector3(control.direction).normalize();
               var startPosition = control.pickableGeom.position;
               var offset = arrToVector3(position).sub(startPosition);
               offset.projectOnVector(controlDirection); // Constrain in direction of dims

               var isExpanding = (offset.dot(controlDirection) > 0); // Dragging in direction of arrow
               dimensionOffset = _.map(offset.toArray(), function(dim) {
                  if (isExpanding)
                     return Math.abs(dim);
                  else
                     return Math.abs(dim) * -1;
               });
               if (isElemRotated(elem))
                  dimensionOffset = transposeRotatedDims(dimensionOffset);

               var newDims = arrToVector3(dimsFromElem(elem)).add(arrToVector3(dimensionOffset)).toArray();
               var oldPosition = arrToVector3(elem.getPosition());
               var newPosition = oldPosition.add(offset.clone().multiplyScalar(0.5)).toArray();

               if (elem.get("type") == "Hall") {
                  // Don't let halls get narrower than their width
                  if (newDims[0] < 4)
                     return;

                  positionHall(elem, newPosition, newDims);
               }
               else {
                  // Don't let spaces invert
                  if (_.any(newDims, function(dim) { return dim <= 0; }))
                     return;

                  elem.set("dimensions", newDims);
                  elem.set("position", newPosition);
               }
            });
         },

         getPosition: function() { return allControls[controlId].pickableGeom.position.toArray(); }
      }
   }

   var adjustControlForCameraPosition = function(geom, position) {
      geom.lookAt(position);
      var scale = Math.min(geom.position.distanceTo(position) / 80, 5);
      geom.scale.set(scale, scale, scale);
   };

   var generateControl = function(material) {
      var geometry = new THREE.Geometry();
      geometry.vertices.push(new THREE.Vector3(-0.7,0,0));
      geometry.vertices.push(new THREE.Vector3(0,0.7,0));
      geometry.vertices.push(new THREE.Vector3(0.7,0,0));
      geometry.faces.push(new THREE.Face3(0, 2, 1));
      var material = new THREE.MeshBasicMaterial(material);

      var mesh = new THREE.Mesh(geometry, material);

      if (window.useOIT()) {
         mesh.isOpaque = true;
         mesh.isFaces = true;

         mesh.isPickable = true;
      }

      initViewCacheItem(mesh);

      return mesh;
   }

   var drawControl = function(controlId, elementId) {
      var controlData = {geoms: []};
      controlData.elementId = elementId;

      var control = generateControl({color: 0xaaaaaa});
      scene.getActiveScene().add(control);
      controlData.geoms.push(control);

      var control = generateControl({color: 0xffffff, transparent: true, opacity: 0.3});
      scene.getOverlayScene().add(control);
      controlData.geoms.push(control);
      controlData.pickableGeom = control;
      control.userData.controlId = controlId;
      picking.addPickableControl(control);

      var control = generateControl({color: 0xaaaaaa, wireframe: true});
      scene.getOverlayScene().add(control);
      controlData.geoms.push(control);

      allControls[controlId] = controlData;

      if (controlsByElemId[elementId] == null)
         controlsByElemId[elementId] = []
      controlsByElemId[elementId].push(controlData)
   }

   var positionControlsForElem = function(element) {
      var controls = controlsByElemId[element.get("uniqueId")];
      if (!_.isArray(controls))
         return;

      var directions = [[1,0,0], [-1,0,0], [0,1,0], [0,-1,0]];
      var isRotated = isElemRotated(element);

      _.each(controls, function(control, index) {
         var direction = directions[isRotated ? (index + 2) % 4 : index];
         control.direction = direction;

         var position = arrToVector3(element.getPosition());

         var dims = dimsFromElem(element);
         if (isRotated)
            dims = transposeRotatedDims(dims);

         var positionOffset = arrToVector3(direction).multiply(arrToVector3(dims).multiplyScalar(0.5));
         positionOffset.z += 1.75;
         position.add(positionOffset);

         _.each(control.geoms, function(geom) {
            geom.up.fromArray(direction);
            geom.position.copy(position);
            adjustControlForCameraPosition(geom, scene.getCameraPosition());
         });
      })
   }

   window.watchSelection(function(selection) {
      // Clean up old controls
      _.each(allControls, function(ctrl) {
         _.each(ctrl.geoms, function(geom) {
            scene.getActiveScene().remove(geom);
            scene.getOverlayScene().remove(geom);
         });
         picking.removePickableControl(ctrl.pickableGeom);
      });
      allControls = {};
      controlsByElemId = {};
      _.each(watchers, function(w) { Groundhog.removeWatch(w) });
      watchers = [];

      if (selection.length < 1)
         return;

      Groundhog.get(selection[0], function(elem) {
         var numControls;
         if (elem.get("type") == "Space")
            numControls = 4;
         else if (elem.get("type") == "Hall")
            numControls = 2; // Only stretch along hallway's long axis
         else
            return; // Only spaces & halls can be resized

         var elemId = elem.get("uniqueId");

         for (var i = 0; i < numControls; i++)
            drawControl(++lastControlId, elemId);

         watchers.push(Groundhog.watch(elemId, positionControlsForElem));

         requestRedraw();
      });
   });


   scene.watchCameraPosition(function(position) {
      _.each(allControls, function(ctrl) { _.each(ctrl.geoms, function(geom) { adjustControlForCameraPosition(geom, position); }); });
   });
});