requirejs.config({
   paths: {
      lib: '../lib'
   }
});

requirejs(['elements/element'], function () {
   requirejs(['lib/three/controls/OrbitControls',
         'akabaConfig', 'scene', 'display/viewCache', 'display/highlight', 'picking', 
         'display/canvasControls', 'display/line', 'display/text',
         'display/oit', 'display/stlxLoader', 'display/boxGraphics', 'display/edgeGraphics', 'display/meshGraphics',
         'analysis/analyzers', 'analysis/gentest',
         'elements/stairs', 'elements/space', 'elements/hall', 'elements/programGenerators', 'elements/mesh',
         'editors/editors', 'editors/undo',
         'generators/ifcGenerator', 'generators/massGenerator', 'generators/wallGenerator',
         'views/analysis', 'views/toolbar', 'views/displayManager', 'views/projectManager', 'views/networkManager'], function () {
      init();
      initializePicking(scene);
      render();
   });
});
