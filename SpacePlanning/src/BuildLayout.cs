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
        internal static int MAXCOUNT = 1;

        internal const string KPU = "kpu";
        internal const string REG = "regular";

        #region - Public Methods

        // adds a point2d to a provided polygon with a given line id
        public static Polygon2d AddPointToPoly(Polygon2d poly, int lineId = 0, double parameter = 0.5)
        {
            if (parameter == 0) return poly;
            if(!ValidateObject.CheckPoly(poly)) return null;
            poly = new Polygon2d(poly.Points, 0);
            if (parameter < 0 || parameter >= 1) parameter = 0.5;
            List<Point2d> ptList = new List<Point2d>();
            for(int i = 0; i < poly.Points.Count; i++)
            {
                int a = i, b = i + 1;
                if (i == poly.Points.Count - 1) b = 0;
                ptList.Add(poly.Points[i]);
                if (a == lineId)
                {
                    Vector2d vec = new Vector2d(poly.Points[a], poly.Points[b]);
                    Point2d added = VectorUtility.VectorAddToPoint(poly.Points[a], vec, parameter);
                    ptList.Add(added);
                }     
            }
            return new Polygon2d(ptList,0);
        }




        //arranges depts on site and updates dept data object
        /// <summary>
        /// Arranges dept on site by assigning polygon2d's to each dept in the Dept Data object.
        /// Returns Dept polygon2d's, Left Over polygon2d's, Circulation polygon2d's and Updated Dept Data object.
        /// </summary>
        /// <param name="deptData">List of DeptData object.</param>
        /// <param name="buildingOutline">Building outline polygon2d geometry.</param>
        /// <param name="kpuDepthList">Depth of the main department.</param>
        /// <param name="acceptableWidth">Acceptable width in meters while allocating area and polygon2d to each dept on site.</param>
        /// <param name="minNotchDistance">Minimum distance below which an edge will be considered as a removable notch.</param>
        /// <param name="circulationFreq">Value to consider while checking frequency of cirulation computation polygon2d.</param>
        /// <param name="designSeed">Regardless of the recompute value, it is used to restart computing the node every time its value is changed.</param>
        /// <param name="noExternalWall">Boolean toggle to turn on or off requirement of external wall for KPU.</param>
        /// <param name="unlimitedKPU">Boolean toggle to turn on or off unlimied KPU placement.</param>
        /// <returns name="DeptData">Updated Dept Data object</returns>
        /// <returns name="LeftOverPolys">Polygon2d's not assigned to any department.</returns>
        /// <returns name="CirculationPolys">Polygon2d's needed to compute circulation networks.</returns>
        /// <returns name="OtherDeptMainPoly">Polygon2d for all other departments except for the primary department.</returns>
        /// <search>
        /// DeptData object, department arrangement on site
        /// </search>
        [MultiReturn(new[] { "DeptData", "LeftOverPolys" })]//"CirculationPolys", "OtherDeptMainPoly" 
        public static Dictionary<string, object> PlaceDepartments(List<DeptData> deptData, List<Polygon2d> buildingOutline, List<double> kpuDepthList, List<double> kpuWidthList,
            double acceptableWidth, double polyDivision = 8, int designSeed = 50, bool noExternalWall = false, 
            bool unlimitedKPU = true, bool mode3D = false, double totalBuildingHeight = 60, double avgFloorHeight = 15)
        {
            List<DeptData> deptDataInp = deptData;
            Dictionary<string, object> obj = new Dictionary<string, object>();
            deptData = deptDataInp.Select(x => new DeptData(x)).ToList(); // example of deep copy
            List<double> heightList = new List<double>();
            if (mode3D == true)
            {
                int numFloors = (int)Math.Floor(totalBuildingHeight / avgFloorHeight);
                for (int i = 0; i < numFloors; i++) heightList.Add((i) * avgFloorHeight);
                Trace.WriteLine("Heightlist formed");
                for (int i = 0; i < deptData.Count; i++)
                {
                    deptData[i].Mode3D = true;
                    deptData[i].FloorHeightList = heightList;
                }
            }
            if (deptData[0].Mode3D)
            {
                return BuildLayout3D.PlaceDepartments3D(deptData, buildingOutline, kpuDepthList, kpuWidthList, acceptableWidth,
                                        polyDivision, designSeed, noExternalWall);
            }
            else {
                return BuildLayout3D.PlaceDepartments2D(deptData, buildingOutline, kpuDepthList, kpuWidthList, acceptableWidth,
                                        polyDivision, designSeed, noExternalWall);
            }  
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
        /// <returns name="ProgramData">Updated program data object.</returns>
        /// <returns name="ProgramsAddedCount">Number of program units added.</returns>
        [MultiReturn(new[] { "ProgramData", "ProgramsAddedCount" })]
        internal static Dictionary<string, object> PlaceKPUPrograms(List<Polygon2d> deptPoly, List<ProgramData> progData, List<double> primaryProgramWidthList, int space = 10)
        {

            if (!ValidateObject.CheckPolyList(deptPoly)) return null;
            if (progData == null || progData.Count == 0) return null;
            int roomCount = 0;
            List<Polygon2d> polyList = new List<Polygon2d>();
            List<Point2d> pointsList = new List<Point2d>();
            Queue<ProgramData> programDataRetrieved = new Queue<ProgramData>();
            List<ProgramData> progDataAddedList = new List<ProgramData>();
            ProgramData copyProgData = new ProgramData(progData[0]);
            int index = 0;
            for (int i = 0; i < progData.Count; i++) programDataRetrieved.Enqueue(progData[i]);
            for (int i = 0; i < deptPoly.Count; i++)
            {
                Polygon2d poly = deptPoly[i];
                if (!ValidateObject.CheckPoly(poly)) continue;
                int dir = 0, count = 0,lineId =0;

                List<double> spans = PolygonUtility.GetSpansXYFromPolygon2d(poly.Points);
                double setSpan = 1000000000000, fac = 1.5;
                if (spans[0] > spans[1]) { setSpan = spans[0]; dir = 1; } // poly is horizontal, dir should be 1
                else { setSpan = spans[1]; dir = 0; }// poly is vertical, dir should be 0
                Polygon2d currentPoly = poly;
                List<Polygon2d> polyAfterSplitting = new List<Polygon2d>();
                ProgramData progItem = new ProgramData(progData[0]);
                Point2d centerPt = PolygonUtility.CentroidOfPoly(currentPoly);

                int lineOrient = ValidateObject.CheckLineOrient(currentPoly.Lines[0]);
                if (lineOrient == dir) lineId = 0;
                else lineId = 1;                
                if (i > 2) index += 1;
                if (index > primaryProgramWidthList.Count - 1) index = 0;
                double primaryProgramWidth = primaryProgramWidthList[index];
                while (setSpan > primaryProgramWidth && count < 200)
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
                { "ProgramData",(UpdatedProgramDataList) },
                { "ProgramsAddedCount" , (roomCount) }
            };
        }






        //arranges program elements inside secondary dept units and updates program data object
        /// <summary>
        /// Assigns program elements inside the secondary department polygon2d.
        /// </summary>
        /// <param name="deptDataInp">Dept Data object.</param>
        /// <param name="recompute">This value is used to restart computing the node every time its value is changed.</param>
        /// <returns></returns>
        [MultiReturn(new[] { "PolyAfterSplit", "ProgramData" })]
        internal static Dictionary<string, object> PlaceREGPrograms(DeptData deptDataInp,double minAllowedDim = 5, int designSeed = 10, bool checkAspectRatio = true)
        {
            if (deptDataInp == null) return null;
            double ratio = 0.5;
            DeptData deptData = new DeptData(deptDataInp);
            List<Polygon2d> deptPoly = deptData.PolyAssignedToDept;
            List<ProgramData> progData = deptData.ProgramsInDept;
            if (!ValidateObject.CheckPolyList(deptPoly)) return null;
            if (progData == null || progData.Count == 0) return null;
            List<List<Polygon2d>> polyList = new List<List<Polygon2d>>();
            List<Polygon2d> polyCoverList = new List<Polygon2d>();


            //SORT THE POLYSUBDIVS
            Point2d center = PolygonUtility.CentroidOfPolyList(deptPoly);
            List<int> sortedPolyIndices = PolygonUtility.SortPolygonsFromAPoint(deptPoly, center);
            List<Polygon2d> sortedPolySubDivs = new List<Polygon2d>();
            for (int k = 0; k < sortedPolyIndices.Count; k++) { sortedPolySubDivs.Add(deptPoly[sortedPolyIndices[k]]); }
            deptPoly = sortedPolySubDivs; 


            //Stack<ProgramData> programDataRetrieved = new Stack<ProgramData>();
            //Stack<Polygon2d> polygonAvailable = new Stack<Polygon2d>();
            Queue<Polygon2d> polygonAvailable = new Queue<Polygon2d>();
            for (int j = 0; j < deptPoly.Count; j++) { polygonAvailable.Enqueue(deptPoly[j]); }
            double areaAssigned = 0, eps = 50, max = 0.73, min = 0.27;
            int count = 0, maxTry = 100;
            Random ran = new Random(designSeed);
            for(int i = 0; i < progData.Count; i++)
            {
                ProgramData progItem = progData[i];
                progItem.PolyAssignedToProg = new List<Polygon2d>();
                double areaNeeded = progItem.ProgAreaNeeded;
                while (areaAssigned < areaNeeded && polygonAvailable.Count > 0)// && count < maxTry
                {
                    ratio = BasicUtility.RandomBetweenNumbers(ran, max, min);
                    Polygon2d currentPoly = polygonAvailable.Dequeue();
                    double areaPoly = PolygonUtility.AreaPolygon(currentPoly);
                    int compareArea = BasicUtility.CheckWithinRange(areaNeeded, areaPoly, eps);
                    if (compareArea == 1) // current poly area is more =  compareArea == 1
                    {
                        Dictionary<string,object> splitObj = SplitObject.SplitByRatio(currentPoly, ratio);
                        if (splitObj != null)
                        {
                            List<Polygon2d> polyAfterSplit = (List<Polygon2d>)splitObj["PolyAfterSplit"];
                            for (int j = 0; j < polyAfterSplit.Count; j++) polygonAvailable.Enqueue(polyAfterSplit[j]);
                            count += 1;
                            continue;
                        }
                        else
                        {
                            //area within range
                            if (ValidateObject.CheckPoly(currentPoly))
                            {
                                if (checkAspectRatio)
                                {
                                    if (ValidateObject.CheckPolyAspectRatio(currentPoly, minAllowedDim))
                                    {
                                        progItem.PolyAssignedToProg.Add(currentPoly);
                                        areaAssigned += areaPoly;
                                    }
                                }
                                else
                                {
                                    progItem.PolyAssignedToProg.Add(currentPoly);
                                    areaAssigned += areaPoly;
                                }
                                
                              
                            }                            
                            count += 1;
                        }
                    }else
                    {
                        //area within range
                        if (ValidateObject.CheckPoly(currentPoly))
                        {
                            if (checkAspectRatio)
                            {
                                if (ValidateObject.CheckPolyAspectRatio(currentPoly, minAllowedDim))
                                {
                                    progItem.PolyAssignedToProg.Add(currentPoly);
                                    areaAssigned += areaPoly;
                                }
                            }
                            else
                            {
                                progItem.PolyAssignedToProg.Add(currentPoly);
                                areaAssigned += areaPoly;
                            }
                        }
                        count += 1;
                    }
                 
                }// end of while

              
                polyList.Add(progItem.PolyAssignedToProg);
                progItem.ProgAreaProvided = areaAssigned;
                if (progItem.PolyAssignedToProg.Count > 1) { if (progItem.ProgramName.IndexOf("##") == -1) progItem.ProgramName += " ##"; }// + progItem.ProgID;  }
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
            */
            //for(int i = 0; i < progData.Count; i++) progData[i].PolyAssignedToProg = polyList[i];

            List<ProgramData> newProgDataList = progData.Select(x => new ProgramData(x)).ToList(); // example of deep copy    
                       
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyList) },
                { "ProgramData",(newProgDataList) }
            };
        }



        //arranges program elements inside secondary dept units and updates program data object
        /// <summary>
        /// Assigns program elements inside the secondary department polygon2d.
        /// </summary>
        /// <param name="deptData">List of Department Data Objects.</param>
        /// <param name="kpuProgramWidthList">Width of the program poly in the primary department</param>
        /// <param name="minAllowedDim">Minimum allowed dimension of the program space.</param>
        /// <param name="checkAspectRatio">Boolean value to toggle check aspect ratio of the programs.</param>
        /// <returns name="DeptData">Updated department data object.</returns>
        [MultiReturn(new[] { "DeptData" })]
        public static Dictionary<string, object> PlacePrograms(List<DeptData> deptData, List<double> kpuProgramWidthList, double minAllowedDim = 5, int designSeed = 5, bool checkAspectRatio = false)
        {
            List<DeptData> deptDataInp = deptData;
            Dictionary<string, object> obj = new Dictionary<string, object>();
            deptData = deptDataInp.Select(x => new DeptData(x)).ToList(); // example of deep copy

            if (deptDataInp[0].Mode3D)
            {
                return BuildLayout3D.PlacePrograms3D(deptData, kpuProgramWidthList, minAllowedDim, designSeed, checkAspectRatio);
            }
            else {
                return BuildLayout3D.PlacePrograms2D(deptData, kpuProgramWidthList, minAllowedDim, designSeed, checkAspectRatio);
            }
        }



        #endregion

        
        #region - Private Methods  

       

        [MultiReturn(new[] { "DeptPoly", "LeftOverPoly", "AllPolys", "AreaAdded", "AllNodes" })]
        internal static Dictionary<string, object> AssignBlocksBasedOnRatio(double deptAreaTarget, List<Polygon2d> polyList)
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


            List<Polygon2d> leftOverList = polyAvailable.ToList();
            Point2d center = PolygonUtility.CentroidOfPolyList(leftOverList);
            List<int> sortedPolyIndices = PolygonUtility.SortPolygonsFromAPoint(leftOverList, center);
            List<Polygon2d> sortedPolySubDivs = new List<Polygon2d>();
            for (int k = 0; k < sortedPolyIndices.Count; k++) { sortedPolySubDivs.Add(leftOverList[sortedPolyIndices[k]]); }
            leftOverList = sortedPolySubDivs; 
            return new Dictionary<string, object>
            {
                { "DeptPoly", (polysToDept) },
                { "LeftOverPoly", (leftOverList) },
                { "AllPolys", (polyList)},
                { "AreaAdded", (areaAssigned) },
                { "AllNodes", (null)}
            };
        }

        // randomize lineList
        internal static List<Line2d> RandomizeLineList(List<Line2d> lineList, int designSeed = 0)
        {
            if (lineList == null) return null;
            List<int> indices = new List<int>();
            for (int i = 0; i < lineList.Count; i++) indices.Add(i);
            List<int> indicesRandom = BasicUtility.RandomizeList(indices, new Random(designSeed));
            List<Line2d> lineNewList = new List<Line2d>();
            for (int i = 0; i < lineList.Count; i++) lineNewList.Add(lineList[indicesRandom[i]]);
            return lineNewList;
        }

        //blocks are assigne based on offset distance, used for inpatient blocks
        [MultiReturn(new[] { "PolyAfterSplit", "LeftOverPoly", "AreaAssignedToBlock", "FalseLines", "LineOptions", "PointAdded" })]
        internal static Dictionary<string, object> AssignBlocksBasedOnDistance(List<Polygon2d> polyList, List<double> distanceList, 
            double area,List<double> areaEachKPUList, double thresDistance = 10, int iteration = 5,  bool noExternalWall = false, double parameter =0.5, bool stackOptions = false)
        {

            if (!ValidateObject.CheckPolyList(polyList)) return null;
            //if (distance < 1) return null;
            if (parameter <= 0 && parameter >= 1) parameter = 0.5;
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
            int index = 0;
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
                Random ran = new Random(iteration);
                double a = 60, b = 20;
                //thresDistance = BasicUtility.RandomBetweenNumbers(ran, a, b);
                double areaCurrentKPU = areaEachKPUList[index];
                double distance = distanceList[index];
                double maxValue = distance * 2, minValue = distance * 0.3;
                while (polyLeftList.Count > 0 && areaAdded < area) //count<recompute count < maxTry
                {    
                    //distance = BasicUtility.RandomBetweenNumbers(ran, maxValue, minValue);
                    double areaLeftToAdd = area - areaAdded;
                    error = false;
                    Polygon2d currentPoly = polyLeftList.Pop();
                    Polygon2d tempPoly = new Polygon2d(currentPoly.Points, 0);
                    Dictionary<string, object> splitObject = CreateBlocksByLines(currentPoly, poly, distance,areaLeftToAdd, thresDistance, noExternalWall,parameter);
                    if (splitObject == null) { count += 1; Trace.WriteLine("Split errored"); continue; }
                    Polygon2d blockPoly = (Polygon2d)splitObject["PolyAfterSplit"];
                    Polygon2d leftPoly = (Polygon2d)splitObject["LeftOverPoly"];
                    lineOptions = (List<Line2d>)splitObject["LineOptions"];
                    if(stackOptions) lineOptions = RandomizeLineList(lineOptions, iteration);
                    Dictionary<string, object> addPtObj = LayoutUtility.AddPointToFitPoly(leftPoly, poly, distance, thresDistance, iteration);
                    leftPoly = (Polygon2d)addPtObj["PolyAddedPts"];
                    falseLines = (List<Line2d>)addPtObj["FalseLineList"];
                    pointAdd = (Point2d)addPtObj["PointAdded"];
                    areaAdded += PolygonUtility.AreaPolygon(blockPoly);
                    polyLeftList.Push(leftPoly);
                    blockPolyList.Add(blockPoly);

                    areaCurrentKPU -= areaAdded;
                    if (areaCurrentKPU < 50)
                    {
                        index += 1;
                        if (index > areaEachKPUList.Count - 1) index = 0;
                        distance = distanceList[index];
                        areaCurrentKPU = areaEachKPUList[index];
                      
                    }


                    count += 1;
                    if (lineOptions.Count == 0) error = true;
                    else
                    {
                        // need to do something with the line we get
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


        //gets a poly and its lineId and distance,
        //checks if area is more then it will provide a parameter value
        internal static Polygon2d BuildPolyToSatisfyArea(Polygon2d poly, int lineId = 0, double areaTarget = 100, double offsetDistance = 10)
        {      
            if (!ValidateObject.CheckPoly(poly)) return null;            
            poly = new Polygon2d(poly.Points, 0);
            List<Point2d> ptList = new List<Point2d>();

            double lineLength = poly.Lines[lineId].Length;
            double areaAvailable = lineLength * offsetDistance, eps = 10;
            //int compareArea = BasicUtility.CheckWithinRange(areaTarget, areaAvailable, eps);
            if (areaTarget / areaAvailable < 0.9) // current poly area is more =  compareArea == 1
            {
                double lineLengthExpected = areaTarget / offsetDistance;
                double parameter = lineLengthExpected / lineLength;
                if (parameter >= 1 || parameter <= 0) return poly;
                return AddPointToPoly(poly, lineId, parameter);
            }
            else return poly;          
        }
    
        //splits a polygon based on offset direction
        [MultiReturn(new[] { "PolyAfterSplit", "LeftOverPoly", "LineOptions", "SortedLengths" })]
        internal static Dictionary<string, object> CreateBlocksByLines(Polygon2d polyOutline, Polygon2d containerPoly, double distance = 10, 
            double areaTarget = 10, double minDist = 20,bool tag = true, double parameter = 0.5)
        {
            if (!ValidateObject.CheckPoly(polyOutline)) return null;
            if (parameter <= 0 && parameter >= 1) parameter = 0.5;
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
            // add a funct, which takes a lineid, poly, areatarget
            // it checks what parameter it should split to have it meet area requirement correctly
            poly = BuildPolyToSatisfyArea(poly, sortedIndices[0], areaTarget, distance);
            poly = AddPointToPoly(poly, sortedIndices[0], parameter);
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
        [MultiReturn(new[] { "DeptData", "LeftOverPolys" })]//"CirculationPolys", "OtherDeptMainPoly" 
        internal static Dictionary<string, object> DeptPlacer(List<DeptData> deptData, List<Polygon2d> polyList, List<double> kpuDepthList, List<double> kpuWidthList,
            double acceptableWidth = 20, double circulationFreq = 10, int designSeed = 5, bool noExternalWall = false, 
            bool unlimitedKPU = true, bool stackOptionsDept = false, bool stackOptionsProg = false, double parameter = 0.5)
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

            ratio = BasicUtility.RandomBetweenNumbers(new Random(designSeed), 0.76, 0.23);

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

            /*
            if (stackOptions)
            {
                Random ran = new Random(designSeed);
                for (int i = 0; i < deptData.Count; i++)
                {
                    if (i == 0)
                    {
                        double prop = BasicUtility.RandomBetweenNumbers(ran, deptData[i].DeptAreaProportionNeeded, 0.4);
                        areaNeededDept[i] = deptData[i].DeptAreaProportionNeeded * totalAreaInPoly;
                    }
                    
                }
            }
            */

            List<Polygon2d> leftOverBlocks = polyList;
            Polygon2d currentPoly = polyList[0];
            List<double> areaEachKPUList = new List<double>();            
            double areaKpu = 0;
            for(int j = 0; j < kpuDepthList.Count; j++) areaEachKPUList.Add(kpuWidthList[j] * kpuDepthList[j]);
            
            for (int i = 0; i < deptData.Count; i++)
            {
                double thresDistance = 20;
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
                    if (unlimitedKPU) areaNeeded = 0.9 * areaLeftOverBlocks;
                    //else areaNeeded = 6000;
                    //if(!stackOptionsDept && areaNeeded> 0.75 * areaLeftOverBlocks) areaNeeded = 0.75 * areaLeftOverBlocks;
                    
                    Dictionary<string, object> inpatientObject = AssignBlocksBasedOnDistance(leftOverBlocks, kpuDepthList, areaNeeded, areaEachKPUList,
                        thresDistance, designSeed, noExternalWall,parameter, stackOptionsDept);
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
                        Point2d center = PolygonUtility.CentroidOfPolyList(leftOverPoly);
                        if(stackOptionsProg)
                        {
                            double arealeft = 0;
                            for (int j = 0; j < leftOverPoly.Count; j++) { arealeft += PolygonUtility.AreaPolygon(leftOverPoly[j]); }
                            double upper = arealeft / 6, lower = arealeft / 12;
                            //acceptableWidth = BasicUtility.RandomBetweenNumbers(new Random(designSeed), upper, lower);
                           
                        }
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
                        //SORT THE POLYSUBDIVS
                        //Point2d center = PolygonUtility.CentroidOfPolyList(leftOverPoly);
                        List<int> sortedPolyIndices = PolygonUtility.SortPolygonsFromAPoint(polySubDivs[0], center);
                        List<Polygon2d> sortedPolySubDivs = new List<Polygon2d>();
                        for(int k = 0; k < sortedPolyIndices.Count; k++) { sortedPolySubDivs.Add(polySubDivs[0][sortedPolyIndices[k]]); }
                        leftOverPoly = sortedPolySubDivs; // polySubDivs[0]
                        //leftOverPoly = polySubDivs[0];
                        polyCirculation = polySubDivs[1];
                        for (int j = 0; j < leftOverPoly.Count; j++) areaAvailable += PolygonUtility.AreaPolygon(leftOverPoly[j]);
                        if (leftOverPoly == null) break;
                        prepareReg = true;
                    }
                    double areaFactor = deptItem.DeptAreaProportionNeeded / totalDeptProp;
                    double areaNeeded = areaFactor * areaAvailable;

                    //areaFactor = BasicUtility.RandomBetweenNumbers(new Random(iteration), 0.8, 0.5); // adding random area factor, need fix later
                    //if(noKpuMode) areaFactor = BasicUtility.RandomBetweenNumbers(new Random(iteration), 0.6, 0.3); // when there is no kpu at all

                    Dictionary<string, object> assignedByRatioObj = AssignBlocksBasedOnRatio(areaNeeded, leftOverPoly);
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
                else
                {
                    newDeptData.DeptAreaProvided = 0;
                    newDeptData.PolyAssignedToDept = null; 
                    UpdatedDeptData.Add(newDeptData);
                }
            
            }

            //added to compute area percentage for each dept
            double totalDeptArea = 0;
            for (int i = 0; i < UpdatedDeptData.Count; i++) totalDeptArea += UpdatedDeptData[i].DeptAreaProvided;
            for (int i = 0; i < UpdatedDeptData.Count; i++)
            {
                UpdatedDeptData[i].DeptAreaProportionAchieved = Math.Round((UpdatedDeptData[i].DeptAreaProvided / totalDeptArea), 3);
                if (stackOptionsProg)
                {
                    UpdatedDeptData[i].ProgramsInDept = ReadData.RandomizeProgramList(UpdatedDeptData[i].ProgramsInDept, designSeed);
                }

            }

            if (leftOverPoly.Count == 0) leftOverPoly = null;
            Trace.WriteLine("DEPT PLACE KPU ENDS +++++++++++++++++++++++++++++++");
            /*
            
                { "CirculationPolys", (polyCirculation) },
                { "OtherDeptMainPoly", (otherDeptPoly) }
            */
            return new Dictionary<string, object>
            {
                { "DeptData", (UpdatedDeptData) },
                { "LeftOverPolys", (leftOverPoly) }
            };
        }

  
        #endregion

    }
}
