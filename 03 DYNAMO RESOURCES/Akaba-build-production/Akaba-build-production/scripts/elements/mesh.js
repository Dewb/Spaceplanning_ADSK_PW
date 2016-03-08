Groundhog.watchEach(".Mesh", function(elem) { addToRedrawList(elem.get("uniqueId")); });

Mesh.prototype.updateGraphics = function() {
   var position = arrToVector3(this.get("position"));
   var rotation = this.get("rotation");

   var inlineData = this.get("inlineData");
   if (inlineData) {
      this.setElementGeometry(createMesh(position, rotation, inlineData));
      return;
   } 

   var url = this.get("url");
   if (url) {
      var thisElement = this;
      var loader = new THREE.XHRLoader(thisElement.manager);
      // loader.setCrossOrigin( this.crossOrigin );
      loader.setResponseType('arraybuffer');
      loader.load(url, function(text) {
         thisElement.setElementGeometry(createMesh(position, rotation, text));
     });
   }
};
