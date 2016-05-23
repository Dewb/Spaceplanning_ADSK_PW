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
        
        internal static double SPACING = 10; //higher value makes code faster, 6, 10 was good too
        internal static double SPACING2 = 10;
        internal static Random RANGENERATE = new Random();
        internal static double RECURSE = 0;
        internal static Point2d REFERENCEPOINT = new Point2d(0,0);
        internal static int MAXCOUNT = 50, MAXROUND = 50;

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
        /// <returns name="DeptPolys">Polygon2d's assigned to each department.</returns>
        /// <returns name="LeftOverPolys">Polygon2d's not assigned to any department.</returns>
        /// <returns name="CirculationPolys">Polygon2d's needed to compute circulation networks.</returns>
        /// <returns name="OtherDeptMainPoly">Polygon2d for all other departments except for the primary department.</returns>
        /// <returns name="UpdatedDeptData">Updated Dept Data object</returns>
        /// <search>
        /// DeptData object, department arrangement on site
        /// </search>
        [MultiReturn(new[] { "DeptPolys", "LeftOverPolys", "CirculationPolys","OtherDeptMainPoly", "UpdatedDeptData"})]
        public static Dictionary<string, object> PlaceDeptOnSite(List<DeptData> deptData, Polygon2d buildingOutline,  double primaryDeptDepth, 
            double acceptableWidth, double minNotchDistance = 20, double circulationFreq = 8, int recompute = 1)
        {           
            Dictionary<string, object> deptArrangement = new Dictionary<string, object>();
            double count = 0;            
            Random rand = new Random();
            bool deptPlaced = false;
            while(deptPlaced == false && count < MAXCOUNT)
            {
                Trace.WriteLine("Lets Go Again for : " + count);
                deptArrangement = DeptPlacer(deptData, buildingOutline, primaryDeptDepth, acceptableWidth, minNotchDistance, circulationFreq, recompute);
                if(deptArrangement != null)
                {
                    List<List<Polygon2d>> deptAllPolys =(List<List<Polygon2d>>) deptArrangement["DeptPolys"];
                    for(int i = 0; i < deptAllPolys.Count; i++)
                    {
                        List<Polygon2d> eachDeptPoly = deptAllPolys[i];
                        if (ValidateObject.CheckPolyList(eachDeptPoly)) deptPlaced = true;
                        else { deptPlaced = false; break; }
                    }
                }
                count += 1;
            }
            return deptArrangement;
        }


        //arranges program elements inside primary dept unit and updates program data object
        /// <summary>
        /// Assigns program elements inside the primary department polygon2d.
        /// </summary>
        /// <param name="deptPoly">Polygon2d's of primary department which needs program arrangement inside.</param>
        /// <param name="progData">Program Data object</param>
        /// <param name="primaryProgramWidth">Width of the primary program element in the department.</param>
        /// <param name="recompute">Regardless of the recompute value, it is used to restart computing the node every time its value is changed.</param>
        /// <returns name="PolyAfterSplit">Polygon2d's obtained after assigning programs inside the department.</returns>
        /// <returns name="UpdatedProgramData">Updated program data object.</returns>
        /// <returns name="ProgramsAddedCount">Number of program units added.</returns>
        [MultiReturn(new[] { "PolyAfterSplit", "UpdatedProgramData", "ProgramsAddedCount" })]
        public static Dictionary<string, object> PlacePrimaryPrograms(List<Polygon2d> deptPoly, List<ProgramData> progData, double primaryProgramWidth, int recompute = 1)
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
                int lineId = 0, count = 0;
                if (poly.Lines[0].Length > poly.Lines[1].Length) lineId = 1;
                else lineId = 1;
                
                List<double> spans = PolygonUtility.GetSpansXYFromPolygon2d(poly.Points);
                double setSpan = 1000000000000, fac = 1.5;
                if (spans[0] > spans[1]) setSpan = spans[0];
                else setSpan = spans[1];
                Polygon2d currentPoly = poly;
                Polygon2d polyAfterSplitting = new Polygon2d(null), leftOverPoly = new Polygon2d(null);
                double area = 0;
                ProgramData progItem = new ProgramData(progData[0]);
                while (setSpan > primaryProgramWidth) //programDataRetrieved.Count > 0
                {
                    double dist = 0;
                    if (setSpan > fac * primaryProgramWidth) dist = primaryProgramWidth;
                    else dist = setSpan;
                    Dictionary<string, object> splitReturn = SplitObject.SplitByOffsetFromLine(currentPoly,lineId, dist);
                    polyAfterSplitting = (Polygon2d)splitReturn["PolyAfterSplit"];
                    leftOverPoly = (Polygon2d)splitReturn["LeftOverPoly"];                    
                    if (ValidateObject.CheckPoly(leftOverPoly))
                    {
                        progItem = programDataRetrieved.Dequeue();
                        currentPoly = leftOverPoly;
                        polyList.Add(polyAfterSplitting);
                        progItem.AreaProvided = PolygonUtility.AreaPolygon(polyAfterSplitting); 
                        setSpan -= dist;
                        progDataAddedList.Add(progItem);
                        count += 1;
                    }
                    else break;
                    if (programDataRetrieved.Count == 0) programDataRetrieved.Enqueue(copyProgData);
                }// end of while loop
                
                progItem = programDataRetrieved.Dequeue();
                polyList.Add(leftOverPoly);
                progItem.AreaProvided = PolygonUtility.AreaPolygon(leftOverPoly); 
                progDataAddedList.Add(progItem);
                if (programDataRetrieved.Count == 0) programDataRetrieved.Enqueue(copyProgData);
                
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
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyList) },
                { "UpdatedProgramData",(UpdatedProgramDataList) },
                { "ProgramsAddedCount" , (roomCount) }
            };
        }


        //arranges program elements inside secondary dept units and updates program data object
        /// <summary>
        /// Assigns program elements inside the secondary department polygon2d.
        /// </summary>
        /// <param name="deptPoly">Polygon2d's of secondary department which needs program arrangement inside.</param>
        /// <param name="progData">Program Data object.</param>
        /// <param name="recompute">Regardless of the recompute value, it is used to restart computing the node every time its value is changed.</param>
        /// <returns></returns>
        [MultiReturn(new[] { "PolyAfterSplit", "UpdatedProgramData" })]
        public static Dictionary<string, object> PlaceSecondaryPrograms(List<Polygon2d> deptPoly, List<ProgramData> progData, int recompute = 0)
        {
            if (!ValidateObject.CheckPolyList(deptPoly)) return null;
            if (progData == null || progData.Count == 0) return null;
            Random ran = new Random();
            List<List<Polygon2d>> polyList = new List<List<Polygon2d>>();
            List<Polygon2d> polyCoverList = new List<Polygon2d>();
            Stack<ProgramData> programDataRetrieved = new Stack<ProgramData>();
            Stack<Polygon2d> polygonAvailable = new Stack<Polygon2d>();
            
            for (int j = 0; j < deptPoly.Count; j++) { polygonAvailable.Push(deptPoly[j]); }
            double areaAssigned = 0, eps = 50;
            int count = 0, maxTry = 100;
            for(int i = 0; i < progData.Count; i++)
            {
                ProgramData progItem = progData[i];
                progItem.PolyAssignedToProg = new List<Polygon2d>();
                double areaNeeded = progItem.AreaNeeded;
                while (areaAssigned < areaNeeded && polygonAvailable.Count > 0 && count < maxTry)
                {
                    Polygon2d currentPoly = polygonAvailable.Pop();
                    double areaPoly = PolygonUtility.AreaPolygon(currentPoly);
                    int compareArea = BasicUtility.CheckWithinRange(areaNeeded, areaPoly, eps);
                    if (compareArea == 1) // current poly area is more
                    {
                        Dictionary<string,object> splitObj = SplitObject.SplitByRatio(currentPoly, 0.5);
                        List<Polygon2d> polyAfterSplit = (List<Polygon2d>)splitObj["PolyAfterSplit"];
                        for (int j = 0; j < polyAfterSplit.Count; j++) polygonAvailable.Push(polyAfterSplit[j]);
                        count += 1;
                        continue;
                    }
                    progItem.PolyAssignedToProg.Add(currentPoly);
                    areaAssigned += areaPoly;                
                    count += 1;
                }// end of while
                polyList.Add(progItem.PolyAssignedToProg);
                progItem.AreaProvided = areaAssigned;
                count = 0;
                areaAssigned = 0;
            }
            List<ProgramData> newProgDataList = new List<ProgramData>();
            for (int i = 0; i < progData.Count; i++) newProgDataList.Add(new ProgramData(progData[i]));
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyList) },
                { "UpdatedProgramData",(newProgDataList) }
            };


        }



        #endregion

        #region - Private Methods  
        /*
        //gets and sets the space between points for any smoothened polygon2d
        internal double SPACINGORIG
        {
            get { return SPACING; }
            set { SPACING = value; }
        }

        //gets and sets the space between points for any smoothened polygon2d
        internal double SPACINGANOTHER
        {
            get { return SPACING2; }
            set { SPACING2 = value; }
        }
        */

        //blocks are assigned based on ratio of split, used for assigning other depts
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


        //blocks are assigne based on offset distance, used for inpatient blocks
        [MultiReturn(new[] { "PolyAfterSplit", "LeftOverPoly", "AreaAssignedToBlock", "FalseLines", "LineOptions", "PointAdded" })]
        internal static Dictionary<string, object> AssignBlocksBasedOnDistance(Polygon2d poly, double distance = 16, double area = 0, double thresDistance = 10, double recompute = 5)
        {

            if (!ValidateObject.CheckPoly(poly)) return null;
            if (distance < 1) return null;
            bool externalIncude = false;
            if (recompute > 5) externalIncude = true;
            int count = 0, maxTry = 100;
            poly = new Polygon2d(poly.Points);
            if (area == 0) area = 0.8 * PolygonUtility.AreaPolygon(poly);
            Stack<Polygon2d> polyLeftList = new Stack<Polygon2d>();
            double areaAdded = 0;
            polyLeftList.Push(poly);
            Point2d pointAdd = new Point2d(0, 0);
            List<Polygon2d> blockPolyList = new List<Polygon2d>();
            List<Polygon2d> leftoverPolyList = new List<Polygon2d>();
            List<Line2d> falseLines = new List<Line2d>();
            List<Line2d> lineOptions = new List<Line2d>();
            bool error = false;
            while (polyLeftList.Count > 0 && areaAdded < area) //count<recompute count < maxTry
            {
                Polygon2d currentPoly = polyLeftList.Pop();
                Polygon2d tempPoly = new Polygon2d(currentPoly.Points, 0);
                Dictionary<string, object> splitObject = CreateBlocksByLines(currentPoly, poly, distance, thresDistance, externalIncude);
                if (splitObject == null) { count += 1; Trace.WriteLine("Split errored"); continue; }
                Polygon2d blockPoly = (Polygon2d)splitObject["PolyAfterSplit"];
                Polygon2d leftPoly = (Polygon2d)splitObject["LeftOverPoly"];
                lineOptions = (List<Line2d>)splitObject["LineOptions"];
                Dictionary<string, object> addPtObj = LayoutUtility.AddPointToFitPoly(leftPoly, poly, distance, thresDistance, recompute);
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
                    for (int i = 0; i < lineOptions.Count; i++)
                    {
                        if (lineOptions[i].Length > thresDistance) { error = false; break; }
                        else error = true;
                    }
                }
                if (error) break;
            }

            //added to allow one more poly
            bool spaceAvailable = false;
            for (int i = 0; i < lineOptions.Count; i++) { if (lineOptions[i].Length > 100) spaceAvailable = true; break; }

            if (spaceAvailable && polyLeftList.Count > 0)
            {
                Polygon2d currentPoly = polyLeftList.Pop();
                Polygon2d tempPoly = new Polygon2d(currentPoly.Points, 0);
                Dictionary<string, object> splitObject = CreateBlocksByLines(currentPoly, poly, distance, thresDistance, externalIncude);
                Trace.WriteLine("Well found that space is available");
                if (splitObject != null)
                {

                    Polygon2d blockPoly = (Polygon2d)splitObject["PolyAfterSplit"];
                    Polygon2d leftPoly = (Polygon2d)splitObject["LeftOverPoly"];
                    lineOptions = (List<Line2d>)splitObject["LineOptions"];
                    Dictionary<string, object> addPtObj = LayoutUtility.AddPointToFitPoly(leftPoly, poly, distance, thresDistance, recompute);
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
                        for (int i = 0; i < lineOptions.Count; i++)
                        {
                            if (lineOptions[i].Length > thresDistance) { error = false; break; }
                            else error = true;
                        }
                    }
                    Trace.WriteLine("Succesfully assigned one extra");

                } // end of if loop


            }



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
        [MultiReturn(new[] { "DeptPolys", "LeftOverPolys","CirculationPolys", "OtherDeptMainPoly", "UpdatedDeptData", })]
        internal static Dictionary<string, object> DeptPlacer(List<DeptData> deptData, Polygon2d poly, double offset, 
            double acceptableWidth = 20,double minNotchDist = 20, double circulationFreq = 10, double recompute = 5)
        {
            if (deptData == null) return null;
            Dictionary<string, object> notchObj = ValidateObject.CheckPolyNotches(poly, minNotchDist);
            poly = (Polygon2d)notchObj["PolyReduced"];
            List<double> AllDeptAreaAdded = new List<double>();
            List<List<Polygon2d>> AllDeptPolys = new List<List<Polygon2d>>();
            List<Polygon2d> leftOverPoly = new List<Polygon2d>(), polyCirculation = new List<Polygon2d>();//changed from stack
            List<Polygon2d> otherDeptPoly = new List<Polygon2d>();
            List<Polygon2d> subDividedPoly = new List<Polygon2d>();

            double totalDeptProp = 0, areaAvailable = 0;
            for (int i = 0; i < deptData.Count; i++) if (i > 0) totalDeptProp += deptData[i].DeptAreaProportionNeeded;

            for (int i = 0; i < deptData.Count; i++)
            {            
                double areaAssigned = 0;
                DeptData deptItem = deptData[i];
                if (i == 0) // inpatient dept
                {
                    //double areaNeeded = deptItem.DeptAreaNeeded;
                    double areaNeeded = deptItem.DeptAreaProportionNeeded * PolygonUtility.AreaPolygon(poly);
                    areaNeeded = 100000;
                    Dictionary<string, object> inpatientObject = AssignBlocksBasedOnDistance(poly, offset, areaNeeded, 10, recompute);
                    List<Polygon2d> inpatienBlocks = (List<Polygon2d>)inpatientObject["PolyAfterSplit"];
                    List<Polygon2d> leftOverBlocks = (List<Polygon2d>)inpatientObject["LeftOverPoly"];
                    areaAssigned = (double)inpatientObject["AreaAssignedToBlock"];
                    AllDeptPolys.Add(inpatienBlocks);
                    AllDeptAreaAdded.Add(areaAssigned);
                    for (int j = 0; j < leftOverBlocks.Count; j++)
                    {
                        otherDeptPoly.Add(new Polygon2d(leftOverBlocks[j].Points));
                        leftOverPoly.Add(leftOverBlocks[j]);
                    }              
                }
                if( i == 1)
                {
                    List<List<Polygon2d>> polySubDivs = SplitObject.SplitRecursivelyToSubdividePoly(leftOverPoly, acceptableWidth, circulationFreq, 0.5);
                    leftOverPoly = polySubDivs[0];
                    polyCirculation = polySubDivs[1];
                    for (int j = 0; j < leftOverPoly.Count; j++) areaAvailable += PolygonUtility.AreaPolygon(leftOverPoly[j]);
                    if (leftOverPoly == null) break;
                }

                if( i > 0 ) // other depts
                {
                    double areaFactor = deptItem.DeptAreaProportionNeeded / totalDeptProp;
                    Dictionary<string, object> assignedByRatioObj = AssignBlocksBasedOnRatio(areaFactor,areaAvailable, leftOverPoly,acceptableWidth,0.5);
                    List<Polygon2d> everyDeptPoly = (List<Polygon2d>)assignedByRatioObj["DeptPoly"];
                    leftOverPoly = (List<Polygon2d>)assignedByRatioObj["LeftOverPoly"];                 
                    areaAssigned = (double)assignedByRatioObj["AreaAdded"];
                    List<Node> AllNodesList = (List<Node>)assignedByRatioObj["AllNodes"];
                    AllDeptAreaAdded.Add(areaAssigned);
                    AllDeptPolys.Add(everyDeptPoly);
                }
            }
            List<DeptData> UpdatedDeptData = new List<DeptData>();
            //make the sorted dept data
            for (int i = 0; i < deptData.Count; i++)
            {
                DeptData newDeptData = new DeptData(deptData[i]);
                newDeptData.AreaProvided = AllDeptAreaAdded[i];
                newDeptData.PolyAssignedToDept = AllDeptPolys[i];
                UpdatedDeptData.Add(newDeptData);
            }

            //added to compute area percentage for each dept
            double totalDeptArea = 0;
            for (int i = 0; i < UpdatedDeptData.Count; i++) totalDeptArea += UpdatedDeptData[i].DeptAreaNeeded;
            for (int i = 0; i < UpdatedDeptData.Count; i++) UpdatedDeptData[i].DeptAreaProportionAchieved = Math.Round((UpdatedDeptData[i].AreaProvided / totalDeptArea), 3);

            if (leftOverPoly.Count == 0) leftOverPoly = null;

            return new Dictionary<string, object>
            {
                { "DeptPolys", (AllDeptPolys) },
                { "LeftOverPolys", (leftOverPoly) },
                { "CirculationPolys", (polyCirculation) },
                { "OtherDeptMainPoly", (otherDeptPoly) },
                { "UpdatedDeptData", (UpdatedDeptData) }
            };
        }

        //places dept and updates dept data with the added area and polys to each dept
        [MultiReturn(new[] { "DeptPolys", "LeftOverPolys", "CentralStation", "UpdatedDeptData", "SpaceDataTree" })]
        internal static Dictionary<string, object> DeptSplitRefined(Polygon2d poly, List<DeptData> deptData, List<Cell> cellInside, double offset, int recompute = 1)
        {
            List<List<Polygon2d>> AllDeptPolys = new List<List<Polygon2d>>();
            List<string> AllDepartmentNames = new List<string>();
            List<double> AllDeptAreaAdded = new List<double>();
            Stack<Polygon2d> leftOverPoly = new Stack<Polygon2d>();
            List<Node> AllNodesList = new List<Node>();
            List<Point2d> polyPts = PolygonUtility.SmoothPolygon(poly.Points, SPACING);
            poly = new Polygon2d(polyPts, 0);

            SortedDictionary<double, DeptData> sortedD = new SortedDictionary<double, DeptData>();
            for (int i = 0; i < deptData.Count; i++) sortedD.Add(deptData[i].DeptAreaNeeded, deptData[i]);

            List<DeptData> sortedDepartmentData = new List<DeptData>();
            foreach (KeyValuePair<double, DeptData> p in sortedD) sortedDepartmentData.Add(p.Value);
            sortedDepartmentData.Reverse();

            leftOverPoly.Push(poly);
            int dir = 0;
            double count3 = 0;

            for (int i = 0; i < sortedD.Count; i++)
            {
                DeptData deptItem = sortedDepartmentData[i];
                double areaDeptNeeds = deptItem.DeptAreaNeeded;
                double areaAddedToDept = 0;
                double areaLeftOverToAdd = areaDeptNeeds - areaAddedToDept;
                double areaCurrentPoly = 0;
                double perc = 0.2;
                double limit = areaDeptNeeds * perc;

                Polygon2d currentPolyObj = poly;
                List<Polygon2d> everyDeptPoly = new List<Polygon2d>();
                List<Polygon2d> polyAfterSplitting = new List<Polygon2d>();
                double count1 = 0;
                double count2 = 0;
                double areaCheck = 0;

                Random ran = new Random();
                Node spaceNode, containerNode;
                bool checkExternalWallAdj = false;

                // when dept is inpatient unit
                if (i == 0)
                {
                    while (areaLeftOverToAdd > limit && leftOverPoly.Count > 0 && count1 < MAXROUND)
                    {
                        dir = BasicUtility.ToggleInputInt(dir);
                        currentPolyObj = leftOverPoly.Pop();
                        areaCurrentPoly = PolygonUtility.AreaPolygon(currentPolyObj);
                        int countNew = 0, maxPlacement = 100;
                        double areaA = 0, areaB = 0;
                        List<Polygon2d> edgeSplitted = new List<Polygon2d>();
                        while (!checkExternalWallAdj && countNew < maxPlacement)
                        {
                            Dictionary<string, object> splitReturned = SplitObject.SplitByDistance(currentPolyObj, ran, offset, dir);
                            if (splitReturned == null)
                            {
                                Trace.WriteLine("Returning as splitbydistance did not work : ");
                                return null;
                            }
                            edgeSplitted = (List<Polygon2d>)splitReturned["PolyAfterSplit"];
                            if (!ValidateObject.CheckPolyList(edgeSplitted))
                            {
                                Trace.WriteLine("2  - ------- - Returning as splitbydistance did not work : ");
                                //return null;
                                countNew += 1;
                                continue;
                            }
                            areaA = PolygonUtility.AreaPolygon(edgeSplitted[0]);
                            areaB = PolygonUtility.AreaPolygon(edgeSplitted[1]);
                            //make a check on the returned polygon2d, if that gets an external wall or not
                            if (areaA < areaB) checkExternalWallAdj = LayoutUtility.CheckPolyGetsExternalWall(edgeSplitted[0], poly, offset);
                            else checkExternalWallAdj = LayoutUtility.CheckPolyGetsExternalWall(edgeSplitted[1], poly, offset);
                            countNew += 1;
                        }// end of while
                        if (areaA < areaB)
                        {
                            everyDeptPoly.Add(edgeSplitted[0]);
                            areaLeftOverToAdd = areaLeftOverToAdd - areaA;
                            areaCheck += areaA;
                            leftOverPoly.Push(edgeSplitted[1]);
                        }
                        else
                        {
                            everyDeptPoly.Add(edgeSplitted[1]);
                            areaLeftOverToAdd = areaLeftOverToAdd - areaB;
                            areaCheck += areaB;
                            leftOverPoly.Push(edgeSplitted[0]);
                        }
                        checkExternalWallAdj = false;
                        count1 += 1;
                    }// end of while loop

                    spaceNode = new Node(i, NodeType.Space);
                    containerNode = new Node(i, NodeType.Container);
                    AllNodesList.Add(spaceNode);
                    AllNodesList.Add(containerNode);
                }
                //when other depts 
                else
                {
                    Random rn = new Random();
                    while (areaLeftOverToAdd > limit && leftOverPoly.Count > 0 && count2 < MAXROUND)
                    {
                        double ratio = BasicUtility.RandomBetweenNumbers(rn, 0.85, 0.15);
                        currentPolyObj = leftOverPoly.Pop();
                        areaCurrentPoly = PolygonUtility.AreaPolygon(currentPolyObj);
                        dir = BasicUtility.ToggleInputInt(dir);
                        if (areaLeftOverToAdd > areaCurrentPoly)
                        {
                            everyDeptPoly.Add(currentPolyObj);
                            areaLeftOverToAdd = areaLeftOverToAdd - areaCurrentPoly;
                            areaCheck += areaCurrentPoly;
                            //Trace.WriteLine("Area left over after assigning when area is greater than current : " + areaLeftOverToAdd);
                        }
                        else
                        {
                            Dictionary<string, object> basicSplit = SplitObject.SplitByRatio(currentPolyObj, ratio, dir);
                            if (basicSplit == null)
                            {
                                Trace.WriteLine("Returning as splitbyratio did not work : ");
                                return null;
                            }
                            List<Polygon2d> polyS = (List<Polygon2d>)basicSplit["PolyAfterSplit"];
                            double areaA = PolygonUtility.AreaPolygon(polyS[0]);
                            double areaB = PolygonUtility.AreaPolygon(polyS[1]);
                            if (areaA < areaB)
                            {
                                everyDeptPoly.Add(polyS[0]);
                                areaLeftOverToAdd = areaLeftOverToAdd - areaA;
                                areaCheck += areaA;
                                leftOverPoly.Push(polyS[1]);
                            }
                            else
                            {
                                everyDeptPoly.Add(polyS[1]);
                                areaLeftOverToAdd = areaLeftOverToAdd - areaB;
                                areaCheck += areaB;
                                leftOverPoly.Push(polyS[0]);
                            }

                            spaceNode = new Node(i, NodeType.Space);
                            containerNode = new Node(i, NodeType.Container);
                            AllNodesList.Add(spaceNode);
                            AllNodesList.Add(containerNode);
                        }
                        count2 += 1;
                    } // end of while loop
                }

                AllDeptAreaAdded.Add(areaCheck);
                AllDeptPolys.Add(everyDeptPoly);
                AllDepartmentNames.Add(deptItem.DepartmentName);

            }// end of for loop
            double minArea = 10, areaMoreCheck = 0;
            AllDeptAreaAdded[0] += areaMoreCheck;
            // adding the left over polys to the 2nd highest dept after inpatient
            if (leftOverPoly.Count > 0)
            {
                double areaLeftOver = 0;
                for (int i = 0; i < leftOverPoly.Count; i++)
                {
                    Polygon2d pol = leftOverPoly.Pop();
                    areaLeftOver += PolygonUtility.AreaPolygon(pol);
                    AllDeptPolys[1].Add(pol);
                }
                AllDeptAreaAdded[1] += areaLeftOver;
            }

            List<DeptData> UpdatedDeptData = new List<DeptData>();
            //make the sorted dept data
            for (int i = 0; i < sortedDepartmentData.Count; i++)
            {
                DeptData newDeptData = new DeptData(sortedDepartmentData[i]);
                newDeptData.AreaProvided = AllDeptAreaAdded[i];
                newDeptData.PolyAssignedToDept = AllDeptPolys[i];
                UpdatedDeptData.Add(newDeptData);
            }

            List<Polygon2d> AllLeftOverPolys = new List<Polygon2d>();
            AllLeftOverPolys.AddRange(leftOverPoly);


            //make the centralStation on second highest dept
            Point2d centerPt = PolygonUtility.CentroidOfPoly(poly);
            Dictionary<string, object> centralPolyLists =LayoutUtility.MakeCentralStation(AllDeptPolys[1], centerPt);
            List<Polygon2d> polyReturned = new List<Polygon2d>();
            if (centralPolyLists != null)
            {
                int index = (int)centralPolyLists["IndexInPatientPoly"];
                polyReturned = (List<Polygon2d>)centralPolyLists["PolyCentral"];

                if (polyReturned.Count > 1)
                {
                    AllDeptPolys[1].RemoveAt(index);
                    AllDeptPolys[1].Add(polyReturned[1]);
                }
                else AllDeptPolys[1][index] = polyReturned[0];
            }
            else
            {
                polyReturned.Add(null);
            }


            //create space data tree
            double spaceX = 22;
            double spaceY = 13;
            double nodeRadius = 4;
            Point origin = Point.ByCoordinates(500, 0);
            Node root = new Node(0, NodeType.Container, true, origin, nodeRadius);
            Dictionary<string, object> SpaceTreeData = CreateSpaceTreeFromDeptData(root, AllNodesList, origin, spaceX, spaceY, nodeRadius, true);

            Trace.WriteLine("Dept Splitting Done ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            //return polyList;

            return new Dictionary<string, object>
            {
                { "DeptPolys", (AllDeptPolys) },
                { "LeftOverPolys", (AllLeftOverPolys) },
                { "CentralStation", (polyReturned[0]) }, //polyReturned[0]
                { "UpdatedDeptData", (UpdatedDeptData)},
                { "SpaceDataTree", (SpaceTreeData) }
            };


        }

 
        #endregion

    }
}
