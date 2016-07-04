using System;
using System.Collections.Generic;
using stuffer;
using Autodesk.DesignScript.Runtime;
using System.Diagnostics;
using Autodesk.DesignScript.Geometry;
using System.Linq;

namespace SpacePlanning
{
    /// <summary>
    /// Builds department and programs polygons based on input contextual data.
    /// </summary>
    public static class BuildLayout
    {
        
        internal static double SPACING = 20; //higher value makes code faster, 6, 10 was good too
        internal static double SPACING2 = 20;
        internal static Random RANGENERATE = new Random();
        internal static double RECURSE = 0;
        internal static Point2d REFERENCEPOINT = new Point2d(0,0);
        internal static int MAXCOUNT = 3;

        internal const string KPU = "kpu";
        internal const string REG = "regular";

        #region - Public Methods



        //arranges depts on site and updates dept data object
        /// <summary>
        /// Arranges dept on site by assigning polygon2d's to each dept in the Dept Data object.
        /// Returns Dept polygon2d's, Left Over polygon2d's, Circulation polygon2d's and Updated Dept Data object.
        /// </summary>
        /// <param name="deptData">List of DeptData object.</param>
        /// <param name="buildingOutline">Building outline polygon2d geometry.</param>
        /// <param name="primaryDeptDepth">Depth in feet of the main department.</param>
        /// <param name="acceptableWidth">Acceptable width in meters while allocating area and polygon2d to each dept on site.</param>
        /// <param name="minNotchDistance">Minimum distance below which an edge will be considered as a removable notch.</param>
        /// <param name="circulationFreq">Value to consider while checking frequency of cirulation computation polygon2d.</param>
        /// <param name="recompute">Regardless of the recompute value, it is used to restart computing the node every time its value is changed.</param>
        /// <param name="randomToggle">Boolean toggle to turn on or off randomness to assign departments. Default is off.</param>
        /// <returns name="UpdatedDeptData">Updated Dept Data object</returns>
        /// <returns name="LeftOverPolys">Polygon2d's not assigned to any department.</returns>
        /// <returns name="CirculationPolys">Polygon2d's needed to compute circulation networks.</returns>
        /// <returns name="OtherDeptMainPoly">Polygon2d for all other departments except for the primary department.</returns>
        /// <search>
        /// DeptData object, department arrangement on site
        /// </search>
        [MultiReturn(new[] { "UpdatedDeptData","LeftOverPolys", "CirculationPolys","OtherDeptMainPoly"})]
        public static Dictionary<string, object> PlaceDepartments(List<DeptData> deptDataInp, List<Polygon2d> buildingOutline,  double kpuDepth, 
            double acceptableWidth,double circulationFreq = 8, int iteration = 50, bool noExternalWall = false, bool unlimitedKPU = true)
        {
            List<DeptData> deptData = deptDataInp.Select(x => new DeptData(x)).ToList(); // example of deep copy

            Dictionary<string, object> deptArrangement = new Dictionary<string, object>();
            double count = 0,eps = 5;            
            Random rand = new Random();
            bool deptPlaced = false;
            while (deptPlaced == false && count < MAXCOUNT)//MAXCOUNT
            {
                Trace.WriteLine("PLACE DEPT STARTS , Lets arrange dept again ++++++++++++++++ : " + count);
                deptArrangement = DeptPlacer(deptData, buildingOutline, kpuDepth, acceptableWidth, circulationFreq, iteration, noExternalWall, unlimitedKPU);
                if(deptArrangement != null)
                {
                    List<DeptData> deptDataUpdated =(List<DeptData>) deptArrangement["UpdatedDeptData"];
                    List<List<Polygon2d>> deptAllPolys = new List<List<Polygon2d>>();
                    for(int i = 0; i < deptDataUpdated.Count; i++) deptAllPolys.Add(deptDataUpdated[i].PolyAssignedToDept);
                    List<Polygon2d> deptPolysTogether = new List<Polygon2d>();
                    for (int i = 0; i < deptAllPolys.Count; i++)
                    {
                        if(ValidateObject.CheckPolyList(deptAllPolys[i])) deptPolysTogether.AddRange(deptAllPolys[i]);
                    }

                    if(deptAllPolys.Count>0) Trace.WriteLine("dept arrangement not null, lets check further");
                    for (int i = 0; i < deptAllPolys.Count; i++)
                    {                       
                        List<Polygon2d> eachDeptPoly = deptAllPolys[i];
                        if (ValidateObject.CheckPolyList(eachDeptPoly)) deptPlaced = true;
                        else { deptPlaced = false; Trace.WriteLine("dept arrangement bad polys, rejected"); break; }
                        bool orthoResult = ValidateObject.CheckPolygon2dListOrtho(deptPolysTogether, eps);
                        Trace.WriteLine("The poly formed is : " + orthoResult);
                        if (orthoResult) deptPlaced = true;
                        else { deptPlaced = false; Trace.WriteLine("dept arrangement non orthogonal, rejected"); break; }
                    }               
                }
                else
                {
                    deptPlaced = false;
                    iteration += 1;
                    Trace.WriteLine("DeptPlacer returned null, rejected for: " + count);
                }
                count += 1;
                Trace.WriteLine(" EXIT PLACE DEPARTMENTS +++++++++++++++++++++++++++++++++");
            }// end of while loop
            return deptArrangement;
        }


       
        
