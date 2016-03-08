function OITRenderer(renderer) {

   // Camera for compositing 2D images
   var quadCamera = new THREE.OrthographicCamera(-1, 1, 1, -1, 0, 1);

   ////////////////////////////////////////////
   // OIT accumulation shader

   var oitAccumulatingUniforms = { 
      "highlight": { type: "f", value: 0.0 },
      "opacityScale": { type: "f", value: 1.0 }
   };

   var accumulationMaterial = new THREE.RawShaderMaterial({
      uniforms: oitAccumulatingUniforms,
      vertexShader: document.getElementById('vertexShader').textContent,
      fragmentShader: document.getElementById('fragmentShaderAccumulation').textContent,
      side: THREE.DoubleSide,
      depthWrite: false,
      depthTest: false,
      transparent: true,
      blending: THREE.CustomBlending,
      blendEquation: THREE.AddEquation,
      blendSrc: THREE.OneFactor,
      blendDst: THREE.OneFactor
   });

   var accumulationTexture = new THREE.WebGLRenderTarget(
      window.innerWidth, window.innerHeight,
      {
         minFilter: THREE.NearestFilter,
         magFilter: THREE.NearestFilter,
         type: THREE.FloatType,
         format: THREE.RGBAFormat,
         stencilBuffer: false
      }
   );

   ////////////////////////////////////////////
   // OIT revelage shader

   var revealageMaterial = new THREE.RawShaderMaterial( {
      vertexShader: document.getElementById('vertexShader').textContent,
      fragmentShader: document.getElementById('fragmentShaderRevealage').textContent,
      side: THREE.DoubleSide,
      depthWrite: false,
      depthTest: false,
      transparent: true,
      blending: THREE.CustomBlending,
      blendEquation: THREE.AddEquation,
      blendSrc: THREE.ZeroFactor,
      blendDst: THREE.OneMinusSrcAlphaFactor
   });

   var revealageTexture = new THREE.WebGLRenderTarget(
      window.innerWidth, window.innerHeight,
      {
         minFilter: THREE.NearestFilter,
         magFilter: THREE.NearestFilter,
         type: THREE.FloatType,
         format: THREE.RGBAFormat,
         stencilBuffer: false
      }
   );

   ////////////////////////////////////////////
   // OIT compositing shader

   var oitCompositingUniforms = { 
      "offscreen": { type: "f", value: 0.0 },
      "tAccumulation": { type: "t", value: null },
      "tRevealage": { type: "t", value: null }
   };

   var compositingMaterial = new THREE.ShaderMaterial({
      uniforms: oitCompositingUniforms,
      vertexShader: document.getElementById('vertexShaderQuad').textContent,
      fragmentShader: document.getElementById('fragmentShaderOITCompositing').textContent,
      transparent: true,
      blending: THREE.CustomBlending,
      blendEquation: THREE.AddEquation,
      blendSrc: THREE.OneMinusSrcAlphaFactor,
      blendDst: THREE.SrcAlphaFactor
   });

   var oitQuadScene = new THREE.Scene();
   oitQuadScene.add(new THREE.Mesh(new THREE.PlaneGeometry(2, 2), compositingMaterial));

   ////////////////////////////////////////////
   // Shading texture

   var shadingTexture = new THREE.WebGLRenderTarget(
      window.innerWidth, window.innerHeight,
      {
         minFilter: THREE.NearestFilter,
         magFilter: THREE.NearestFilter,
         type: THREE.FloatType,
         format: THREE.RGBAFormat,
         stencilBuffer: false
      }
   );

   ////////////////////////////////////////////
   // Shading compositing shader

   var shadingCompositingUniforms = { 
      "tShading": { type: "t", value: null }
   };

   var shadingMaterial = new THREE.ShaderMaterial({
      uniforms: shadingCompositingUniforms,
      vertexShader: document.getElementById('vertexShaderQuad').textContent,
      fragmentShader: document.getElementById('fragmentShaderShadingCompositing').textContent,
      transparent: true,
      blending: THREE.MultiplyBlending
   });

   var shadingQuadScene = new THREE.Scene();
   shadingQuadScene.add(new THREE.Mesh(new THREE.PlaneGeometry(2, 2), shadingMaterial));

   ////////////////////////////////////////////
   // Main OIT shading methods

   function renderOITImpl(scene, camera, target) {

      ////////////////////////////////////////////////////////
      // Render to accumulation texture

      scene.overrideMaterial = accumulationMaterial;
      var hasSelection = (window.getSelection().length !== 0);
      var opacityScale = hasSelection ? 0.5 : 1.0;

      // Render non-prehighlighted or selected items normally depending on if there is a selection
      setRenderState(RenderState.transparent | RenderState.faces, true, hasSelection);
      renderer.render(scene, camera, accumulationTexture, true);

      // Render prehighlighted items with a slight white-shift in color
      setRenderState(RenderState.highlighted | RenderState.transparent | RenderState.faces, true, hasSelection);
      oitAccumulatingUniforms["highlight"].value = 0.2;
      oitAccumulatingUniforms["opacityScale"].value = opacityScale;
      renderer.render(scene, camera, accumulationTexture, false);

      // Render unselected items as more transparent if there is a selection
      if (hasSelection) {
         setRenderState(RenderState.transparent | RenderState.faces | RenderState.unselected, true, hasSelection);
         oitAccumulatingUniforms["highlight"].value = 0.0; // No highlight
         oitAccumulatingUniforms["opacityScale"].value = opacityScale;
         renderer.render(scene, camera, accumulationTexture, false);
      }

      // Reset uniforms to their default values
      oitAccumulatingUniforms["highlight"].value = 0.0;
      oitAccumulatingUniforms["opacityScale"].value = 1.0;

      ////////////////////////////////////////////////////////
      // Render to revealage texture

      scene.overrideMaterial = revealageMaterial;
      setRenderState(RenderState.transparent | RenderState.faces);
      renderer.render(scene, camera, revealageTexture, true);

      ////////////////////////////////////////////////////////
      // Composite to main scene

      // The compositing material is attached to the screen quad being rendered 
      scene.overrideMaterial = null;
      oitCompositingUniforms["tAccumulation"].value = accumulationTexture;
      oitCompositingUniforms["tRevealage"].value = revealageTexture;

      if (target !== undefined) {
         oitCompositingUniforms["offscreen"].value = 1.0;
         compositingMaterial.blendSrc = THREE.SrcAlphaFactor; 
        compositingMaterial.blendDst = THREE.OneMinusSrcAlphaFactor;
      } else {
         oitCompositingUniforms["offscreen"].value = 0.0;
         compositingMaterial.blendSrc = THREE.OneMinusSrcAlphaFactor;
         compositingMaterial.blendDst = THREE.SrcAlphaFactor;
      }

      renderer.render(oitQuadScene, quadCamera, target);
   }

   function renderShadingImpl(scene, camera, target) {
      if (target === undefined) {
         setRenderState(RenderState.shaded);
         renderer.render(scene, camera, shadingTexture, true);

         shadingCompositingUniforms["tShading"].value = shadingTexture;
         renderer.render(shadingQuadScene, quadCamera, target);
      }
   }

   this.render = function(scene, camera, target) {

      //////////////////////////////////////////////////////////
      // Draw opaque items (background) and transparent faces

      // Pass 1a: Draw opaque items
      setRenderState(RenderState.opaque);
      renderer.render(scene, camera, target);

      // Pass 1b: Render OIT items (faces), blended onto pass 1
      renderOITImpl(scene, camera, target);

      // Pass 1c: Render the shading
      renderer.clear(false, true, true);
      renderShadingImpl(scene, camera, target);
      renderer.clear(false, true, true);

      //////////////////////////////////////////////////////////
      // Draw internal items (stairs, paths) 

      // Pass 2a: Draw path items
      setRenderState(RenderState.path);
      renderer.render(scene, camera, target);

      // Pass 2b: Draw faces to depth only
      renderer.context.colorMask(false, false, false, false);
      setRenderState(RenderState.faces | RenderState.noInternal);
      renderer.render(scene, camera, target);
      renderer.context.colorMask(true, true, true, true);

      // Pass 2c: Draw edges with depth test
      setRenderState(RenderState.edges | RenderState.internalLines);
      renderer.render(scene, camera, target);

      //////////////////////////////////////////////////////////
      // Draw external items (edges, level boundaries) 

      // Pass 3a: Draw faces to depth only
      renderer.context.colorMask(false, false, false, false);
      setRenderState(RenderState.faces | RenderState.internal);
      renderer.render(scene, camera, target);
      renderer.context.colorMask(true, true, true, true);

      // Pass 3b: Draw edges with depth test
      setRenderState(RenderState.edges | RenderState.externalLines);
      renderer.render(scene, camera, target);

      resetRenderState();
   }
}

var isUsingOIT = true;

var toggleOIT = function() {
   isUsingOIT = !isUsingOIT;
   requestFullRedraw();
   
   return "OIT is " + ((isUsingOIT) ? "on." : "off.");
}

window.useOIT = function() {
   return isUsingOIT;
}
