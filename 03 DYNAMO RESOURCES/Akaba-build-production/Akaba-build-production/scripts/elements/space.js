Groundhog.watchEach(".Space", function(elem) { addToRedrawList(elem.get("uniqueId")); });

// Redraw all spaces whenever the space program changes - this can affect their color
Groundhog.watchEach("program.uses", function(elem) {
   Groundhog.getEach(".Space", function(elem) { addToRedrawList(elem.get("uniqueId")); });
});

spaceColorFromIndex = function(index) {
   if (index == null || index == 0)
      return {r: 0.5, g: 0.5, b: 0.5};
   else {
      var color = {};
      THREE.Color.prototype.setHSL.apply(color, [(index * 0.15 + 0.5) % 1, 0.5, 0.5])
      return color;
   }
}

Space.prototype.updateGraphics = function() {
   var dims = this.get("dimensions");

   var colorIndex;
   var usageName = this.get("usageName");
   if (usageName) {
      Groundhog.get("program", {passive: true}, function(program) {
         var uses = program.get("uses");
         var use = _.find(uses, function(use) { return use.name == usageName });
         if (use != null)
            colorIndex = use.colorIndex;
         else
            colorIndex = 0;
      });
      if (colorIndex == null) // If the program wasn't loaded yet,
         return; // we'll be called again when the program is available
   }
   
   var color = spaceColorFromIndex(colorIndex);

   var object = new THREE.Object3D();
   object.add(createBox(dims, color, 0.7, false, false));
   object.add(createBox(dims, color, 0.7, false, true));
   object.add(createBoxEdges(dims));
   object.position = arrToVector3(this.get("position"));
   object.rotation.z = this.get("rotation");
   this.setElementGeometry(object);
};
