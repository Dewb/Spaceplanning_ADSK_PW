var container, stats;

var camera, orbitControls, scene, overlayScene, renderer, oitRenderer;
// var sceneObjects = [];

arrToVector3 = function(arr) { return (new THREE.Vector3()).fromArray(arr); };

var cameraPositionWatchers = [];

define({
   getActiveScene: function() { return scene; },
   getOverlayScene: function() { return overlayScene; },
   requestRedraw: function() { redrawRequested = true; },
   watchCameraPosition: function(cb) {
      cameraPositionWatchers.push(cb);
      if (camera != null)
         cb(camera.position);
   },
   getCameraPosition: function() { return camera.position; }
})

function init() {
   container = $("#container");

   camera = new THREE.PerspectiveCamera(30, window.innerWidth / window.innerHeight, 1, 1000);
   camera.position.y = 100;
   camera.position.z = 10;
   camera.up = new THREE.Vector3(0, 0, 1);
   _.each(cameraPositionWatchers, function(cb) { cb(camera.position); })

   orbitControls = new THREE.OrbitControls( camera, container[0] );
   orbitControls.noKeys = true;
   orbitControls.addEventListener( 'change', function() {
      _.each(cameraPositionWatchers, function(cb) { cb(camera.position); })
      requestRedraw();
   } );

   scene = new THREE.Scene();
   overlayScene = new THREE.Scene();

   // Ground plane
   plane = new THREE.Mesh(new THREE.PlaneGeometry(10000, 10000), new THREE.MeshBasicMaterial({ color: 0xe0e0e0 }));
   plane.receiveShadow = true;
   scene.add(plane);
   plane.isOpaque = true;
   sceneObjects.push(plane);
   // initViewCacheItem(plane);

   // lights
   light = new THREE.DirectionalLight( 0xfefefe, 2 );
   light.position.set( 1000, 1000, 1000 );
   light.castShadow = true;
   light.shadowDarkness = 0.03;
   light.shadowMapWidth = 4096;
   light.shadowMapHeight = 4096;
   scene.add(light);

   light = new THREE.DirectionalLight( 0xfefefe );
   light.position.set( -1, -1, -1 );
   scene.add(light);

   // renderer
   renderer = new THREE.WebGLRenderer( { antialias: true, preserveDrawingBuffer: true } );
   renderer.setClearColor( 0xf0f0f0, 1 );
   renderer.setSize(container.innerWidth(), container.innerHeight());
   renderer.shadowMapEnabled = true;
   renderer.autoClear = false;

   container.append(renderer.domElement);

   oitRenderer = new OITRenderer(renderer);

   window.addEventListener( 'resize', onWindowResize, false );
   
   animate();
}


function onWindowResize() {
   camera.aspect = window.innerWidth / window.innerHeight;
   camera.updateProjectionMatrix();

   renderer.setSize(window.innerWidth, window.innerHeight);

   requestRedraw();
}

function animate() {
   requestAnimationFrame(animate);

   render();
}

var renderCallbacks = [];
var redrawRequested = true;
var fullRedrawRequested = false;

function addRenderCallback(cb) {
   renderCallbacks.push(cb);
}

function requestRedraw() {
   redrawRequested = true;
}

var needRedraw = {};
window.addToRedrawList = function(id, field) {
   if (needRedraw[id] == null)
      needRedraw[id] = {};
   if (field)
      needRedraw[id][field] = true;
};

function requestFullRedraw() {
   fullRedrawRequested = true;
}

function render() {
   for (var x in renderCallbacks) {
      renderCallbacks[x](scene, camera);
   }

   var doRedraw = (_.size(needRedraw) > 0);
   if (fullRedrawRequested) {
      fullRedrawRequested = false;
      doRedraw = true;
      Groundhog.getEach(".Space", {passive: true}, 
         function(element) { 
            element.updateGraphics(); });
      Groundhog.getEach(".Hall", {passive: true}, 
         function(element) { 
            element.updateGraphics(); });
      Groundhog.getEach(".Stairs", {passive: true}, 
         function(element) { 
            element.updateGraphics(true); });
   } else {
      var needRedrawCopy = needRedraw;
      needRedraw = {}; // Elements may request redraw for the next pass during this callback
      _.each(needRedrawCopy, function(fields, id) {
         Groundhog.get(id, {passive: true}, 
            function(element) { element.updateGraphics(fields); });
      });
   }

   if (redrawRequested || doRedraw) {
      renderer.clear(true, true, true);

      if (window.useOIT())
         oitRenderer.render(scene, camera);
      else
         renderer.render(scene, camera);

      renderer.clearDepth();
      renderer.render(overlayScene, camera);
   }

   redrawRequested = false;
}

