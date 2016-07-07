﻿using System.Collections.Generic;
using stuffer;
using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Interfaces;
using System;
using DSCore;
using Display;
using System.Linq;
using Math = System.Math;

namespace SpacePlanning
{
    /// <summary>
    /// Class to perform spatial analytis to score and appraise space plan layouts.
    /// </summary>
    public static class SpaceAnalysis
    {
        //const colors
        
        

        #region - Public Methods
        //provides information related to dept data
        /// <summary>
        /// Provides analytics on Department data after spaces has been assigned.
        /// </summary>
        /// <param name="deptData">Department data object.</param>
        /// <returns name="DepartmentNames">Name of the departments.</returns>
        /// <returns name="NumCellsTaken">Number of cells assgined to each department.</returns>
        /// <returns name="AreaSatisfied">If Department area is satisfied</returns>
        /// <search>
        /// department analytics
        /// </search>
        [MultiReturn(new[] { "DepartmentNames", "NumCellsTaken", "AreaSatisfied", "AreaNeeded", "AreaProvided", "ProgramsInDepts", "PolyAssignedDepts" })]
        public static Dictionary<string, object> AnalyticsDeptData(List<DeptData> deptData)
        {

            if (deptData == null) return null; 
            List<string> deptNameList = new List<string>();
            List<int> numCellsList = new List<int>();
            List<bool> areaSatisfiedList = new List<bool>();
            List<List<ProgramData>> progLists = new List<List<ProgramData>>();
            List<double> areaNeededList = new List<double>();
            List<double> areaProvidedList = new List<double>();
            List<List<Polygon2d>> polyAssignedList = new List<List<Polygon2d>>();
            for (int i = 0; i < deptData.Count; i++)
            {
                deptNameList.Add(deptData[i].DepartmentName);
                numCellsList.Add(deptData[i].NumCellsInDept);
                areaSatisfiedList.Add(deptData[i].IsAreaSatisfied);
                progLists.Add(deptData[i].ProgramsInDept);
                areaNeededList.Add(deptData[i].DeptAreaNeeded);
                areaProvidedList.Add(deptData[i].DeptAreaProvided);
                polyAssignedList.Add(deptData[i].PolyAssignedToDept);
            }
            return new Dictionary<string, object>
            {
                { "DepartmentNames", (deptNameList) },
                { "NumCellsTaken", (numCellsList) },
                { "AreaSatisfied", (areaSatisfiedList) },
                { "AreaNeeded", (areaNeededList) },
                { "AreaProvided", (areaProvidedList) },
                { "ProgramsInDepts", (progLists) },
                { "PolyAssignedDepts", (polyAssignedList) }
            };
        }
        //Provides information related to program data

        [MultiReturn(new[] { "DisplayGeomList" })]
        public static Dictionary<string, object> VisualizeCirculation(List<Polygon2d> deptCirculationPoly, List<Polygon2d> progCirculationPoly, int height =0)
        {
            List<Color> colorList = new List<Color>();
            colorList.Add(Color.ByARGB(255, 0, 255, 255)); // cyan
            colorList.Add(Color.ByARGB(255, 153, 255, 102)); // fluoro green

            if (!ValidateObject.CheckPolyList(deptCirculationPoly) || !ValidateObject.CheckPolyList(progCirculationPoly)) return null;
            
            List<List<Polygon2d>> polyProgsList = new List<List<Polygon2d>>();
            polyProgsList.Add(deptCirculationPoly);
            polyProgsList.Add(progCirculationPoly);
       
            List<List<Surface>> srfListAll = new List<List<Surface>>();
            List<List<Display.Display>> displayListAll = new List<List<Display.Display>>();
            for (int i = 0; i < polyProgsList.Count; i++)
            {
                List<Polygon2d> polyProgs = polyProgsList[i];
                Color col = colorList[i];
                List<Surface> srfList = new List<Surface>();
                List<Display.Display> displayList = new List<Display.Display>();
                for (int j = 0; j < polyProgs.Count; j++)
                {
                    
                    Polygon2d polyReduced = new Polygon2d(polyProgs[j].Points);
                    List<Point2d> ptList = polyReduced.Points;
                    List<Point> ptNewList = new List<Point>();
                    for (int k = 0; k < ptList.Count; k++) ptNewList.Add(Point.ByCoordinates(ptList[k].X, ptList[k].Y));
                    Surface srf;
                    try { srf = Surface.ByPerimeterPoints(ptNewList); }
                    catch { continue; }                    
                    Geometry gm = srf.Translate(0, 0, height);
                    Display.Display dis = Display.Display.ByGeometryColor(gm, col);
                    displayList.Add(dis);
                    //srfList.Add(srf);                    
                    srf.Dispose();                   
                    ptNewList.Clear();
                    
                }
                displayListAll.Add(displayList);
                //srfListAll.Add(srfList);
            }
            return new Dictionary<string, object>
            {
                { "DisplayGeomList", (displayListAll) }
            };
        }


