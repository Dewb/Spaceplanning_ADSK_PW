define(['analysis/analyzers', "generators/wallGenerator"], function(analyzers, wallGenerator) {
   var levelHeight = 3.5;

   var saveIFC = function() {
      var ifc = {lastLabel: 399, text: ""};
      ifc.addLine = function(line) {
         this.lastLabel++;
         this.text += "#" + this.lastLabel + "= " + line + "\n";
         return "#" + this.lastLabel;
      };
      ifc.addLines = function(lines) {
         var self = this;
         var split = lines.split("\n");
         _.each(split, function(line) {
            if (line.length > 0)
               self.text += "#" + ++self.lastLabel + "= ";
            self.text += line + "\n";
         });
      }

      addHeaderAndStandardDefinitions(ifc);
      addTypeDefinitions(ifc, levelHeight);

      var finishIFC = function() {
         addFooter(ifc);

         // Name the project
         var baseName = "";
         var projName = Groundhog.getProjectName();
         if (_.isString(projName))
            baseName += projName;
         var designName = Groundhog.getDesignOptionName();
         if (_.isString(designName)) {
            if (baseName.length > 0)
               baseName += " - ";
            baseName += designName;
         }
         if (baseName.length == 0)
            baseName = "Unnamed";

         // Save the file
         var pom = document.createElement('a');
         pom.setAttribute('href', 'data:text/plain;charset=utf-8,' + encodeURIComponent(ifc.text));
         pom.setAttribute('download', baseName + ".ifc");
         pom.click();
      }

      runAnalysis(function(analysisResults) {
         // Door placement: Find all direct adjacencies between rooms, based on the paths that we expect occupants to walk
         var doors = [];
         _.each(analysisResults.layout, function(result) {
            if (result.pathRooms == null)
               return;
            for (var i = 1; i < result.pathRooms.length; i++) {
               var position = result.pathRooms[i].internalPath[0];
               position = [position[0], position[1], (position[2] - 1) * levelHeight];
               if (!_.any(doors, function(old) { return _.isEqual(old, position)})) {
                  doors.push(position);
               }
            }
         });

         wallGenerator.generate(function(walls) {
            var numberOfLevels = getNumberOfLevels();
            var levels = {};
            for (var i = 0; i < numberOfLevels; i++)
               levels[i + 1] = addLevel(ifc, "Level " + (i+1), levelHeight*i);

            var isBetween = function(cand, end1, end2) { return ((cand >= end1 && cand <= end2) || (cand >= end2 && cand <= end1)) };

            _.each(walls, function(wall) {
               var level = Math.round(wall[0][2] / levelHeight) + 1;
               var wallLine = addStraightWall(ifc, levels[level], level, wall[0], wall[1]);
               
               var axis = almostEqual(wall[0][1], wall[1][1]) ? "x" : "y";
               _.each(doors, function(door) {
                  // Not on the same level
                  if (!almostEqual(door[2], wall[0][2]))
                     return;

                  // Not along the path of the door
                  if ((axis == "x" && !almostEqual(wall[0][1], door[1])) || (axis != "x" && !almostEqual(wall[0][0], door[0])))
                     return;

                  // Not between the endpoints
                  if ((axis == "x" && !isBetween(door[0], wall[0][0], wall[1][0])) || (axis != "x" && !isBetween(door[1], wall[0][1], wall[1][1])))
                     return;

                  addDoor(ifc, levels[level], wallLine, door, axis);
               });
            });

            Groundhog.getAll(".Stairs", function(allStairs) {
               _.each(allStairs, function(stairs) {
                  var rotRad = stairs.get("rotation") % (2*Math.PI);
                  var rotation = [0,0,0];
                  if (almostEqual(rotRad, 0))
                     rotation[1] = -1;
                  else if (almostEqual(rotRad, Math.PI/2))
                     rotation[0] = 1;
                  else if (almostEqual(rotRad, Math.PI))
                     rotation[1] = 1;
                  else if (almostEqual(rotRad, Math.PI*3/2))
                     rotation[0] = -1;
                  else {
                     console.log("IFC export error: Stairs didn't appear axis-oriented: ", stairs.get("uniqueId"));
                     return;
                  }

                  var stairsPosition = stairs.get("position");
                  var maxLevel = stairs.displayedLevels-1 + stairs.displayedBaseLevel;
                  for (var level = stairs.displayedBaseLevel; level < maxLevel; level++) {
                     var position = [stairsPosition[0], stairsPosition[1], (level-1)*3.5];
                     addStairs(ifc, levels[level], position, rotation, (level == maxLevel-1));
                  }
               });
               finishIFC();
            });
         });
      });
   };

   var lenToIFC = function(len) { return (len * 1000).toFixed(6) }
   var coordsToIFC = function(coords) { return _.map(coords, function(c) { return lenToIFC(c)}).join(",") }

   var addHeaderAndStandardDefinitions = function(ifc) {
      ifc.text = "ISO-10303-21;\n\
HEADER;\n\
FILE_DESCRIPTION(('ViewDefinition [CoordinationView]'),'2;1');\n\
FILE_NAME('','2014-07-31T14:55:30',(''),(''),'The EXPRESS Data Manager Version 5.02.0100.07 : 28 Aug 2013','Development Build - Exporter 2016.0.0.0 - Default UI','');\n\
FILE_SCHEMA(('IFC2X3'));\n\
ENDSEC;\n\
\n\
DATA;\n\
#1= IFCORGANIZATION($,'Autodesk Akaba 2016 (ENU)',$,$,$);\n\
#2= IFCAPPLICATION(#1,'2016','Autodesk Akaba 2016 (ENU)','Akaba');\n\
#3= IFCCARTESIANPOINT((0.,0.,0.));\n\
#4= IFCCARTESIANPOINT((0.,0.));\n\
#5= IFCDIRECTION((1.,0.,0.));\n\
#6= IFCDIRECTION((-1.,0.,0.));\n\
#7= IFCDIRECTION((0.,1.,0.));\n\
#8= IFCDIRECTION((0.,-1.,0.));\n\
#9= IFCDIRECTION((0.,0.,1.));\n\
#10= IFCDIRECTION((0.,0.,-1.));\n\
#11= IFCDIRECTION((1.,0.));\n\
#12= IFCDIRECTION((-1.,0.));\n\
#13= IFCDIRECTION((0.,1.));\n\
#14= IFCDIRECTION((0.,-1.));\n\
#15= IFCAXIS2PLACEMENT3D(#3,$,$);\n\
#16= IFCLOCALPLACEMENT(#72,#15);\n\
#17= IFCPERSON($,'','lawrence',$,$,$,$,$);\n\
#18= IFCORGANIZATION($,'','',$,$);\n\
#19= IFCPERSONANDORGANIZATION(#17,#18,$);\n\
#20= IFCOWNERHISTORY(#19,#2,$,.NOCHANGE.,$,$,$,0);\n\
#21= IFCSIUNIT(*,.LENGTHUNIT.,.MILLI.,.METRE.);\n\
#22= IFCSIUNIT(*,.LENGTHUNIT.,$,.METRE.);\n\
#23= IFCSIUNIT(*,.AREAUNIT.,$,.SQUARE_METRE.);\n\
#24= IFCSIUNIT(*,.VOLUMEUNIT.,$,.CUBIC_METRE.);\n\
#25= IFCSIUNIT(*,.PLANEANGLEUNIT.,$,.RADIAN.);\n\
#26= IFCDIMENSIONALEXPONENTS(0,0,0,0,0,0,0);\n\
#27= IFCMEASUREWITHUNIT(IFCRATIOMEASURE(0.0174532925199433),#25);\n\
#28= IFCCONVERSIONBASEDUNIT(#26,.PLANEANGLEUNIT.,'DEGREE',#27);\n\
#29= IFCSIUNIT(*,.MASSUNIT.,.KILO.,.GRAM.);\n\
#30= IFCSIUNIT(*,.TIMEUNIT.,$,.SECOND.);\n\
#31= IFCSIUNIT(*,.FREQUENCYUNIT.,$,.HERTZ.);\n\
#32= IFCSIUNIT(*,.THERMODYNAMICTEMPERATUREUNIT.,$,.KELVIN.);\n\
#33= IFCSIUNIT(*,.THERMODYNAMICTEMPERATUREUNIT.,$,.DEGREE_CELSIUS.);\n\
#34= IFCDERIVEDUNITELEMENT(#29,1);\n\
#35= IFCDERIVEDUNITELEMENT(#32,-1);\n\
#36= IFCDERIVEDUNITELEMENT(#30,-3);\n\
#37= IFCDERIVEDUNIT((#34,#35,#36),.THERMALTRANSMITTANCEUNIT.,$);\n\
#38= IFCDERIVEDUNITELEMENT(#22,3);\n\
#39= IFCDERIVEDUNITELEMENT(#30,-1);\n\
#40= IFCDERIVEDUNIT((#38,#39),.VOLUMETRICFLOWRATEUNIT.,$);\n\
#41= IFCSIUNIT(*,.ELECTRICCURRENTUNIT.,$,.AMPERE.);\n\
#42= IFCSIUNIT(*,.ELECTRICVOLTAGEUNIT.,$,.VOLT.);\n\
#43= IFCSIUNIT(*,.POWERUNIT.,$,.WATT.);\n\
#44= IFCSIUNIT(*,.FORCEUNIT.,.KILO.,.NEWTON.);\n\
#45= IFCSIUNIT(*,.ILLUMINANCEUNIT.,$,.LUX.);\n\
#46= IFCSIUNIT(*,.LUMINOUSFLUXUNIT.,$,.LUMEN.);\n\
#47= IFCSIUNIT(*,.LUMINOUSINTENSITYUNIT.,$,.CANDELA.);\n\
#48= IFCDERIVEDUNITELEMENT(#29,-1);\n\
#49= IFCDERIVEDUNITELEMENT(#22,-2);\n\
#50= IFCDERIVEDUNITELEMENT(#30,3);\n\
#51= IFCDERIVEDUNITELEMENT(#46,1);\n\
#52= IFCDERIVEDUNIT((#48,#49,#50,#51),.USERDEFINED.,'Luminous Efficacy');\n\
#53= IFCSIUNIT(*,.PRESSUREUNIT.,$,.PASCAL.);\n\
#54= IFCUNITASSIGNMENT((#21,#23,#24,#28,#29,#30,#31,#33,#37,#40,#41,#42,#43,#44,#45,#46,#47,#53));\n\
#55= IFCAXIS2PLACEMENT3D(#3,$,$);\n\
#56= IFCDIRECTION((6.12303176911189E-17,1.));\n\
#57= IFCGEOMETRICREPRESENTATIONCONTEXT($,'Model',3,0.01,#55,#56);\n\
#58= IFCGEOMETRICREPRESENTATIONSUBCONTEXT('Axis','Model',*,*,*,*,#57,$,.GRAPH_VIEW.,$);\n\
#59= IFCGEOMETRICREPRESENTATIONSUBCONTEXT('Body','Model',*,*,*,*,#57,$,.MODEL_VIEW.,$);\n\
#60= IFCGEOMETRICREPRESENTATIONSUBCONTEXT('Box','Model',*,*,*,*,#57,$,.MODEL_VIEW.,$);\n\
#61= IFCGEOMETRICREPRESENTATIONSUBCONTEXT('FootPrint','Model',*,*,*,*,#57,$,.MODEL_VIEW.,$);\n\
#62= IFCGEOMETRICREPRESENTATIONCONTEXT($,'Annotation',3,0.01,#55,#56);\n\
#63= IFCGEOMETRICREPRESENTATIONSUBCONTEXT($,'Annotation',*,*,*,*,#62,0.01,.PLAN_VIEW.,$);\n\
#64= IFCPROJECT('3AyZfN7yTA6fZfINbBrBTJ',#20,'',$,$,'C:\\\\Users\\\\campbem\\\\Desktop\\\\testProject.ifc','',(#57,#62),#54);\n\
#65= IFCPOSTALADDRESS($,$,$,$,(),$,'','','','');\n\
#66= IFCBUILDING('3AyZfN7yTA6fZfINbBrBTI',#20,'',$,$,#16,$,'',.ELEMENT.,$,$,#65);\n\
#67= IFCAXIS2PLACEMENT3D(#3,$,$);\n\
#68= IFCPROPERTYSINGLEVALUE('Category',$,IFCLABEL('Project Information'),$);\n\
#69= IFCPROPERTYSET('2WeuRw9Sv2ghe_1xPirXDp',#20,'Other',$,(#68));\n\
#70= IFCPROPERTYSET('0ho8$LEFD2qhCFaVd4Iw3H',#20,'Other',$,(#68));\n\
#71= IFCAXIS2PLACEMENT3D(#3,$,$);\n\
#72= IFCLOCALPLACEMENT($,#71);\n\
#73= IFCSITE('3AyZfN7yTA6fZfINbBrBTH',#20,'Default',$,'',#72,$,$,.ELEMENT.,(42,24,53,508911),(-71,-15,-29,-58837),0.,$,$);\n\
#74= IFCRELDEFINESBYPROPERTIES('3Va66FCb9FHRC6KnmfEwYY',#20,$,$,(#73),#69);\n\
#75= IFCRELDEFINESBYPROPERTIES('1XQcd91l5EIh0$7dLiyrDY',#20,$,$,(#66),#70);\n\
#76= IFCPROPERTYSINGLEVALUE('Building Name',$,IFCTEXT(''),$);\n\
#77= IFCPROPERTYSINGLEVALUE('Organization Description',$,IFCTEXT(''),$);\n\
#78= IFCPROPERTYSINGLEVALUE('Organization Name',$,IFCTEXT(''),$);\n\
#79= IFCPROPERTYSET('1t7wbHTYz1xQTQKkDqBI$n',#20,'Identity Data',$,(#76,#77,#78));\n\
#80= IFCPROPERTYSET('0MAD2nC0f2HP3wEvUUl0ty',#20,'Identity Data',$,(#76,#77,#78));\n\
#81= IFCRELDEFINESBYPROPERTIES('1t7wbHTYz1xQTQK_DqBI$n',#20,$,$,(#73),#79);\n\
#82= IFCRELDEFINESBYPROPERTIES('0iyshikgLBGOY_BoHzoDP1',#20,$,$,(#66),#80);\n\
#83= IFCRELAGGREGATES('2c_xifeQ5A8gL3gQAT2W0u',#20,$,$,#64,(#73));\n\
#84= IFCRELAGGREGATES('3mBjxSJyTCnv$wAL7nudO4',#20,$,$,#73,(#66));\n\
#85= IFCPROPERTYSINGLEVALUE('NumberOfStoreys',$,IFCINTEGER(100),$);\n\
#86= IFCPROPERTYSET('1t7wbHTYz1xQTQNmbqBI$n',#20,'Pset_BuildingCommon',$,(#85));\n\
#87= IFCRELDEFINESBYPROPERTIES('2ialgzK755MgCEpiWkb5GK',#20,$,$,(#66),#86);\n\
#88= IFCCLASSIFICATION('http://www.csiorg.net/uniformat','1998',$,'Uniformat');\n";
   }

   var addFooter = function(ifc) {
      ifc.text += "ENDSEC;\n\
\n\
END-ISO-10303-21;";
   };

   var addTypeDefinitions = function(ifc, levelHeight) {
      ifc.text += "/* Material */\n\
#100= IFCCOLOURRGB($,0.498039215686275,0.498039215686275,0.498039215686275);\n\
#101= IFCSURFACESTYLERENDERING(#100,0.,$,$,$,$,IFCNORMALISEDRATIOMEASURE(0.5),IFCSPECULAREXPONENT(64.),.NOTDEFINED.);\n\
#102= IFCSURFACESTYLE('Default Wall',.BOTH.,(#101));\n\
#103= IFCMATERIAL('Default Wall');\n\
#104= IFCPRESENTATIONSTYLEASSIGNMENT((#102));\n\
#105= IFCSTYLEDITEM($,(#104),$);\n\
#106= IFCSTYLEDREPRESENTATION(#57,'Style','Material',(#105));\n\
#107= IFCMATERIALDEFINITIONREPRESENTATION($,$,(#106),#103);\n\
#108= IFCMATERIALLAYER(#103,200.,$);\n\
#109= IFCMATERIALLAYERSET((#108),'Basic Wall:Wall 1');\n\
#110= IFCMATERIALLAYERSETUSAGE(#109,.AXIS2.,.NEGATIVE.,100.);\n\
\n\
\n\
/* Wall type */\n\
#130= IFCWALLTYPE('1t7wbHTYz1xQTQLE9qBIxD',#20,'Basic Wall:Wall 1',$,$,(#134,#135,#136,#137,#138),$,'1513',$,.STANDARD.);\n\
#131= IFCRELASSOCIATESMATERIAL('2mYoKWT695xgz6WqlJkk0K',#20,$,$,(#130),#109);\n\
#132= IFCPROPERTYSINGLEVALUE('Absorptance',$,IFCREAL(0.1),$);\n\
#133= IFCPROPERTYSINGLEVALUE('Roughness',$,IFCINTEGER(1),$);\n\
#134= IFCPROPERTYSET('1t7wbHTYz1xQTQKhzqBIxD',#20,'Analytical Properties',$,(#132,#133));\n\
#135= IFCPROPERTYSET('1t7wbHTYz1xQTQKkPqBIxD',#20,'Construction',$,(#139,#140,#141,#142));\n\
#136= IFCPROPERTYSET('1t7wbHTYz1xQTQKkTqBIxD',#20,'Graphics',$,(#143));\n\
#137= IFCPROPERTYSET('1t7wbHTYz1xQTQKkDqBIxD',#20,'Identity Data',$,(#144,#145,#146));\n\
#138= IFCPROPERTYSET('3Wq_C7z59D6x7ncMxPjBdS',#20,'Other',$,(#147,#148));\n\
#139= IFCPROPERTYSINGLEVALUE('Function',$,IFCIDENTIFIER('Exterior'),$);\n\
#140= IFCPROPERTYSINGLEVALUE('Width',$,IFCLENGTHMEASURE(200.),$);\n\
#141= IFCPROPERTYSINGLEVALUE('Wrapping at Ends',$,IFCIDENTIFIER('None'),$);\n\
#142= IFCPROPERTYSINGLEVALUE('Wrapping at Inserts',$,IFCIDENTIFIER('Do not wrap'),$);\n\
#143= IFCPROPERTYSINGLEVALUE('Coarse Scale Fill Color',$,IFCINTEGER(0),$);\n\
#144= IFCPROPERTYSINGLEVALUE('Assembly Code',$,IFCTEXT(''),$);\n\
#145= IFCPROPERTYSINGLEVALUE('Assembly Description',$,IFCTEXT(''),$);\n\
#146= IFCPROPERTYSINGLEVALUE('Type Name',$,IFCTEXT('Wall 1'),$);\n\
#147= IFCPROPERTYSINGLEVALUE('Category',$,IFCLABEL('Walls'),$);\n\
#148= IFCPROPERTYSINGLEVALUE('Family Name',$,IFCTEXT('Basic Wall'),$);\n\
\n\
/* Popular wall parameters */\n\
#180= IFCPROPERTYSINGLEVALUE('Base Extension Distance',$,IFCLENGTHMEASURE(0.),$);\n\
#181= IFCPROPERTYSINGLEVALUE('Base is Attached',$,IFCBOOLEAN(.F.),$);\n\
#182= IFCPROPERTYSINGLEVALUE('Base Offset',$,IFCLENGTHMEASURE(0.),$);\n\
#183= IFCPROPERTYSINGLEVALUE('Location Line',$,IFCIDENTIFIER('Wall Centerline'),$);\n\
#184= IFCPROPERTYSINGLEVALUE('Related to Mass',$,IFCBOOLEAN(.F.),$);\n\
#185= IFCPROPERTYSINGLEVALUE('Room Bounding',$,IFCBOOLEAN(.T.),$);\n\
#186= IFCPROPERTYSINGLEVALUE('Top Extension Distance',$,IFCLENGTHMEASURE(0.),$);\n\
#187= IFCPROPERTYSINGLEVALUE('Top is Attached',$,IFCBOOLEAN(.F.),$);\n\
#188= IFCPROPERTYSINGLEVALUE('Top Offset',$,IFCLENGTHMEASURE(0.),$);\n\
#189= IFCPROPERTYSINGLEVALUE('Unconnected Height',$,IFCLENGTHMEASURE(" + lenToIFC(levelHeight) + "),$);\n\
#198= IFCPROPERTYSINGLEVALUE('Family',$,IFCLABEL('Basic Wall: Wall 1'),$);\n\
#199= IFCPROPERTYSINGLEVALUE('Family and Type',$,IFCLABEL('Basic Wall: Wall 1'),$);\n\
#200= IFCPROPERTYSINGLEVALUE('Type',$,IFCLABEL('Basic Wall: Wall 1'),$);\n\
#201= IFCPROPERTYSINGLEVALUE('Type Id',$,IFCLABEL('Basic Wall: Wall 1'),$);\n\
#213= IFCPROPERTYSET('1t7wbHTYz1xQTQKlPqBI9u',#20,'Constraints',$,(#180,#181,#182,#183,#184,#185,#186,#187,#188,#189));\n\
#227= IFCPROPERTYSET('26FcwhEvrEWR48INjbwywD',#20,'Other',$,(#147,#198,#199,#200,#201));\n\
#258= IFCPROPERTYSINGLEVALUE('Reference',$,IFCIDENTIFIER('Wall 1'),$);\n\
#259= IFCPROPERTYSINGLEVALUE('LoadBearing',$,IFCBOOLEAN(.F.),$);\n\
#260= IFCPROPERTYSINGLEVALUE('ExtendToStructure',$,IFCBOOLEAN(.F.),$);\n\
#261= IFCPROPERTYSINGLEVALUE('IsExternal',$,IFCBOOLEAN(.T.),$);\n\
#262= IFCPROPERTYSET('1t7wbHTYz1xQTQNnzqBI9u',#20,'Pset_WallCommon',$,(#258,#259,#260,#261));\n\
\n\
#111= IFCLOCALPLACEMENT(#16,#67);\n\
\n\
/* Wall shape */\n\
#275= IFCCARTESIANPOINT((500.,0.));\n\
#276= IFCAXIS2PLACEMENT2D(#275,#12);\n\
#277= IFCRECTANGLEPROFILEDEF(.AREA.,$,#276,1000.,200.);\n\
#278= IFCAXIS2PLACEMENT3D(#3,$,$);\n\
#279= IFCEXTRUDEDAREASOLID(#277,#278,#9," + lenToIFC(levelHeight) + ");\n\
#280= IFCPRESENTATIONSTYLEASSIGNMENT((#102));\n\
#281= IFCSTYLEDITEM(#279,(#280),$);\n\
#282= IFCSHAPEREPRESENTATION(#59,'Body','SweptSolid',(#279));\n";
   }

   // Create IFC guids
   // Omitting '/' from character set to avoid implementing their ban on "/*"" & "*/"
   var ifcGUIDCharacters = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz!#$%&^|*+,-.:;<=>?~`@_";
   var splitIFCGUIDCharacters = ifcGUIDCharacters.split("");
   var newIFCGUID = function() {
      var string = "";
      for (var i=0; i<22; i++)
         string += splitIFCGUIDCharacters[_.random(splitIFCGUIDCharacters.length - 1)];
      return "'" + string + "'";
   };

   var addPropertySet = function(ifc, name, propertyLines) {
      var properties = propertyLines.split("\n");
      var lineNumbers = [];
      _.each(properties, function(property) {
         lineNumbers.push(ifc.addLine(property));
      });
      return ifc.addLine("IFCPROPERTYSET(" + newIFCGUID() + ",#20,'" + name + "',$,(" + lineNumbers.join(",") + "));");
   };

   var applyPropertySet = function(ifc, recipientLine, setLine) {
      ifc.addLine("IFCRELDEFINESBYPROPERTIES(" + newIFCGUID() + ",#20,$,$,(" + recipientLine + ")," + setLine + ");");
   };

   var addLevel = function(ifc, levelName, elevation) {
      var storyLine = ifc.addLine("IFCBUILDINGSTOREY(" + newIFCGUID() + ",#20,'" + levelName + "',$,$,#111,$,'" + levelName + "',.ELEMENT.," + lenToIFC(elevation) + ");")
      applyPropertySet(ifc, storyLine, addPropertySet(ifc, "Pset_BuildingStoreyCommon", "IFCPROPERTYSINGLEVALUE('AboveGround',$,IFCLOGICAL(.U.),$);"));
      applyPropertySet(ifc, storyLine, addPropertySet(ifc, "Constraints", "IFCPROPERTYSINGLEVALUE('Elevation',$,IFCLENGTHMEASURE(" + lenToIFC(elevation) + "),$);"));
      applyPropertySet(ifc, storyLine, addPropertySet(ifc, "Dimensions", "IFCPROPERTYSINGLEVALUE('Computation Height',$,IFCLENGTHMEASURE(" + lenToIFC(elevation) + "),$);"));
      applyPropertySet(ifc, storyLine, addPropertySet(ifc, "Identity Data", "IFCPROPERTYSINGLEVALUE('Structural',$,IFCBOOLEAN(.F.),$);\n\
IFCPROPERTYSINGLEVALUE('Building Story',$,IFCBOOLEAN(.T.),$);\n\
IFCPROPERTYSINGLEVALUE('Name',$,IFCTEXT('" + levelName + "'),$);"));
      applyPropertySet(ifc, storyLine, addPropertySet(ifc, "Other", "IFCPROPERTYSINGLEVALUE('Category',$,IFCLABEL('Levels'),$);\n\
IFCPROPERTYSINGLEVALUE('Family',$,IFCLABEL('Level: " + levelName + "'),$);\n\
IFCPROPERTYSINGLEVALUE('Family and Type',$,IFCLABEL('Level: " + levelName + "'),$);\n\
IFCPROPERTYSINGLEVALUE('Type',$,IFCLABEL('Level: " + levelName + "'),$);\n\
IFCPROPERTYSINGLEVALUE('Type Id',$,IFCLABEL('Level: " + levelName + "'),$);"));
      applyPropertySet(ifc, storyLine, addPropertySet(ifc, "Constraints", "IFCPROPERTYSINGLEVALUE('Elevation Base',$,IFCIDENTIFIER('Project Base Point'),$);"));
      applyPropertySet(ifc, storyLine, addPropertySet(ifc, "Graphics", "IFCPROPERTYSINGLEVALUE('Color',$,IFCINTEGER(0),$);\n\
IFCPROPERTYSINGLEVALUE('Line Pattern',$,IFCLABEL('Solid'),$);\n\
IFCPROPERTYSINGLEVALUE('Line Weight',$,IFCIDENTIFIER('1'),$);\n\
IFCPROPERTYSINGLEVALUE('Symbol at End 1 Default',$,IFCBOOLEAN(.F.),$);\n\
IFCPROPERTYSINGLEVALUE('Symbol at End 2 Default',$,IFCBOOLEAN(.T.),$);"));
      applyPropertySet(ifc, storyLine, addPropertySet(ifc, "Identity Data", "IFCPROPERTYSINGLEVALUE('Type Name',$,IFCTEXT('" + levelName + "'),$);"));
      applyPropertySet(ifc, storyLine, addPropertySet(ifc, "Other", "IFCPROPERTYSINGLEVALUE('Family Name',$,IFCTEXT('Level'),$);\n\
IFCPROPERTYSINGLEVALUE('Category',$,IFCLABEL('Levels'),$);"));

      ifc.addLine("IFCRELAGGREGATES(" + newIFCGUID() + ",#20,$,$,#66,(" + storyLine + "));");

      return storyLine;
   }

   var createNullPosition = function(ifc) {
      var axisPlacement = ifc.addLine("IFCAXIS2PLACEMENT3D(#3,$,$);");
      var localPlacement = ifc.addLine("IFCLOCALPLACEMENT($," + axisPlacement + ");");
      var localPlacement2 = ifc.addLine("IFCLOCALPLACEMENT(" + localPlacement + "," + axisPlacement + ");");
      var localPlacement3 = ifc.addLine("IFCLOCALPLACEMENT(" + localPlacement2 + "," + axisPlacement + ");");
      var position = ifc.addLine("IFCLOCALPLACEMENT(" + ifc.addLine("IFCLOCALPLACEMENT(" + localPlacement3 + "," + axisPlacement + ");") + "," + axisPlacement + ");");

      return position;
   }

   var addDoor = function(ifc, levelLine, wallLine, position, axis) {
      var height = 2.1336;
      var width = 0.914;
      if (axis == "x")
         position[0] -= width/2;
      else
         position[1] -= width/2;
      var axisPlacement = ifc.addLine("IFCAXIS2PLACEMENT3D(" + ifc.addLine("IFCCARTESIANPOINT((" + coordsToIFC(position) + "));") + ",$,$);");
      var localPlacement = ifc.addLine("IFCLOCALPLACEMENT($,"+ axisPlacement + ");");
      var placement = ifc.addLine("IFCLOCALPLACEMENT(" + localPlacement + "," + ifc.addLine("IFCAXIS2PLACEMENT3D(#3,$,$);") + ");");

      // We're not actually creating any door-like geometry for the door yet since we don't know how doors swing. That should replace the $ after placement.
      var doorLine = ifc.addLine("IFCDOOR(" + newIFCGUID() + ",#20,'Single-Flush:36\" x 84\":2945',$,'36\" x 84\"'," + placement + ",$,'2945'," + lenToIFC(height) + "," + lenToIFC(width) + ");");

      // Add to the level
      ifc.addLine("IFCRELCONTAINEDINSPATIALSTRUCTURE(" + newIFCGUID() + ",#20,$,$,(" + doorLine + ")," + levelLine + ");");

      // Create the opening geometry
      var openingPoint = ifc.addLine("IFCCARTESIANPOINT((1066.8,457.2));");
      var profilePlacement = ifc.addLine("IFCAXIS2PLACEMENT2D(" + openingPoint + ",#13);");
      var profileDef = ifc.addLine("IFCRECTANGLEPROFILEDEF(.AREA.,$," + profilePlacement + "," + lenToIFC(width) + "," + lenToIFC(height) + ");");
      var axisPlacement2 = ifc.addLine("IFCAXIS2PLACEMENT3D(#3," + (axis == "x" ? "#7" : "#6") + ",#9);");
      var extrudedArea = ifc.addLine("IFCEXTRUDEDAREASOLID(" + profileDef + "," + axisPlacement2 + ",#9,200.);");
      var shape = ifc.addLine("IFCPRODUCTDEFINITIONSHAPE($,$,(" + ifc.addLine("IFCSHAPEREPRESENTATION(#59,'Body','SweptSolid',(" + extrudedArea + "));") + "));");

      // Create  the opening
      var openingLine = ifc.addLine("IFCOPENINGELEMENT(" + newIFCGUID() + ",#20,'Single-Flush:36\" x 84\":2945:1',$,'Opening'," + localPlacement + "," + shape + ",$);");
      ifc.addLine("IFCRELVOIDSELEMENT(" + newIFCGUID() + ",#20,$,$," + wallLine + "," + openingLine + ");");
      ifc.addLine("IFCRELFILLSELEMENT(" + newIFCGUID() + ",#20,$,$," + openingLine + "," + doorLine + ");");
   }

   var addSlab = function(ifc, center, direction, depth, width, thickness) {
      // Rotating 90º or 270º
      if (almostEqual(direction[0], 0)) {
         var temp = depth;
         depth = width;
         width = temp;
      }

      var point = ifc.addLine("IFCCARTESIANPOINT((0.,0.));");
      var axisPlacement = ifc.addLine("IFCAXIS2PLACEMENT2D(" + point + ",#11);");
      var rectangle = ifc.addLine("IFCRECTANGLEPROFILEDEF(.AREA.,'Non-Monolithic Landing'," + axisPlacement + "," + lenToIFC(width) + "," + lenToIFC(depth) + ");");
      var position = ifc.addLine("IFCCARTESIANPOINT((" + coordsToIFC(center) + "));");
      var axisPlacement2 = ifc.addLine("IFCAXIS2PLACEMENT3D(" + position + ",#9,#8);");
      var extrusion = ifc.addLine("IFCEXTRUDEDAREASOLID(" + rectangle + "," + axisPlacement2 + ",#9," + lenToIFC(thickness) + ");");
      var sweep = ifc.addLine("IFCSHAPEREPRESENTATION(#59,'Body','SweptSolid',(" + extrusion + "));");
      var shape = ifc.addLine("IFCPRODUCTDEFINITIONSHAPE($,$,(" + sweep + "));");

      var slabLine = ifc.addLine("IFCSLAB(" + newIFCGUID() + ",#20,'Assembled Stair:Stair:2849 Landing 1',$,'Assembled Stair:Stairs 1:1918'," + createNullPosition(ifc) + "," + shape + ",'3681',.LANDING.);");
      return slabLine;
   }

   var addStairsFlight = function(ifc, center, direction, depth, width, treadThickness, riserDepth) {
      var components = [];
      var numSteps = standardStairsParameters.stepsPerRun;
      var treadDepth = depth / numSteps;
      var treadWidth = width;
      var riseZ = levelHeight / (numSteps*2+2);
      var riserStart = [center[0], center[1], center[2]];
      var treadStart = [center[0], center[1], center[2] + riseZ - treadThickness];
      var offset = [0, 0, riseZ];
      var riserDirection = "#7";

      if (almostEqual(direction[0], 0)) {
         offset[1] = direction[1] * treadDepth;
         treadStart[1] += direction[1] * (treadDepth - riserDepth - depth)/2;
         riserStart[1] -= direction[1] * depth / 2;
         riserDirection = "#6";

         treadWidth = treadDepth;
         treadDepth = width;
      }
      else if (almostEqual(direction[1], 0)) {
         offset[0] = direction[0] * treadDepth;
         treadStart[0] += direction[0] * (treadDepth - riserDepth - depth)/2;
         riserStart[0] -= direction[0] * depth / 2;
      }


      var addRiser = function(i) {
         var axisPlacement = ifc.addLine("IFCAXIS2PLACEMENT2D(" + ifc.addLine("IFCCARTESIANPOINT((0.,0.));") + ",#11);");
         var rectangleProfile = ifc.addLine("IFCRECTANGLEPROFILEDEF(.AREA.,'50 mm Tread 10 mm Riser'," + axisPlacement + "," + lenToIFC(width) + "," + lenToIFC(riserDepth) + ");");
         var riserCoords = [offset[0]*i + riserStart[0], offset[1]*i + riserStart[1], offset[2]*i + riserStart[2]];
         if (i > 0)
            riserCoords[2] -= treadThickness;
         var axisPlacement2 = ifc.addLine("IFCAXIS2PLACEMENT3D(" + ifc.addLine("IFCCARTESIANPOINT((" + coordsToIFC(riserCoords) + "));") + ",#9," + riserDirection + ");");
         var rise = (i == 0 ? riseZ - treadThickness : riseZ);
         components.push(ifc.addLine("IFCEXTRUDEDAREASOLID(" + rectangleProfile + "," + axisPlacement2 + ",#9," + lenToIFC(rise) + ");"));
      }

      for (var i=0; i<numSteps; i++) {
         addRiser(i);

         // Tread
         var axisPlacement3 = ifc.addLine("IFCAXIS2PLACEMENT2D(" + ifc.addLine("IFCCARTESIANPOINT((0.,0.));") + ",#11);");
         var rectangleProfile2 = ifc.addLine("IFCRECTANGLEPROFILEDEF(.AREA.,'50 mm Tread 10 mm Riser'," + axisPlacement3 + "," + lenToIFC(treadDepth) + "," + lenToIFC(treadWidth) + ");");
         var treadCoords = [offset[0]*i + treadStart[0], offset[1]*i + treadStart[1], offset[2]*i + treadStart[2]];
         var axisPlacement4 = ifc.addLine("IFCAXIS2PLACEMENT3D(" + ifc.addLine("IFCCARTESIANPOINT((" + coordsToIFC(treadCoords) + "));") + ",#9,#6);");
         components.push(ifc.addLine("IFCEXTRUDEDAREASOLID(" + rectangleProfile2 + "," + axisPlacement4 + ",#9," + lenToIFC(treadThickness) + ");"));
      }
      addRiser(numSteps);

      var sweep = ifc.addLine("IFCSHAPEREPRESENTATION(#59,'Body','SweptSolid',(" + components.join(",") + "));");
      var shape = ifc.addLine("IFCPRODUCTDEFINITIONSHAPE($,$,(" + sweep +"));");

      var flight = ifc.addLine("IFCSTAIRFLIGHT(" + newIFCGUID() + ",#20,'Assembled Stair:Stair:2849 Run 1',$,'Assembled Stair:Stairs 1:1918'," + createNullPosition(ifc) + "," + shape + 
         ",'3540'," + (numSteps+1) + "," + numSteps + ",0.588235294117647,0.918635170603675);");

      return flight;
   }

   var addStairs = function(ifc, levelLine, center, direction, isTop) {
      var components = [];
      var width = standardStairsParameters.width - 0.2; // Remove wall width
      var length = standardStairsParameters.length - 0.2;
      var treadThickness = 0.05;
      var riserDepth = 0.01;
      var landingLength = standardStairsParameters.landingLengthRatio * length;

      var flight1Center = [center[0], center[1], center[2]];
      var flight2Center = [center[0], center[1], center[2] + levelHeight / 2];
      var flight2Direction = [direction[0]*-1, direction[1]*-1, direction[2]];
      var midLandingCenter = [center[0], center[1], levelHeight / 2 + center[2] - treadThickness];
      var baseLandingCenter = [center[0], center[1], center[2] - treadThickness];

      if (almostEqual(direction[0], 0)) {
         flight1Center[0] -= width/4 * direction[1];
         flight2Center[0] += width/4 * direction[1];
         midLandingCenter[1] += (length - landingLength) / 2 * direction[1];
         baseLandingCenter[1] -= (length - landingLength) / 2 * direction[1];
      }
      else if (almostEqual(direction[1], 0)) {
         flight1Center[1] -= width/4 * direction[0];
         flight2Center[1] += width/4 * direction[0];
         midLandingCenter[0] += (length - landingLength) / 2 * direction[0];
         baseLandingCenter[0] -= (length - landingLength) / 2 * direction[0];
      }
      else {
         console.log("IFC error: stair flights require an axis-oriented direction");
         return;
      }

      var stairsLine = ifc.addLine("IFCSTAIR(" + newIFCGUID() + ",#20,'Assembled Stair:Stair:2849',$,'Assembled Stair:Stairs 1:1918'," + createNullPosition(ifc) + ",$,'2849',.NOTDEFINED.);");

      // Add to the level
      ifc.addLine("IFCRELCONTAINEDINSPATIALSTRUCTURE(" + newIFCGUID() + ",#20,$,$,(" + stairsLine + ")," + levelLine + ");");

      // Landings
      components.push(addSlab(ifc, midLandingCenter, direction, landingLength + riserDepth, width, treadThickness));
      components.push(addSlab(ifc, baseLandingCenter, direction, landingLength + riserDepth, width, treadThickness));
      if (isTop) {
         var topLandingCenter = [baseLandingCenter[0], baseLandingCenter[1], baseLandingCenter[2]+levelHeight];
         components.push(addSlab(ifc, topLandingCenter, direction, landingLength + riserDepth, width, treadThickness));
      }

      components.push(addStairsFlight(ifc, flight1Center, direction, length - landingLength*2, width/2, treadThickness, riserDepth));
      components.push(addStairsFlight(ifc, flight2Center, flight2Direction, length - landingLength*2, width/2, treadThickness, riserDepth));
      ifc.addLine("IFCRELAGGREGATES(" + newIFCGUID() + ",#20,$,$," + stairsLine + ",(" + components.join(",") + "));");
   }

   var addStraightWall = function(ifc, levelLine, levelNumber, end1, end2) {
      // Add the start point
      var startLine = ifc.addLine("IFCLOCALPLACEMENT(#111," + ifc.addLine("IFCAXIS2PLACEMENT3D(" + ifc.addLine("IFCCARTESIANPOINT((" + coordsToIFC(end1) + "));") + ",$,$);") + ");");

      // Add the length/direction
      var dir = lenToIFC(end2[0] - end1[0]) + "," + lenToIFC(end2[1] - end1[1]);
      var shapeRepLine = ifc.addLine("IFCSHAPEREPRESENTATION(#58,'Axis','Curve2D',(" + ifc.addLine("IFCPOLYLINE((#4," + ifc.addLine("IFCCARTESIANPOINT((" + dir + "));") + "));") + "));");
      ifc.addLine("IFCPRESENTATIONLAYERASSIGNMENT('A-WALL',$,(" + shapeRepLine + ",#142),$);");
      var shapeLine = ifc.addLine("IFCPRODUCTDEFINITIONSHAPE($,$,(" + shapeRepLine + ",#282));");

      var wallLine = ifc.addLine("IFCWALLSTANDARDCASE(" + newIFCGUID() + ",#20,'Basic Wall:Wall 1:2396',$,'Basic Wall:Wall 1:1513'," + startLine + "," + shapeLine + ",'2396');");
      ifc.addLines("IFCRELCONTAINEDINSPATIALSTRUCTURE(" + newIFCGUID() + ",#20,$,$,(" + wallLine + ")," + levelLine + ");\n\
IFCRELASSOCIATESMATERIAL(" + newIFCGUID() + ",#20,$,$,(" + wallLine + "),#110);");

      applyPropertySet(ifc, wallLine, addPropertySet(ifc, "Constraints", "IFCPROPERTYSINGLEVALUE('Base Constraint',$,IFCLABEL('Level: Level" + levelNumber + "'),$);"));
      applyPropertySet(ifc, wallLine, "#130");
      applyPropertySet(ifc, wallLine, "#213");
      applyPropertySet(ifc, wallLine, "#227");
      applyPropertySet(ifc, wallLine, "#262");

      return wallLine;
   }

   return { save: saveIFC};
});