        //arranges program elements inside primary dept unit and updates program data object
        /// <summary>
        /// Assigns program elements inside the primary department polygon2d.
        /// </summary>
        /// <param name="deptPoly">Polygon2d's of primary department which needs program arrangement inside.</param>
        /// <param name="progData">Program Data object</param>
        /// <param name="primaryProgramWidth">Width of the primary program element in  department.</param>
        /// <param name="recompute">Regardless of the recompute value, it is used to restart computing the node every time it's value is changed.</param>
        /// <returns name="PolyAfterSplit">Polygon2d's obtained after assigning programs inside the department.</returns>
        /// <returns name="UpdatedProgramData">Updated program data object.</returns>
        /// <returns name="ProgramsAddedCount">Number of program units added.</returns>
        [MultiReturn(new[] { "UpdatedProgramData", "ProgramsAddedCount" })]
        internal static Dictionary<string, object> PlaceKPUPrograms(List<Polygon2d> deptPoly, List<ProgramData> progData, double primaryProgramWidth, int recompute = 1, int space = 10)
        {

            if (!ValidateObject.CheckPolyList(deptPoly)) return null;
            if (progData == null || progData.Count == 0) return null;
            int roomCount = 0;
            List<Polygon2d> polyList = new List<Polygon2d>();
            List<Point2d> pointsList = new List<Point2d>();
            Queue<ProgramData> programDataRetrieved = new Queue<ProgramData>();
            List<ProgramData> progDataAddedList = new List<ProgramData>();
            ProgramData copyProgData = new ProgramData(progData[0]);

            for (int i = 0; i < progData.Count; i++) programDataRetrieved.Enqueue(progData[i]);
            for (int i = 0; i < deptPoly.Count; i++)
            {
                Polygon2d poly = deptPoly[i];
                if (!ValidateObject.CheckPoly(poly)) continue;
                int dir = 0, count = 0,lineId =0;

                List<double> spans = PolygonUtility.GetSpansXYFromPolygon2d(poly.Points);
                double setSpan = 1000000000000, fac = 1.1;
                if (spans[0] > spans[1]) { setSpan = spans[0]; dir = 1; } // poly is horizontal, dir should be 1
                else { setSpan = spans[1]; dir = 0; }// poly is vertical, dir should be 0
                Polygon2d currentPoly = poly;
                List<Polygon2d> polyAfterSplitting = new List<Polygon2d>();
                ProgramData progItem = new ProgramData(progData[0]);
                Point2d centerPt = PolygonUtility.CentroidOfPoly(currentPoly);

                int lineOrient = ValidateObject.CheckLineOrient(currentPoly.Lines[0]);
                if (lineOrient == dir) lineId = 0;
                else lineId = 1;

                while (setSpan > primaryProgramWidth && count < 2000)
                {
                    if (programDataRetrieved.Count == 0) programDataRetrieved.Enqueue(copyProgData);
                    //Trace.WriteLine("Keep going : " + count);
                    double dist = 0;
                    if (setSpan < fac * primaryProgramWidth)
                    {
                        progItem = programDataRetrieved.Dequeue();
                        progItem.ProgAreaProvided = PolygonUtility.AreaPolygon(currentPoly);
                        polyList.Add(currentPoly);
                        progDataAddedList.Add(progItem);
                        count += 1;
                        break;
                    }
                    else dist = primaryProgramWidth;
           
                    Dictionary<string, object> splitReturn = SplitObject.SplitByOffsetFromLine(currentPoly, lineId, dist, 10);
                    if(splitReturn != null)
                    {
                        polyAfterSplitting.Clear();
                        Polygon2d polyA = (Polygon2d)splitReturn["PolyAfterSplit"];
                        Polygon2d polyB = (Polygon2d)splitReturn["LeftOverPoly"];
                        polyAfterSplitting.Add(polyA); polyAfterSplitting.Add(polyB);
                        progItem = programDataRetrieved.Dequeue();
                        progItem.ProgAreaProvided = PolygonUtility.AreaPolygon(polyAfterSplitting[0]);
                        polyList.Add(polyAfterSplitting[0]);
                        currentPoly = polyAfterSplitting[1];
                        setSpan -= dist;
                        progDataAddedList.Add(progItem);
                        count += 1;
                    }          
                }// end of while
                //add the last left over poly for each dept poly
                if (polyAfterSplitting.Count > 0)
                {
                    polyList.Add(polyAfterSplitting[1]);
                    progItem = copyProgData;
                    progItem.ProgAreaProvided = PolygonUtility.AreaPolygon(polyAfterSplitting[1]);
                    progDataAddedList.Add(progItem);
                    count += 1;
                }
            }// end of for loop

            roomCount = progDataAddedList.Count;
            List<ProgramData> UpdatedProgramDataList = new List<ProgramData>();
            for (int i = 0; i < progDataAddedList.Count; i++) //progData.Count
            {
                ProgramData progItem = progDataAddedList[i];
                ProgramData progNew = new ProgramData(progItem);
                if (i < polyList.Count) progNew.PolyAssignedToProg = new List<Polygon2d> { polyList[i] };
                else progNew.PolyAssignedToProg = null;
                UpdatedProgramDataList.Add(progNew);
            }
            List<Polygon2d> cleanPolyList = ValidateObject.CheckAndCleanPolygon2dList(polyList);
            return new Dictionary<string, object>
            {
                { "UpdatedProgramData",(UpdatedProgramDataList) },
                { "ProgramsAddedCount" , (roomCount) }
            };
        }