        //Provides information related to program data

        [MultiReturn(new[] { "DisplayGeomList" })]
        public static Dictionary<string, object> VisualizeDeptPrograms(List<DeptData> deptDataInp, int height = 0, int transparency = 255, int colorScheme = 0)
        {
            if (transparency < 0 || transparency > 255) transparency = 255;
            // hard coded list of colors for 20 depts
            List<Color> colorList = new List<Color>(), colorListSelected = new List<Color>();
            colorList.Add(Color.ByARGB(transparency, 119, 179, 0)); // light green
            colorList.Add(Color.ByARGB(transparency, 255, 51, 204)); // bright pink
            colorList.Add(Color.ByARGB(transparency, 102, 102, 255)); // violetish blue
            colorList.Add(Color.ByARGB(transparency, 255, 195, 77)); // orangish yellow
            colorList.Add(Color.ByARGB(transparency, 204, 153, 255)); // violet blue
            colorList.Add(Color.ByARGB(transparency, 51, 51, 204)); // darker blue
            colorList.Add(Color.ByARGB(transparency, 0,128,0)); // darker green
            colorList.Add(Color.ByARGB(transparency, 98, 98, 98)); // grey dark
            colorList.Add(Color.ByARGB(transparency, 204, 255, 102)); // light green
            colorList.Add(Color.ByARGB(transparency, 255, 51, 153)); // reddish pink
            colorList.Add(Color.ByARGB(transparency, 0, 102, 153)); // teal blue
            colorList.Add(Color.ByARGB(transparency, 153, 0, 204)); // purple
            List<int> indicesList = new List<int>();
            for (int i = 0; i < colorList.Count; i++) indicesList.Add(i);

            if (colorScheme == 0) colorListSelected = colorList;
            else
            {
                Random ran = new Random(colorScheme);
                List<int> indicesRandomList = BasicUtility.RandomizeList(indicesList, ran);
                for(int i = 0; i < colorList.Count; i++) { colorListSelected.Add(colorList[indicesRandomList[i]]); }
            }



            if (deptDataInp == null) return null;
            List<DeptData> deptData = deptDataInp.Select(x => new DeptData(x)).ToList(); // example of deep copy
            List<List<Polygon2d>> polyProgsList = new List<List<Polygon2d>>();
            List<Curve> crvProgs = new List<Curve>();
            for (int i = 0; i < deptData.Count; i++)
            {
                List<ProgramData> progsInDept = deptData[i].ProgramsInDept;
                List<Polygon2d> polyList = new List<Polygon2d>();
                for (int j = 0; j < progsInDept.Count; j++) polyList.AddRange(progsInDept[j].PolyAssignedToProg);
                polyProgsList.Add(polyList);
            }
            List<List<Surface>> srfListAll = new List<List<Surface>>();
            List<List<Display.Display>> displayListAll = new List<List<Display.Display>>();
            for (int i = 0; i < polyProgsList.Count; i++)
            {
                List<Polygon2d> polyProgs = polyProgsList[i];            
               
                int index = i;
                if (index > colorList.Count) index = 0;
                Color col = colorListSelected[index];
                List<Surface> srfList = new List<Surface>();
                List<Display.Display> displayList = new List<Display.Display>();
                for (int j = 0; j < polyProgs.Count; j++)
                {
                    Polygon2d polyReduced = new Polygon2d(polyProgs[j].Points);
                    //polyReduced = PolygonUtility.PolyExtraEdgeRemove(polyReduced);
                    //polyReduced = PolygonUtility.CreateOrthoPoly(polyReduced);
                    List<Point2d> ptList = polyReduced.Points;
                    List<Point> ptNewList = new List<Point>();
                    for (int k = 0; k < ptList.Count; k++) ptNewList.Add(Point.ByCoordinates(ptList[k].X, ptList[k].Y));                    
                    Surface srf;
                    try { srf = Surface.ByPerimeterPoints(ptNewList); }
                    catch { continue; }
         
                    Geometry gm = srf.Translate(0, 0, height);
                    Display.Display dis = Display.Display.ByGeometryColor(gm, col);
                    displayList.Add(dis);
                    //srfList.Add(srf);
                    srf.Dispose();
                    ptNewList.Clear();
                    }
                displayListAll.Add(displayList);
                //srfListAll.Add(srfList);
              
            }
            
            return new Dictionary<string, object>
            {
                { "DisplayGeomList", (displayListAll) }
            };
        }

