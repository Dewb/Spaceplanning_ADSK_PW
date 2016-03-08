// returns: the generated button, as a jQuery element
// name (in): the primary text to display on the button
// mainImagePath (in): the primary image to display on the button
// mainCallback (in): the function that will be called when the button is clicked
// secondaryImagePath (in, optional): image for the button's secondary action (a small icon on the side off the button)
// secondaryCallback (in, optional): the function that will be called when the secondary image is clicked
// generatedDivs (output, optional): returns a collection of interesting elements in the button, if your code needs to do anything dynamic
function createLibraryButton(name, mainImagePath, mainCallback, secondaryImagePath, secondaryCallback, generatedDivs) {
   var button = $("<div/>", {"class": "libraryButton"});

   var mainImageSpan = $("<span/>");
   var mainImage = $("<img/>", {src: mainImagePath, "class": "libraryMainButton"});
   var mainLabelContainer = $("<span/>", {"class": "libraryButtonLabel"});
   var mainLabel = $("<div/>", {text: name});
   mainImage.click(mainCallback);
   mainLabelContainer.click(mainCallback);
   mainLabelContainer.append(mainLabel);

   mainImageSpan.append(mainImage);
   button.append(mainImageSpan);
   button.append(mainLabelContainer);

   if (secondaryImagePath && secondaryCallback) {
      var secondaryImage = $("<img/>", {src: secondaryImagePath, "class": "librarySecondaryButton"});
      secondaryImage.click(secondaryCallback);
      button.append(secondaryImage);
   }

   if (generatedDivs != null) {
      generatedDivs.fullButton = button;
      generatedDivs.mainLabel = mainLabel;
      generatedDivs.mainImage = mainImage;

      // Subtext (small text under the label) isn't visible unless the caller does something smart, so only generate it if they ask for it
      var subtext = $("<div />", {text: subtext, "class": "libraryButtonSubtext", style: "display:none"});
      generatedDivs.subtext = subtext;
      mainLabelContainer.append(subtext);
   }

   return button;
}


function createLibrarySubButton(name, callback) {
   var button = $("<div/>", {"class": "librarySubButton"});
   button.append($("<label/>", {text: name}));
   button.click(callback);
   return button;
}

var massDisabledButtons = [];

function createViewControls() {
   var controls = $("<div/>", {id: "viewControls"});

   controls.append(createLibraryButton("Home View", "images/home.svg", window.centerIsoView));
   var infoButton = createLibraryButton("Show Info", "images/information.svg", window.toggleInfoPane);
   infoButton.addClass("infoButton");
   controls.append(infoButton);

   var massButtonDivs = {};
   var selectAllLevelsFunc; // Set below. Mass mode calls this
   controls.append(createLibraryButton("Show Mass", "images/mass.svg", function() {
         selectAllLevelsFunc();
         var visible = toggleMassVisibility();
         if (visible) {
            massButtonDivs.mainImage.css("background-color", "deepskyblue");
            disableElementPicking();
            _.each(massDisabledButtons, function(button) {
               button.css("opacity", 0.2);
               button.css("pointer-events", "none")
            } );
         }
         else {
            massButtonDivs.mainImage.css("background-color", "transparent");
            enableElementPicking();
            _.each(massDisabledButtons, function(button) {
               button.css("opacity", "initial");
               button.css("pointer-events", "auto")
            } );
         }
      },
      null, null, massButtonDivs));

   var levelsDropdown = $("<ul/>", {"class": "levelsDropdown", style: "display: none"});
   var levelsButtonImage;
   var selectedLevel;
   var selectedLevelDiv;
   watchNumberOfLevels(function(numLevels) {
      levelsDropdown.empty();
      for (var i = numLevels; i > 0; i--) { (function(i) {
         var levelDropdownButton = $("<li/>", {text: "Level " + i});
         levelDropdownButton.click(function() {
            isolateLevelVisiblity(i);
            if (selectedLevelDiv != null)
               selectedLevelDiv.removeClass("selectedDropdownRow");
            selectedLevel = i;
            selectedLevelDiv = levelDropdownButton;
            levelDropdownButton.addClass("selectedDropdownRow");
            levelsButtonImage.css("background-color", "deepskyblue");
         });
         if (selectedLevel == i) {
            levelDropdownButton.addClass("selectedDropdownRow");
            selectedLevelDiv = levelDropdownButton;
         }
         levelsDropdown.append(levelDropdownButton);
      })(i)}

      var allLevelsButton = $("<li/>", {text: "All levels"});
      selectAllLevelsFunc = function() {
         isolateLevelVisiblity(null);
         if (selectedLevelDiv != null)
            selectedLevelDiv.removeClass("selectedDropdownRow");
         selectedLevel = null;
         selectedLevelDiv = allLevelsButton;
         allLevelsButton.addClass("selectedDropdownRow");
         levelsButtonImage.css("background-color", "transparent");
      };
      allLevelsButton.click(selectAllLevelsFunc);
      if (selectedLevel == null) {
         allLevelsButton.addClass("selectedDropdownRow");
         selectedLevelDiv = allLevelsButton;
      }
      levelsDropdown.append(allLevelsButton);
   });
   $("body").append(levelsDropdown);
   levelsDropdown.mouseleave(function() { levelsDropdown.fadeOut(500) });
   
   var levelsButtonDivs = {};
   controls.append(createLibraryButton("Levels", "images/layers.svg", function() { levelsDropdown.show(); }, null, null, levelsButtonDivs));
   levelsButtonImage = levelsButtonDivs.mainImage;
   massDisabledButtons.push(levelsButtonDivs.fullButton);

   return controls;
}

