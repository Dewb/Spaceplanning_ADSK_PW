function hasEmissiveMaterial(obj) {
   if (!obj.material)
      return false;

   // Emissive value can equal zero
   if (obj.material.emissive == undefined)
      return false;

   return true;
}

function canSelect(obj) {
   // If the isPickable flag is true using OIT selection code
   if (obj.isPickable)
      return true;

   // Otherwise can only select items with the emissive property (typically meshes)
   if (hasEmissiveMaterial(obj))
      return true;

   return false;
}

function setPrehighlight(obj) {
   if (obj.isPickable) {
      // NOTE: There did not seem to be any way to prehighlight something
      //       without it being highlighted as well (which overrides prehighlighting)
      setHighlight(obj, true);
      return;
   }

   // Set standard (emmissive) prehighlighting
   if (hasEmissiveMaterial(obj)) {
      obj.currentHex = obj.material.emissive.getHex();
      obj.material.emissive.setHex(0x333333);
   }
}

function setHighlight(obj, set) {
   if (obj.isPickable) {
      if (set)
         obj.isHighlighted = true;
      else
         delete obj.isHighlighted;
      return;
   }

   // Set standard (emmissive) highlighting
   if (hasEmissiveMaterial(obj)) {
      if (set) {
         if (obj.currentHex === undefined)
            obj.currentHex = obj.material.emissive.getHex();
         obj.material.emissive.setHex(0x666666);
      }
      else {
         if (obj.currentHex !== undefined)
            obj.material.emissive.setHex(obj.currentHex);
      }
   }
}
