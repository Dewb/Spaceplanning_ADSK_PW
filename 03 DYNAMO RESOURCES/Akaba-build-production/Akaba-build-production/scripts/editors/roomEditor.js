define( {
   launcher: function (dims, usageName) {
         if (dims == null)
            dims = [6, 4, 3.5];
         var spaceData = new SpaceData(getNewId(), [0,0,0], dims, 0);
         spaceData.usageName = usageName;
         window.preAddElementForTransaction(spaceData.uniqueId);
         startPlacementEditor(Groundhog.addElement(spaceData), {placeCallback: function() { window.endTransaction() }});
      }
});