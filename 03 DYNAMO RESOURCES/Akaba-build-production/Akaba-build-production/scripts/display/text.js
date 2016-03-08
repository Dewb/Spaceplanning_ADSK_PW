define(['scene'], function(scene) {
   var allText = {};
   var lastTextId = 0;

   window.createText = function(string, position, size, color) {
      var depth = 0.8;
      var bevelEnabled = true;
      var textGeo = new THREE.TextGeometry(string, {
         size: size,
         height: depth,
         curveSegments: 8,

         font: "optimer",
         weight: "normal",
         style: "normal",

         bevelThickness: 0.2 * (size / 7),
         bevelSize: 0.1 * (size / 7),
         bevelEnabled: bevelEnabled,

         material: 0,
         extrudeMaterial: 1

      });

      textGeo.computeBoundingBox();
      textGeo.computeVertexNormals();

      var material = new THREE.MeshBasicMaterial( { color: color });

      var centerOffset = -0.5 * (textGeo.boundingBox.max.x - textGeo.boundingBox.min.x);

      var mesh = new THREE.Mesh(textGeo, material);

      mesh.position.copy(position);
      mesh.position.x += centerOffset;

      mesh.up.fromArray([0,0,1]);
      mesh.lookAt(scene.getCameraPosition());

      mesh.castShadow = true;

      scene.getActiveScene().add(mesh);

      lastTextId++;
      allText[lastTextId] = mesh;

      requestRedraw();

      return lastTextId;
   };

   window.removeText = function(id) {
      var text = allText[id];
      if (text != null) {
         scene.getActiveScene().remove(text);
         delete allText[id];
         requestRedraw();
      }
   }

   window.clearAllText = function() {
      _.each(allText, function(text) {
         scene.getActiveScene().remove(text);
      })
      requestRedraw();
      allText = {};
   }

   scene.watchCameraPosition(function(position) {
      _.each(allText, function(text) { text.lookAt(position) });
   });
});