function moveInfoPane(e){
   $('#infoPane').css({
      left:  e.pageX + 20,
      top:   e.pageY + 20
   });
   if (pickedElementId != null || pickedControlId != null) {
      
      pickId = pickedElementId
      if (pickedControlId != null) {
         var controlInfo = window.getControlFromId(pickedControlId);
         pickId = controlInfo.controlledElementId;
      }

      Groundhog.get(pickId, function(elem) {
         var infoPane = $('#infoPane');
         infoPane.empty();
         var spaceTypeName = (elem.data.type == "Space" ? elem.data.usageName : elem.data.type);
         
         if (spaceTypeName && spaceTypeName != "")
            spaceTypeName = spaceTypeName.charAt(0).toUpperCase() + spaceTypeName.slice(1);

         infoPane.append($("<div/>", {text: spaceTypeName}));
         var dims = elem.getWorldDimensions();
         var w = dims[0];
         var h = dims[1];
         infoPane.append($("<div/>", {text: w * h + " m² (" + w + "m x " + h + "m)", style: "font-size: smaller"}))

         var adjacencies = getAdjacencyResultsFromElemId(pickId);
         if (adjacencies.length > 0) {
            var somethingReachable = false;
            infoPane.append($("<h2/>", {text: "Circulation:"}));
            var adjacenciesTable = $("<table/>");
            adjacenciesTable.append($("<tr><th>Required adjacency</th><th>Best path</th><th>Goal</th></tr>"));
            _.each(adjacencies, function(adj) {
               var row = $("<tr/>");

               row.append($("<td/>", {text: adj.requirement.to}));
               if (adj.reachable) {
                  somethingReachable = true;
                  row.append($("<td/>", {text: Math.round(adj.distance) + window.displayUnits, "class": (adj.satisfied ? "satisfied" : "tooFar")}));
               }
               else
                  row.append($("<td/>", {text: "None", "class": "unreachable"}));

               row.append($("<td/>", {text: "< " + adj.requirement.maxDistance + window.displayUnits}));
               adjacenciesTable.append(row);
            });
            infoPane.append(adjacenciesTable);

            if (_.isEmpty(selection)) {
               if (somethingReachable)
                  drawAdjacenciesForElemId(pickId, true);
               else
                  drawAdjacenciesForElemId(null);
            }
         }

         infoPane.show();
      });
   } else {
      $('#infoPane').hide();
      if (_.isEmpty(selection))
         drawAdjacenciesForElemId(null);
   }
}

var infoPaneVisible = false;

window.toggleInfoPane = function() {
   if (!($('#infoPane').length)) {
      $("body").append($('<div/>', {id: "infoPane"}));
   }

   if (!infoPaneVisible) {
      $(document).on('mousemove', moveInfoPane);
      $('.infoButton img').css("background-color", "deepskyblue");
      showRoomAlertLabels();
   } else {
      $(document).off('mousemove', moveInfoPane);
      $('#infoPane').remove();
      $('.infoButton img').css("background-color", "transparent");
      hideRoomAlertLabels();
   }
   infoPaneVisible = !infoPaneVisible;
}

