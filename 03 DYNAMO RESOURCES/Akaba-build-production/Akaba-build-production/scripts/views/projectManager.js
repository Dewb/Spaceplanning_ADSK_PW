

var applicationState = "projectManager";
window.setApplicationState = function(state) {
   var animSpeed = 400;
   if (state == "projectManager") {
      $("#projectManager").show('slide', { direction: "left" }, animSpeed);

      if (applicationState == "wholeProject")
         $("#designOptionsViewer").hide('slide', { direction: "right" }, animSpeed);
   }
   else if (state == "wholeProject") {
      if (applicationState == "projectManager") {
         $("#projectManager").hide('slide', { direction: "left" }, animSpeed);
         $("#designOptionsViewer").show('slide', { direction: "right" }, animSpeed);
      }
      else if (applicationState == "optionEditor") {
         $("#designOptionsViewer").show('slide', { direction: "left" }, animSpeed);
      }
   }
   else if (state == "optionEditor") {
      $("#container").show();

      if (applicationState == "wholeProject")
         $("#designOptionsViewer").hide('slide', { direction: "left" }, animSpeed);

      else if (applicationState == "projectManager")
         $("#projectManager").hide('slide', { direction: "left" }, animSpeed);
   }
   else
      throw new Error("Unknown state " + state + " passed to setApplicationState");

   applicationState = state;
   $("body").trigger("applicationStateChanged", state);
}