        //Provides information related to program data
        /// <summary>
        /// Provides analytics on Program data after spaces has been assigned.
        /// </summary>
        /// <param name="deptData">List of Department Data Object</param>
        /// <param name="height">Z height of the geometry returned</param>
        /// <returns name="progPolygons">Polygons representing programs.</returns>
        /// <returns name="progPolyOrigin">Centroid of the polygons representing programs.</returns>
        /// <returns name="progNameAsText">Name of the programs.</returns>
        /// <search>
        /// visualize program polgons, program polylines
        /// </search>
        [MultiReturn(new[] { "progPolygons", "progPolyOrigin", "progNameAsText" })]
        public static Dictionary<string, object> VisualizeProgramPolyLinesAndOrigin(List<DeptData> deptData, double height =0)
        {
            if (deptData == null) return null;
            List<List<List<Polygon>>> polyDeptListMega = new List<List<List<Polygon>>>();
            List<List<List<Point>>> ptDeptListMega = new List<List<List<Point>>>();
            List<List<string>> nameDeptListMega = new List<List<string>>();
            for (int i = 0; i < deptData.Count; i++)
            {
                List<ProgramData> progInDept = deptData[i].ProgramsInDept;
                List<List<Polygon>> polyDeptList = new List<List<Polygon>>();
                List<List<Point>> ptDeptList = new List<List<Point>>();
                List<string> nameDeptList = new List<string>();
                for (int j = 0; j < progInDept.Count; j++)
                {
                    List<Polygon2d> polyProg = progInDept[j].PolyAssignedToProg;
                    List<Point> ptCenterList = new List<Point>();
                    List<Polygon> polyList = new List<Polygon>();
                   
                    for (int k = 0; k < polyProg.Count; k++)
                    {
                        Point2d center2d = PolygonUtility.CentroidOfPoly(polyProg[k]);
                        Point center = Point.ByCoordinates(center2d.X, center2d.Y,height+1);
                        ptCenterList.Add(center);
                        Polygon poly = DynamoGeometry.PolygonByPolygon2d(polyProg[k], height);
                        polyList.Add(poly);
                    }
                    polyDeptList.Add(polyList);
                    ptDeptList.Add(ptCenterList);
                    nameDeptList.Add(progInDept[j].ProgramName);
                }
                polyDeptListMega.Add(polyDeptList);
                ptDeptListMega.Add(ptDeptList);
                nameDeptListMega.Add(nameDeptList);
            }            
            return new Dictionary<string, object>
            {
                { "progPolygons", (polyDeptListMega) },
                { "progPolyOrigin", (ptDeptListMega) },
                { "progNameAsText", (nameDeptListMega) }
            };
        }


