define( {
   launcher: function placeHall() {
         var data = new HallData(getNewId(), [0,0,0], [0,0,0], 0);
         data.width = window.hallWidth;
         window.preAddElementForTransaction(data.uniqueId);
         var hall = Groundhog.addElement(data);
         hall.showGuides = true;

         startPlacementEditor(hall, {
            placeCallback: function() {
               setPickCallback(function(position) {
                  var pos1Vec = arrToVector3(hall.get("position1"));
                  hall.set("position2", pos1Vec.sub(pos1Vec.clone().sub(arrToVector3(position)).projectOnVector(arrToVector3(hall.rotationVector()))).toArray());
               });
               setClickCallback(function() {
                  setPickCallback(null);
                  setClickCallback(null);
                  hall.showGuides = false;
                  addToRedrawList(hall.get("uniqueId")); // showGuides isn't persisted - explicitly trigger redraw
                  window.endTransaction();
               });
            }
         });
      }
});