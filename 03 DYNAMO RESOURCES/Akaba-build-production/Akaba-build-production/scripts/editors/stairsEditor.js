define( {
   launcher: function placeStairs() {
         var data = new StairsData(getNewId(), [0,0,0], 0, 35);
         window.preAddElementForTransaction(data.uniqueId);
         var stairs = Groundhog.addElement(data);
         startPlacementEditor(stairs, {placeCallback: function() {window.endTransaction() }});
      }
});