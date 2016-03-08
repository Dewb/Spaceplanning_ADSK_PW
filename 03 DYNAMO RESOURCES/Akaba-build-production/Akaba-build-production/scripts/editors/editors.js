var pickedElement;
var placeCallback;
var cancelCallback;

function startPlacementEditor(element, callbacks) {
   if (!callbacks)
      callbacks = {};

   // Cancel the previous editor
   if (cancelCallback)
      cancelCallback();

   drawAdjacenciesForElemId(null);

   var isolatedLevel = getIsolatedLevel();
   if (isolatedLevel != null)
      workplane.position.z = (isolatedLevel - 1) * 3.5;
   else if (element.keepOnGround)
      workplane.position.z = 0;

   pickedElement = element;

   setPickCallback( function(pos) {
      doSnappedDrag(pickedElement.get("uniqueId"), pos);
   });

   setClickCallback( function() {
      commitPlacementEditor();
   });

   placeCallback = callbacks.placeCallback;
   cancelCallback = callbacks.cancelCallback;
}

function cancelEditor() {
   if (pickedElement)
      Groundhog.deleteElement(pickedElement.get("uniqueId"));

   cb = cancelCallback;
   resetEditorData();

   if (cb)
      cb();
}

function commitPlacementEditor() {
   cb = placeCallback;

   runAnalysis();
   drawAdjacenciesForElemId(pickedElement.get("uniqueId"));

   resetEditorData();

   if (cb)
      cb();
}

function resetEditorData() {
   setPickCallback(null);
   setClickCallback(null);
   pickedElement = null;
   cancelCallback = null;
   placeCallback = null;
}

var copyBuffer = [];
var isPasteBufferPopulated = function () { return (copyBuffer.length > 0) }

var copySelectedElementsToBuffer = function() {
   copyBuffer = [];
   Groundhog.getEach(getSelection().join(","), {passive: true}, function (element) {
      copyBuffer.push(JSON.stringify(element.data));
   });
}

var pasteFromBuffer = function() {
   var addedElems = [];
   _.each(copyBuffer, function(json, index) {
      var data = JSON.parse(json);
      data.uniqueId = window.getNewId();
      window.preAddElementForTransaction(data.uniqueId);
      var newElem = Groundhog.addElement(data);

      var position = newElem.getPosition();
      if (position != null) {
         var dims = newElem.getWorldDimensions();
         position[0] += dims[0];
         newElem.moveTo(position);

         copyBuffer[index] = JSON.stringify(newElem.data);
      }
      addedElems.push(newElem);
   });
   window.endTransaction();
}

window.pasteFromBufferOnNewLevel = function(zoffset) {
   var addedElems = [];
   _.each(copyBuffer, function(json, index) {
      var data = JSON.parse(json);
      data.uniqueId = window.getNewId();
      window.preAddElementForTransaction(data.uniqueId);
      var newElem = Groundhog.addElement(data);

      var position = newElem.getPosition();
      if (position != null) {
         position[2] += zoffset;
         newElem.moveTo(position);

         copyBuffer[index] = JSON.stringify(newElem.data);
      }
      addedElems.push(newElem);
   });
   window.endTransaction();
   return addedElems;
}

$(document).ready(function () {
   $(document).keyup(function(e) {
      var element = e.target.nodeName.toLowerCase();
      if (element == 'input' || element == 'textarea')
         return;

      if (e.keyCode == 27) // escape
         cancelEditor();
      else if (e.keyCode == 32) { // space
         if (pickedElement)
            pickedElement.rotate();
         e.preventDefault();
         return false;
      };
      return true;
   });

   $(document).keydown(function(e) {
      var element = e.target.nodeName.toLowerCase();
      if (element == 'input' || element == 'textarea')
         return;

      var selection = window.getSelection();

      // Undo (z)
      if (e.keyCode == 90 && !e.shiftKey && (e.ctrlKey || e.metaKey))
         window.undoTransaction();

      // Redo (shift-z or y)
      if (((e.keyCode == 90 && e.shiftKey) || e.keyCode == 89) && (e.ctrlKey || e.metaKey))
         window.redoTransaction();

      // Copy
      if (e.keyCode == 67 && !e.shiftKey && (e.ctrlKey || e.metaKey) && selection.length > 0)
         window.copySelectedElementsToBuffer();

      // Cut
      if (e.keyCode == 88 && !e.shiftKey && (e.ctrlKey || e.metaKey) && selection.length > 0) {
         copyBuffer = [];
         Groundhog.getEach(selection.join(","), {passive: true}, function(element) {
            var elemId = element.get("uniqueId");
            window.pretouchElementForTransaction(elemId);
            copyBuffer.push(JSON.stringify(element.data));
            Groundhog.deleteElement(elemId);
         });
         window.endTransaction();
         unselectAll();
      }

      // Paste
      if (e.keyCode == 86 && !e.shiftKey && (e.ctrlKey || e.metaKey))
         window.pasteFromBuffer();
   });
});