define(['scripts/editors/hallEditor.js', 'scripts/editors/roomEditor.js', 'scripts/editors/stairsEditor.js', 'generators/ifcGenerator', 'views/requirementsEditor', 'views/projectManager'], 
      function (hallEditor, roomEditor, stairsEditor, ifc, requirementsEditor, projectManager) {
   var configureProjectName = function(div) {
      // Display the project name and support renaming it
      Groundhog.watchProjectName(function(name) {
         div.show().text(name);
      });
      div.css("cursor", "text");
      div.click(function() {
         var newName = prompt("New name for this project:", Groundhog.getProjectName());
         if (newName)
            Groundhog.renameProject(newName);
         return false;
      });
   };
   var configureOptionName = function(div) {
      Groundhog.watchDesignOptionName(function(name) {
         div.show().text(name);
      });
      div.css("cursor", "text");
      div.click(function() {
         var newName = prompt("New name for this design:", Groundhog.getDesignOptionName());
         if (newName)
            Groundhog.renameDesignOption(newName);
         return false;
      });
   };

   var updateToolbarForApplicationState = function(newState) {
      if (newState == "optionEditor")
         $("#library").show();
      else
         $("#library").hide();
   }

   window.hallWidth = 2.5;

   $(document).ready(function () {
      var library = $("<div/>", {id: "library", class: "noselect"});

      var projectButtonDivs = {};
      library.append(createLibraryButton("Projects", "images/cloud.png", function() {setApplicationState("projectManager")}, null, null, projectButtonDivs));
      configureProjectName(projectButtonDivs.subtext);

      var designsButtonDivs = {};
      library.append(createLibraryButton("Designs", "images/options.svg", function() {setApplicationState("wholeProject")}, null, null, designsButtonDivs));
      configureOptionName(designsButtonDivs.subtext);

      library.append(createLibraryButton("Goals", "images/graph.svg", function() {requirementsEditor.launcher()}));
      
      var buttonDivs = {};

      library.append(createLibraryButton("Stairs", "images/stairs.svg", stairsEditor.launcher, null, null, buttonDivs));
      massDisabledButtons.push(buttonDivs.fullButton);

      var hallButton = createLibraryButton("Hall", "images/exit.svg", hallEditor.launcher, null, null, buttonDivs)
      library.append(hallButton);
      massDisabledButtons.push(hallButton);

      var hallWidthControl = $("<div/>", { id: "hallWidthControl", class: "librarySubButton", style: ""});
      hallWidthControl.append($("<a>").html("&#9664;").click(function () { 
         window.hallWidth -= 0.5; 
         $("#hallWidthDisplay").html(window.hallWidth + " m"); 
      }));
      hallWidthControl.append($("<div>", { id: "hallWidthDisplay", style: "display: inline-block; width: 3em; text-align: center" })
         .html(window.hallWidth + " m"));
      hallWidthControl.append($("<a>").html("&#9654;").click(function () { 
         window.hallWidth += 0.5; 
         $("#hallWidthDisplay").html(window.hallWidth + " m"); 
      }));
      hallButton.after(hallWidthControl);

      var roomButton = createLibraryButton("Room", "images/room.svg", function() { roomEditor.launcher() });
      library.append(roomButton);
      massDisabledButtons.push(roomButton);

      var useButtons = [];
      var useButtonsLibrary = $("<div/>", {id: "useButtonsLibrary"});

      var removeUseButtons = function() {
         _.invoke(useButtons, "remove");

         // Remove the buttons from massDisabledButtons (try to do it in one pass through)
         useButtons.unshift(massDisabledButtons); // Reuse useButtons for without's argument list 
         massDisabledButtons = _.without.apply(_, useButtons);

         useButtons = [];
      };

      Groundhog.watch("program", function(program) {
         removeUseButtons();

         window.prepareSpaceStandardsForGenerator(function(spaceTypes) {
            var sortedSpaceTypes = _.sortBy(spaceTypes, function (s) { return s.data.usageName; });
            _.each(sortedSpaceTypes, function(spaceType) {
               if (spaceType.data.type != "Space")
                  return;
               var name = spaceType.data.usageName;
               var capName = name.charAt(0).toUpperCase() + name.slice(1);
               var dims = spaceType.data.dimensions;
               var button = createLibrarySubButton(capName, function() { roomEditor.launcher(dims, name); });
               useButtonsLibrary.append(button);
               massDisabledButtons.push(button);
               useButtons.push(button);
            });
         })

         roomButton.after(useButtonsLibrary);
      });

      Groundhog.watchUnload("program", removeUseButtons);

      library.append(createLibraryButton("Randomize", "images/dice.svg", function() { testGen(300); }));

      library.append(createLibraryButton("Export", "images/ifc.png", function() { ifc.save(); }));

      library.append(createViewControls())

      $("body").append(library);

      if (projectManager != null)
         updateToolbarForApplicationState(projectManager.getApplicationState());
   });

   $("body").on("applicationStateChanged", function(event, newState) { updateToolbarForApplicationState(newState); });

   return {};
});