        //Provides information related to program data
        /// <summary>
        /// Provides analytics on Program data after spaces has been assigned.
        /// </summary>
        /// <param name="progData">Program data object</param>
        /// <returns name="ProgramNames">Name of the programs.</returns>
        /// <returns name="NumCellsTaken">Number of cells assgined to each program.</returns>
        /// <returns name="AreaSatisfied">Area of the program is satisfied or not</returns>
        /// <search>
        /// program analytics
        /// </search>
        [MultiReturn(new[] { "ProgramNames", "NumCellsTaken", "AreaSatisfied", "AreaNeeded", "AreaProvided", "Quantity", "PolyAssignedProgs" })]
        public static Dictionary<string, object> AnalyticsProgramData(List<ProgramData> progData)
        {
            if (progData == null) return null;

            List<string> progNameList = new List<string>();
            List<int> numCellsList = new List<int>();
            List<bool> areaSatisfiedList = new List<bool>();
            List<double> areaNeededList = new List<double>();
            List<double> areaProvidedList = new List<double>();
            List<double> quantList = new List<double>();
            List<List<Polygon2d>> polyProgList = new List<List<Polygon2d>>();
            for (int i = 0; i < progData.Count; i++)
            {
                progNameList.Add(progData[i].ProgramName);
                numCellsList.Add(progData[i].NumberofCellsAdded);
                areaSatisfiedList.Add(progData[i].IsAreaSatisfied);
                areaNeededList.Add(progData[i].ProgAreaNeeded);
                areaProvidedList.Add(progData[i].ProgAreaProvided);
                quantList.Add(progData[i].Quantity);
                polyProgList.Add(progData[i].PolyAssignedToProg);
            }
            return new Dictionary<string, object>
            {
                { "ProgramNames", (progNameList) },
                { "NumCellsTaken", (numCellsList) },
                { "AreaSatisfied", (areaSatisfiedList) },
                { "AreaNeeded", (areaNeededList) },
                { "AreaProvided", (areaProvidedList) },
                { "Quantity", (quantList) },
                { "PolyAssignedProgs", (polyProgList) }
            };
        }
        
        //Visualizes the space plan scores and displays as text
        /// <summary>
        /// Visualizes space plan scores on the viewport.
        /// </summary>
        /// <param name="totalScore">Total score of the space plan layout.</param>
        /// <param name="programFitScore">Program fitness score of the space plan layout.</param>
        /// <param name="extViewScore">External view score of the space plan layout.</param>
        /// <param name="travelDistScore">Travel distance score of the space plan layout.</param>
        /// <param name="percKPUScore">Percentage KPU score of the space plan layout.</param>
        /// <param name="x">X coordinate of the visualization.</param>
        /// <param name="y">Y coordinate of the visualization.</param>
        /// <param name="spacingX">Spacing in the direction of X axis.</param>
        /// <param name="spacingY">Spacing in the direction of Y axis.</param>
        /// <returns name="TextToWrite">String to visualize.</returns>
        /// <returns name="Points">Point at visualiation.</returns>        
        [MultiReturn(new[] { "TextToWrite", "Points" })]
        public static Dictionary<string, object> SpacePlanFitnessVisualize(double totalScore, double programFitScore,
            double extViewScore, double travelDistScore, double percKPUScore, double x = 0, double y = 0, double spacingX = 10, double spacingY = 10 )
        {
            List<string> textList = new List<string>();
            List<Point> ptList = new List<Point>();
            int num = 5, extra = -10;
            double xDim = x - spacingX, yDim = y;

            totalScore = Math.Round(totalScore, 2);
            programFitScore = Math.Round(programFitScore, 4)*100;
            extViewScore = Math.Round(extViewScore, 4)*100;
            travelDistScore = Math.Round(travelDistScore, 4)*100;
            percKPUScore = Math.Round(percKPUScore, 4)*100;

            for (int i = 0; i < num; i++)
            {
                ptList.Add(Point.ByCoordinates(xDim, yDim));
                if (i == 0) yDim += spacingY + extra;
                else yDim += spacingY;
            }
            xDim = x; yDim = y;
            for (int i=0;i< num; i++)
            {
                ptList.Add(Point.ByCoordinates(xDim, yDim));
                if (i == 0) yDim += spacingY + extra;
                else yDim += spacingY;
            }
            textList.Add("Total Design Score");
            textList.Add("Program Fitted Score");
            textList.Add("External View Score");
            textList.Add("Travel Distance Score");
            textList.Add("KPU Proportion Score");
            textList.Add(totalScore.ToString());
            textList.Add(programFitScore.ToString());
            textList.Add(extViewScore.ToString());
            textList.Add(travelDistScore.ToString());
            textList.Add(percKPUScore.ToString());

            return new Dictionary<string, object>
            {
                { "TextToWrite", (textList) },
                { "Points", (ptList) }
            };
        }
        
