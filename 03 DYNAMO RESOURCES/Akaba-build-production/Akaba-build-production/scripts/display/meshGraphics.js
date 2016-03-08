var Shell = {
   displayMesh: true,
   toggleMesh: function() {
      this.displayMesh = !this.displayMesh;
      Groundhog.getEach(".Mesh", {passive: true}, 
         function(element) { 
            element.updateGraphics(); });
      
      return "Mesh display is " + ((this.displayMesh) ? "on." : "off.");
   },
   
   displayOutlines: true,
   toggleOutlines: function() {
      this.displayOutlines = !this.displayOutlines;
      Groundhog.getEach(".Mesh", {passive: true}, 
         function(element) { 
            element.updateGraphics(); });
      
      return "Outline display is " + ((this.displayOutlines) ? "on." : "off.");
   },
   
   displayGrid: false,
   toggleGrid: function() {
      this.displayGrid = !this.displayGrid;
      Groundhog.getEach(".Mesh", {passive: true}, 
         function(element) { 
            element.updateGraphics(); });
      
      return "Grid display is " + ((this.displayGrid) ? "on." : "off.");
   }   
}

function createMesh(position, rotation, text) {
   var object = new THREE.Object3D();

   var stlLoader = new THREE.STLLoader();
   var meshGeometry = stlLoader.parseASCII(text);

   if (Shell.displayMesh) {
      var material = new THREE.MeshPhongMaterial( { color: 0x66aaff, transparent: true, opacity: 0.16 } );
      var mesh = new THREE.Mesh(meshGeometry, material);
      mesh.position = position;
      mesh.rotation.z = rotation;

      object.add(mesh);
   }

   var isolatedLevel = getIsolatedLevel();

   var lineMaterial = new THREE.LineBasicMaterial({ color: 0x0000FF });

   if (Shell.displayOutlines) {
      var stlxLoader = new STLxLoader();
      var outlines = stlxLoader.parseASCIIOutlines(text);
      if (isolatedLevel === undefined) {
         for (var index = 0;index < outlines.length;++index) {
            var polyline = createPolyLine(outlines[index], lineMaterial);
            object.add(polyline);
         }
      } else {
         var outline = outlines[isolatedLevel];
         if (outline !== undefined) {
         var polyline = createPolyLine(outline, lineMaterial);
         object.add(polyline);
         }
      }
   }

   if (Shell.displayGrid) {
      if (isolatedLevel === undefined)
         isolatedLevel = 0;
      var stlxLoader = new STLxLoader();
      var grids = stlxLoader.parseASCIIGrids(text);
      var grid = grids[isolatedLevel];
      if (grid !== undefined) {
         for (var indexAxes = 0;indexAxes < grid.axes.length;++indexAxes) {
            var axis = grid.axes[indexAxes];
            var major = grid.basis[indexAxes];
            var minor = grid.basis[(indexAxes + 1)%2];
            for (var indexSeg = 0;indexSeg < axis.length;++indexSeg) {
               var segment = axis[indexSeg];
               var axisPos = {
                  x: segment.coord*major.x*major.gridSize,
                  y: segment.coord*major.y*major.gridSize
               }
               var p0 = {
                  x: axisPos.x + segment.v0*minor.x*minor.gridSize,
                  y: axisPos.y + segment.v0*minor.y*minor.gridSize,
                  z: grid.height
               }
               var p1 = {
                  x: axisPos.x + segment.v1*minor.x*minor.gridSize,
                  y: axisPos.y + segment.v1*minor.y*minor.gridSize,
                  z: grid.height
               }
               var line = createLine(p0, p1, lineMaterial, true);
               object.add(line);
            }
         }
      }
   }

   return object;
}
