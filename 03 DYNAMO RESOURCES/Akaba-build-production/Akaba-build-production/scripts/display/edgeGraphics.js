function createBoxEdges(dims) {
   var edges = new THREE.BoxHelper();
   edges.scale.set(dims[0]/2.0, dims[1]/2.0, dims[2]/2.0);
   edges.material.color.setRGB(1.0, 1.0, 1.0);
   edges.material.transparent = true;
   edges.material.opacity = 1.0;
   edges.position.z = dims[2]/2;

   if (window.useOIT()) {
      edges.isTransparent = true;
      edges.isEdges = true;

      edges.externalLines = true;
   }
   
   initViewCacheItem(edges);

   return edges;
}

function createFlatEdges(size, pos, material, internal) {
   var geometry = new THREE.Geometry();
   for (var i = 0; i < 5; i++) {
      geometry.vertices.push(
         new THREE.Vector3(
            pos.x + (i < 2 || i == 4 ? size.x : -1*size.x)/2, 
            pos.y + (i % 2 == i < 2 && i != 4 ? size.y : -1*size.y)/2,
            pos.z
      ));
   }

   var edges = new THREE.Line(geometry, material);

   if (window.useOIT()) {
      edges.isTransparent = true;
      edges.isEdges = true;

      if (internal)
         edges.internalLines = true;
      else
         edges.externalLines = true;
   }
   
   initViewCacheItem(edges);

   return edges;
}