        //scores the space plan layout. currently there are four individual scores and total score is the summation of them.
        /// <summary>
        /// Scores the space plan layout based on four key metrics, program fitness score, external view score, travel distance score, percentage of key planning units score.
        /// </summary>
        /// <param name="deptData">Department data object.</param>
        /// <param name="primaryProgPoly">Primary program element polygon2d list.</param>
        /// <param name="cellList">List of cell objects for the building outline.</param>
        /// <param name="siteArea">Area of the site.</param>
        /// <param name="programFitWeight">User assigned weight for program fitness score.</param>
        /// <param name="extViewWeight">User assigned weight for external view score.</param>
        /// <param name="traveDistWeight">User assigned weight for travel distance score.</param>
        /// <param name="percKPUWeight">User assigned weight for percentage of key planning units score.</param>
        /// <returns name="TotalScore">Total score of the space plan layout.</returns>
        /// <returns name="ProgramFitScore">Program fitness score of the space plan layout.</returns>
        /// <returns name="ExtViewKPUScore">External view score of the space plan layout.</returns>
        /// <returns name="TravelDistanceScore">Travel distance score of the space plan layout.</returns>
        /// <returns name="PercentageKPUScore">Percentage KPU score of the space plan layout.</returns>
        /// <returns name="InpatientDeptData">Department data of primary department.</returns>
        /// <search>
        /// space plane scoring, space plan metrics
        /// </search>
        [MultiReturn(new[] { "TotalScore", "ProgramFitScore", "ExtViewKPUScore", "TravelDistanceScore", "PercentageKPUScore","InpatientDeptData"})]
        public static Dictionary<string, object> SpacePlanFitness(List<DeptData> deptData, List<List<Polygon2d>> primaryProgPoly, 
            List<Cell> cellList, double siteArea = 0,  double programFitWeight = 0.6, double extViewWeight = 1, double traveDistWeight = 0.8,
            double percKPUWeight = 0.70)
        {
            if (deptData == null) return null;
            if (cellList == null) return null;
            DeptData inPatientDeptData = deptData[0];
            List<double> testData = new List<double>();
            List<double> inPatientData = new List<double>();
            int totalPatientRoomCount = 0;
            double areaInpatientRooms = 0, percInpatientFromSite = 0, dim = cellList[0].DimX;
            for(int i = 0; i < primaryProgPoly.Count; i++)
            {
                if (!ValidateObject.CheckPolyList(primaryProgPoly[i])) continue;
                for(int j = 0; j < primaryProgPoly[i].Count; j++)
                {
                    if (!ValidateObject.CheckPoly(primaryProgPoly[i][j])) continue;
                    totalPatientRoomCount += 1;
                    areaInpatientRooms += PolygonUtility.AreaPolygon(primaryProgPoly[i][j]);
                }
            }   

            percInpatientFromSite = areaInpatientRooms / (2*siteArea);
            testData.Add(totalPatientRoomCount);
            testData.Add(areaInpatientRooms);
            testData.Add(percInpatientFromSite);
            inPatientData.Add(inPatientDeptData.AreaPercentageAchieved);
            inPatientData.Add(inPatientDeptData.DeptAreaNeeded);
            inPatientData.Add(inPatientDeptData.DeptAreaProvided);          


            Dictionary<string,object> cellNeighborObj = GridObject.BuildCellNeighborMatrix(cellList);
            List<List<int>> cellNeighborMatrix = (List<List<int>>)cellNeighborObj["CellNeighborMatrix"];
            List<Cell> sortedCells = (List<Cell>)cellNeighborObj["SortedCells"];
            List<int> borderCellIndices = GridObject.GetCornerAndEdgeCellId(cellNeighborMatrix);
            List<Point2d> borderPts = new List<Point2d>();
            List<Point2d> polyCenterList = new List<Point2d>();
            for (int i = 0; i < borderCellIndices.Count;i++)
            {
                borderPts.Add(sortedCells[borderCellIndices[i]].CenterPoint);
            }

            List<bool> getsExternalWall = new List<bool>();
            Point2d buildingCenter = PointUtility.CentroidInPointLists(borderPts);
            List<Polygon2d> polyFlatList = PolygonUtility.FlattenPolygon2dList(primaryProgPoly);
            double dimPoly = 0, numTrues = 0, travelDistancePatientRms =0, arbLargeValue = 10000;
            for(int i = 0; i < polyFlatList.Count; i++)
            {
                bool check = false;
                Point2d cenPoly = PointUtility.CentroidInPointLists(polyFlatList[i].Points);
                polyCenterList.Add(cenPoly);
                List<double> spanList = PolygonUtility.GetSpansXYFromPolygon2d(polyFlatList[i].Points);
                if (spanList[0] > spanList[1]) dimPoly = spanList[0];
                else dimPoly = spanList[1];
                double dimAdd = dim + dimPoly;

                travelDistancePatientRms += PointUtility.DistanceBetweenPoints(buildingCenter, cenPoly);

                for (int j = 0; j < borderPts.Count; j++)
                {                    
                    double distToCell = PointUtility.DistanceBetweenPoints(borderPts[j], cenPoly);    
                    if (distToCell <= dimAdd) { check = true; numTrues += 1;  break; }
                }
                getsExternalWall.Add(check);
            }          
            //double programFitWeight = 0.6, extViewWeight = 1, traveDistWeight = 0.8, percKPUWeight = 0.70;
            double programFitScore = 1, extViewScore = 1, travelDistScore = 1, percKPUScore = 1;
            

            for(int i = 0; i < deptData.Count; i++) programFitScore += deptData[i].DeptAreaProportionAchieved;
            programFitScore = programFitScore / deptData.Count;
            extViewScore = numTrues / polyFlatList.Count;
            travelDistScore = travelDistancePatientRms / arbLargeValue;
            percKPUScore = inPatientDeptData.DeptAreaProportionAchieved;
            double totalScore = Math.Round(((programFitWeight * programFitScore + extViewWeight * extViewScore +
                                traveDistWeight * travelDistScore + percKPUWeight * percKPUScore) * 40), 2);
            
            return new Dictionary<string, object>
            {
                { "TotalScore", (totalScore) },
                { "ProgramFitScore", (programFitWeight * programFitScore) },
                { "ExtViewKPUScore", (extViewWeight * extViewScore) },
                { "TravelDistanceScore", (traveDistWeight * travelDistScore) },
                { "PercentageKPUScore", (percKPUWeight * percKPUScore) },
                { "InpatientDeptData", (inPatientData) }
            };
        }

