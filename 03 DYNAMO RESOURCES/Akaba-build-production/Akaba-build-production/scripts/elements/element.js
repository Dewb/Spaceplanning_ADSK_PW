var groundhogEpsilon = 1e-8;

almostEqual = function(num1, num2) {
   return (Math.abs(num1-num2) < groundhogEpsilon);
}

getRotatedDims = function(rotation, width, length, height) {
   if (Math.abs(Math.abs(rotation / Math.PI % 1) - 0.5) < 0.25)
      return [length, width, height];
   else
      return [width, length, height];
}

Element.prototype.setElementGeometry = function(geometry) {
   this.deleteElementGeometry()
   this.geometry = geometry;
   geometry.userData.elemId = this.get("uniqueId");
   scene.add(geometry);
   addPickableElement(geometry);

   updateElementAppearance(this);
};

Element.prototype.getElementGeometry = function() {
   return this.geometry;
};

Element.prototype.deleteElementGeometry = function() {
   if (!this.geometry)
      return;
   scene.remove(this.geometry);
   removePickableElement(this.geometry);
   requestRedraw();
   this.geometry = null;
};

// Finds all solids within the geometry
// Returns copies of them in an array
Element.prototype.cloneModelGeometry = function() {
   var geom = this.getElementGeometry();
   if (geom == null)
      return null;

   var children = [];
   for (var id in geom.children) {
      var child = geom.children[id];
      if (child instanceof THREE.Mesh || child instanceof THREE.Geometry) {
         var childClone = child.clone();
         childClone.position.add(geom.position);
         childClone.rotation.copy(geom.rotation);
         children.push(childClone);
      }
   }
   return children;
};

Element.prototype.getPosition = function() {
   // Base assumes it's stored in a field called position, or is omitted entirely
   var pos = this.get("position");
   if (pos == null)
      return null;
   else
      // Return a copy to prevent accidental modification of the stored version (this tests much faster than slice(0) or _.clone)
      return [pos[0], pos[1], pos[2]];
};

Element.prototype.setPosition = function(pos) {
   this.set("position", pos);
}

Element.prototype.getWorldDimensions = function() {
   var dims = this.get("dimensions");
   return getRotatedDims(this.get("rotation"), dims[0], dims[1], dims[2]);
};

Element.prototype.getBoundingBox = function() {
   var dims = this.getWorldDimensions();
   var pos = this.getPosition();
   return [[pos[0] - dims[0]/2, pos[1] - dims[1]/2, pos[2]], [pos[0] + dims[0]/2, pos[1] + dims[1]/2, pos[2] + dims[2]]];
}

Element.prototype.getLevel = function() {
   var pos = this.getPosition();
   return Math.round(pos[2] / 3.5) + 1;
};

// Move element to a 3d position
// Default implementation only moves the geometry. Override for smarter behavior.
Element.prototype.moveTo = function(pos) {
   if (_.isArray(this.get("position")))
      this.set("position", pos);
}

// Rotate element 90º
// Default implementation only rotates the geometry. Override for smarter behavior.
Element.prototype.rotate = function() {
   var oldRotation = this.get("rotation");
   if (_.isNumber(this.get("rotation")))
      this.set("rotation", oldRotation + Math.PI * 0.5);
}

Groundhog.watchUnload("*", function(element) {
   return element.deleteElementGeometry();
});
