using System.Collections.Generic;
using stuffer;
using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Geometry;
using System;

namespace SpacePlanning
{
    public class SpaceAnalysis
    {
        #region - Public Methods
        //provides information related to dept data
        [MultiReturn(new[] { "DepartmentNames", "NumCellsTaken", "AreaSatisfied", "AreaNeeded", "AreaProvided", "ProgramsInDepts", "PolyAssignedDepts" })]
        public static Dictionary<string, object> DeptAnalytics(List<DeptData> deptData)
        {

            if (deptData == null) return null; // throw new ArgumentNullException("deptData", "You must supply valid Department Data.");
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
                numCellsList.Add(deptData[i].NumberofCellsAdded);
                areaSatisfiedList.Add(deptData[i].IsAreaSatisfied);
                progLists.Add(deptData[i].ProgramsInDept);
                areaNeededList.Add(deptData[i].DeptAreaNeeded);
                areaProvidedList.Add(deptData[i].AreaProvided);
                polyAssignedList.Add(deptData[i].PolyDeptAssigned);
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

        //Pprovides information related to program data
        [MultiReturn(new[] { "ProgramNames", "NumCellsTaken", "AreaSatisfied", "AreaNeeded", "AreaProvided", "Quantity", "PolyAssignedProgs" })]
        public static Dictionary<string, object> ProgramAnalytics(List<ProgramData> progData)
        {
            if (progData == null) return null;// throw new ArgumentNullException("progData", "You must supply valid Program Data.");

            List<string> progNameList = new List<string>();
            List<int> numCellsList = new List<int>();
            List<bool> areaSatisfiedList = new List<bool>();
            List<double> areaNeededList = new List<double>();
            List<double> areaProvidedList = new List<double>();
            List<double> quantList = new List<double>();
            List<List<Polygon2d>> polyProgList = new List<List<Polygon2d>>();
            for (int i = 0; i < progData.Count; i++)
            {
                progNameList.Add(progData[i].ProgName);
                numCellsList.Add(progData[i].NumberofCellsAdded);
                areaSatisfiedList.Add(progData[i].IsAreaSatisfied);
                areaNeededList.Add(progData[i].AreaNeeded);
                areaProvidedList.Add(progData[i].AreaProvided);
                quantList.Add(progData[i].Quantity);
                polyProgList.Add(progData[i].PolyProgAssigned);
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


        //Visualizes the scores and displays as text
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


        //scores the design, currently there are four individual scores and total score as summation
        [MultiReturn(new[] { "TotalScore", "ProgramFitScore", "ExtViewKPUScore",
            "TravelDistanceScore", "PercentageKPUScore", "TestBorderCells","TestBorderPts",
            "TestData", "InpatientDeptInfo", "ExternalWallCheck", "PolyFlatList", "PolyCenterPts"})]
        public static Dictionary<string, object> ScoreSpacePlan(List<DeptData> deptData, List<List<Polygon2d>> inPatientPoly, 
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
            for(int i = 0; i < inPatientPoly.Count; i++)
            {
                if (!PolygonUtility.CheckPolyList(inPatientPoly[i])) continue;
                for(int j = 0; j < inPatientPoly[i].Count; j++)
                {
                    if (!PolygonUtility.CheckPoly(inPatientPoly[i][j])) continue;
                    totalPatientRoomCount += 1;
                    areaInpatientRooms += PolygonUtility.AreaCheckPolygon(inPatientPoly[i][j]);
                }
            }

    

            percInpatientFromSite = areaInpatientRooms / (2*siteArea);
            testData.Add(totalPatientRoomCount);
            testData.Add(areaInpatientRooms);
            testData.Add(percInpatientFromSite);
            inPatientData.Add(inPatientDeptData.AreaPercentageAchieved);
            inPatientData.Add(inPatientDeptData.DeptAreaNeeded);
            inPatientData.Add(inPatientDeptData.AreaProvided);
            



            Dictionary<string,object> cellNeighborObj = GridObject.FormsCellNeighborMatrix(cellList);
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
            Point2d buildingCenter = GraphicsUtility.CentroidInPointLists(borderPts);
            List<Polygon2d> polyFlatList = GraphicsUtility.FlattenPolygon2dList(inPatientPoly);
            double dimPoly = 0, numTrues = 0, travelDistancePatientRms =0, arbLargeValue = 10000;
            for(int i = 0; i < polyFlatList.Count; i++)
            {
                bool check = false;
                Point2d cenPoly = GraphicsUtility.CentroidInPointLists(polyFlatList[i].Points);
                polyCenterList.Add(cenPoly);
                List<double> spanList = PolygonUtility.GetSpansXYFromPolygon2d(polyFlatList[i].Points);
                if (spanList[0] > spanList[1]) dimPoly = spanList[0];
                else dimPoly = spanList[1];
                double dimAdd = dim + dimPoly;

                travelDistancePatientRms += GraphicsUtility.DistanceBetweenPoints(buildingCenter, cenPoly);

                for (int j = 0; j < borderPts.Count; j++)
                {                    
                    double distToCell = GraphicsUtility.DistanceBetweenPoints(borderPts[j], cenPoly);    
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
                { "TestBorderCells", (borderCellIndices) },
                { "TestBorderPts", (borderPts) },
                { "TestData", (testData) },
                { "InpatientDeptInfo", (inPatientData) },
                { "ExternalWallCheck", (getsExternalWall) },
                { "PolyFlatList", (polyFlatList) },
                { "PolyCenterPts", (buildingCenter) }
            };
        }



        #endregion

    }
}
