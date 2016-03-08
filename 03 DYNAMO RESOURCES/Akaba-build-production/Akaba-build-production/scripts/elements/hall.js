Groundhog.watchEach(".Hall", function(elem) { addToRedrawList(elem.get("uniqueId")); });

Hall.prototype.moveTo = function(newCenter) {
   var oldCenter = arrToVector3(this.getPosition());
   var delta = arrToVector3(newCenter).sub(oldCenter);
   this.set("position1", arrToVector3(this.get("position1")).add(delta).toArray());
   this.set("position2", arrToVector3(this.get("position2")).add(delta).toArray());
};

Hall.prototype.rotationVector = function() {
   return (new THREE.Vector3(1,0,0)).applyAxisAngle(new THREE.Vector3(0,0,1), this.get("rotation")).toArray();
};

Hall.prototype.getPosition = function() {
   return arrToVector3(this.get("position2")).lerp(arrToVector3(this.get("position1")), 0.5).toArray();
};

Element.prototype.setPosition = function(pos) {
   this.moveTo(pos);
}

Hall.prototype.getWorldDimensions = function() {
   var width = this.width();
   var length = this.length();
   return getRotatedDims(this.get("rotation"), length, width, 3.5);
};

Hall.prototype.length = function() {
   // halls extend beyond the positions by one-half the default width at both ends
   return arrToVector3(this.get("position2")).distanceTo(arrToVector3(this.get("position1"))) + Hall.defaultWidth(); 
}

Hall.prototype.width = function() {
   return this.get("width") || Hall.defaultWidth();
}

// static method
Hall.defaultWidth = function() {
   return 4.0;
}

Hall.prototype.updateGraphics = function() {
   var dims = [this.length(), this.width(), 3.5];
   var color = { r: 0.6, g: 0.6, b: 0.6 };

   var object = new THREE.Object3D();
   object.add(createBox(dims, color, 0.3, false, false));
   object.add(createBox(dims, color, 0.3, false, true));
   object.add(createBoxEdges(dims));

   if (this.showGuides) {
      for (var i = 0; i < 4; i++) {
         var pos = new THREE.Vector3(0, (i < 2 ? this.width() : -1 * this.width()) / 2, (i % 2 ? 3.5 : -3.5) / 2 + 1.75);
         object.add(createInfiniteLine(pos, new THREE.Vector3(1, 0, 0), color));
      }
   }

   object.position = arrToVector3(this.getPosition());
   object.rotation.z = this.get("rotation");

   this.setElementGeometry(object);
};