        //scores the space plan layout. currently there are four individual scores and total score is the summation of them.
        /// <summary>
        /// Exports Cell Data in excel format.
        /// </summary>
        /// <param name="deptData">List of Department data object.</param>
        /// <param name="cellList">List of Cell Object.</param>
        /// <param name="cellNeighborMatrix">List of list of integers representing cell neighboring matrix.</param>    
        /// <returns name="CellData">Export of cell data in excel format</returns> 
        /// <search>
        /// export data, cell data
        /// </search>
        public static List<List<string>> ExportCellData(List<DeptData> deptDataInp, List<Cell> cellList, List<List<int>> cellNeighborMatrix)
        {
            

            if (deptDataInp == null) return null;
            if (cellList == null) return null;
            List<DeptData> deptData = deptDataInp.Select(x => new DeptData(x)).ToList(); // example of deep copy

            List<List<string>> dataToWriteList = new List<List<string>>();
            List<string> cellStrings = new List<string>();
            cellStrings.Add("CELL ID");
            cellStrings.Add("CELL TYPE");
            cellStrings.Add("PRORGAM ASSIGNED");
            cellStrings.Add("DEPT ASSIGNED");
            cellStrings.Add("CELL AVAILABILITY");
            dataToWriteList.Add(cellStrings);

            List<Polygon2d> polyList = new List<Polygon2d>();
            List<string> progrNameList = new List<string>(), deptNameList = new List<string>();
            for (int i = 0; i < deptData.Count; i++)
            {
                if (deptData[i].ProgramsInDept == null) continue;
                for (int j = 0; j < deptData[i].ProgramsInDept.Count; j++)
                {
                    polyList.AddRange(deptData[i].ProgramsInDept[j].PolyAssignedToProg);
                    for(int k=0;k< deptData[i].ProgramsInDept[j].PolyAssignedToProg.Count; k++)
                    {
                        progrNameList.Add(deptData[i].ProgramsInDept[j].ProgramName);
                        deptNameList.Add(deptData[i].DepartmentName);
                    }
                    
                    
                }
            }

            List<Polygon2d> polyListinCell = new List<Polygon2d>();
            List<string> progrNameListinCell = new List<string>(), deptNameListinCell = new List<string>();
            List<string> cellIdList = new List<string>(), cellTypeList = new List<string>(), cellAvailList = new List<string>();
            for (int i = 0; i < cellList.Count; i++)
            {
                bool cellAssigned = false;
                List<int> neighborCells = cellNeighborMatrix[i];
                neighborCells.RemoveAll(s => s == -1);
                for (int j =0;j< polyList.Count; j++)
                {
                    if (GraphicsUtility.PointInsidePolygonTest(polyList[j], cellList[i].LeftDownCorner))
                    {
                        progrNameListinCell.Add(progrNameList[j]); // error
                        deptNameListinCell.Add(deptNameList[j]);
                        cellIdList.Add(i.ToString());
                        if (neighborCells.Count == 2) cellTypeList.Add("CornerCell");
                        else if (neighborCells.Count == 3) cellTypeList.Add("EdgeCell");
                        else  cellTypeList.Add("CoreCell");
                        cellList[i].CellAvailable = false;
                        cellAvailList.Add("False");
                        cellAssigned = true;
                        break;
                    }            
                }
                //cell not inside any prorgam
                if (!cellAssigned)
                {
                    progrNameListinCell.Add("Not Assigned");
                    deptNameListinCell.Add("Not Assigned");
                    cellIdList.Add(i.ToString());
                    if (neighborCells.Count == 2) cellTypeList.Add("CornerCell");
                    else if (neighborCells.Count == 3) cellTypeList.Add("EdgeCell");
                    else cellTypeList.Add("CoreCell");
                    cellList[i].CellAvailable = true;
                    cellAvailList.Add("True");
                }
            
            }

            for(int i = 0; i < cellList.Count; i++)
            {
                cellStrings = new List<string>();
                cellStrings.Add(cellIdList[i]);
                cellStrings.Add(cellTypeList[i]);
                cellStrings.Add(progrNameListinCell[i]);
                cellStrings.Add(deptNameListinCell[i]);
                cellStrings.Add(cellAvailList[i]);
                dataToWriteList.Add(cellStrings);
            }
            return dataToWriteList;
        }