define(["views/requirementsEditor"], function() {
   var sortDropdown, filterDropdown, sortTypeLabel, filterTypeLabel, currentDesignOptions, optionsManagerContent;
   var minScore = 0;
   var maxScore = 1;
   var currentSortOption = "by overall score";
   var currentFilterOption = "all designs";
   var idToDisplayState = {};
   var deleteList = [];

   var filterOptions = {
      "all designs": null, 
      "Random Domino": 'RD', 
      "Cellular Automata": 'CA', 
      "L-System Grammar": 'LS',
      "Shell Stuffer": 'SS'
      //,
      //"Exe Bridge": 'EB',
      //"Facilitator": 'FC'
   };

   var updatePageTitle = function() {
      var projectName = Groundhog.getProjectName();
      var optionName = Groundhog.getDesignOptionName();

      var pageTitle = "";
      if (_.isString(projectName) && projectName.length > 0)
         pageTitle += projectName;
      if (_.isString(optionName) && optionName.length > 0)
         pageTitle += ": " + optionName;
      
      $(document).attr("title", pageTitle);
   };

   var getNewProjectName = function() {
      var newProjectName = "New Project " + (new Date()).toLocaleString("en-US", {hour12: false});
      newProjectName = newProjectName.replace(/\//g, "-").replace(/:/g, ".");
      return newProjectName;
   };

   var getQueryVariable = function(variable) {
      var query = window.location.search.substring(1);
      var vars = query.split('&');
      for (var i = 0; i < vars.length; i++) {
         var pair = vars[i].split('=');
         if (decodeURIComponent(pair[0]) == variable) {
            return decodeURIComponent(pair[1]);
         }
      }
      return null;
   };

   var projectSelectionFromWindowLocation = function() {
      var result = {};
      result.projectId = getQueryVariable("project");

      if (!result.projectId)
         return null;

      var optionId = getQueryVariable("option");
      if (optionId)
         result.optionId = optionId;

      return result;
   };

   var getStateColor = function(score) {
      if (score == null)
         return '#B3B3A7';

      //map red->yellow color to bigger->smaller score value
      if (score > maxScore)
         return "rgb(255,0,0)"; //red
      else if (almostEqual(score, minScore))
         return "rgb(50,205,50)"; //green
      else
         var remapped = Math.ceil((score-minScore)*255 /(maxScore-minScore));
         var greenValue = 255-remapped;
         return "rgb(255,"+ greenValue +",0)";
   };

   var createScorePanel = function(scores, genType) {
      var scorePanel = $("<div/>", {"class": "scorePanel"});

      scorePanel.append($("<div/>", {"class": "generatorIcon", text: genType}));

      if (scores != null) {
         scorePanel.append($("<div/>", {"class": "stateIcon", title: "program satisfaction", style: "background-color:" + getStateColor(scores.program)}));
         scorePanel.append($("<div/>", {"class": "stateIcon", title: "layout satisfaction", style: "background-color:"  + getStateColor(scores.layout)}));
         scorePanel.append($("<div/>", {"class": "stateIcon", title: "site fit", style: "background-color:" + getStateColor(scores.siteFit)}));
         scorePanel.append($("<div/>", {"class": "stateIcon", title: "minimization of circulation", style: "background-color:"  + getStateColor(scores.billableSpace)}));
         
         var averageCustomScore; // Leave undefined if there are no scores
         if (_.isObject(scores.customScores) && !_.isEmpty(scores.customScores))
            averageCustomScore = _.reduce(scores.customScores, function(m, score){ return m + score; }, 0) / _.size(scores.customScores);
         scorePanel.append($("<div/>", {"class": "stateIcon", title: "custom analysis", style: "background-color:" + getStateColor(averageCustomScore)}));
      } 
      return scorePanel;
   };

   var limitCheckBoxSelection = function(thisCheckbox, limit) {
      var checkedCount = $("[type='checkbox']:checked").length;
      if (checkedCount>limit) {
         alert("You can only select a maximum of "+limit+" checkboxes");
         thisCheckbox.checked=false;
      }
   };

   var createComparisonRadarChart = function() {
      var colorscale = d3.scale.category10();
      var radarChartContainer =  $("<div/>", {"class": "comparisonRadarChart", "name": name});
      
      var checkedThumbnailNames = [];
      $.each($("[type='checkbox']:checked"), function (index, checkedOption) {
         var thumbnailContainer = checkedOption.parentNode.parentNode;
         var designOptionName = $(thumbnailContainer.childNodes).find('.thumbnailLink').text();
         checkedThumbnailNames.push(designOptionName);
      });

      var data = [];
      var count = 0;
      var chartClassToOption = {};
      $.each(currentDesignOptions, function (id, designOption) {
         if ($.inArray(designOption.name, checkedThumbnailNames)>-1) {
            data.push({className: 'radarChartOption_' + count, axes: []});
            chartClassToOption['radarChartOption_' + count] = [id, designOption.name];
            var scores = designOption.metadata.scorePerProgramReq;
            $.each(scores,  function(name, score) {
               var dataEntry = {axis: name, value: (1-score).toFixed(3)};
               data[count].axes.push(dataEntry);
            });
            count++;
         }
      });

      var radarChart = RadarChart.chart();
      radarChart.config({w: 380, h:380, axisText: true, levels: 3, circles: true,  color: colorscale});
      var config = radarChart.config(); 
      
      var radarChartSvg = d3.select(radarChartContainer.get(0)).append('svg')
         .attr('width', config.w + 300)
         .attr('height', config.h)
         .attr('class', 'comparisonRadarChart');

      radarChartSvg.append('g').classed('single', 1).datum(data).call(radarChart);

      var polygons = radarChartSvg.selectAll("polygon");

      //legend container
      var legend = radarChartSvg.append("g")
         .attr("class", "legend")
         .attr("height", 100)
         .attr("width", 400)
         .attr('transform', 'translate(90,20)');
      //Legend squares
      legend.selectAll('rect')
        .data(Object.keys(chartClassToOption))
        .enter()
        .append("rect")
        .attr("x", 350)
        .attr("y", function(d, i) { return i * 20;})
        .attr("width", 20)
        .attr("height", 10)
        .style("fill", function(d, i){ return colorscale(i);});

      //Text next to squares
      legend.selectAll('text')
        .data(Object.keys(chartClassToOption))
        .enter()
        .append("text")
        .attr("x", 380)
        .attr("y", function(d, i){ return i * 20 + 10;})
        .attr("font-size", "14px")
        .text(function(d) { return chartClassToOption[d][1] + " >"; })
        .on("click", function(d) { selectProject(Groundhog.projectId, chartClassToOption[d][0]); })
        .attr("class", "link");;

      return radarChartContainer;
   };

   var createThumbnailRadarChart = function(name, scores) {
      var radarChartContainer =  $("<div/>", {"class": "radarChartContainer", "name": name});

      if (scores != null) {
         // Display a subset of the analysis results
         var scoresForDisplay = {};
         scoresForDisplay.program = scores.program;
         scoresForDisplay.layout = scores.layout;
         scoresForDisplay.billableSpace = scores.billableSpace;
         scoresForDisplay.siteFit = scores.siteFit;

         scoresForDisplay.customScore = maxScore;
         if (_.isObject(scores.customScores) && !_.isEmpty(scores.customScores))
            scoresForDisplay.customScore = _.reduce(scores.customScores, function(m, score){ return m + score; }, 0) / _.size(scores.customScores)

         // Format scores
         _.each(scoresForDisplay, function(score, key) {
            if (score == null)
               scoresForDisplay[key] = maxScore;
            else
               scoresForDisplay[key] = (1-trimScore(score, minScore, maxScore)).toFixed(3);
         });

         var data = [{ 
            className: 'defaultRadarChart', 
            axes: [
            {axis: "custom analysis", value: scoresForDisplay.customScore}, 
            {axis: "minimal circulation", value: scoresForDisplay.billableSpace}, 
            {axis: "site fit", value: scoresForDisplay.siteFit}, 
            {axis: "program", value: scoresForDisplay.program}, 
            {axis: "layout", value: scoresForDisplay.layout}]
         }];


         var radarChart = RadarChart.chart();
         radarChart.config({w: 170, h: 170, axisText: true, levels: 3, circles: true, radius:2, maxValue: 1.0});
         var config = radarChart.config(); 
         
         var radarChartSvg = d3.select(radarChartContainer.get(0)).append('svg')
         .attr('width', config.w)
         .attr('height', config.h)
         .attr('class', 'thumbnailRadarChart');

         radarChartSvg.append('g').classed('single', 1).datum(data).call(radarChart);

         var optionCheckBox = $("<input/>", {"class": "optionSelector", type: "checkbox"});

         optionCheckBox.click(function(event) {
            if (this.checked)
               limitCheckBoxSelection(this, 2);
            updateComparisonChart();
            event.stopPropagation();
         });

         radarChartContainer.append(optionCheckBox);
      }

      return radarChartContainer;
   };

   var determineRadarChartDisplay = function(thumbnailId, thumbnailChart) {
      if (idToDisplayState[thumbnailId]) {
         thumbnailChart.css("display", idToDisplayState[thumbnailId]);
      } else {
         var state = (flipped) ? "block" : "none";
         thumbnailChart.css("display", state);
         idToDisplayState[thumbnailId] = state; 
      }
   };

   var createThumbnailCell = function(id, name, image, clickAction, deleteAction, scores, genType) {
      var optionLinkCell = $("<div/>", {"class": "thumbnail"});

      if (image != null)
         optionLinkCell.append($("<img/>", {"src": image}));

      if (typeof(scores) !== "undefined") {
         optionLinkCell.append(createScorePanel(scores, genType));
         var radarChart = createThumbnailRadarChart(name, scores);
         determineRadarChartDisplay(id, radarChart);         
         optionLinkCell.append(radarChart);
      }

      var optionCaption = $("<span/>", {"class": "thumbnailCaption"});
      optionLinkCell.append(optionCaption);

      var optionLink = $("<span/>", {text: name, "class": "thumbnailLink"});
      optionLinkCell.click(function(event) { clickAction(event, $(this)); return false; });
      optionCaption.append(optionLink);

      if (typeof(scores) !== "undefined") {
         var flipIcon = $("<span/>", {text: "~", 
            class: "link thumbnailFlip", title: "Flip thumbnail", style: "display:none"});
         optionLinkCell.hover( function() { flipIcon.show() }, function() { flipIcon.hide() });
         flipIcon.click(function() {
            var thumbnailCell = $(this).parent();
            var chartContainer = thumbnailCell.children(".radarChartContainer");
            chartContainer.toggle();
            idToDisplayState[id] = $(chartContainer).css("display");
            return false;
         });
         optionLinkCell.append(flipIcon);
      }

      if (deleteAction != null) {
         var deleteIcon = $("<span/>", {text: "×", class: "link thumbnailDelete", title: "Delete design", style: "display:none"});
         optionLinkCell.hover( function() { deleteIcon.show() }, function() { deleteIcon.hide() });
         deleteIcon.click(function() {
            if (confirm("Really delete \"" + _.escape(name) + "?\""))
               deleteAction();
            return false;
         });
         optionLinkCell.append(deleteIcon);
      }

      return optionLinkCell;
   };

   var lastDisplayedScores = {};
   var lastDisplayedNames = {};

   var updateThumbnail = function(optionId, thumbnailListItem, name, image, scores, genType) {
      var thumbnail = $(thumbnailListItem).children(".thumbnail"); //the thumbnail div
      var currentImage = $(thumbnail).children(".thumbnail img");
      var nameCell = $(thumbnail).find(".thumbnailLink");

      if (name != null && nameCell.text() != name)
         nameCell.text(name);

      if (image != null) {
         if (currentImage.length != 0) {
            if (currentImage.attr("src") != image)
               currentImage.attr("src", image);
         }
         else
            thumbnail.append($("<img/>", {"src": image}));
      }

      if (scores != null && !_.isEqual(lastDisplayedScores[optionId], scores)) {
         lastDisplayedScores[optionId] = scores;
         var scorePanel = $(thumbnail).children(".scorePanel");
         var radarChartContainer = $(thumbnail).children(".radarChartContainer");

         var scoresToAdd = createScorePanel(scores, genType);
         var chartToAdd = createThumbnailRadarChart($(thumbnailListItem).data("name"), scores);
         determineRadarChartDisplay(optionId, chartToAdd);

         if (scorePanel.length != 0) 
            scorePanel.replaceWith(scoresToAdd);
         else
            thumbnail.append(scoresToAdd);

         if (radarChartContainer.length != 0)
            radarChartContainer.replaceWith(chartToAdd);
         else
            thumbnail.append(chartToAdd);
      }
   };

   var createScoresObject = function(metadata) {
      var scores = {};
      if (metadata.analysisResults != null)
         _.extend(scores, metadata.analysisResults);
      scores.program = metadata.programScore;
      scores.layout = metadata.proximityScore;
      return scores;
   };

   var createNewDesignThumbnail = function() {
      var listItem = $("<li/>", {"data-id": "newDesign"});
      listItem.data("name", "newDesign");
      var newDesignThumbnail = createThumbnailCell(null, "New design", "images/thickPlus-white.svg", function() { 
         var name = prompt("New design name:", "Design");
         if (_.isString(name) && name.length > 0)
            Groundhog.createDesignOption(name, function(optId, projectId) {selectProject(projectId, optId)});
      });
      newDesignThumbnail.addClass("newButton");
      listItem.append(newDesignThumbnail);
      return listItem;
   };

   var updateOptionsManagerContent = function(designOptions, thumbnailList) {
      if (!_.isObject(designOptions))
         return;

      var listItems = $(".thumbnailList li").get(); //the ul contains at least one li - the new design thumbnail
      var completedListItems = $(".thumbnailList li").filter(".completed");

      //CASE 1: a design option has just been deleted - thumbnail list needs to be updated
      if (completedListItems.length > Object.keys(designOptions).length) {
         $.each(listItems, function(index, li) {
            var dataId = $(li).attr("data-id");
            if(!(dataId in designOptions) && dataId != "newDesign" && $(li).hasClass("completed"))
               $(li).remove();
               delete idToDisplayState[dataId];
         });  
      }
      else {
         var reSortingRequested = false;
         Groundhog.getAll(".Thumbnail", function (thumbs) {
            $.each(designOptions, function (optionId, option) {
               var contained = false;
               var listItem = null;
               var image = null;
               var scores = null;
               var generatorType;

               $.each(listItems, function(index, li) {
                  if ($(li).attr("data-id") == optionId) {
                     contained = true;
                     listItem = li;
                     return;
                  }            
               });

               _.each(thumbs, function(t) {
                  if (t.get("thumbnailDesignOptionId") == optionId) {
                     image = t.get("data");
                  }
               });

               if (option.metadata != null && option.metadata.generatorType)
                  generatorType = filterOptions[option.metadata.generatorType];
               else
                  generatorType = 'NA'

               if (option.metadata != null)
                  scores = createScoresObject(option.metadata);


               if (completedListItems.length < Object.keys(designOptions).length) {
                  //create a thumbnail div reflecting the new design option data
                  var thumbnailCell = createThumbnailCell(
                     optionId,
                     option.name, 
                     image,  
                     function(event, element) { 
                        if (event.shiftKey) {
                           event.preventDefault();
                           if (deleteList.indexOf(optionId) > -1) {
                              deleteList.splice(deleteList.indexOf(optionId), 1);
                              element.removeClass('selectedDesign');
                           }
                           else {
                              deleteList.push(optionId);
                              element.addClass('selectedDesign');
                           }
                        }
                        else
                           selectProject(Groundhog.projectId, optionId) },
                     function() { deleteDesignOption(optionId) },
                     scores,
                     generatorType);
                  lastDisplayedScores[optionId] = scores;
                  lastDisplayedNames[optionId] = option.name;

                  //DESIGN OPTION ID CONTAINED: 
                  //a .li placeholder for the new design option already exists in the thumbnail list
                  //we need to add the thumbnail cell to the .li placeholder 
                  if (contained) {
                     listItem = $("li[data-id="+ optionId +"]");
                     listItem.removeClass('pending');
                     listItem.addClass('completed');

                     var statusDiv = $(listItem).children(".statusThumbnail");
                     statusDiv.replaceWith(thumbnailCell);
                  }
                  //DESIGN OPTION ID NOT CONTAINED: 
                  //a .li placeholder for the new design option does not exist in the thumbnail list
                  //a new .li needs to be created and added to the list
                  else {
                     listItem = $("<li/>", {"data-id": optionId, "data-gen": generatorType});
                     listItem.addClass('completed');
                     listItem.append(thumbnailCell);
                     thumbnailList.append(listItem);
                  }
                  reSortingRequested = true;
               }
               else {
                  updateThumbnail(optionId, listItem, option.name, image, scores, generatorType);
                  reSortingRequested = true; 
               }
            })
            if (reSortingRequested)
               sortByAnimating();
         })
      }
   };

   var sortByAnimating = function() {
      //get the initial thumbnail list
      var thumbnailList = $(".thumbnailList");

      //get a list of the thumbnails excluding 'new design' thumbnail
      var listItems = thumbnailList.children('li:not(:first)');
      var newDesignThumbnail = thumbnailList.children('li:first');

      Groundhog.getAll(analysisPrioritiesElemId, function(prioritiesElems) {
         var priorities = prioritiesElems[analysisPrioritiesElemId];

         //sort the sublist
         var sortCallback = sortOptions[currentSortOption];
         var sortedData = _.sortBy(listItems, sortCallback, {priorities: priorities});

         //abort unless the sorting changed
         if (sortedData.length != listItems.length)
            return;
         var anyChanges = _.any(sortedData, function(newThumb, i) {
            return ($(newThumb).attr("data-id") != $(listItems[i]).attr("data-id"));
         });
         if (!anyChanges)
            return;

         //add back 'new design' thumbnail at the beginning of the thumbnail list
         sortedData = newDesignThumbnail.get().concat(sortedData);

         //reorder the thumbnails
         $(thumbnailList).quicksand(
            $(sortedData), 
            {easing: 'easeInOutQuad'},
            function() {
               $(".thumbnailList").css('width', 'auto');
            }
         );
      });
   };

   var createOptionsManagerContent = function(designOptions, sortCallback) {
      if (sortCallback==null)
         sortCallback = sortOptions[currentSortOption];

      var optionsList = $("<div/>", {"class": "thumbnailListContainer"});

      var newCell = createThumbnailCell(null, "New design", "images/thickPlus-white.svg", function() { 
         var name = prompt("New design name:", "Design");
         if (_.isString(name) && name.length > 0)
            Groundhog.createDesignOption(name, function(optId, projectId) {selectProject(projectId, optId)});
      });
      newCell.addClass("newButton");
      optionsList.append(newCell);

      Groundhog.getAll(analysisPrioritiesElemId, function(prioritiesElems) {
         if (!_.isObject(designOptions))
            return;

         var priorities = prioritiesElems[analysisPrioritiesElemId];

         var sortedOptionPairs = _.sortBy(_.pairs(designOptions), sortCallback, {priorities: priorities});
         _.each(sortedOptionPairs, function (option) {
            var image;
            var scores = {};
            if (option[1].metadata != null && _.isString(option[1].metadata.thumbnail))
               image = option[1].metadata.thumbnail;
            if (option[1].metadata != null)
               scores = createScoresObject(option[1].metadata);
            optionsList.append(
               createThumbnailCell(option[0], option[1].name, image,  
                  function() { selectProject(Groundhog.projectId, option[0]) },
                  function() { deleteDesignOption(option[0]) },
                  scores
               )
            );
            lastDisplayedScores[option[0]] = scores;
            lastDisplayedNames[option[0]] = option[1].name;
         });
      });

      return optionsList;
   };

   var showProjectCreator = function(originDiv) {
      var backsplashDiv = $("<div/>", {"class": "fullscreenBackdrop", style: "display:none"});
      $("body").append(backsplashDiv);

      var hideFunction = function() {
         backsplashDiv.fadeOut(400, function() {backsplashDiv.remove()});
      }

      var contentsDiv = $("<div/>", {"class": "projectCreator"});
      backsplashDiv.append(contentsDiv);

      var cancelButton = $("<div/>", {"class": "projectManagerHeaderLinkLeft link", text: "< Cancel"});
      cancelButton.click(hideFunction);
      contentsDiv.append(cancelButton);

      contentsDiv.append($("<div/>", {"class": "projectManagerTitle projectCreatorTitle", text: "New project"}));

      var createDiv = $("<span/>", {text: "Name:", "class": "projectManagerLabel"});
      contentsDiv.append(createDiv);

      var createForm = $("<form/>");
      var nameInput = $("<input/>", {name: "name", placeholder: "Project name"});
      nameInput.attr('autocomplete', 'off');
      createForm.append(nameInput);
      createForm.submit(false);
      contentsDiv.append(createForm);

      var programOptionsDiv = $("<div/>", {"class": "projectCreatorProgram", style: "display: none"});
      contentsDiv.append(programOptionsDiv);

      createProject = function(program) {
         var name = nameInput.val();
         if (_.isString(name) && name.length > 0) {
            contentsDiv.fadeOut(1000);
            Groundhog.createProject(name, function(projId) {
               selectProject(projId);
               createProgramFromPreset(program);
               hideFunction();
            });
         }
         nameInput.val("");
         return false;
      };

      var addProgramButton = function(imgPath, label, program) {
         var span = $("<span/>", {"class": "projectCreatorProgramButton"});
         span.append($("<img/>", {src: imgPath}));
         span.append($("<div/>", {text: label}))
         programOptionsDiv.append(span);

         span.click(function() { createProject(program); })
      };
      
      addProgramButton("images/programIcons/hospital.svg", "Hospital", window.defaultPrograms.hospital);
      addProgramButton("images/programIcons/office.svg", "Office", window.defaultPrograms.custom);
      addProgramButton("images/programIcons/wellness.svg", "Wellness Center", window.defaultPrograms.wellnessCenter);
      addProgramButton("images/programIcons/plant.svg", "Botanical Museum", window.defaultPrograms.botanicalGardenMuseum);
      addProgramButton("images/programIcons/school.svg", "High School", window.defaultPrograms.highSchool);
      addProgramButton("images/programIcons/wellness.svg", "simplified WC", window.defaultPrograms.simplifiedWellnessCenter);
      addProgramButton("images/programIcons/plant.svg", "simplified BMC", window.defaultPrograms.simplifiedBotanicalGardenMuseum);
      addProgramButton("images/programIcons/school.svg", "simplified HS", window.defaultPrograms.simplifiedHighSchool);
      addProgramButton("images/question.svg", "Other", window.defaultPrograms.userDefined);
      
      nameInput.keyup(function () {
         if ($(this).val().length > 0) {
            if (!programOptionsDiv.is(":visible"))
               programOptionsDiv.show('slide', { direction: "down" }, 200);
         }
         else
            programOptionsDiv.hide();
      });

      backsplashDiv.fadeIn();
      nameInput.focus();
   };

   var createProjectManagerContent = function(projects) {
      var projectList = $("<div/>", {"class": "thumbnailListContainer"});

      var newCell;
      newCell = createThumbnailCell(null, "New project", "images/thickPlus-white.svg", function() { showProjectCreator(newCell) });
      newCell.addClass("newButton");
      projectList.append(newCell);

      if (_.isObject(projects)) {
         var sortedProjectPairs = _.sortBy(_.pairs(projects), function(s) { return s[1].name.toLowerCase(); });
         _.each(sortedProjectPairs, function (project) {
            var image;
            if (_.isString(project[1].thumbnail))
               image = project[1].thumbnail;

            projectList.append(
               createThumbnailCell(project[0], project[1].name, image,
                  function() { selectProject(project[0]) },
                  function() { Groundhog.deleteProject(project[0]) }
               )
            );
         });
      }

      return projectList;
   };

   var createProjectManager = function() {
      var backsplashDiv = $("<div/>", {id: "projectManager", "class": "fullscreenBackdrop", style: "display:none"});

      var contentsDiv = $("<div/>", {"class": "fullscreenThumbnailViewer"});
      
      var backToProjectLink = $("<div/>", {"class": "projectManagerHeaderLinkRight link"});
      backToProjectLink.click(function() { setApplicationState("wholeProject") });
      contentsDiv.append(backToProjectLink);
      Groundhog.watchProjectName(function(projectName) {
         backToProjectLink.empty();
         if (projectName.length > 0)
            backToProjectLink.text(projectName + " >");
      });

      contentsDiv.append($("<div/>", {"class": "projectManagerTitle", text: "Projects"}));
      backsplashDiv.append(contentsDiv);

      var projectListing = $("<div/>");
      contentsDiv.append(projectListing);

      var publicProjects = {};
      var privateProjects = {};

      var redrawProjectList = function() {
         var allProjects = _.extend(_.clone(publicProjects), privateProjects);

         projectListing.html("");
         projectListing.append(createProjectManagerContent(allProjects));
      };

      Groundhog.watchPublicProjects(function (projs) {
         publicProjects = projs;
         redrawProjectList();
      });
      Groundhog.watchMyProjects(function (projs) {
         privateProjects = projs;
         redrawProjectList();
      });

      $("body").append(backsplashDiv);
   };

   window.addEventListener("popstate", function() {
      if (Groundhog.getOfflineMode())
         return;

      var projectInfo = projectSelectionFromWindowLocation();
      if (projectInfo == null) {
         Groundhog.selectProject(null);
         setApplicationState("projectManager");
      }
      else {
         Groundhog.selectProject(projectInfo.projectId, projectInfo.optionId);
         if (projectInfo.optionId == null)
            setApplicationState("wholeProject");
         else
            setApplicationState("optionEditor");
      }
   });

   window.selectProject = function(projectId, optionId, preserveApplicationState) {
      clearAllText();

      //empty the thumbnail list except 'new design' thumbnail when switching to a new project
      if (optionId == null && Groundhog.projectId!==projectId)
         $(".thumbnailList").find('li:not(:first)').remove();

      Groundhog.selectProject(projectId, optionId);
      var queryString = "?project=" + projectId;
      if (optionId != null) {
         queryString += "&option=" + optionId;
         if (!preserveApplicationState)
            setApplicationState("optionEditor");
         clearUndoStacks();
         requestRedraw();
      }
      else {
         if (!preserveApplicationState)
            setApplicationState("wholeProject");
      }
      window.history.pushState(null, null, queryString);
   };

   var trimScore = function(score, min, max) {
      return  Math.max(Math.min(score, max), min);
   };

   var sortByOverallScore = function(s) {
      var designId = $(s).attr("data-id");
      var scores = lastDisplayedScores[designId];
      if (scores == null)
         return Number.MAX_VALUE;

      var weights = getAllWeights(this.priorities);
      var totalScore = 0;

      // Weigh non-custom results
      _.each(weights, function(weight, key) { 
         var score = scores[key];
         if (score == null)
            totalScore += 1*weight; // assume the worst
         else
            totalScore += trimScore(score, minScore, maxScore) * weight;
      });

      // Weigh custom results
      if (_.isObject(scores.customScores))
         _.each(scores.customScores, function(score, key) {
            var weight = weights[key];
            if (weight == null)
               weight = defaultCustomAnalysisWeight;
            totalScore += trimScore(score, minScore, maxScore) * weight;
         });

      return totalScore;
   };

   var sortOptions = { 
      "by overall score": sortByOverallScore,
      "by usage": function(s) {
            var designId = $(s).attr("data-id");
            var data = lastDisplayedScores[designId];
            if (data != null && data.program != null) {
               return data.program;
            }
            return Number.MAX_VALUE;
         },
      "by layout": function(s) {
            var designId = $(s).attr("data-id");
            var data = lastDisplayedScores[designId];
            if (data != null && data.layout!=null) {
               return data.layout;
            }
            return Number.MAX_VALUE;
         },
      "by site fit": function(s) {
            var designId = $(s).attr("data-id");
            var data = lastDisplayedScores[designId];
            if (data != null && data.siteFit != null) {
               return data.siteFit;
            }
            return Number.MAX_VALUE;
         },
      "by least circulation": function(s) {
            var designId = $(s).attr("data-id");
            var data = lastDisplayedScores[designId];
            if (data != null && data.billableSpace != null) {
               return data.billableSpace;
            }
            return Number.MAX_VALUE;
         },
      "by custom analysis results": function(s) {
            var designId = $(s).attr("data-id");
            var data = lastDisplayedScores[designId];
            if (data != null && _.isObject(data.customScores)) {
               return _.reduce(data.customScores, function(m, score){ return m + score; }, 0);
            }
            return Number.MAX_VALUE;
         },
      "by name": function(s) {
         var designId = $(s).attr("data-id");
         var name = lastDisplayedNames[designId];
         if (name != null) 
            return name.toLowerCase();
      }
   };

   var requestArray = [];
   var portToGenerator = {
      "34568": "Cellular Automata",
      "34569": "L-System Grammar",
      "34570": "Shell Stuffer"
      // ,
      // "34571": "ExeBridge",
      // "34572": "Facilitator"
   }

   var assignJobIdToThumbnail = function(serviceURL, jobId) {
      var delayAndRemoveThumbnail = function(liItem) {
         setTimeout(function() { 
            $(liItem).remove();
         }, 2000);
      };
      for (var i = requestArray.length-1; i >= 0 ; i--) {
         var listItem = requestArray[i][1];

         //find the first unclaimed request of the given generatorType
         if (requestArray[i][0] == portToGenerator[serviceURL.split(':')[2]] ) {
            //if there is a jobId "POST" has succeded, otherwise failed
            if (jobId) {
               //assign the jobId to the corresponding li element
               requestArray[i][0] = jobId;
               listItem.attr("data-jobId", jobId);
               listItem.addClass('pending');
               //update the job status text on the status div
               $(listItem).children().find(".jobStatus").text("in progress");
               var deleteIcon = $(listItem).children().find(".link.thumbnailDelete");
               var thumbnailCell = $(listItem).children();
               thumbnailCell.hover( function() { deleteIcon.show() }, function() { deleteIcon.hide() });
            }
            else {
               $(listItem).children().find(".jobStatus").text("job failed"); 
               requestArray.splice(i,1); 
               delayAndRemoveThumbnail(listItem);
            }
            break;
         }
      }       
   };

   var assignDesignIdToThumbnail = function(jobId, designId, serviceURL) {
      for (i = 0; i < requestArray.length; i++) {
         var generator = portToGenerator[serviceURL.split(':')[2]]
         if (requestArray[i][0] == jobId && requestArray[i][1].attr("data-gen") == generator) {
            Groundhog.setDesignOptionMetadata("generatorType", generator, designId);
            requestArray[i][1].attr("data-id", designId);
            $(requestArray[i][1]).children().find(".jobStatus").text("loading data");
            break;
         }
      }
   };

   var generateOptions = {
      "Random Domino": function() {
         testGen(null);
      },  
      "L-System Grammar": function() {
         initiateThumbnailCell("L-System Grammar", getServiceURL("lsystem"));
         window.callRestGeneratorService(getServiceURL("lsystem"), assignJobIdToThumbnail, assignDesignIdToThumbnail);
      },
      "Shell Stuffer": function() {
         initiateThumbnailCell("Shell Stuffer", getServiceURL("stuffer"));
         window.callRestGeneratorService(getServiceURL("stuffer"), assignJobIdToThumbnail, assignDesignIdToThumbnail);
      },
      "Cellular Automata": function() {
         initiateThumbnailCell("Cellular Automata", getServiceURL("cellular"));
         window.callRestGeneratorService(getServiceURL("cellular"), assignJobIdToThumbnail, assignDesignIdToThumbnail);
      }
      // ,
      // "Exe Bridge": function() {
      //    initiateThumbnailCell("Exe Bridge", getServiceURL("ExeBridge"));
      //    window.callRestGeneratorService(getServiceURL("ExeBridge"), assignJobIdToThumbnail, assignDesignIdToThumbnail);
      // },
      // "Facilitator": function() {
      //    initiateThumbnailCell("Facilitator", getServiceURL("Facilitator"));
      //    window.callRestGeneratorService(getServiceURL("Facilitator"), assignJobIdToThumbnail, assignDesignIdToThumbnail);
      // }
   };

   var removeThumbnail = function(listItem) {
      //remove from the .ul thumbanail list
      $(listItem).remove();
      //remove from the request array
      for (var i = requestArray.length -1; i >= 0 ; i--) {
         var thisLiItem = requestArray[i][1];
         if (thisLiItem==listItem) {
            requestArray.splice(i,1); 
            break;
         }
      }
   };

   var initiateThumbnailCell = function(generatorType, serviceURL) {
      var thumbnailList = $(".thumbnailList");
      //Create a placeholder thumbnail cell to be later filled with the design option's data.
      //Each thumbnail placeholder maintains 3 data attributes:
      //data-gen: generator type 
      //data-jobId: id of the requested service -> to be filled once job is posted
      //data-id: unique Id of the design option -> to be filled when design option added to Groundhog DB
      var listItem = $("<li/>", {"data-id": "unassigned", "data-jobId": "unassigned", "data-gen": generatorType});
      listItem.append(createStatusDiv(generatorType, 
         function() {
            window.jobsToCancel.push(serviceURL+listItem.attr("data-jobId"));
            window.cancelRestGeneratorService(serviceURL, listItem.attr("data-jobId"), removeThumbnail(listItem));
         }
      ));
      thumbnailList.append(listItem);

      requestArray.push([generatorType, listItem]);
   };

   var createStatusDiv = function(generatorType, deleteAction) {
      var thumbnailCell = $("<div/>", {"class": "statusThumbnail"});
      $('<h1>', {class: 'genStatus', text: filterOptions[generatorType]}).appendTo(thumbnailCell);
      $('<p>', {class: 'jobStatus', text: "request sent"}).appendTo(thumbnailCell);

      if (deleteAction != null) {
         var deleteIcon = $("<span/>", {text: "×", class: "link thumbnailDelete", title: "Delete design", style: "display:none"});
         deleteIcon.click(function() {
            if (confirm("Really cancel the posted job?"))
               deleteAction();
            return false;
         });
         thumbnailCell.append(deleteIcon);
      }
      return thumbnailCell;
   };

   var updateDesignListHeaderLinks = function(designOptions, headerLinks) {
      headerLinks.empty();
      var designCount = 50;

      var autoGenerate = function() {
         setAutogenerationLinkState(true);
         generateAndScore(designCount, null, function() { 
            setAutogenerationLinkState(false); });
      };

      var createGeneratorSelectorDropdown = function() {
         var generateDropdown = $("<ul/>", {"class": "generateDropdown"});
         $.each(generateOptions, function (generateOption, generatorCallback) {
            var genDropdownButton = $("<li/>", {text: generateOption});
            genDropdownButton.click(function() {
               generatorCallback();
            });
            generateDropdown.append(genDropdownButton);
         });
         generateDropdown.mouseleave(function() { generateDropdown.fadeOut(500) });
         return generateDropdown;
      };

      var addComputationLink = function(startText, stopText, identifyingClass, currentlyOn, startAction, stopAction) {
         var container = $("<div/>", {"class": identifyingClass});
         var startLink = $("<div/>", {"class": "on link", text: startText});
         var stopLink = $("<div/>", {"class": "off link", text: stopText});

         //startAction can be either fed by a callback function or 
         //a dropdown menu linked to a series of start actions
         if (jQuery.isFunction(startAction))
            startLink.click(startAction);
         else {
            container.append(startAction);
            startLink.click(function(){startAction.show();});
         }

         stopLink.click(stopAction);
         container.append(startLink);
         container.append(stopLink);
         headerLinks.append(container);

         if (currentlyOn)
            startLink.hide();
         else
            stopLink.hide();

         return function(running) {
            $("." + identifyingClass + " .on").toggle(!running);
            $("." + identifyingClass + " .off").toggle(running);
         }
      };

      var genSelectorDropdownMenu = createGeneratorSelectorDropdown();
      
      var setAutogenerationLinkState = addComputationLink("Autogenerate", "Stop generating", "designListAutogenerateLink",
         currentlyAutogenerating, 
         autoGenerate,
         function() {
            cancelGeneration();
         }
      );

      var setSendJobLinkState = addComputationLink("Post Job", "Cancel job", "designListGenerateLink", 
         false, 
         genSelectorDropdownMenu,
         function() {
            cancelGeneration();
         }
      );

      if (!_.isEmpty(designOptions)) {
         var setAnalysisLinkState = addComputationLink("Re-analyze all", "Cancel analysis", "designListAnalysisLink", 
            currentlyAnalyzingDesignOptions, 
            function() {
               setAnalysisLinkState(true);
               analyzeDesignOptions(Groundhog.projectId, _.keys(designOptions), function() {
                  setApplicationState("wholeProject");
                  setAnalysisLinkState(false);
               });
            },
            cancelAnalyzeDesignOptions
         );
      }
   };

   var displayComparisonChart = function() {
      if ($(".designOptionsComparator").css("display")=="none") {
         if ($("[type='checkbox']:checked").length ==2) {
            $(".designOptionsComparator").show();
         }
         else {
            alert("Select two design options to perform a comparison.");
         }
      }
      else {
         $(".designOptionsComparator").hide();
      }
   };

   var updateComparisonChart = function() {
      if ($("[type='checkbox']:checked").length==2) {
         var radarChart = createComparisonRadarChart();
         if ($(".designOptionsComparator").children().length == 0)
            $(".designOptionsComparator").append(radarChart);
         else 
            $(".designOptionsComparator").empty();
            $(".designOptionsComparator").append(radarChart);
      } else {
         $(".designOptionsComparator").empty();
      }
   };

   var getAnalysisPrioritiesInfo = function() {
      return {
         program: {
            name: "Usage",
            title: "Importance of meeting all space allocation goals (see \"usage\" tab above)",
            initial: 1
         },
         layout: {
            name: "Layout",
            title: "Importance of moving between spaces within required walking distances (see \"layout\" tab above)",
            initial: 0.8
         },
         siteFit: {
            name: "Site",
            title: "Importance of fitting within the site dimensions (see \"site\" tab above)",
            initial: 1.0
         },
         billableSpace: {
            name: "Minimize circulation",
            title: "Importance of using fewer hallways and stairs",
            initial: 0.4
         }
      };
   };

   var analysisPrioritiesElemId = "analysisPriorities";

   var defaultCustomAnalysisWeight = 0.2;
   var getAllWeights = function(prioritiesElem) {
      var weights = {};
      if (prioritiesElem != null)
         weights = _.clone(prioritiesElem.get("weights"));

      // Use the defaults if the user didn't specify anything else
      _.each(getAnalysisPrioritiesInfo(), function(info, key) {
         if (weights[key] == null)
            weights[key] = info.initial;
      });

      return weights;
   };

   var createPrioritiesPane = function() {
      var div = $("<div/>", {"class": "prioritiesPane", style: "display:none"});

      var sliderRows = {}; // may from analysis key to jQuery elem

      var addSlider = function(id, name, titleText) {
         var row = $("<div/>");
         var labelInfo = {text: name + ": "};
         if (_.isString(titleText)) {
            labelInfo.title = titleText;
            labelInfo.style = "cursor:help";
         }
         row.append($("<span/>", labelInfo));
         var input = $("<input/>", {"class": "designListControlElements", type:"range", min:0, max:1, step:0.01, style:"height:20px;"});
         sliderRows[id] = row;
         row.append(input);
         div.append(row);

         input.change(function() {
            Groundhog.getAll(analysisPrioritiesElemId, function (elems) {
               var elem = elems[analysisPrioritiesElemId];
               if (elem == null) {
                  var data = new AnalysisPrioritiesData(analysisPrioritiesElemId, {});
                  window.preAddElementForTransaction(data.uniqueId);
                  elem = Groundhog.addElement(data, {global: true});
               }

               var parsedVal = parseFloat(input.val());
               if (_.isNaN(parsedVal))
                  return;

               var weights = elem.get("weights");
               weights[id] = parsedVal;
               elem.set("weights", weights);

               window.endTransaction();

               if (currentSortOption == "by overall score")
                  sortByAnimating();
            });
         });
         return row;
      }

      _.each(getAnalysisPrioritiesInfo(), function(info, key) {
         addSlider(key, info.name, info.title);
      });

      // Set all sliders to their default values
      var currentWeights; // Cache the weights where the .Script callback can see them, if custom sliders get created later
      Groundhog.watchAll(analysisPrioritiesElemId, function(elems) {
         div.show();

         var elem = elems[analysisPrioritiesElemId];
         currentWeights = getAllWeights(elems[analysisPrioritiesElemId]);

         _.each(sliderRows, function(sliderRow, key) {
            var weight = currentWeights[key];
            if (weight == null)
               weight = defaultCustomAnalysisWeight;

            var slider = sliderRow.find("input");
            if (!slider.is(":focus"))
               slider.val(weight);
         });
      });

      var customHeader = $("<div/>", {text: "Custom scripts:", "class": "projectManagerSubLabel", style: "display:none"});
      div.append(customHeader);
      var numCustomSliders = 0; // Used to show/hide the header

      Groundhog.watchEach(".Script", function(elem) {
         var elemId = elem.get("uniqueId");
         var sliderRow = sliderRows[elemId];
         var name = elem.get("name");
         if (sliderRow == null) {
            numCustomSliders++;
            if (numCustomSliders == 1)
               customHeader.show();

            sliderRow = addSlider(elemId, name);
            var weight;
            if (currentWeights != null)
               weight = currentWeights[elemId];
            if (weight == null)
               weight = defaultCustomAnalysisWeight;
            sliderRow.find("input").val(weight);
         }
         else
            sliderRow.find("span").text(name + ": ");
      });
      Groundhog.watchUnload(".Script", function(elem) {
         var elemId = elem.get("uniqueId");
         var sliderRow = sliderRows[elemId];
         if (sliderRow != null) {
            sliderRow.remove();
            delete sliderRows[elemId];

            numCustomSliders--;
            if (numCustomSliders == 0)
               customHeader.hide();
         }
      });

      return div;
   };

   var deleteDesignOption = function (optionIdToBeDeleted) {
      //check if the option to be deleted is the one currently displayed 
      //in the option editor
      if (optionIdToBeDeleted == Groundhog.optionId) 
         Groundhog.selectProject(Groundhog.projectId);
      
      Groundhog.deleteDesignOption(optionIdToBeDeleted);
   };

   var deleteSelectedOptions = function() {
      for (var i = 0; i < deleteList.length; i++) {
         deleteDesignOption(deleteList[i]);
      }
      deleteList = [];
   };

   var flipped = false;
   var createDesignListControls = function() {
      var designListControls = $("<div/>", {id: "designListControls"});

      //button to delete all thumbnails
      var deleteAllButton = $("<img/>", {src: "images/deleteAll.svg", 
         "class": "designListControlElements", style: "width:18px; height:18px"});
      deleteAllButton.click( function(){ 
         if (Object.keys(currentDesignOptions).length == 0) {
            alert("No designs to be deleted.");
            return false;
         }
         else {
            var label = "all"
            if (deleteList.length > 0)
               label = "the " + deleteList.length + " selected";

            if (confirm("Really delete " + label + " designs?")) {
               if (deleteList.length > 0) {
                  deleteSelectedOptions();
               }
               else {
                  $.each(currentDesignOptions, function (optionId, option) {
                     deleteDesignOption(optionId);
                  });
               }
            }
            return false;   
         }
      });
      designListControls.append(deleteAllButton);

      //button to display the radar chart of every thumbnail
      var flipAllThumbnailsButton = $("<img/>", {src: "images/flip.svg", 
         "class": "designListControlElements", style: "width:18px; height:18px"});
      flipAllThumbnailsButton.click(function(){
         if (!flipped) {
            $(".radarChartContainer").show();
            $.each($(".radarChartContainer"), function() {
               idToDisplayState[$(this).attr('data-id')] = "block";});
            flipped = true;
         } else {
            $(".radarChartContainer").hide();
            $.each($(".radarChartContainer"), function() {
               idToDisplayState[$(this).attr('data-id')] = "none";});
            flipped = false;
         }         
      });
      designListControls.append(flipAllThumbnailsButton);

      //button to simultaneously display the radar chart of two selected designs
      var juxtaposeButton = $("<img/>", {src: "images/juxtapose.svg", 
         "class": "designListControlElements", style: "width:18px; height:18px; left: 2px"});
      juxtaposeButton.click(displayComparisonChart);
      designListControls.append(juxtaposeButton);

      //menu for filtering by type of autogenerator
      filterTypeLabel = $("<span/>", {text: currentFilterOption, 
         "class": "designListControlElements", style: "left: 21px;"});
      var filterMenuPanel = createSelectionPanel("images/filter.svg", filterOptions, filterDropdown, 
         function(option) {
            currentFilterOption = option;
            //FILTERING NETHODS TO BE CALLED HERE
         },
         filterTypeLabel,
         function() { 
            var newFilterTypeLabel = $("<span/>", {text: currentFilterOption, 
               "class": "designListControlElements", style: "left: 21px;"});
            filterTypeLabel.replaceWith(newFilterTypeLabel);
            filterTypeLabel = newFilterTypeLabel;
         });
      designListControls.append(filterMenuPanel);  

      //menu for sorting by score option
      sortTypeLabel = $("<span/>", {text: currentSortOption, 
         "class": "designListControlElements", style: "left: 21px;"});
      var sortMenuPanel = createSelectionPanel("images/sort.svg", sortOptions, sortDropdown, 
         function(option) {
            currentSortOption = option;
            sortByAnimating();
         }, 
         sortTypeLabel,
         function() { 
            var newSortTypeLabel = $("<span/>", {text: currentSortOption, 
               "class": "designListControlElements", style: "left: 21px;"});
            sortTypeLabel.replaceWith(newSortTypeLabel);
            sortTypeLabel = newSortTypeLabel;
         });
      designListControls.append(sortMenuPanel);   
      
      return designListControls;
   };

   var createSelectionPanel = function(imagePath, options, dropdownMenu, callback, label, updateLabel)  {
      var selectionPanel = $("<div/>", {"class": "designListControlElements", 
         style: "width: 150px; left: 15px"});
      
      var imageButton = $("<img/>", {src: imagePath, "class": "designListControlElements", 
         style: "position: absolute; width:20px; height:20px"});
      imageButton.click(function(){dropdownMenu.show();});
      selectionPanel.append(imageButton);

      selectionPanel.append(label);

      dropdownMenu = createDropdownMenu(options, callback, updateLabel);
      selectionPanel.append(dropdownMenu);

      return selectionPanel;
   }

   var createDropdownMenu = function(options, callback, updateLabel) {
      var selectedDiv;
      var dropdownMenu = $("<ul/>", {"class": "designOptionsDropdown"});

      $.each(options, function (option) {
         var dropdownMenuButton = $("<li/>", {text: option});
         dropdownMenuButton.click(function(e) {
            if (selectedDiv != null) 
               selectedDiv.removeClass("selectedDropdownRow");
            selectedDiv = dropdownMenuButton;
            dropdownMenuButton.addClass("selectedDropdownRow");

            callback(option);
            updateLabel();

            return false;
         });
         dropdownMenu.append(dropdownMenuButton);
      });

      dropdownMenu.mouseleave(function() { dropdownMenu.fadeOut(500) });

      return dropdownMenu;
   };

   var createOptionViewer = function() {
      var backsplashDiv = $("<div/>", {id: "designOptionsViewer", "class": "fullscreenBackdrop", style: "display:none"});
      $("body").append(backsplashDiv);

      var viewer = $("<div/>", {"class": "fullscreenThumbnailViewer"});
      backsplashDiv.append(viewer);

      var otherProjects = $("<div/>", {"class": "projectManagerHeaderLinkLeft link", text: "< Other Projects"});
      otherProjects.click(function() { setApplicationState("projectManager") });
      viewer.append(otherProjects);

      var backToOptionLink = $("<div/>", {"class": "projectManagerHeaderLinkRight link"});
      backToOptionLink.click(function() { setApplicationState("optionEditor") });
      viewer.append(backToOptionLink);
      Groundhog.watchDesignOptionName(function(designOptionName) {
         backToOptionLink.empty();
         if (_.isString(designOptionName) && designOptionName.length > 0)
            backToOptionLink.text(designOptionName + " >");
      });

      var title = $("<div/>", {"class": "projectManagerTitle"});
      viewer.append(title);

      Groundhog.watchProjectName(function(projectName) {
         title.empty();
         title.append($("<div/>", {text: projectName}));
      });

      var contentPane = $("<div/>", {"class": "projectManagerContent"});
      viewer.append(contentPane);

      var goalsPane = $("<div/>", {"class": "projectManagerLeftPane"});
      contentPane.append(goalsPane);
      goalsPane.append($("<div/>", {text: "Goals:", "class": "projectManagerLabel"}));
      createRequirementsEditor(goalsPane);

      goalsPane.append($("<div/>", {text: "Priorities:", "class": "projectManagerLabel"}));
      goalsPane.append(createPrioritiesPane());

      var designOptionsPane = $("<div/>", {"class": "designOptionList"});
      contentPane.append(designOptionsPane);
      
      designOptionsPane.append($("<div/>", {text: "Designs:", "class": "projectManagerLabel"}));

      //div placeholder for headerlinks
      var headerLinks = $("<div/>", {"class": "projectManagerHeaderLinkRight"});
      designOptionsPane.append(headerLinks);

      designOptionsPane.append(createDesignListControls());
      designOptionsPane.append($("<div/>", {"class": "designOptionsComparator"}));

      var thumbnailListContainer = $("<div/>", {"class": "thumbnailListContainer"});
      designOptionsPane.append(thumbnailListContainer);

      //ul placeholder for design options thumbnails
      var thumbnailList = $("<ul/>", {"class": "thumbnailList"});
      thumbnailListContainer.append(thumbnailList);

      var newDesignThumbnail = createNewDesignThumbnail();
      thumbnailList.append(newDesignThumbnail);
      
      Groundhog.watchDesignOptions(function(designOptions) {
         currentDesignOptions = designOptions;
         updateDesignListHeaderLinks(designOptions, headerLinks);
         updateOptionsManagerContent(designOptions, thumbnailList);         
      });
   };

   $(document).ready(function() {
      createProjectManager();
      createOptionViewer();

      Groundhog.watchProjectName(updatePageTitle);
      Groundhog.watchDesignOptionName(updatePageTitle);

      if (Groundhog.getOfflineMode())
         Groundhog.renameProject(getNewProjectName());
      else {
         var projectInfo = projectSelectionFromWindowLocation();
         if (projectInfo != null)
            Groundhog.selectProject(projectInfo.projectId, projectInfo.optionId);
      };

      Groundhog.watchProjectConnection(function(connected) {
         if (connected) {
            if (Groundhog.optionId != null)
               setApplicationState("optionEditor");
            else
               setApplicationState("wholeProject");
         }
         else
            setApplicationState("projectManager");
      });
   });

   $(document).on('keyup',function(event) {
      if (event.keyCode == 27 && deleteList.length > 0) {
         var thumbnails = $(".thumbnailList li").get();
         for (var i = 0; i < thumbnails.length; i++) {
            $(thumbnails[i].children[0]).removeClass('selectedDesign');
         }
         deleteList = [];
      }
      if (event.keyCode == 46 || event.keyCode == 8) {
         if (deleteList.length == 0)
            return false;

         if (confirm("Really delete the " + deleteList.length + " designs?")) {
            var currentProjectId = Groundhog.projectId;
            var currentOptionId = Groundhog.optionId;
            deleteSelectedOptions(currentProjectId, currentOptionId);
         }
         return false; 
      }
   });

   return {
      getApplicationState: function() { return applicationState; }
   }
});
