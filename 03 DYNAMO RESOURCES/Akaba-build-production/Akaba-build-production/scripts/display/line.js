function createInfiniteLine(pos, dir, color)
{
   dir.normalize();
   var extender = (new THREE.Vector3()).copy(dir).multiplyScalar(5000);

   var point1 = new THREE.Vector3();
   point1.addVectors(pos, extender);

   var point2 = new THREE.Vector3();
   point2.subVectors(pos, extender);

   var geometry = new THREE.Geometry();
   geometry.vertices.push(point1);
   geometry.vertices.push(point2);

   var material = new THREE.LineBasicMaterial();
   material.color.setRGB(color.r, color.g, color.b);

   var line = new THREE.Line(geometry, material);

   if (window.useOIT()) {
      line.isTransparent = false;
      line.isEdges = true;
   }
   
   return line;
}

function createLine(v1, v2, material, internal) {
   var geometry = new THREE.Geometry();
   geometry.vertices.push(new THREE.Vector3(v1.x, v1.y, v1.z));
   geometry.vertices.push(new THREE.Vector3(v2.x, v2.y, v2.z));

   var line = new THREE.Line(geometry, material);

   if (window.useOIT()) {
      line.isTransparent = true;
      line.isEdges = true;

      if (internal)
         line.internalLines = true;
      else
         line.externalLines = true;
   }
   
   initViewCacheItem(line);
   
   return line;
}

function createPolyLine(vertices, material, internal) {
   var geometry = new THREE.Geometry();
   for (var index = 0;index < vertices.length;++index) {
      var vertex = vertices[index];
      geometry.vertices.push(new THREE.Vector3(vertex.x, vertex.y, vertex.z));
   }
   var line = new THREE.Line(geometry, material);

   if (window.useOIT()) {
      line.isTransparent = true;
      line.isEdges = true;

      if (internal)
         line.internalLines = true;
      else
         line.externalLines = true;
   }
   
   initViewCacheItem(line);
   
   return line;
}
