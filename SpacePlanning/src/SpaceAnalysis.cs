using System.Collections.Generic;
using stuffer;
using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Geometry;
using System;
//using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;



namespace SpacePlanning
{
    /// <summary>
    /// Class to perform spatial analytis to score and appraise space plan layouts.
    /// </summary>
    public static class SpaceAnalysis
    {
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
        public static Dictionary<string, object> DeptAnalytics(List<DeptData> deptData)
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
        public static Dictionary<string, object> ProgramAnalytics(List<ProgramData> progData)
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
                areaProvidedList.Add(progData[i].AreaProvided);
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
        public static Dictionary<string, object> Visualizer(double totalScore, double programFitScore,
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
        public static Dictionary<string, object> SpacePlanScorer(List<DeptData> deptData, List<List<Polygon2d>> primaryProgPoly, 
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
        public static List<List<string>> CellDataExport(List<DeptData> deptData, List<Cell> cellList, List<List<int>> cellNeighborMatrix)
        {
            if (deptData == null) return null;
            if (cellList == null) return null;

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
                for (int j = 0; j < deptData[i].ProgramsInDept.Count; j++)
                {
                    polyList.AddRange(deptData[i].ProgramsInDept[j].PolyAssignedToProg);
                    progrNameList.Add(deptData[i].ProgramsInDept[j].ProgramName);
                    deptNameList.Add(deptData[i].DepartmentName);
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
                        progrNameListinCell.Add(progrNameList[j]);
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
        /// <param name="progDataList"> List of program data object.</param>
        /// <returns name="ProgramData">Export of program data in excel format.</returns>
        /// <search>
        /// export data, program data
        /// </search>
        public static List<List<string>>ProgramDataExport(List<ProgramData> progDataList)
        {
            if (progDataList == null) return null;
            List<List<string>> dataToWriteList = new List<List<string>>();
            List<string> progStrings = new List<string>();
            progStrings.Add("DEPT NAME");
            progStrings.Add("PROG NAME");
            progStrings.Add("PROG AREA NEEDED");
            progStrings.Add("PROG AREA PROVIDED");
            progStrings.Add("PROG NUM POLYS ASSIGNED");
            progStrings.Add("PROG POLY LENGTH");
            progStrings.Add("PROG POLY WIDTH");
            dataToWriteList.Add(progStrings);
           
            for (int i = 0; i < progDataList.Count; i++)
            {
               
                if (progDataList[i] == null) continue;
                progStrings = new List<string>();
                progStrings.Add(progDataList[i].DeptName.ToString());
                progStrings.Add(progDataList[i].ProgramName.ToString());
                progStrings.Add(progDataList[i].ProgAreaNeeded.ToString());
                progStrings.Add(progDataList[i].AreaProvided.ToString());
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