captureSceneAsDataURL = function(width, height) {
   if (width == null)
      width = 1024;
   if (height == null)
      height = 1024;

   // Create a camera from a reasonably consistent viewpoint
   var screenShotCamera = new THREE.PerspectiveCamera(30, width / height, 1, 1000);
   var boundingSphere = computeSceneBoundingSphere(scene);
   screenShotCamera.position = new THREE.Vector3(120, 120, 25);
   screenShotCamera.position.add(boundingSphere.center);
   screenShotCamera.up = new THREE.Vector3(0, 0, 1);
   screenShotCamera.lookAt(boundingSphere.center);

   var offscreenRT = new THREE.WebGLRenderTarget(width, height);
   renderer.clearTarget(offscreenRT, true, true, true);
   if (window.useOIT())
      oitRenderer.render(scene, screenShotCamera, offscreenRT);
   else
      renderer.render(scene, screenShotCamera, offscreenRT);

   // Convert to image
   // The rest of this function comes from: http://stackoverflow.com/questions/23999954/texture-to-data-uri-in-three-js

   var pixelsIn = new Uint8Array(4 * width * height);
   var gl = renderer.context;
   var framebuffer = offscreenRT.__webglFramebuffer;
   gl.bindFramebuffer(gl.FRAMEBUFFER, framebuffer);
   gl.viewport(0, 0, width, height);
   gl.readPixels(0, 0, width, height, gl.RGBA, gl.UNSIGNED_BYTE, pixelsIn);
   gl.bindFramebuffer(gl.FRAMEBUFFER, null);

   var canvas = document.createElement("canvas");
   canvas.width = width;
   canvas.height = height;
   var context = canvas.getContext('2d');

   var pixelsOut = context.createImageData(width, height);

   // Convert data + flip Y:
   var row, col, k = 4*height;
   for(var i = 0; i < pixelsIn.length; i++) {
       row = Math.floor(i/k);
       col = i % k;
       pixelsOut.data[(height-1-row)*k+col] = pixelsIn[i];
   }

   context.putImageData(pixelsOut,0,0);

   return canvas.toDataURL("image/png");
}

window.storeThumbnail = function() {
   render();
   var data = captureSceneAsDataURL(512, 512);

   // update project thumbnail in metadata
   Groundhog.setDesignOptionMetadata("thumbnail", data);

   // update design option thumbnail in DB
   Groundhog.getAll(".Thumbnail", {global: true}, function (thumbs) {
      var found = false;
      _.each(thumbs, function (thumb) {
         if (thumb.get("thumbnailDesignOptionId") == Groundhog.getDesignOptionId()) {
            if (!found) {
               thumb.set("data", data);
               thumb.set("designOptionId", null);
               found = true;
            } else {
               Groundhog.deleteElement(thumb.get("uniqueId"));
            }
         }
      });
      if (!found) {
         Groundhog.addElement(new ThumbnailData(getNewId(), data, Groundhog.getDesignOptionId()), {global: true});
      }
   });
}

function computeSceneBoundingSphere(root)
{
   var sceneBSCenter = new THREE.Vector3();
   var sceneBSRadius = 0;
   
    root.traverse(function (object) 
    {
        if (object instanceof THREE.Mesh && object.geometry.boundingSphere != null)
        {
            // Object radius
            var radius = object.geometry.boundingSphere.radius;

            // Object center in world space
            var objectCenterLocal = object.position.clone();
            var objectCenterWorld = objectCenterLocal.applyMatrix4(object.matrixWorld);

            // New center in world space
            var newCenter = new THREE.Vector3();
            newCenter.addVectors(sceneBSCenter, objectCenterWorld);
            newCenter.divideScalar(2.0);

            // New radius in world space
            var dCenter = newCenter.distanceTo(sceneBSCenter);
            var newRadius = Math.max(dCenter + radius, dCenter + sceneBSRadius);
            sceneBSCenter = newCenter;
            sceneBSRadius = newRadius;
        }
    });

    return {
       center: sceneBSCenter,
       radius: sceneBSRadius
    };
}

window.centerIsoView = function() {
   var boundingSphere = computeSceneBoundingSphere(scene);
   var pos = new THREE.Vector3(100, 100, 100);
   pos.add(boundingSphere.center);
   camera.position = orbitControls.position = pos;
   camera.lookAt(boundingSphere.center);
   orbitControls.target = boundingSphere.center;
   orbitControls.update();
   requestRedraw();
   render();
}
