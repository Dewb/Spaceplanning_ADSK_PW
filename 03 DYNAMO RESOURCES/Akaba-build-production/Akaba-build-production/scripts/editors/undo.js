define(function() {
   var undoStack = [{}];
   var redoStack = [{}];

   // Special flags for non-modification actions
   var Added = {};
   var Deleted = {};

   var canUndo = function() { return (undoStack.length > 1 || (undoStack.length == 1 && !_.isEmpty(getCurrentTransaction()))) };
   var canRedo = function() { return (redoStack.length > 1 || (redoStack.length == 1 && !_.isEmpty(getCurrentRedoTransaction()))) };

   window.clearUndoStacks = function() {
      if (canUndo())
         undoStack = [{}];
      if (canRedo())
         redoStack = [{}];
   };

   var getCurrentTransaction = function() { return undoStack[undoStack.length - 1] };
   var getCurrentRedoTransaction = function() { return redoStack[redoStack.length - 1] };

   window.endTransaction = function() {
      if (undoStack.length == 0 || !_.isEmpty(getCurrentTransaction()))
         undoStack.push({});

      if (Groundhog.optionId != null) {
         storeThumbnail();
         runAnalysis( function(analysisResults) {
            storeOptionScore(analysisResults);
         });
      }
   };

   window.pretouchElementForTransaction = function(id, forRedo) {
      // Don't store another backup when the element was already pretouched in this transaction
      if (getCurrentTransaction()[id] != null)
         return;

      // Clear the redo stack when a change happens
      if (!forRedo)
         redoStack = [{}];

      Groundhog.get(id, {passive: true}, function(element) { getCurrentTransaction()[id] = JSON.stringify(element.data) });
   };

   window.preAddElementForTransaction = function(id, forRedo) {
      // Clear the redo stack when a change happens
      if (!forRedo)
         redoStack = [{}];
         
      getCurrentTransaction()[id] = Added;
   };

   var endRedoTransaction = function() {
      if (redoStack.length == 0 || !_.isEmpty(getCurrentRedoTransaction()))
         redoStack.push({});
   };

   var pretouchElementForRedo = function(id) {
      Groundhog.get(id, {passive: true}, function(element) { getCurrentRedoTransaction()[id] = JSON.stringify(element.data) });
   };

   var addDeletionToRedoStack = function(id) { getCurrentRedoTransaction()[id] = Deleted };

   window.undoTransaction = function() {
      if (undoStack.length == 0)
         return;

      var transaction = undoStack.pop();

      // If we are currently in an empty transaction, undo the previous transaction instead
      if (_.isEmpty(transaction))
         transaction = undoStack.pop();
      
      _.each(transaction, function(json, id) {
         // If it was added, just delete it
         if (json == Added) {
            pretouchElementForRedo(id);
            Groundhog.deleteElement(id);
            return;
         }

         // get "all" elements with that id. Ensures that we get called back even if it doesn't exist anymore
         Groundhog.getAll(id, function(elems) {
            var object = JSON.parse(json);
            var elem = elems[id];
            if (elem != null) {
               pretouchElementForRedo(id);
               var elemData = elem.data;
               var newerFields = _.difference(_.keys(elemData), _.keys(object));
               _.each(newerFields, function(fieldName) { delete elemData[fieldName] });
               var touchedFields = newerFields;

               _.each(object, function(data, fieldName) {
                  if (_.isEqual(elemData[fieldName], data))
                     return;

                  elemData[fieldName] = data;
                  touchedFields.push(fieldName);
               });

               _.each(touchedFields, function (fieldName) { elem.touch(fieldName) });
            }

            else {
               addDeletionToRedoStack(id);
               Groundhog.addElement(object);
            }
         });
      });

      endRedoTransaction();
      endTransaction();

      window.unselectAll();
   };

   window.redoTransaction = function() {
      if (redoStack.length == 0)
         return;

      var transaction = redoStack.pop();

      // If we are currently in an empty transaction, redo the previous transaction instead
      if (_.isEmpty(transaction))
         transaction = redoStack.pop();

      _.each(transaction, function(json, id) {
         // Deleted elements
         if (json == Deleted) {
            pretouchElementForTransaction(id, true);
            Groundhog.deleteElement(id);
            return;
         }

         // get "all" elements with that id. Ensures that we get called back even if it doesn't exist anymore
         Groundhog.getAll(id, function(elems) {
            var object = JSON.parse(json);
            var elem = elems[id];
            if (elem != null) {
               pretouchElementForTransaction(id, true);
               var elemData = elem.data;
               var newerFields = _.difference(_.keys(elemData), _.keys(object));
               _.each(newerFields, function (fieldName) { delete elemData[fieldName] });
               var touchedFields = newerFields;

               _.each(object, function(data, fieldName) {
                  if (_.isEqual(elemData[fieldName], data))
                     return;

                  elemData[fieldName] = data;
                  touchedFields.push(fieldName);
               });

               _.each(touchedFields, function (fieldName) { elem.touch(fieldName) });
            }

            else {
               preAddElementForTransaction(id, true);
               Groundhog.addElement(object);
            }
         });
      });

      endTransaction();
      endRedoTransaction();
      if (redoStack.length == 0)
         redoStack = [{}];

      window.unselectAll();
   };
});