define ( ['analysis/analyzers'], function(analyzers) {
   window.displayUnits = "m";
   var debug = false;

   var controlsFormatter = function(row, cell, value, columnDef, dataContext) {
      if (dataContext.internalUsage)
         return "";
      else
         return "<input type='image' class='deleteUseButton' src='images/delete.svg'/>";
   };

   var labelFormatter = function(row, cell, value, columnDef, dataContext) {
      var color, cssColor;
      cssColor = "255, 255, 255";
      if (dataContext.colorIndex > 0) {
         color = spaceColorFromIndex(dataContext.colorIndex);
         cssColor = sceneColorToCSS(color);
      }
      return "<div style='height:20px;font-size:14px;background:rgba(" + cssColor + ", 0.5)' class='usageCells'>" + (_.escape(value)) + "</div>";
   };

   // "success" is a float from 0-1, with 1 being complete success
   var cssColorFromSuccess = function(success, alpha) {
      // Transition from red to green-ish, but don't hit full green until 100%
      var red = 0;
      var green = 255;
      if (success < .9999) {
         red = 255;
         green = 255 * success;
      }
    
      red = Math.floor(red);
      green = Math.floor(green);
    
      return "rgba(" + red + ", " + green + ", 0, " + alpha + ")";
   };

   // Quick implementation of reassigning program colors via the console
   window.assignColorIndex = function(usageName, newColorIndex) {
      Groundhog.getAll("program", function(programs) {
         var program = programs["program"];

         if (program == null) {
            return;
         }

         var uses = program.get("uses");

         var matchingId = undefined;
         _.each(uses, function (v, k) {
            if (v.name === usageName) {
               matchingId = k;
            }
         });

         if (matchingId) {
            uses[matchingId].colorIndex = newColorIndex;
            program.touch("uses");
            window.endTransaction();
         }
      });
   }

   var assignedFormatter = function(row, cell, value, columnDef, dataContext) {
      var amountUsedStyle = "";
      if (columnDef.id == "assigned") {
         if (_.isNumber(value) && _.isNumber(dataContext.goalArea) && dataContext.goalArea > 0) {
            var successColorCSS = cssColorFromSuccess(value / dataContext.goalArea, 0.3)
            amountUsedStyle = ";background:" + successColorCSS;
         }

         if (_.isNumber(value))
            value = (Math.round(value * 100) / 100).toFixed(2);
      }
      else {
         if (_.isNumber(value) && _.isNumber(dataContext.goalCount) && dataContext.goalCount > 0) {
            var successColorCSS = cssColorFromSuccess(value / dataContext.goalCount, 0.3)
            amountUsedStyle = ";background:" + successColorCSS;
         }
      }

      return "<div style='" + amountUsedStyle + "'>" + _.escape(value) + "</div>";
   };

   var proximityFormatter = function(row, cell, value, columnDef, dataContext) {
      if (cell == row+1) {
         return "<input type='image' class='dotIcon' src='images/dot.svg' style='width:12px; height:12px;'/>";
      } 
      if (value==undefined) {
         return '';
      }
      return value;
   };

   var populateCoreDiv = function(div, forSpecificDesign) {
      // Input for number of square meters an elevator can serve
      var areaPerElevatorDiv = $("<div/>", {"class": "coreRequirement"});
      areaPerElevatorDiv.append($("<span/>", {text: "Area/elevator: ", "class": "coreInputLabel"}));
      
      var areaPerElevatorInput = $("<div/>", {"class": "coreInputDiv"});
      var areaPerElevatorField = $("<input/>", {"class": "coreInput"});
      areaPerElevatorInput.append(areaPerElevatorField);
      areaPerElevatorInput.append($("<span/>", {text: " sq.m"}));
      
      areaPerElevatorDiv.append(areaPerElevatorInput);
      div.append(areaPerElevatorDiv);

      // Input for maximum core distance
      var coreDistanceDiv = $("<div/>", {"class": "coreRequirement"});
      coreDistanceDiv.append($("<span/>", {text: "Max core distance: ", "class": "coreInputLabel"}));
      
      var coreDistanceInput = $("<div/>", {"class": "coreInputDiv"});
      var coreDistanceField = $("<input/>", {"class": "coreInput"});
      coreDistanceInput.append(coreDistanceField);
      coreDistanceInput.append($("<span/>", {text: " m"}));

      coreDistanceDiv.append(coreDistanceInput);
      div.append(coreDistanceDiv);

      // Range input for number of elevators per bank
      var elevatorsNumDiv = $("<div/>", {"class": "coreRequirement"});
      elevatorsNumDiv.append($("<span/>", {text: "Elevators/core: ", "class": "coreInputLabel"}));
      var elevatorsNumSlider = $("<div/>", {"class": "coreInputDiv coreSlider"});
      elevatorsNumDiv.append(elevatorsNumSlider);
      elevatorsNumSlider.slider({
         range: true,
         showTip: true,
         min: 4,
         max: 10,
         values: [6,8],
         slide: function( event, ui ) {
            updateElement("elevatorsNumMin", ui.values[0]);
            updateElement("elevatorsNumMax", ui.values[1]);
            elevatorsNumSlider.find("a:eq(0)").css("text-decoration", "none").text(ui.values[0]);
            elevatorsNumSlider.find("a:eq(1)").css("text-decoration", "none").text(ui.values[1]);
         },
         change: function( event, ui ) {
            elevatorsNumSlider.find("a:eq(0)").css("text-decoration", "none").text(ui.values[0]);
            elevatorsNumSlider.find("a:eq(1)").css("text-decoration", "none").text(ui.values[1]);
         }
      });

      elevatorsNumSlider.find("a:eq(0)").text(elevatorsNumSlider.slider('values', 0));
      elevatorsNumSlider.find("a:eq(1)").text(elevatorsNumSlider.slider('values', 1));

      div.append(elevatorsNumDiv);

      var elemId = "buildingStandards";
      //Update the data model when input changes
      var updateElement = function(field, value) {
         Groundhog.getAll(elemId, function(elems) {
            var elem = elems[elemId];
            if (elem == null) {
               var standards = new BuildingStandardsData(elemId);
               window.preAddElementForTransaction(standards.uniqueId);
               elem = Groundhog.addElement(standards, {global: true});
               elem.data.coreReqs = {};
               elem.data.coreReqs["elevatorsNumMin"] = 6;
               elem.data.coreReqs["elevatorsNumMax"] = 8;
            }

            var parsedVal = parseFloat(value);
            if (_.isNaN(parsedVal))
               parsedVal = null;

            var coreReqs = elem.get("coreReqs");
            coreReqs[field] = parsedVal;
            elem.touch("coreReqs");
         })
      };

      areaPerElevatorField.change(function() { 
         updateElement("areaPerElevator", areaPerElevatorField.val()); 
      });
      coreDistanceField.change(function() { 
         updateElement("coreDistance", coreDistanceField.val()); 
      });

      //Update the target and actual
      Groundhog.watch(".BuildingStandards", function(elem) {
         if (elem.get("uniqueId") == elemId) {
            var coreReqs = elem.get("coreReqs");
            if (coreReqs) {
               if (!areaPerElevatorField.is(":focus"))
                  areaPerElevatorField.val(elem.get("coreReqs")["areaPerElevator"]);
               
               if (!coreDistanceField.is(":focus"))
                  coreDistanceField.val(elem.get("coreReqs")["coreDistance"]);
               
               if (!elevatorsNumSlider.is(":focus")) {
                     elevatorsNumSlider.slider('values',0, elem.get("coreReqs")["elevatorsNumMin"]);
                     elevatorsNumSlider.slider('values',1, elem.get("coreReqs")["elevatorsNumMax"]);
               } 
            }
         }
      });

      Groundhog.watchUnload(".BuildingStandards", function(elem) {
         if (elem.get("uniqueId") == elemId) {
            areaPerElevatorField.val("");
            coreDistanceField.val("");
            elevatorsNumSlider.slider('values',0, 6);
            elevatorsNumSlider.slider('values',1, 8);
         }
      }) 
   };

   var shellShapes = {
      R:"images/shellShapes/rShape.svg", 
      L:"images/shellShapes/lShape.svg", 
      U:"images/shellShapes/uShape.svg", 
      O:"images/shellShapes/oShape.svg"};

   var populateSiteDiv = function(div, forSpecificDesign) {
      
      //Input for target site dimensions
      var siteDiv = $("<div/>");
      siteDiv.append($("<span/>", {text: "Target site dimensions: "}));
      var targetWidthField = $("<input/>");
      siteDiv.append(targetWidthField);
      siteDiv.append($("<span/>", {text: "m"}));
      siteDiv.append($("<span/>", {text: " × ", style: "font-size:1.5em"}));
      var targetHeightField = $("<input/>");
      siteDiv.append(targetHeightField);
      siteDiv.append($("<span/>", {text: "m"}));
      div.append(siteDiv);
      
      //Selection of shell shape
      var shellDiv = $("<div/>",  {style: "margin-top: 20px"});
      shellDiv.append($("<div/>", {text: "Shell shape: "}));
      $.each(shellShapes, function (shellName, path) {
         var imageButton = $("<img/>", {src: path, "class": "shellIcon", "data-shellName": shellName});
         imageButton.click(function(){
            if (imageButton.hasClass("selected")) {
               updateShellShape(null);
            }
            else
               updateShellShape(shellName);            
         });
         shellDiv.append(imageButton);
      });
      div.append(shellDiv);

      //Update the data model when site dimensions input changes
      var siteElemId = "targetSiteDimensions";
      var updateSiteElement = function(updateWidth, value) {
         Groundhog.getAll(siteElemId, function(elems) {
            var elem = elems[siteElemId];
            if (elem == null) {
               var data = new FuzzySiteDimensionsData(siteElemId);
               window.preAddElementForTransaction(data.uniqueId);
               elem = Groundhog.addElement(data, {global: true});
            }

            var parsedVal = parseFloat(value);
            if (_.isNaN(parsedVal))
               parsedVal = null;
            if (updateWidth)
               elem.set("width", parsedVal);
            else
               elem.set("height", parsedVal);

            window.endTransaction();
         })
      };

      targetWidthField.change(function() { updateSiteElement(true, targetWidthField.val()) });
      targetHeightField.change(function() { updateSiteElement(false, targetHeightField.val()) });

      //Update the data model when shell shape selection changes
      var shellElemId = "mesh";
      var updateShellShape = function(shellName) {
         Groundhog.getAll(shellElemId, function(elems) {
            var elem = elems[shellElemId];
            if (elem == null) {
               var data = new MeshData(shellElemId);
               window.preAddElementForTransaction(data.uniqueId);
               elem = Groundhog.addElement(data, {global: true});
            }
            if (shellName)
               elem.set("name", shellName);
            else
               elem.set("name", "None");
            elem.set("dimensions", [0,0,0]);
            elem.set("position", [0,0,0]);
            elem.set("rotation", 0);
            window.endTransaction();
         });
      };

      // Display actual site dimensions for the design
      if (forSpecificDesign) {
         var actualDiv = $("<div/>", {style: "display:none"});
         actualDiv.append($("<span/>", {text: "Actual dimensions: "}));
         var actualWidthField = $("<span/>");
         actualDiv.append(actualWidthField);
         actualDiv.append($("<span/>", {text: "m"}));
         actualDiv.append($("<span/>", {text: " × ", style: "font-size:1.5em"}));
         var actualHeightField = $("<span/>");
         actualDiv.append(actualHeightField);
         actualDiv.append($("<span/>", {text: "m"}));
         div.append(actualDiv);
      }

      // Update the target and actual site dimensions
      Groundhog.watch(".FuzzySiteDimensions", function(elem) {
         if (elem.get("uniqueId") == siteElemId) {
            if (!targetWidthField.is(":focus"))
               targetWidthField.val(elem.get("width"));
            if (!targetHeightField.is(":focus"))
               targetHeightField.val(elem.get("height"));
         }
         // The non-global FuzzySiteDimensions is the analysis result for this design
         else if (forSpecificDesign) {
            actualDiv.show();
            actualWidthField.text(elem.get("width"));
            actualHeightField.text(elem.get("height"));
         }
      });

      Groundhog.watchUnload(".FuzzySiteDimensions", function(elem) {
         if (elem.get("uniqueId") == siteElemId) {
            targetWidthField.val("");
            targetHeightField.val("");
         }
         // The non-global FuzzySiteDimensions is the analysis result for this design
         else if (forSpecificDesign) {
            actualDiv.hide();
         }
      });

      //Update the site selection
      Groundhog.watch(".Mesh", function(elem) {
         if (elem.get("uniqueId") == shellElemId) {
            var shapeName = elem.get("name");
            var shells = $(shellDiv).children(".shellIcon");
            _.each(shells, function(icon) {
               if (shapeName==$(icon).attr("data-shellName")) 
                  $(icon).addClass("selected");
               else 
                  $(icon).removeClass("selected");
            });
         }
      });

      Groundhog.watchUnload(".Mesh", function(elem) {
         if (elem.get("uniqueId") == shellElemId) {
            var shells = $(shellDiv).children(".shellIcon");
            _.each(shells, function(icon) {
               if ($(icon).hasClass("selected"))
                  $(icon).removeClass("selected");
            });
         }
      });
   };

   var populateCustomDiv = function(div) {
      var newScriptRow = $("<div/>");
      var newScriptId; // Used to automatically reveal a script's editor after creation
      div.append(newScriptRow);
      var newScriptButton = $("<span/>", {"class": "link", text: "+ New custom analyzer", style: "font-weight: bold"});
      newScriptRow.append(newScriptButton);
      newScriptButton.click(function() {
         var data = new ScriptData(getNewId(), "My analyzer", "", {});
         window.preAddElementForTransaction(data.uniqueId);
         newScriptId = data.uniqueId;
         Groundhog.addElement(data, {global: true});
      });

      // Add a dropdown to create from a template
      var templateSelection = $("<select/>", {style: "float: right"});
      newScriptRow.append(templateSelection);
      var selectorTitle = $("<option/>", {text: "New from template", disabled: true, selected: true});
      templateSelection.append(selectorTitle);
      _.each([{
            label: "Level affinity",
            name: "Put public space on lower floors",
            code: "if (elem.get(\"usageName\") == \"public\")\n    setResult((elem.getLevel() - 1) / 3);"
         },
         {
            label: "Room dimensions",
            name: "Golden ratio rooms",
            code: "if (elem.get(\"usageName\")) {\n   var dims = elem.getWorldDimensions();\n   var goldenness = Math.min(\n      Math.abs(dims[0]/dims[1] - 1.618),\n      Math.abs(dims[1]/dims[0] - 1.618));\n   setResult(goldenness / 0.25);\n}"
         }],
         function(optionData) {
            var option = $("<option/>", {text: optionData.label});
            templateSelection.append(option);
            option.data("scriptData", optionData);
         }
      );
      templateSelection.change(function () {
         var optionSelected = $(this).find("option:selected");
         var scriptData = optionSelected.data("scriptData");
         selectorTitle.attr('selected', 'selected');
         var data = new ScriptData(getNewId(), scriptData.name, scriptData.code, {});
         window.preAddElementForTransaction(data.uniqueId);
         newScriptId = data.uniqueId;
         Groundhog.addElement(data, {global: true});
      });

      var accordion = $("<div/>", {"class": "accordion"});
      div.append(accordion);

      accordion.accordion({collapsible: true, animate: { duration: 200 }, active: false, heightStyle: "content"});
      var scriptDivs = {};

      accordion.accordion({
         activate: function(event, ui) {
            var activeId = ui.newHeader.data("elemId");
            if (activeId != null) {
               var scriptDiv = scriptDivs[activeId];
               if (scriptDiv != null)
                  scriptDiv.editor.refresh();
            }
         }
      });

      Groundhog.watchEach(".Script", function(scriptElem) {
         var elemId = scriptElem.get("uniqueId");
         var scriptDiv = scriptDivs[elemId];
         if (!scriptDiv) {
            scriptDiv = {};
            scriptDiv.header = $("<h3/>");
            scriptDiv.header.data("elemId", elemId)
            scriptDiv.header.append($("<span/>", {"class": "innerText"}));
            scriptDiv.body = $("<div/>");
            accordion.prepend(scriptDiv.body);
            accordion.prepend(scriptDiv.header);
            scriptDivs[elemId] = scriptDiv;

            var nameRow = $("<div/>");
            nameRow.append($("<span/>", {text: "Name: "}));
            var nameInput = $("<input/>", {"class": "scriptName", style: "width:200px"});
            nameRow.append(nameInput);
            var deleteButton = $("<span/>", {text: "× Delete", style: "margin-left:10px;color: #aaa;", "class": "link"});
            deleteButton.click( function() {
               var answer = confirm("Really delete \"" + nameRow.find(".scriptName").val() + "\"?");
               if (answer)
                  Groundhog.deleteElement(elemId);
            });
            nameRow.append(deleteButton);
            scriptDiv.body.append(nameRow);

            var codeInput = $("<textarea/>");
            scriptDiv.body.append(codeInput);

            scriptDiv.editor = CodeMirror.fromTextArea(codeInput.get()[0], {
              tabSize: 3,
              indentUnit: 3,
              indentWithTabs: false,
              mode: "javascript",
              theme: "elegant",
              viewportMargin: Infinity
            });

            var buttonDiv = $("<div/>");
            var cancelButton = $("<input/>", {type: "button", value: "Cancel", disabled: true});
            buttonDiv.append(cancelButton);
            var saveButton = $("<input/>", {type: "button", value: "Save", disabled: true});
            buttonDiv.append(saveButton);
            scriptDiv.body.append(buttonDiv);

            saveButton.click(function() {
               Groundhog.get(elemId, function(elem) {
                  window.pretouchElementForTransaction(elemId);
                  var name = scriptDiv.body.find(".scriptName").val();
                  var code = scriptDiv.editor.getValue();
                  elem.set("name", name);
                  elem.set("code", code);
                  window.endTransaction();

                  saveButton.prop('disabled', true);
                  cancelButton.prop('disabled', true);
               });
            });

            cancelButton.click(function() {
               nameInput.val(scriptDiv.originalName);
               scriptDiv.editor.setValue(scriptDiv.originalCode);
               saveButton.prop('disabled', true);
               cancelButton.prop('disabled', true);
            })

            // Update Save/Cancel button state based on whether the user changed anything
            changeHandler = function() {
               if (nameInput.val() != scriptDiv.originalName || scriptDiv.editor.getValue() != scriptDiv.originalCode) {
                  saveButton.prop('disabled', false);
                  cancelButton.prop('disabled', false);
               }
               else {
                  saveButton.prop('disabled', true);
                  cancelButton.prop('disabled', true);
               }
            };
            nameInput.on('input propertychange paste', changeHandler);
            scriptDiv.editor.on("change", changeHandler);

            accordion.accordion("refresh");
            if (newScriptId == elemId) {
               accordion.accordion("option", "active", 0);
               newScriptId = null;
            }
         }

         scriptDiv.originalName = scriptElem.get("name");
         scriptDiv.originalCode = scriptElem.get("code");
         scriptDiv.header.find(".innerText").text(scriptDiv.originalName);
         scriptDiv.body.find(".scriptName").val(scriptDiv.originalName);
         scriptDiv.editor.setValue(scriptDiv.originalCode);
      });

      Groundhog.watchUnload(".Script", function(scriptElem) {
         var elemId = scriptElem.get("uniqueId");
         var scriptDiv = scriptDivs[elemId];
         if (scriptDiv != null) {
            scriptDiv.header.remove();
            scriptDiv.body.remove();
            delete scriptDivs[elemId];
         }
      });
   };

   var populateBubbleDiv = function(div, forSpecificDesign) {
      if (forSpecificDesign)
         var width = 500;
      else
         var width = 750;
      
      var height = 500;
      var minArea = Number.MAX_VALUE;
      var maxArea = 0;
      var nodes = []
      var links = [];
      var mousedownNode = null;
      var mouseupNode = null;
      var selectedNode = null;
      var selectedLink = null;
      var lastClick = null;
      var areaCap = 2000; 
      var defaultRadius = 12;

      /* On drag user fixes node's position */
      var dragstart = function(d) {
         d3.select(this).classed("fixed", d.fixed = true);
      };

      /* Updates force layout on each iteration - called automatically */
      var tick = function() {
         link.attr('d', function(d) {
            var deltaX = d.target.x - d.source.x;
            var deltaY = d.target.y - d.source.y;
            var dist = Math.sqrt(deltaX * deltaX + deltaY * deltaY);
            var normX = deltaX / dist;
            var normY = deltaY / dist;
            var sourcePadding = d.source.rad;
            var targetPadding = d.target.rad + 7;
            var sourceX = d.source.x + (sourcePadding * normX);
            var sourceY = d.source.y + (sourcePadding * normY);
            var targetX = d.target.x - (targetPadding * normX);
            var targetY = d.target.y - (targetPadding * normY);
            return 'M' + sourceX + ',' + sourceY + 'L' + targetX + ',' + targetY;
         });

         node.attr("transform", function(d) { 
            return "translate(" + d.x + "," + d.y + ")"; });
      };

      //initialize d3 force layout
      var force = d3.layout.force()
         .nodes(nodes)
         .links(links)
         .size([width, height])
         .charge(-400)
         .linkDistance(130)
         .on("tick", tick);

      var drag = force.drag()
         .on("dragstart", dragstart);

      //set up SVG to host the d3 elements
      var svg = d3.select(div.get()[0]).append('svg')
         .attr('width', width)
         .attr('height', height);

      //define arrow symbol for graph links
      svg.append('svg:defs').append('svg:marker')
         .attr('id', 'end-arrow')
         .attr('viewBox', '0 -5 10 10')
         .attr('refX', 6)
         .attr('markerWidth', 3)
         .attr('markerHeight', 3)
         .attr('orient', 'auto')
         .append('svg:path')
         .attr('d', 'M0,-5L10,0L0,5')
         .attr('fill', '#b0b0b0');

      //instantiate the line displayed when adding a link
      var dragLine = svg.append('svg:path')
         .attr('class', 'link dragline hidden')
         .attr('d', 'M0,0L0,0');

      var createContextMenu = function(inputDivList, saveAction) {
         d3.event.preventDefault();
         contextMenuShowing = true;
         
         var popup = d3.select(div.get()[0])
            .append("div")
            .attr("class", "popup")
            .style("left", d3.mouse(div.get()[0])[0]+ "px")
            .style("top", d3.mouse(div.get()[0])[1] + "px");

         var $popupDiv = $(popup[0]);
         for (i = 0; i < inputDivList.length; i++) { 
            $popupDiv.append(inputDivList[i]);
         }

         var $cancelButton = $("<img/>", {src: "images/cancel.svg", "class": "popupButton"});
         var $saveButton = $("<img/>", {src: "images/tick.svg", "class": "popupButton"});
         $cancelButton.click(function() {
               d3.select(".popup").remove();
               contextMenuShowing = false;
         });
         $saveButton.click(saveAction);
         var $buttonDiv = $("<div/>");
         $popupDiv.append($buttonDiv); 

         $buttonDiv.append($saveButton);
         $buttonDiv.append($cancelButton);    
      }

      //context menu implementation
      var contextMenuShowing = false;
      d3.select( div.get()[0]).on('contextmenu',function () {
         if (contextMenuShowing) {
            d3.event.preventDefault();
            d3.select(".popup").remove();
            contextMenuShowing = false;
         } 
         else {
            var d3target = d3.select(d3.event.target);
            d = d3target.datum();

            if (d3target.classed("spaceUsage")) {
               var inputDivList = [
                  '<tr><td>usage <td> <input type="text" class="popupTextBox" id="usageInput" value= "' 
                  + d.name + '"> </td></tr>',
                  '<tr><td>area <td> <input type="text" class="popupTextBox" id="areaInput" value= "' 
                  + d.area + '"> </td></tr>'
               ];

               createContextMenu(inputDivList, function() {
                  var areaInputToInt = parseInt($('#areaInput').val());
                  if (!isNaN(areaInputToInt)) {
                     updateUsageRequirement (d.id, $('#usageInput').val(), areaInputToInt, 1);
                     d3.select(".popup").remove();
                     contextMenuShowing = false;
                  }
               }); 
            }
            if (d3target.classed("link")) {
               var inputDivList = [
                  '<p>' + d.source.name + ' -> ' + d.target.name + '<p>',
                  '<tr><td>proximity <td> <input type="text" class="popupTextBox" id="proximityInput" value= "' 
                  + d.proximity + '"> </td></tr>'];

               createContextMenu(inputDivList, function() {
                  var proximityInputToInt = parseFloat($('#proximityInput').val());

                  if (!isNaN(proximityInputToInt)) {
                     addNewProximityRequirement(d.source.id, d.target.id, proximityInputToInt);
                     window.endTransaction();
                  } 
                  d3.select(".popup").remove();
                  contextMenuShowing = false;
               });    
            }
         }
      });

      //handles to link and node groups
      var link = svg.append('svg:g').selectAll('path');
      var node = svg.append('svg:g').selectAll('g');
      
      
      Groundhog.watchAll("program", function(elements) {
         var program = elements.program;
         if (!program)
            return;
         var uses = program.get("uses");
         var programReqs = program.get("programReqs");
         var proximityReqs = program.get("proximityReqs");

         var informMinMaxAreaVars = function(area) {
            if (typeof(area) !== "undefined") {
               var trimArea = Math.min(area, areaCap);
               minArea = Math.min(minArea, trimArea);
               maxArea = Math.max(maxArea, trimArea);
            }
         };

         //space nodes need to added or a usage requirement has been updated
         if (Object.keys(uses).length >= svg.selectAll(".node")[0].length) {
            for (useId in uses) {
               var found = false;
               var foundNode = null;

               svg.selectAll(".node").each(
                  function(d) {
                     if (useId === d.id) {
                        found = true;
                        foundNode = d;
                        return false;
                     }
               });

               if (!found) {
                  var area = programReqs[useId].requiredArea;
                  informMinMaxAreaVars(area);

                  var thisNode = { name: uses[useId].name, 
                     color: uses[useId].colorIndex, id: useId, area: area};
                  
                  if (lastClick) {
                     thisNode.x = lastClick[0];
                     thisNode.y = lastClick[1];
                     thisNode.fixed = true;
                     lastClick = null;
                  }
                  nodes.push(thisNode);
               }
               else { //update the found node
                  var name = uses[foundNode.id].name;
                  var area = programReqs[foundNode.id].requiredArea;
                  var count = programReqs[foundNode.id].quantity;
                  if (name!=foundNode.name) foundNode.name = name;
                  if (area!=foundNode.area) {
                     foundNode.area = area;
                     informMinMaxAreaVars(area);
                  }
                  //if (count!=d.name) d.name = name;
               }
            }
         }
         //space nodes need to be deleted
         else if (Object.keys(uses).length < svg.selectAll(".node")[0].length) {
            var index = 0;
            svg.selectAll(".node").each(
               function(d) {
                  var found = false;
                  for (use in uses) {
                     if (use === d.id) {
                        found = true;
                        index++;
                        break;
                     }
                  }
                  if (!found) {
                     nodes.splice(index,1);
                  }
               }
            );
         }

         updateLinks(programReqs, proximityReqs);
         startLayout();
      });

      var updateLinks = function(programReqs, proximityReqs) {
         for (i = links.length-1; i>=0; i--) {
            var proximityReq = proximityReqs[links[i].source.id];
            if (!proximityReq || (proximityReq && !proximityReq.distanceTo[links[i].target.id]))
               links.splice(i,1);
         }

         for (id in programReqs) {
            var proximityReq = proximityReqs[id];
            if (proximityReq) {
               //reference the source graph node with the given id
               var thisNode = nodes.filter(function(n) {return n.id === id;})[0];

               for (key in proximityReq.distanceTo) {
                  //reference the target graph node with the given id
                  var otherNode = nodes.filter(function(n) {return n.id === key;})[0];
                  
                  var thisLink = links.filter(
                     function(n) {
                        return (n.source.id === thisNode.id && n.target.id === otherNode.id);
                     });
                  //if the link does not exist add it
                  if (thisLink.length == 0 && thisNode.id!==otherNode.id) {
                     links.push({source: thisNode, target: otherNode, proximity: proximityReq.distanceTo[key]});
                  }
                  //if the proximity requirement has been updated
                  else if (thisNode.id!==otherNode.id) {
                     if (proximityReq.distanceTo[key] !== thisLink[0].proximity)
                        thisLink[0].proximity = proximityReq.distanceTo[key];
                  }
               }
            }
         }
      };

      var getRadius = function(d) {
         if (isNaN(minArea)|| isNaN(maxArea) || typeof(d.area) === "undefined" || d.area===0) {
            d['rad'] = defaultRadius;
            return d['rad'];
         }
         var radius = mapRange(Math.min(d.area, areaCap), minArea, maxArea, 12, 36);
         d['rad'] = radius;
         return d['rad'];
      };

      function startLayout() {
         //NODE GROUP ------------------------
         node = node.data(force.nodes(), function(d) { return d.id; });

         //update existing nodes on 'selected' state
         node.classed('selected', function(d) { return d === selectedNode; })
         //Update existing nodes if usage names changed
         d3.selectAll("circle").attr("r", function(d) { return getRadius(d); });

         //add new nodes
         var nodeEnter = node.enter().append("svg:g")
            .attr("class", "node")
            .on("dblclick", function(d) {
               d3.select(this).classed("fixed", d.fixed = false);
               d3.event.stopPropagation(); //to prevent adding a node when clicking on one
            }) 
            .call(drag);

         //add new circles
         nodeEnter.append("svg:circle")
            .attr("class", "spaceUsage")
            .attr("r", function(d) { return getRadius(d); })
            .style("fill", function(d) { 
               var color, cssColor;
               cssColor = "255, 255, 255";
               if (d.color > 0) {
                  color = spaceColorFromIndex(d.color);
                  cssColor = sceneColorToCSS(color);
               }
               return "rgba(" + cssColor + ", 0.5)"; 
            })
            .on("mouseover", function(d) {
               if (!mousedownNode || d === mousedownNode) 
                  return;
               d3.select(this).attr('transform', 'scale(1.2)');
            })
            .on("mouseout", function(d) {
               if(!mousedownNode || d === mousedownNode) 
                  return;
               d3.select(this).attr('transform', '');
            })
            .on("click", function(d) {
               if (d3.event.defaultPrevented) return; //click suppressed in case of drag event

               if (selectedNode === d) {
                  selectedNode = null;
                  if (debug)
                     console.log("node deselected: " + d.name);
               }
               else {
                  selectedLink = null;
                  selectedNode = d;
                  if (debug)
                     console.log("node selected: " + d.name);
               }
               startLayout(); 
            })
            .on("mousedown", function(d) {
               if (d3.event.shiftKey) {
                  mousedownNode = d;
                  if (debug)
                     console.log("mousedown at: " + mousedownNode.name);
                  dragLine
                     .style('marker-end', 'url(#end-arrow)')
                     .classed('hidden', false)
                     .attr('d', 'M' + mousedownNode.x + ',' + mousedownNode.y 
                        + 'L' + mousedownNode.x + ',' + mousedownNode.y);
               }
            })
            .on('mouseup', function(d) {
               if (!mousedownNode) return;
             
               dragLine.classed('hidden', true)
                  .style('marker-end', '');

               mouseupNode = d;
               if (mouseupNode === mousedownNode) {
                  resetMouseVars(); 
                  return; 
               }

               d3.select(this).attr('transform', '');

               var source = mousedownNode;
               var target = mouseupNode;

               thisLink = links.filter( function(l) {
                  return (l.source === source && l.target === target);
               })[0];

               if (!thisLink) {
                  addNewProximityRequirement(source.id, target.id, 100);
               }            
            });

         //Update existing name tags if names and radii changed
         d3.selectAll("text")
            .text( function(d) { return d.name; })
            .attr("x", function(d) { 
               return d.rad; 
            });

         //Add usage labels to the nodes
         nodeEnter.append("svg:text")
            .attr("x", function(d) { 
               return d.rad; 
            })
            .attr("dy", ".35em")
            .text( function(d) { 
               return d.name; 
            });
         
         //Remove old nodes
         node.exit().remove();

         //LINK GROUP ------------------------
         link = link.data(force.links());

         //update existing links on selected state
         link.classed('selected', function(d) { return d === selectedLink; })
            .style('marker-end', 'url(#end-arrow)');

         link.enter().append('svg:path')
            .attr("class", "link")
            .style('marker-end', 'url(#end-arrow)')
            .on("click", function(d) {
               if (d === selectedLink) {
                  if (debug)
                     console.log("link deselected: " + d.source.name + " -> " + d.target.name);
                  selectedLink = null;
               }
               else {
                  if (debug)
                     console.log("link selected: " + d.source.name + " -> " + d.target.name);
                  selectedNode = null;
                  selectedLink = d;
               }
               startLayout();
            });

         //remove old links
         link.exit().remove();

         force.start();
      };

      svg.on("mousedown", function() {
         //to prevent curson changing to text-selection cursor
         d3.event.preventDefault();
      });

      svg.on("mousemove", function() {
         if (!mousedownNode) return;
         //show dragline following the mouse move
         dragLine.attr('d', 'M' + mousedownNode.x + ',' + mousedownNode.y 
            + 'L' + d3.mouse(this)[0] + ',' + d3.mouse(this)[1]);
      });

      svg.on("mouseup", function() {
         if (mousedownNode) {
            //hide drag line
            dragLine.classed('hidden', true).style('marker-end', '');
         } 

         svg.classed('active', false);
         resetMouseVars();
      });

      /* 
      By holding shift and double clicking on the svg you add a space 
      labeled as 'undefined usage'. 
      */
      svg.on("dblclick", function() {
         if (mousedownNode) return;
         if (d3.event.shiftKey) {
            //extract and reference the click location
            lastClick = d3.mouse(this);
            return addNewProgramRequirement("undefined usage", null, null);
         }
      });

      /* 
      KEY EVENTS:
         - Delete a selected node by pressing 'D' key. 
         - Delete a selected link by pressing 'D' key.
         - When pressing shift and click on a node dragging of that node is disabled.
      */
      d3.select(window).on('keydown', function() {
         if (d3.event.keyCode===16) { //SHIFT
            node.on('mousedown.drag', null)
               .on('touchstart.drag', null);
         }
         if (d3.event.keyCode===68) { //KEY 'D'
            if (selectedNode) {
               if (debug)
                  console.log("Deleting: " + selectedNode.name);
               deleteProgramUsage(selectedNode.id);
               selectedNode = null;
            }
            if (selectedLink) {
               if (debug) {
                  console.log("Deleting: " + selectedLink.source.name 
                     + " -> " + selectedLink.target.name);
               }
               removeProximityRequirement(selectedLink.source.id, selectedLink.target.id);
               selectedLink = null;
            }      
         }
      });

      /* When the shift key stops being pressed dragging of the node 
       is being reactivated.
      */
      d3.select(window).on('keyup', function() {
         if (d3.event.keyCode===16) //SHIFT
            node.call(drag);
      });

      function mapRange(value, low1, high1, low2, high2) {
         if ((high1 - low1)!==0)
            return low2 + (high2 - low2) * (value - low1) / (high1 - low1);
         else
            return value;
      };

      function resetMouseVars() {
         mousedownNode = null;
         mouseupNode = null;
      };
   };

   var isValidUsageName = function(name, otherNames) {
      if (name == null || name.length == 0)
         return false;
      return !_.some(otherNames, function(otherName) { return otherName.toUpperCase() == name.toUpperCase(); });
   };

   var populateUsageDiv = function(div, forSpecificDesign) {
      var usageColumns = [
      {
         id: "use",
         name: "Use",
         field: "use",
         width: 140,
         editor: Slick.Editors.Text,
         sortable: true,
         formatter: labelFormatter
      }, {
         id: "goalArea",
         name: "Goal (sq.m.)",
         field: "goalArea",
         width: 100,
         editor: Slick.Editors.Integer,
         sortable: true
      }, {
         id: "goalCount",
         name: "Quantity",
         field: "goalCount",
         width: 70,
         editor: Slick.Editors.Integer,
         sortable: true
      }];

      if (forSpecificDesign) {
         usageColumns.push({
            id: "assigned",
            name: "Output (sq.m.)",
            field: "assigned",
            width: 120,
            formatter: assignedFormatter,
            sortable: true
         }, {
            id: '#',
            name: 'QTY', 
            field: 'quantity',
            width: 40,
            resizable: false,
            formatter: assignedFormatter,
            sortable: true
         });
      }

      usageColumns.push({
         id: 'delete',
         name: '', 
         field: 'delete',
         width: 10,
         resizable: false,
         formatter: controlsFormatter
      });

      var usageOptions = {
         editable: true,
         autoEdit: true,
         enableCellNavigation: true,
         enableColumnReorder: false,
         enableAddRow: true,
         autoHeight: true
      };

      var usageData = [];
      var usageGrid = new Slick.Grid(div, usageData, usageColumns, usageOptions);
      usageGrid.registerPlugin(new Slick.AutoTooltips());

      var programGridWatchSelector = "program";
      if (forSpecificDesign)
         programGridWatchSelector += ", .SpaceAssignmentResults";

      Groundhog.watchAll(programGridWatchSelector, function(elements) {
         var program = elements.program;
         if (!program)
            return;

         if (forSpecificDesign) {
            var potentialResults = _.values(_.omit(elements, "program"));
            if (potentialResults.length > 1)
               console.log("Warning: multiple potential space assignment results within a single design option");
            if (potentialResults.length >= 1)
               var assignmentResults = potentialResults[0].get("assignedSpace");
         }

         var uses = program.get("uses");
         var programReqs = program.get("programReqs");

         usageData = [];

         if (assignmentResults != null) {
            var circulationSpace = 0;
            if (assignmentResults["Hall"] != null)
               circulationSpace += assignmentResults["Hall"].area;
            if (assignmentResults["Stair"] != null)
               circulationSpace += assignmentResults["Stair"].area;
            usageData.push({use: "circulation", assigned: circulationSpace, internalUsage: true});
         }

         var id, use, programReq, programRow;
         for (id in programReqs) {
            use = uses[id];
            programReq = programReqs[id];

            //set data for a row of the usage grid
            programRow = {};
            programRow.use = use.name;
            if (programReq.requiredArea != null)
               programRow.goalArea = programReq.requiredArea;
            if (programReq.quantity != null)
               programRow.goalCount = programReq.quantity;
            if (forSpecificDesign && assignmentResults != null) {
               var assignment = assignmentResults[use.name];
               if (assignment != null) {
                  programRow.assigned = assignment.area;
                  programRow.quantity = assignment.quantity;
               } else {
                  programRow.assigned = 0;
                  programRow.quantity = 0;
               }
            }
            programRow.colorIndex = (_ref3 = use.colorIndex) != null ? _ref3 : 0;
            programRow.id = id;
            usageData.push(programRow);
         }

         usageGrid.setData(usageData);
         gridChanged(usageGrid);
      });

      Groundhog.watchUnload("program", function(program) {
         usageData = [];
         usageGrid.setData(usageData);
         gridChanged(usageGrid);
      });

      usageGrid.onAddNewRow.subscribe(function(e, args) {
         usageGrid.invalidateRow(usageData.length);
         return addNewProgramRequirement(args.item.use, args.item.goalArea, args.item.goalCount);
      });

      usageGrid.onClick.subscribe(function(e, args) {
         if ($(e.target).hasClass('deleteUseButton'))
            deleteProgramUsage(usageData[args.row].id);
      });

      usageGrid.onCellChange.subscribe(function(e, args) {
         var _ref, _ref1;
         var id = args.item.id;
         var useName = args.item.use;
         var goalArea = args.item.goalArea;
         if (goalArea == 0)
            goalArea = null;
         var goalCount = args.item.goalCount;
         if (goalCount == 0)
            goalCount = null;

         Groundhog.get("program", function(program) {
            var programReqs = (_ref1 = program.get("programReqs")) != null ? _ref1 : {};
            var programReq = programReqs[id];
            
            if (programReq == null) {
               return;
            }
            var uses = program.get("uses");
            var use = uses[id];
            changed = false;
            if (use.name != useName) {
               var takenNames = _.pluck(uses, "name");
               if (isValidUsageName(useName, takenNames)) {
                  use.name = useName;
                  changed = true;
               }
               else {
                  program.touch("uses"); // force the grid to refresh
               }
            } else if (programReq.requiredArea != goalArea) {
               programReq.requiredArea = goalArea;
               changed = true;
            } else if (programReq.quantity != goalCount) {
               programReq.quantity = goalCount;
               changed = true;
            }
            if (changed) {
               program.touch("programReqs");
               program.touch("uses");
               window.endTransaction();
            }
         });
      });

      usageGrid.onSort.subscribe(function(e, args) {
         sortProgramDataByField(usageData, args.sortCol.field, args.sortAsc);
         gridChanged(usageGrid);
      });

      if (forSpecificDesign) {
         var isolateUseTypeFilterId = null;
         usageGrid.onMouseEnter.subscribe(function(e, args) {
            if (isolateUseTypeFilterId) {
               removeAppearanceFilter(isolateUseTypeFilterId);
               isolateUseTypeFilterId = null;
            }

            var cell = args.grid.getCellFromEvent(e);
            if (cell.cell != 0)
               return;
            var item = usageData[cell.row];
            if (item)
               isolateUseTypeFilterId = addWireframeFilter(function(elem) { return elem.get("type") == "Space" && elem.get("usageName") == item.use });
         });

         usageGrid.onMouseLeave.subscribe(function(e, args) {
            if (isolateUseTypeFilterId) {
               removeAppearanceFilter(isolateUseTypeFilterId);
               isolateUseTypeFilterId = null;
            }
         });
      }

      usageGrid.onBeforeEditCell.subscribe(function(e,args) {
         if ((args.column.id == "goalArea" || args.column.id == "goalCount") && args.item != null && args.item.internalUsage)
            return false;
         else
            return true;
      });

      // See comment for blur handler in $(document).ready below
      var destroyingCellCallback = function() {
         destroyingSlickGridCell = true;
         setTimeout(function() { destroyingSlickGridCell = false}, 0);
      };
      usageGrid.onBeforeCellEditorDestroy.subscribe(destroyingCellCallback);
   };

   var populateProximityDiv = function(div, forSpecificDesign) {
      var proximityColumns = [
      {
         id: "use",
         name: "Use",
         field: "use",
         resizable: false,
         width: 80
      }];

      var proximityOptions = {
         editable: true,
         autoEdit: true,
         enableCellNavigation: true,
         asyncEditorLoading: false,
         enableColumnReorder: false,
         enableAddRow: false,
         autoHeight: true
      };

      var proximityData = [];
      var proximityGrid = new Slick.Grid(div, proximityData, proximityColumns, proximityOptions);
      proximityGrid.registerPlugin(new Slick.AutoTooltips());

      var programGridWatchSelector = "program";
      if (forSpecificDesign)
         programGridWatchSelector += ", .SpaceAssignmentResults";

      Groundhog.watchAll(programGridWatchSelector, function(elements) {
         var program = elements.program;
         if (!program)
            return;

         var uses = program.get("uses");
         var programReqs = program.get("programReqs");
         var proximityReqs = program.get("proximityReqs");

         proximityData = [];
         proximityColumns = [{ id: "use", name: "Use", field: "use", resizable: false, width: 80 }];

         var id, use, proximityRow, proximityReq, proximityColumn;
         for (id in programReqs) {
            use = uses[id];

            //set data for a column of the proximity grid
            proximityColumn = {};
            proximityColumn.field = id;
            proximityColumn.id = id;
            proximityColumn.name = use.name;
            proximityColumn.resizable = false;
            proximityColumn.width = 80;
            proximityColumn.toolTip = use.name;
            proximityColumn.formatter = proximityFormatter;
            proximityColumn.editor = Slick.Editors.Text;
            proximityColumns.push(proximityColumn);

            //set data for a row of the proximity grid
            proximityRow = {};
            proximityRow.use = use.name;
            proximityRow.id = id;

            proximityReq = proximityReqs[id];

            if (proximityReq!=null) {
               for (key in proximityReq.distanceTo) {
                  proximityRow[key] = proximityReq.distanceTo[key].toString();
               }
            }
            proximityData.push(proximityRow);
         }

         proximityGrid.setData(proximityData);
         proximityGrid.setColumns(proximityColumns);
         gridChanged(proximityGrid);
      });

      Groundhog.watchUnload("program", function(program) {
         proximityData = [];
         proximityColumns = [{ id: "use", name: "Use", field: "use", resizable: false, width: 80 }];
         proximityGrid.setData(proximityData);
         proximityGrid.setColumns(proximityColumns);
         gridChanged(proximityGrid);
      });

      proximityGrid.onCellChange.subscribe(function(e, args) {
         var currentColumnId = proximityGrid.getColumns()[args.cell].id;
         var distance = args.item[currentColumnId];
         if (!isNaN(parseFloat(distance))) {
            addNewProximityRequirement(args.item.id, currentColumnId, parseFloat(distance));
         } else {
            removeProximityRequirement(args.item.id, currentColumnId);
         }
         window.endTransaction();
      });

      proximityGrid.onBeforeEditCell.subscribe(function(e,args) {
         if (args.cell == args.row+1) {
            return false;
         }
         return true;
      });

      // See comment for blur handler in $(document).ready below
      var destroyingCellCallback = function() {
         destroyingSlickGridCell = true;
         setTimeout(function() { destroyingSlickGridCell = false}, 0);
      };

      proximityGrid.onBeforeCellEditorDestroy.subscribe(destroyingCellCallback);
   };

   var updateUsageRequirement = function(id, newUseName, newGoalArea, newGoalCount) {
      Groundhog.get("program", function(program) {
         var programReqs = (_ref1 = program.get("programReqs")) != null ? _ref1 : {};
         var programReq = programReqs[id];
         
         if (programReq == null) {
            return;
         }
         var uses = program.get("uses");
         var use = uses[id];
         changed = false;
         if (use.name != newUseName) {
            var takenNames = _.pluck(uses, "name");
            if (isValidUsageName(newUseName, takenNames)) {
               use.name = newUseName;
               changed = true;
            }
            else {
               program.touch("uses"); // force the grid to refresh
            }
         } else if (programReq.requiredArea != newGoalArea) {
            programReq.requiredArea = newGoalArea;
            changed = true;
         } else if (programReq.quantity != newGoalCount) {
            programReq.quantity = newGoalCount;
            changed = true;
         }
         if (changed) {
            program.touch("programReqs");
            program.touch("uses");
            window.endTransaction();
         }
      });
   };

   var lastRequirementsEditorTabIndex = 0; // jQuery's UI tabs need unique element IDs
   var destroyingSlickGridCell = false; // See comment for blur handler in $(document).ready below
   window.createRequirementsEditor = function(parentContainer, forSpecificDesign) {
      var requirementsEditor = $("<div/>", {"class": "requirementsEditor"});
      parentContainer.append(requirementsEditor);

      lastRequirementsEditorTabIndex++;
      var usageTabId = "usageTab" + lastRequirementsEditorTabIndex;
      var proximityTabId = "proximityTab" + lastRequirementsEditorTabIndex;
      var bubbleDiagramTabId = "bubbleDiagramTab" + lastRequirementsEditorTabIndex;
      var siteTabId = "siteTab" + lastRequirementsEditorTabIndex;
      var coreTabId = "coreTab" + lastRequirementsEditorTabIndex;
      var customTabId = "customTab" + lastRequirementsEditorTabIndex;

      var tabList = $("<ul/>");
      var usageWidth, proximityWidth, customWidth;
      if (forSpecificDesign) {
         usageWidth = 500;
         proximityWidth = customWidth;
         customWidth = usageWidth;
      }
      else {
         usageWidth = 340;
         proximityWidth = 780;
         customWidth = 400;
      }

      var usageSelectorDiv = $("<div/>", {id: usageTabId, style: "width:" + usageWidth + "px"});
      var proximitySelectorDiv = $("<div/>", {id: proximityTabId, style: "width:" + proximityWidth + "px"});
      var bubbleDiagramDiv = $("<div/>", {id: bubbleDiagramTabId, style: "width:" + proximityWidth + "px"});
      var siteDiv = $("<div/>", {id: siteTabId, "class": "requirementsEditorSite", style: "width:" + usageWidth + "px"});
      var coreDiv = $("<div/>", {id: coreTabId, "class": "requirementsEditorSite", style: "width:" + usageWidth + "px"});
      var customDiv = $("<div/>", {id: customTabId, "class": "requirementsEditorCustom", style: "width:" + customWidth + "px"});

      tabList.append(
         $("<li>", {}).append(
            $("<a>", {href: "#" + usageTabId}).text("usage")
            )
       );

      tabList.append(
         $("<li>", {}).append(
            $("<a>", {href: "#" + proximityTabId}).text("layout")
            )
       );

      tabList.append(
         $("<li>", {}).append(
            $("<a>", {href: "#" + bubbleDiagramTabId}).text("bubbles")
            )
       );

      tabList.append(
         $("<li>", {}).append(
            $("<a>", {href: "#" + siteTabId}).text("site/shell")
            )
       );

      tabList.append(
         $("<li>", {}).append(
            $("<a>", {href: "#" + coreTabId}).text("core")
            )
       );

      tabList.append(
         $("<li>", {}).append(
            $("<a>", {href: "#" + customTabId}).text("custom")
            )
       );

      requirementsEditor.append(tabList);
      requirementsEditor.append(usageSelectorDiv);
      requirementsEditor.append(proximitySelectorDiv);
      requirementsEditor.append(bubbleDiagramDiv);
      requirementsEditor.append(siteDiv);
      requirementsEditor.append(coreDiv);
      requirementsEditor.append(customDiv);

      populateUsageDiv(usageSelectorDiv, forSpecificDesign);
      populateProximityDiv(proximitySelectorDiv, forSpecificDesign);
      populateBubbleDiv(bubbleDiagramDiv, forSpecificDesign);
      populateSiteDiv(siteDiv, forSpecificDesign);
      populateCoreDiv(coreDiv, forSpecificDesign);
      populateCustomDiv(customDiv);

   /*
      if (forSpecificDesign)
         requirementsEditor.tabs();
      else
         requirementsEditor.tabs({activate: function(event, ui) {
            if (ui.newPanel.attr('id') == proximityTabId)
               requirementsEditor.css("max-width", proximityWidth + 20);
            else if (ui.newPanel.attr('id') == customTabId)
               requirementsEditor.css("max-width", customWidth + 20);
            else
               requirementsEditor.css("max-width", usageWidth + 20);
         }});
   */
      
      requirementsEditor.tabs();
      return requirementsEditor;
   };

   $(document).ready(function() {
      var requirementsEditor = createRequirementsEditor($("body"), true);
      requirementsEditor.attr("id", "requirementsEditor");

      // Clicking out of the text editors should commit the edit (SlickGrid doesn't support this internally)
      // This raises exceptions if the blur was caused by removal of the editor (because the user hit enter or escape),
      //    so we use destroyingSlickGridCell to determine whether the cell is being actively destroyed.
      // StackOverflow suggested putting a timeout in the blur handler to avoid the exception, but that would unfocus the
      //    just-created editor in the case where the user hit enter.
      $("body").on("blur", "input.editor-text", function() {
         if (destroyingSlickGridCell)
            return;
         if (Slick.GlobalEditorLock.isActive())
            Slick.GlobalEditorLock.commitCurrentEdit();
      });
   });

   var removeProximityRequirement = function(thisUseId, otherUseId) {
      Groundhog.getAll("program", function(programs) {
         var program = programs["program"];
         var proximityReqs = program.get("proximityReqs");
         var proximityReq = proximityReqs[thisUseId];

         if (proximityReq != null && proximityReq.distanceTo[otherUseId]!=null) {
            delete proximityReq.distanceTo[otherUseId];
            if (Object.keys(proximityReq.distanceTo).length == 0)
               delete proximityReqs[thisUseId];
         }

         program.touch("proximityReqs");
      });
   };

   var addNewProximityRequirement = function(thisUseId, otherUseId, distance) {
      Groundhog.getAll("program", function(programs) {
         var program = programs["program"];
         var proximityReqs = program.get("proximityReqs");
         var proximityReq = proximityReqs[thisUseId];

         if (proximityReq == null) {
            proximityReqs[thisUseId] = {};
            proximityReqs[thisUseId].distanceTo = {};
         }
         proximityReqs[thisUseId].distanceTo[otherUseId] = distance;

         program.touch("proximityReqs");
      });
   };

   var addNewProgramRequirement = function(useName, goalArea, goalCount) {
      Groundhog.getAll("program", function(programs) {
         var program = programs["program"];

         if (program == null) {
            var programData = new ProgramData("program");
            window.preAddElementForTransaction(programData.uniqueId);
            program = Groundhog.addElement(programData, {global: true});
            program.data.uses = {};
            program.data.programReqs = {};
            program.data.proximityReqs = {};
         }

         var newId = getNewId();
         var newColor = getNextColorIndex(program.data);
         var programReqs = program.get("programReqs");
         var uses = program.get("uses");

         // Name the use if the user didn't (generally by typing an area/quantity before the name)
         var takenNames = _.pluck(uses, "name");
         if (!isValidUsageName(useName, takenNames)) {
            var root = useName;
            if (root == null || root.length == "")
               root = "Unnamed";

            var candidateName;
            var candidateNumber = 2;
            do {
               candidateName = root + " " + candidateNumber;
               candidateNumber++;
            } while (!isValidUsageName(candidateName, takenNames))
            useName = candidateName;
         }

         program.data.programReqs[newId] = {
            requiredArea: goalArea,
            quantity: goalCount
         };
         program.data.uses[newId] = {
            name: useName,
            colorIndex: newColor
         };
         program.touch("programReqs");
         program.touch("uses");
      });
   };

   var gridChanged = function(grid) {
      grid.invalidate();
      grid.render();
   };

   var sortProgramDataByField = function(data, field, ascending) {
      return data.sort(function(a, b) {
         var result, valA, valB;
         if (a.id === 0) {
            return 1;
         } else if (b.id === 0) {
            return -1;
         }
         result = 0;
         valA = a[field];
         valB = b[field];
         if (field === "assigned") {
            if (a.goalArea > 0) {
               valA = a.assigned / a.goalArea;
            }
            if (b.goalArea > 0) {
               valB = b.assigned / b.goalArea;
            }
         }
         if (valA > valB) {
            result = 1;
         } else if (valA < valB) {
            result = -1;
         }
         if (ascending) {
            return result;
         } else {
            return -result;
         }
      });
   };

   var sceneColorToCSS = function(sceneColor) {
      var str;
      str = "";
      str += Math.floor(sceneColor.r * 255) + ",";
      str += Math.floor(sceneColor.g * 255) + ",";
      str += Math.floor(sceneColor.b * 255);
      return str;
   };

   window.getNextColorIndex = function(programData) {
      var takenColorIndices = _.pluck(programData.uses, "colorIndex");
      var candidate = 1;
      while (_.include(takenColorIndices, candidate))
         candidate++;
      return candidate;
   };

   window.deleteProgramUsage = function(usageId) {
      return Groundhog.get("program", function(program) {
         var answer, programReqs, proximityReqs, programReq, uses;

         programReqs = program.get("programReqs");
         proximityReqs = program.get("proximityReqs");
         if (programReqs != null) {
            programReq = programReqs[usageId];
            if (programReq == null) {
               return;
            }
            uses = program.get("uses");
            answer = confirm("Really delete the \"" + uses[usageId].name + "\" requirement?");
            if (!answer) {
               return;
            }
            delete programReqs[usageId];    
            delete proximityReqs[usageId]; 
            for (id in proximityReqs) {
               var proximityReq = proximityReqs[id];
               if (proximityReq!=null) {
                  for (key in proximityReq.distanceTo) {
                     if (key===usageId) {
                        delete proximityReq.distanceTo[key];
                     }
                  }
               }
            }
            delete uses[usageId];
            program.touch("programReqs");
            program.touch("proximityReqs");
            program.touch("uses");
         }
      });
   };

   function roundToReasonableSpaceDimension(number) {
      //return Math.ceil(number * 2) / 2;
      return Math.ceil(number);
   };

   window.prepareSpaceStandardsForGenerator = function(callback) {
      var dimensionPool = [5, 10, 10];
      var spaceStandards = [
      {data: new HallData("", [-8, 0, 0], [8, 0, 0], 0)},
      {data: new HallData("", [-8, 0, 0], [8, 0, 0], Math.PI/2)}
      ];

      Groundhog.getAll("program", function(programs) {
         var program = programs.program;
         if (program == null) {
            callback(spaceStandards);
            return;
         }

         var programReqs = program.get("programReqs");
         var uses = program.get("uses");

         var id, programReq, use, index, length, width, spaceStandard;
         //Add requirements from the usage editor
         for (id in programReqs) {
            programReq = programReqs[id];
            use = uses[id];

            index = Math.floor(dimensionPool.length * Math.random());
            length =   dimensionPool[index];
            //assign width according to golden ratio
            width = Math.round(length/1.618);

            var count;
            if (programReq.quantity > 0)
               count = programReq.quantity;
               if (length * width * count < programReq.requiredArea) {
                  // Make sure the dimensions are large enough to satisfy the required area with the requested quantity
                  var d = Math.sqrt(programReq.requiredArea / count * (1 / 1.618));
                  length = roundToReasonableSpaceDimension(d * 1.618)
                  width = roundToReasonableSpaceDimension(d);
                  programReq.spaceStandards = { 'default': [length, width, 3.5] };
               }
            else if (programReq.requiredArea > 0)
               count = Math.floor(programReq.requiredArea / (length * width)) + 1;
            else
               continue; // Don't place a space that doesn't satisfy a program goal

            spaceStandard = {
               data: createSpaceType(use.name, [width, length, 3.5]),
               count: count
            }

            spaceStandards.push(spaceStandard);
         }
         callback(spaceStandards);
      });
   };

   window.getRequirements = function(callback, addDesignData) {

      var requirements = {
         code: {
            minimumDoorWidth: 0.5,
            maximumEgressDistance: 300
         },
         spaces: [
         { usage: "hallway", circulation: true },
         { usage: "stairwell", circulation: true }
         ],
         scripts: {},
         adjacencies: [],
         existingDesigns: []
      };

      if (addDesignData && Groundhog.optionId) {
         var existingDesign = { 
            spaces: [],
            meshes: [] 
         };

         Groundhog.getAll(".Space, .Hall, .Stairs", function (elems) {
            var layout = _.values(elems);

            _.each(layout, function(elem) {
               var spaceEntry = {};
               //we do not include starcases for now
               if (elem.data.type == "Hall") {
                  var length = Math.abs(elem.data.position1[0]-elem.data.position2[0]) + Hall.defaultWidth();
                  var width = elem.data.width;
                  if (elem.data.rotation == 0) 
                     spaceEntry["dimensions"] = [length, width, 3.5];
                  else 
                     spaceEntry["dimensions"] = [width, length, 3.5];

                  spaceEntry["position"] = [elem.data.position1[0]+ length/2 - Hall.defaultWidth()/2,
                           elem.data.position1[1], 
                           elem.data.position1[2]];
                  spaceEntry["usageName"] = "Hall";
               }
               if ((elem.data.type == "Space")) {
                  spaceEntry["dimensions"] = elem.data.dimensions;
                  spaceEntry["position"] = elem.data.position;
                  spaceEntry["usageName"] = elem.data.usageName;
               }
               if (!jQuery.isEmptyObject(spaceEntry))
                  existingDesign.spaces.push(spaceEntry);
            });
         });

         Groundhog.getAll(".Mesh", function (elems) {
            var meshes = _.values(elems);

            _.each(meshes, function(elem) {
               var meshEntry = {};
               meshEntry["name"] = elem.data.name;

               if (elem.data.url !== undefined)
                  meshEntry["url"] = elem.data.url;

               if (elem.data.inlineData !== undefined)
                  meshEntry["inlineData"] = elem.data.inlineData;
   
               meshEntry["position"] = elem.data.position;
               meshEntry["dimensions"] = elem.data.dimensions;
               meshEntry["rotation"] = elem.data.rotation;

               if (!jQuery.isEmptyObject(meshEntry))
                  existingDesign.meshes.push(meshEntry);
            });
         });

         requirements.existingDesigns.push(existingDesign);
      }
   
      Groundhog.getAll("program, targetSiteDimensions, buildingStandards, mesh", function(elems) {
         var program =   elems["program"];
         var totalProgramArea = 0;
         
         if (program!=null) {
            var uses = program.get("uses");
            var programReqs = program.get("programReqs");
            var proximityReqs = program.get("proximityReqs");

            for (var id in programReqs) {
               var useName = uses[id].name;

               requirements.spaces.push ({
                  usage: useName,
                  minimumArea: programReqs[id].requiredArea,
                  minimumCount: programReqs[id].quantity
               });
               totalProgramArea += programReqs[id].requiredArea;

               // Add specific egress requirement for this useName
               requirements.adjacencies.push ({
                     from: useName,
                     to: "egress",
                     maxDistance: requirements.code.maximumEgressDistance
               });

               if (proximityReqs[id]!=null) {
                  var idToDistanceMap = proximityReqs[id].distanceTo;

                  for (var toId in idToDistanceMap) {
                     var use = uses[toId];
                     if (use == null)
                        continue;

                     var toUseName = use.name;
                     requirements.adjacencies.push ({
                        from: useName,
                        to: toUseName,
                        maxDistance: idToDistanceMap[toId]
                     });
                  }
               }
            }
            
            //Total usable area = program area + circulation area
            //To compute the circulation are we use the recommended 
            //Circulation Multiplier for workplace environments
            var totalUsableArea = totalProgramArea * 1.5;
            requirements.totalUsableArea = totalUsableArea;
         }

         var siteDimensions = elems["targetSiteDimensions"];
         if (siteDimensions != null) {
            var width = siteDimensions.get("width");
            var height = siteDimensions.get("height");
            requirements.site = {
               width: width,
               height: height
            }
         }

         var buildingStandards = elems["buildingStandards"];
         if (buildingStandards != null) {
            var coreStandards = buildingStandards.get("coreReqs");
            requirements.core = {
               areaPerElevator: coreStandards.areaPerElevator,
               coreDistance: coreStandards.coreDistance,
               elevatorsNumMin: coreStandards.elevatorsNumMin,
               elevatorsNumMax: coreStandards.elevatorsNumMax
            }
         }

         var shellData = elems["mesh"];
         if (shellData != null) {
            requirements.shells = [];

            var shell = {
               name: shellData.data.name,
               inlineData: shellData.data.inlineData,
               position: shellData.data.position,
               dimensions: shellData.data.dimensions,
               rotation: shellData.data.rotation
            }
            requirements.shells.push(shell)
         }

         // We couldn't include this in the top fetch because of a framework bug in ModelView.idsAreCached - fix soon
         Groundhog.getAll(".Script", function(scripts) {
            _.each(scripts, function(elem, id) {
               requirements.scripts[id] = {
                  name: elem.get("name"),
                  code: elem.get("code")
               }
            });

            callback(requirements);
         });
      });
   };

   return { launcher: function () { $("#requirementsEditor").toggle('slide', { direction: "right" }); }};
});