        //exports data to excel
        /// <summary>
        /// Exports Program Data in excel format.
        /// </summary>
        /// <param name="deptData"> List of Department Data object.</param>
        /// <returns name="ProgramDataExport">Export of program data in excel format.</returns>
        /// <search>
        /// export data, program data
        /// </search>
        public static List<List<string>> ExportDepartmentProgramData(List<DeptData> deptDataInp)
        {
            if (deptDataInp == null) return null;
            List<DeptData> deptData = deptDataInp.Select(x => new DeptData(x)).ToList(); // example of deep copy

            List<List<string>> dataAll = new List<List<string>>();
            List<string> progStrings = new List<string>();
            progStrings.Add("DEPT NAME");
            progStrings.Add("PROG NAME");
            progStrings.Add("PROG AREA NEEDED");
            progStrings.Add("PROG AREA PROVIDED");
            progStrings.Add("PROG NUM POLYS ASSIGNED");
            progStrings.Add("PROG POLY LENGTH");
            progStrings.Add("PROG POLY WIDTH");
            dataAll.Add(progStrings);
            List<ProgramData> progData = new List<ProgramData>();
            for(int i = 0; i < deptData.Count; i++)
            {
                List<List<string>> dataOut = EachDeptProgramDataExport(deptData[i].ProgramsInDept);
                dataAll.AddRange(dataOut);
            }
            return dataAll;
        }


