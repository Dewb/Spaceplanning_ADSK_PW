function createBox(dims, color, opacity, hasInternal, shaded) {
   var geometry;
   var material;
   if (window.useOIT()) {
      if (shaded) {
         geometry = new THREE.BoxGeometry(dims[0], dims[1], dims[2]);
         material = new THREE.MeshLambertMaterial({
            blending: THREE.MultiplyBlending
            // ,
            // transparent: true
            // , 
            // opacity: opacity
         });
         material.color.setRGB(1.0, 0.0, 0.0);
      } else {
         geometry = new THREE.BoxGeometry(dims[0], dims[1], dims[2]);
    
         var vertexPositions = [];
         for (var faceIndex in geometry.faces) {
            var face = geometry.faces[faceIndex];
            vertexPositions.push(geometry.vertices[face.a]);
            vertexPositions.push(geometry.vertices[face.b]);
            vertexPositions.push(geometry.vertices[face.c]);
         }
         
         var vertices = new THREE.BufferAttribute();
         vertices.array = new Float32Array(vertexPositions.length*3);
         vertices.itemSize = 3;

         var colors = new THREE.BufferAttribute();
         colors.array = new Float32Array(vertexPositions.length*4);
         colors.itemSize = 4;

         for (var index = 0; index < vertexPositions.length; ++index) {
            var raw = vertexPositions[index];
            vertices.setXYZ(index, raw.x, raw.y, raw.z);

            colors.setXYZW(index, color.r, color.g, color.b, opacity);
         }

         geometry = new THREE.BufferGeometry();
         geometry.addAttribute('position', vertices);
         geometry.addAttribute('color', colors);

         var material = new THREE.RawShaderMaterial( {
            vertexShader: document.getElementById('vertexShader').textContent,
            fragmentShader: document.getElementById('fragmentShader').textContent,
            side: THREE.DoubleSide
         });
      }
   } else {
      geometry = new THREE.BoxGeometry(dims[0], dims[1], dims[2]);
      material = new THREE.MeshLambertMaterial({transparent: true, opacity: opacity});
      material.color.setRGB(color.r, color.g, color.b);
   }

   var mesh = new THREE.Mesh(geometry, material);
   mesh.castShadow = true;
   mesh.position.z = dims[2]/2;

   if (window.useOIT()) {
      mesh.isTransparent = (opacity != 1.0);
      mesh.isFaces = true;
      if (shaded)
         mesh.shaded = true;

      mesh.isPickable = true;

      if (hasInternal)
         mesh.internal = true;
      else
         mesh.noInternal = true;
   }

   initViewCacheItem(mesh);

   return mesh;
}