        //arranges program elements inside secondary dept units and updates program data object
        /// <summary>
        /// Assigns program elements inside the secondary department polygon2d.
        /// </summary>
        /// <param name="deptData">Dept Data object.</param>
        /// <param name="recompute">This value is used to restart computing the node every time its value is changed.</param>
        /// <returns></returns>
        [MultiReturn(new[] { "PolyAfterSplit", "UpdatedProgramData" })]
        public static Dictionary<string, object> PlaceREGPrograms(DeptData deptDataInp,int recompute = 0,bool extra = true)
        {
            if (deptDataInp == null) return null;

            DeptData deptData = new DeptData(deptDataInp);
            List<Polygon2d> deptPoly = deptData.PolyAssignedToDept;
            List<ProgramData> progData = deptData.ProgramsInDept;
            if (!ValidateObject.CheckPolyList(deptPoly)) return null;
            if (progData == null || progData.Count == 0) return null;
            List<List<Polygon2d>> polyList = new List<List<Polygon2d>>();
            List<Polygon2d> polyCoverList = new List<Polygon2d>();
            //Stack<ProgramData> programDataRetrieved = new Stack<ProgramData>();
            Stack<Polygon2d> polygonAvailable = new Stack<Polygon2d>();            
            for (int j = 0; j < deptPoly.Count; j++) { polygonAvailable.Push(deptPoly[j]); }
            double areaAssigned = 0, eps = 50;
            int count = 0, maxTry = 100;
            for(int i = 0; i < progData.Count; i++)
            {
                ProgramData progItem = progData[i];
                progItem.PolyAssignedToProg = new List<Polygon2d>();
                double areaNeeded = progItem.ProgAreaNeeded;
                while (areaAssigned < areaNeeded && polygonAvailable.Count > 0)// && count < maxTry
                {
                    Polygon2d currentPoly = polygonAvailable.Pop();
                    double areaPoly = PolygonUtility.AreaPolygon(currentPoly);
                    int compareArea = BasicUtility.CheckWithinRange(areaNeeded, areaPoly, eps);
                    if (compareArea == 1) // current poly area is more =  compareArea == 1
                    {
                        Dictionary<string,object> splitObj = SplitObject.SplitByRatio(currentPoly, 0.5);
                        if (splitObj != null)
                        {
                            List<Polygon2d> polyAfterSplit = (List<Polygon2d>)splitObj["PolyAfterSplit"];
                            for (int j = 0; j < polyAfterSplit.Count; j++) polygonAvailable.Push(polyAfterSplit[j]);
                            count += 1;
                            continue;
                        }                        
                    }
                    //area within range
                    progItem.PolyAssignedToProg.Add(currentPoly);
                    areaAssigned += areaPoly;                
                    count += 1;
                }// end of while

              
                polyList.Add(progItem.PolyAssignedToProg);
                progItem.ProgAreaProvided = areaAssigned;
                count = 0;
                areaAssigned = 0;
            }// end of for loop


            /*
            // do the following if there is still vacant space left in the deptdata
            List<ProgramData> fakeProgList = new List<ProgramData>();
            while (polygonAvailable.Count > 0 && extra)
            {
                Trace.WriteLine("Filling fake program in , empty poly left =  " + polygonAvailable.Count);
                ProgramData dummyProg = new ProgramData(progData[0]);
                dummyProg.PolyAssignedToProg = new List<Polygon2d>();
                double areaNeeded = progData[0].ProgAreaNeeded;
                areaAssigned = 0;
                while (areaAssigned < areaNeeded && polygonAvailable.Count > 0)// && count < maxTry
                {
                    Polygon2d currentPoly = polygonAvailable.Pop();
                    double areaPoly = PolygonUtility.AreaPolygon(currentPoly);
                    int compareArea = BasicUtility.CheckWithinRange(areaNeeded, areaPoly, eps);
                    if (compareArea == 1) // current poly area is more
                    {
                        Dictionary<string, object> splitObj = SplitObject.SplitByRatio(currentPoly, 0.5);
                        if (splitObj != null)
                        {
                            List<Polygon2d> polyAfterSplit = (List<Polygon2d>)splitObj["PolyAfterSplit"];
                            for (int j = 0; j < polyAfterSplit.Count; j++) polygonAvailable.Push(polyAfterSplit[j]);
                            count += 1;
                            continue;
                        }

                    }// end of if loop
                    dummyProg.PolyAssignedToProg.Add(currentPoly);
                    dummyProg.ProgramName = "Dummy Fake Program";
                    areaAssigned += areaPoly;
                    count += 1;
                }// end of while
                
                fakeProgList.Add(dummyProg);
                polyList.Add(dummyProg.PolyAssignedToProg);
                dummyProg.ProgAreaProvided = areaAssigned;
                
            } // end of while



            if(extra) progData.AddRange(fakeProgList);
            List<ProgramData> newProgDataList = new List<ProgramData>();
            // int programID,string programName,string programDept,
            //int programQuant,double programUnitArea, int programPrefValue, List< string > programAdjList,List<Cell> programCell, double dimX, double dimY, string progType
            for (int i = 0; i < progData.Count; i++)
            {
                ProgramData progNew = new ProgramData(progData[i].ProgID, progData[i].ProgramName, progData[i].DeptName, progData[i].Quantity, progData[i].UnitArea,
                progData[i].ProgPreferenceVal, progData[i].ProgAdjList, progData[i].ProgCell, progData[i].DimX, progData[i].DimY, progData[i].ProgramType);

                progNew.NumberofCellsAdded = progData[i].NumberofCellsAdded;
                progNew.ProgAreaProvided = progData[i].ProgAreaProvided;
                progNew.IsAreaSatisfied = progData[i].IsAreaSatisfied;
                progNew.PolyAssignedToProg = progData[i].PolyAssignedToProg;
                newProgDataList.Add(progNew);
            }

            */


            //for(int i = 0; i < progData.Count; i++) progData[i].PolyAssignedToProg = polyList[i];

            List<ProgramData> newProgDataList = new List<ProgramData>();
            for (int i = 0; i < progData.Count; i++)
            {
                newProgDataList.Add(new ProgramData(progData[i]));
            }
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyList) },
                { "UpdatedProgramData",(newProgDataList) }
            };
        }


        [MultiReturn(new[] { "UpdatedDeptData" })]
        public static Dictionary<string,object> RegProgPlacerTest(List<DeptData> deptData)
        {
            if (deptData == null) return null;
            List<List<Polygon2d>> polyPorgsAdded = new List<List<Polygon2d>>();
            List<ProgramData> progDataNew = new List<ProgramData>();
            for (int i = 0; i < deptData.Count; i++)
            {              
                    Dictionary<string, object> placedSecondaryProg = PlaceREGPrograms(deptData[i], i);
                    if (placedSecondaryProg != null) deptData[i].ProgramsInDept = (List<ProgramData>)placedSecondaryProg["UpdatedProgramData"];
                    else deptData[i].ProgramsInDept = null;            

            }
            List<DeptData> newDeptData = new List<DeptData>();
            for (int i = 0; i < deptData.Count; i++) newDeptData.Add(new DeptData(deptData[i]));
            return new Dictionary<string, object>
            {
                { "UpdatedDeptData",(newDeptData) }
            };
        }



        //arranges program elements inside secondary dept units and updates program data object
        /// <summary>
        /// Assigns program elements inside the secondary department polygon2d.
        /// </summary>
        /// <param name="deptData">List of Department Data Objects.</param>
        /// <param name="primaryProgramWidth">Width of the program poly in the primary department</param>
        /// <param name="recompute">Regardless of the recompute value, it is used to restart computing the node every time its value is changed.</param>
        /// <returns></returns>
        [MultiReturn(new[] { "UpdatedDeptData" })]
        public static Dictionary<string, object> PlacePrograms(List<DeptData> deptDataInp, double primaryProgramWidth = 30, int recompute = 0)
        {
            if (deptDataInp == null) return null;
            List<DeptData> deptData = deptDataInp.Select(x => new DeptData(x)).ToList(); // example of deep copy
            List<List<Polygon2d>> polyPorgsAdded = new List<List<Polygon2d>>();
            List<ProgramData> progDataNew = new List<ProgramData>();
            for(int i = 0; i < deptData.Count; i++)
            {
                if (i == 0)
                {                    
                    Dictionary<string, object> placedPrimaryProg = PlaceKPUPrograms(deptData[i].PolyAssignedToDept, deptData[i].ProgramsInDept, primaryProgramWidth, recompute);
                    deptData[i].ProgramsInDept = (List<ProgramData>)placedPrimaryProg["UpdatedProgramData"];
                }
                else
                {
                    Dictionary<string, object> placedSecondaryProg = PlaceREGPrograms(deptData[i], recompute);
                    if (placedSecondaryProg != null) deptData[i].ProgramsInDept = (List<ProgramData>)placedSecondaryProg["UpdatedProgramData"];
                    else deptData[i].ProgramsInDept = null;
                }
              
            }
            List<DeptData> newDeptData = new List<DeptData>();
            for(int i = 0; i < deptData.Count; i++) newDeptData.Add(new DeptData(deptData[i]));
            return new Dictionary<string, object>
            {
                { "UpdatedDeptData",(newDeptData) }
            };
        }



        #endregion

        
        #region - Private Methods  

        [MultiReturn(new[] { "DeptPoly", "LeftOverPoly", "AllPolys", "AreaAdded", "AllNodes" })]
        internal static Dictionary<string, object> AssignBlocksBasedOnRatio(double areaFactor, double areaAvailable, List<Polygon2d> polyList, double acceptableWidth = 10, double ratio = 0.5)
        {
            //if (!ValidateObject.CheckPolyList(polyList)) return null;           
            //for (int i = 0; i < polyList.Count; i++) areaAvailable += PolygonUtility.AreaPolygon(polyList[i]);
            Queue<Polygon2d> polyAvailable = new Queue<Polygon2d>();
            List<Polygon2d> polysToDept = new List<Polygon2d>(), leftOverPoly = new List<Polygon2d>();
            for (int i = 0; i < polyList.Count; i++) polyAvailable.Enqueue(polyList[i]);
            double deptAreaTarget = areaFactor * areaAvailable, areaAssigned = 0;
            //deptAreaTarget = areaFactor;
            //double deptAreaTarget = deptItem.DeptAreaNeeded,areaAssigned = 0;
            while (areaAssigned < deptAreaTarget && polyAvailable.Count > 0)
            {
                Polygon2d currentPoly = polyAvailable.Dequeue();
                areaAssigned += PolygonUtility.AreaPolygon(currentPoly);
                polysToDept.Add(currentPoly);
            }
            return new Dictionary<string, object>
            {
                { "DeptPoly", (polysToDept) },
                { "LeftOverPoly", (polyAvailable.ToList()) },
                { "AllPolys", (polyList)},
                { "AreaAdded", (areaAssigned) },
                { "AllNodes", (null)}
            };
        }



        [MultiReturn(new[] { "DeptPoly", "LeftOverPoly", "AllPolys", "AreaAdded", "AllNodes" })]
        internal static Dictionary<string, object> AssignBlocksBasedOnRatioTest(double deptAreaTarget, List<Polygon2d> polyList, double acceptableWidth = 10, double ratio = 0.5)
        {
            if (!ValidateObject.CheckPolyList(polyList))
            {
                //Trace.WriteLine("Poly given is null"); 
                return null;
            }           
            //for (int i = 0; i < polyList.Count; i++) areaAvailable += PolygonUtility.AreaPolygon(polyList[i]);
            Queue<Polygon2d> polyAvailable = new Queue<Polygon2d>();
            List<Polygon2d> polysToDept = new List<Polygon2d>(), leftOverPoly = new List<Polygon2d>();
            for (int i = 0; i < polyList.Count; i++) polyAvailable.Enqueue(polyList[i]);
            //double deptAreaTarget = areaFactor * areaAvailable, areaAssigned = 0;
            //deptAreaTarget = areaFactor;
            //double deptAreaTarget = deptItem.DeptAreaNeeded,
            double areaAssigned = 0;
            while (areaAssigned < deptAreaTarget && polyAvailable.Count > 0)
            {
                Polygon2d currentPoly = polyAvailable.Dequeue();
                areaAssigned += PolygonUtility.AreaPolygon(currentPoly);
                polysToDept.Add(currentPoly);
            }
            return new Dictionary<string, object>
            {
                { "DeptPoly", (polysToDept) },
                { "LeftOverPoly", (polyAvailable.ToList()) },
                { "AllPolys", (polyList)},
                { "AreaAdded", (areaAssigned) },
                { "AllNodes", (null)}
            };
        }

        //blocks are assigne based on offset distance, used for inpatient blocks
        [MultiReturn(new[] { "PolyAfterSplit", "LeftOverPoly", "AreaAssignedToBlock", "FalseLines", "LineOptions", "PointAdded" })]
        internal static Dictionary<string, object> AssignBlocksBasedOnDistance(List<Polygon2d> polyList, double distance = 16, double area = 0, double thresDistance = 10, int iteration = 5, bool noExternalWall = false)
        {

            if (!ValidateObject.CheckPolyList(polyList)) return null;
            if (distance < 1) return null;
            PriorityQueue<double, Polygon2d> priorityPolyQueue = new PriorityQueue<double, Polygon2d>();
            List<Polygon2d> blockPolyList = new List<Polygon2d>();
            List<Polygon2d> leftoverPolyList = new List<Polygon2d>();
            List<Line2d> falseLines = new List<Line2d>();
            List<Line2d> lineOptions = new List<Line2d>();
            Stack<Polygon2d> polyLeftList = new Stack<Polygon2d>();
            double areaAdded = 0;
            Point2d pointAdd = new Point2d(0, 0);
            //if (area == 0) area = 0.8 * PolygonUtility.AreaPolygon(poly);
            for (int i = 0; i < polyList.Count; i++)
            {
                double areaPoly = PolygonUtility.AreaPolygon(polyList[i]); // negated to make sorted dictionary store in negative 
                priorityPolyQueue.Enqueue(-1 * areaPoly, polyList[i]);
            }
            for (int i = 0; i < polyList.Count; i++)
            {
                if (areaAdded > area) break;
                Polygon2d poly = polyList[i];            
                int count = 0, maxTry = 100;
                poly = new Polygon2d(poly.Points);                
                // if (externalInclude) area = 0.25*area;
                polyLeftList.Push(poly);   
                bool error = false;
                //int number = 4;
                int number = (int)BasicUtility.RandomBetweenNumbers(new Random(iteration), 7, 4);
                //while starts
                while (polyLeftList.Count > 0 && areaAdded < area) //count<recompute count < maxTry
                {
                    error = false;
                    Polygon2d currentPoly = polyLeftList.Pop();
                    Polygon2d tempPoly = new Polygon2d(currentPoly.Points, 0);
                    Dictionary<string, object> splitObject = CreateBlocksByLines(currentPoly, poly, distance, thresDistance, noExternalWall);
                    if (splitObject == null) { count += 1; Trace.WriteLine("Split errored"); continue; }
                    Polygon2d blockPoly = (Polygon2d)splitObject["PolyAfterSplit"];
                    Polygon2d leftPoly = (Polygon2d)splitObject["LeftOverPoly"];
                    lineOptions = (List<Line2d>)splitObject["LineOptions"];
                    Dictionary<string, object> addPtObj = LayoutUtility.AddPointToFitPoly(leftPoly, poly, distance, thresDistance, iteration);
                    leftPoly = (Polygon2d)addPtObj["PolyAddedPts"];
                    falseLines = (List<Line2d>)addPtObj["FalseLineList"];
                    pointAdd = (Point2d)addPtObj["PointAdded"];
                    areaAdded += PolygonUtility.AreaPolygon(blockPoly);
                    polyLeftList.Push(leftPoly);
                    blockPolyList.Add(blockPoly);
                    count += 1;
                    if (lineOptions.Count == 0) error = true;
                    else
                    {
                        for (int j = 0; j < lineOptions.Count; j++)
                        {
                            if (lineOptions[j].Length > thresDistance) { error = false; break; }
                            else error = true;
                        }
                    }
                    if (error) break;
                    if (noExternalWall && count > number) break;
                    //Trace.WriteLine("still inside while loop at assgineblocksbydistance");
                }// end of while loop

            }// end of for loop


            leftoverPolyList.AddRange(polyLeftList);
            blockPolyList = PolygonUtility.CleanPolygonList(blockPolyList);
            leftoverPolyList = PolygonUtility.CleanPolygonList(leftoverPolyList);
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (blockPolyList) },
                { "LeftOverPoly", (leftoverPolyList) },
                { "AreaAssignedToBlock", (areaAdded)},
                { "FalseLines", (falseLines) },
                { "LineOptions", (lineOptions) },
                { "PointAdded" , (pointAdd)}
            };
        }


        //makes a space data tree from dept data
        [MultiReturn(new[] { "SpaceTree", "NodeList" })]
        internal static Dictionary<string, object> CreateSpaceTreeFromDeptData(Node root, List<Node> nodeList,
            Point origin, double spaceX, double spaceY, double radius, bool symettry = true)
        {
            SpaceDataTree tree = new SpaceDataTree(root, origin, spaceX, spaceY);
            Node current = root;
            Node nodeAdditionResult = null;
            for (int i = 0; i < nodeList.Count; i++)
            {
                if (current.NodeType == NodeType.Space) current = current.ParentNode;
                nodeAdditionResult = tree.AddNewNodeSide(current, nodeList[i]);
                if (nodeAdditionResult == current) break;
                else if (nodeAdditionResult != current && nodeAdditionResult != null) current = nodeAdditionResult;
                else current = nodeList[i];
            }
            return new Dictionary<string, object>
            {
                { "SpaceTree", (tree) },
                { "NodeList", (nodeList) }
            };
        }

    
        //splits a polygon based on offset direction
        [MultiReturn(new[] { "PolyAfterSplit", "LeftOverPoly", "LineOptions", "SortedLengths" })]
        internal static Dictionary<string, object> CreateBlocksByLines(Polygon2d polyOutline, Polygon2d containerPoly, double distance = 10, double minDist = 20,bool tag = true)
        {
            if (!ValidateObject.CheckPoly(polyOutline)) return null;
            Polygon2d poly = new Polygon2d(polyOutline.Points,0);
            List<double> lineLength = new List<double>();
            List<Line2d> lineOptions = new List<Line2d>();
            Dictionary<string, object> checkLineOffsetObject = ValidateObject.CheckLinesOffsetInPoly(poly, containerPoly, distance, tag);
            List<bool> offsetAble = (List<bool>)checkLineOffsetObject["Offsetables"];
            for (int i = 0; i < poly.Points.Count; i++)
            {
                if (offsetAble[i] == true) { lineLength.Add(poly.Lines[i].Length); }
                else lineLength.Add(0);
            }       
            List<int> sortedIndices = BasicUtility.Quicksort(lineLength);
            if (sortedIndices != null) sortedIndices.Reverse();
            for (int i = 0; i < poly.Points.Count; i++) if (lineLength[i] > 0 && i != sortedIndices[0]) { lineOptions.Add(poly.Lines[i]); }
            Dictionary<string, object> splitObj = SplitObject.SplitByOffsetFromLine(poly, sortedIndices[0], distance, minDist);
            Polygon2d polyBlock = (Polygon2d)splitObj["PolyAfterSplit"];
            Polygon2d leftPoly = (Polygon2d)splitObj["LeftOverPoly"];
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyBlock) },
                { "LeftOverPoly", (leftPoly) },
                { "LineOptions" , (lineOptions) },
                { "SortedLengths", (sortedIndices) }           
            };

        }
        

        //dept assignment new way
        [MultiReturn(new[] { "UpdatedDeptData", "LeftOverPolys", "CirculationPolys", "OtherDeptMainPoly" })]
        internal static Dictionary<string, object> DeptPlacer(List<DeptData> deptData, List<Polygon2d> polyList, double kpuDepth,
            double acceptableWidth = 20, double circulationFreq = 10, int iteration = 5, bool noExternalWall = false, bool unlimitedKPU = true)
        {
            if (deptData == null) { return null; }
            if (!ValidateObject.CheckPolyList(polyList)) return null;
 
            Trace.WriteLine("DEPT PLACE KPU STARTS +++++++++++++++++++++++++++++");
            List<double> AllDeptAreaAdded = new List<double>();
            List<List<Polygon2d>> AllDeptPolys = new List<List<Polygon2d>>();
            List<Polygon2d> leftOverPoly = new List<Polygon2d>(), polyCirculation = new List<Polygon2d>();//changed from stack
            List<Polygon2d> otherDeptPoly = new List<Polygon2d>();
            List<Polygon2d> subDividedPoly = new List<Polygon2d>();
            int count = 0, maxTry = 20;
            bool prepareReg = false, kpuPlaced = false, noKpuMode = false;// to disable multiple KPU
            double  areaAvailable = 0, ratio = 0.6;

            double totalAreaInPoly = 0;
            for (int i = 0; i < polyList.Count; i++) totalAreaInPoly += Math.Abs(PolygonUtility.AreaPolygon(polyList[i]));

            // build the areaneeded for each department based on polys we have and based on
            // original dept area needed

            double totalDeptProp = 0;
            
            for (int i = 0; i < deptData.Count; i++)
            {
                double areaAssigned = 0;
                DeptData deptItem = deptData[i];
                //Trace.WriteLine("kpuPlaced = " + kpuPlaced);

                //kpuplaced is added to make sure only one kpu added
                if ((deptItem.DepartmentType.IndexOf(KPU.ToLower()) == -1) && kpuPlaced)
                {
                    totalDeptProp += deptItem.DeptAreaProportionNeeded;
                    //Trace.WriteLine("Area prop = " + deptItem.DeptAreaProportionNeeded);
                }

                if ((deptItem.DepartmentType.IndexOf(KPU.ToLower()) != -1 ||
                    deptItem.DepartmentType.IndexOf(KPU.ToUpper()) != -1))
                {
                    kpuPlaced = true;
                }
            }
            kpuPlaced = false;
            List<double> areaNeededDept = new List<double>();
            for (int i = 0; i < deptData.Count; i++) areaNeededDept.Add(deptData[i].DeptAreaProportionNeeded * totalAreaInPoly);

            List<Polygon2d> leftOverBlocks = polyList;
            Polygon2d currentPoly = polyList[0];
            for (int i = 0; i < deptData.Count; i++)
            {
                double areaAssigned = 0;
                DeptData deptItem = deptData[i];     
                if ((deptItem.DepartmentType.IndexOf(KPU.ToLower()) != -1 ||
                    deptItem.DepartmentType.IndexOf(KPU.ToUpper()) != -1) && !kpuPlaced)// key planning unit - disabled multiple kpu same lvl
                {
                    double areaAvailablePoly = 0;
                    for (int j = 0; j < polyList.Count; j++) areaAvailablePoly += PolygonUtility.AreaPolygon(polyList[j]);              
                    double areaNeeded = areaNeededDept[i];
                    double areaLeftOverBlocks = 0;
                    for (int k = 0; k < leftOverBlocks.Count; k++) areaLeftOverBlocks += PolygonUtility.AreaPolygon(leftOverBlocks[k]);
                    if (unlimitedKPU) areaNeeded = 0.75 * areaLeftOverBlocks;
                    //else areaNeeded = 6000;
                    if(areaNeeded> 0.75 * areaLeftOverBlocks) areaNeeded = 0.75 * areaLeftOverBlocks;
                    Dictionary<string, object> inpatientObject = AssignBlocksBasedOnDistance(leftOverBlocks, kpuDepth, areaNeeded, 20, iteration, noExternalWall);
                    if (inpatientObject == null) return null;
                    List<Polygon2d> inpatienBlocks = (List<Polygon2d>)inpatientObject["PolyAfterSplit"];
                    leftOverBlocks = (List<Polygon2d>)inpatientObject["LeftOverPoly"];
                    if (!ValidateObject.CheckPolyList(inpatienBlocks) || !ValidateObject.CheckPolyList(leftOverBlocks)) return null;
                    areaAssigned = (double)inpatientObject["AreaAssignedToBlock"];
                    AllDeptPolys.Add(inpatienBlocks);
                    AllDeptAreaAdded.Add(areaAssigned);

                    
                    for (int j = 0; j < leftOverBlocks.Count; j++)
                    {
                        otherDeptPoly.Add(new Polygon2d(leftOverBlocks[j].Points));// just for debugging
                        leftOverPoly.Add(leftOverBlocks[j]);
                    }
                    kpuPlaced = true;
                }else // regular depts
                {
                    //Trace.WriteLine("Dept playing : " + i);
                    //when there is no kpu in the requirement
                    if (!kpuPlaced) { leftOverPoly = leftOverBlocks; kpuPlaced = true; noKpuMode = true; }
                    if (!prepareReg) // only need to do once, places a grid of rectangles before other depts get alloted
                    {
                        List<List<Polygon2d>> polySubDivs = new List<List<Polygon2d>>();
                        polySubDivs = SplitObject.SplitRecursivelyToSubdividePoly(leftOverPoly, acceptableWidth, circulationFreq, ratio);
                        bool checkPoly1 = ValidateObject.CheckPolygon2dListOrtho(polySubDivs[0], 0.5);
                        bool checkPoly2 = ValidateObject.CheckPolygon2dListOrtho(polySubDivs[1], 0.5);
                        while (polySubDivs == null || polySubDivs.Count == 0 || !checkPoly1 || !checkPoly2 && count < maxTry)
                        {
                            ratio -= 0.01;
                            if (ratio < 0) ratio = 0.6; break;
                            ///Trace.WriteLine("Ratio problem faced , ratio reduced to : " + ratio);
                            polySubDivs = SplitObject.SplitRecursivelyToSubdividePoly(leftOverPoly, acceptableWidth, circulationFreq, ratio);
                            count += 1;
                        }
                        leftOverPoly = polySubDivs[0];
                        polyCirculation = polySubDivs[1];
                        for (int j = 0; j < leftOverPoly.Count; j++) areaAvailable += PolygonUtility.AreaPolygon(leftOverPoly[j]);
                        if (leftOverPoly == null) break;
                        prepareReg = true;
                    }
                    double areaFactor = deptItem.DeptAreaProportionNeeded / totalDeptProp;
                    double areaNeeded = areaFactor * areaAvailable;

                    //areaFactor = BasicUtility.RandomBetweenNumbers(new Random(iteration), 0.8, 0.5); // adding random area factor, need fix later
                    //if(noKpuMode) areaFactor = BasicUtility.RandomBetweenNumbers(new Random(iteration), 0.6, 0.3); // when there is no kpu at all

                    Dictionary<string, object> assignedByRatioObj = AssignBlocksBasedOnRatioTest(areaNeeded, leftOverPoly, acceptableWidth, 0.5);
                    if (assignedByRatioObj == null)
                    {
                        //Trace.WriteLine("Null it is " + i);
                        continue;
                    }
                    Trace.WriteLine("Assignment worked " + i);
                    List<Polygon2d> everyDeptPoly = (List<Polygon2d>)assignedByRatioObj["DeptPoly"];
                    leftOverPoly = (List<Polygon2d>)assignedByRatioObj["LeftOverPoly"];
                    areaAssigned = (double)assignedByRatioObj["AreaAdded"];
                    List<Node> AllNodesList = (List<Node>)assignedByRatioObj["AllNodes"];
                    AllDeptAreaAdded.Add(areaAssigned);
                    AllDeptPolys.Add(everyDeptPoly);
                }
            }
            //clean dept polys based on their fitness
            for (int i = 0; i < AllDeptPolys.Count; i++) AllDeptPolys[i] = ValidateObject.CheckAndCleanPolygon2dList(AllDeptPolys[i]);

            //update dept data based on polys assigned
            List<DeptData> UpdatedDeptData = new List<DeptData>();
            for (int i = 0; i < deptData.Count; i++)
            {
                DeptData newDeptData = new DeptData(deptData[i]);
                if (i < AllDeptAreaAdded.Count)
                {
                    Trace.WriteLine("Dept playing : " + i);
                    newDeptData.DeptAreaProvided = AllDeptAreaAdded[i];
                    newDeptData.PolyAssignedToDept = AllDeptPolys[i];
                    UpdatedDeptData.Add(newDeptData);
                }
            
            }

            //added to compute area percentage for each dept
            double totalDeptArea = 0;
            for (int i = 0; i < UpdatedDeptData.Count; i++) totalDeptArea += UpdatedDeptData[i].DeptAreaProvided;
            for (int i = 0; i < UpdatedDeptData.Count; i++) UpdatedDeptData[i].DeptAreaProportionAchieved = Math.Round((UpdatedDeptData[i].DeptAreaProvided / totalDeptArea), 3);

            if (leftOverPoly.Count == 0) leftOverPoly = null;
            Trace.WriteLine("DEPT PLACE KPU ENDS +++++++++++++++++++++++++++++++");
            return new Dictionary<string, object>
            {
                { "UpdatedDeptData", (UpdatedDeptData) },
                { "LeftOverPolys", (leftOverPoly) },
                { "CirculationPolys", (polyCirculation) },
                { "OtherDeptMainPoly", (otherDeptPoly) }
            };
        }

  
        #endregion

    }
}