        //exports data to excel
        /// <summary>
        /// Exports Program Data in excel format.
        /// </summary>
        /// <param name="progDataList"> List of program data object.</param>
        /// <returns name="ProgramData">Export of program data in excel format.</returns>
        /// <search>
        /// export data, program data
        /// </search>
        internal static List<List<string>>EachDeptProgramDataExport(List<ProgramData> progDataListInp)
        {
            if (progDataListInp == null) return null;
            List<ProgramData> progDataList = progDataListInp.Select(x => new ProgramData(x)).ToList(); // example of deep copy

            List<List<string>> dataToWriteList = new List<List<string>>();
            List<string> progStrings = new List<string>();
            progStrings.Add("DEPT NAME");
            progStrings.Add("PROG NAME");
            progStrings.Add("PROG AREA NEEDED");
            progStrings.Add("PROG AREA PROVIDED");
            progStrings.Add("PROG NUM POLYS ASSIGNED");
            progStrings.Add("PROG POLY LENGTH");
            progStrings.Add("PROG POLY WIDTH");
            //dataToWriteList.Add(progStrings);
           
            for (int i = 0; i < progDataList.Count; i++)
            {
               
                if (progDataList[i] == null) continue;
                progStrings = new List<string>();
                progStrings.Add(progDataList[i].DeptName.ToString());
                progStrings.Add(progDataList[i].ProgramName.ToString());
                progStrings.Add(progDataList[i].ProgAreaNeeded.ToString());
                progStrings.Add(progDataList[i].ProgAreaProvided.ToString());
                if(progDataList[i].PolyAssignedToProg == null || progDataList[i].PolyAssignedToProg.Count == 0)
                {
                    progStrings.Add(0.ToString());
                    progStrings.Add("Invalid Length");
                    progStrings.Add("Invalid Width");
                }
                else
                {
                    progStrings.Add(progDataList[i].PolyAssignedToProg.Count.ToString());
                    if(progDataList[i].PolyAssignedToProg.Count == 1)
                    {
                        List<double> spans = PolygonUtility.GetSpansXYFromPolygon2d(progDataList[i].PolyAssignedToProg[0].Points);
                        progStrings.Add(spans[0].ToString());
                        progStrings.Add(spans[1].ToString());
                    }
                    else
                    {
                        progStrings.Add("Many Poly");
                        progStrings.Add("Many Poly");
                    }
                }
                
                dataToWriteList.Add(progStrings);
            }
            progDataList.Clear();
            return dataToWriteList;
        }

        #endregion

    }
}
