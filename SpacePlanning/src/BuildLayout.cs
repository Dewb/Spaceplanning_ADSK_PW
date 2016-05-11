using System;
using System.Collections.Generic;
using stuffer;
using Autodesk.DesignScript.Runtime;
using System.Diagnostics;
using Autodesk.DesignScript.Geometry;
using System.Linq;

namespace SpacePlanning
{
    public class BuildLayout
    {
        
        internal static double spacingSet = 10; //higher value makes code faster, 6, 10 was good too
        internal static double spacingSet2 = 10;
        internal static Random ranGenerate = new Random();
        internal static double recurse = 0;
        internal static Point2d reference = new Point2d(0,0);
        internal static int maxCount = 200, maxRound = 50;      

        #region - Public Methods

        //use makePolypointsOrtho multiple times
        public static Polygon2d FitPolyToBeOrtho(Polygon2d inputPoly, int times = 5)
        {
            Polygon2d polyReturn = MakePolyPointsOrtho(inputPoly);
            for (int i=0; i< times; i++)
            {
                polyReturn = MakePolyPointsOrtho(inputPoly);
            }
            return polyReturn;
        }

        //make the given poly all points orthonogonal to each other
        public static Polygon2d MakePolyPointsOrtho(Polygon2d poly)
        {
            Polygon2d polyReg = new Polygon2d(poly.Points);
            List<Point2d> ptForOrthoPoly = new List<Point2d>();
            for(int i = 0; i < polyReg.Points.Count; i++)
            {
                Point2d pt = Point2d.ByCoordinates(polyReg.Points[i].X, polyReg.Points[i].Y);
                ptForOrthoPoly.Add(pt);
            }
           
            for(int i = 0; i < polyReg.Points.Count; i++)
            {
                int a = i, b = i + 1;
                double eps = 50;
                if (i == polyReg.Points.Count - 1) b = 0;
                Line2d line = new Line2d(polyReg.Points[a], polyReg.Points[b]);
                if(GraphicsUtility.CheckLineOrient(line) == -1)
                {
                    //double diffX = Math.Abs(line.StartPoint.X - line.EndPoint.X);
                    //double diffY = Math.Abs(line.StartPoint.Y - line.EndPoint.Y);
                    Point2d cenPoly = PolygonUtility.CentroidFromPoly(polyReg);
                    Point2d ptEndA = Point2d.ByCoordinates(polyReg.Points[a].X + eps, polyReg.Points[a].Y);
                    Line2d refLineA = Line2d.ByStartPointEndPoint(polyReg.Points[a], ptEndA);

                    Point2d ptEndB = Point2d.ByCoordinates(polyReg.Points[b].X + eps, polyReg.Points[b].Y);
                    Line2d refLineB = Line2d.ByStartPointEndPoint(polyReg.Points[b], ptEndB);

                    Point2d projectedPtA = GraphicsUtility.ProjectedPointOnLine(refLineB, polyReg.Points[a]);
                    Point2d projectedPtB = GraphicsUtility.ProjectedPointOnLine(refLineA, polyReg.Points[b]);

                    Vector2d vecA = new Vector2d(projectedPtA, cenPoly);
                    Vector2d vecB = new Vector2d(projectedPtB, cenPoly);
                    double vecALength = vecA.Length;
                    double vecBLength = vecB.Length;
                    if (vecALength > vecBLength)
                    {
                        //ptForOrthoPoly[i] = projectedPtA;
                        ptForOrthoPoly.Insert(b, projectedPtB);
                    }
                    else
                    {
                        //ptForOrthoPoly[i] = projectedPtB;
                        ptForOrthoPoly.Insert(b, projectedPtA);
                    }

                    /*
                    if (diffX > diffY)
                    {
                        Point2d ptEndA = Point2d.ByCoordinates(polyReg.Points[a].X, polyReg.Points[a].Y + eps);
                        Line2d refLineA = Line2d.ByStartPointEndPoint(polyReg.Points[a], ptEndA);
                        refLineA = LineUtility.extend(refLineA);

                        Point2d ptEndB = Point2d.ByCoordinates(polyReg.Points[b].X, polyReg.Points[b].Y + eps);
                        Line2d refLineB = Line2d.ByStartPointEndPoint(polyReg.Points[b], ptEndB);
                        refLineB = LineUtility.extend(refLineB);

                        Point2d projectedPtA = GraphicsUtility.ProjectedPointOnLine(refLineB, polyReg.Points[a]);
                        Point2d projectedPtB = GraphicsUtility.ProjectedPointOnLine(refLineA, polyReg.Points[b]);

                        Vector2d vecA = new Vector2d(projectedPtA, cenPoly);
                        Vector2d vecB = new Vector2d(projectedPtB, cenPoly);
                        double vecALength = vecA.Length;
                        double vecBLength = vecB.Length;
                        if(vecALength < vecBLength)
                        {
                            //ptForOrthoPoly[i] = projectedPtA;
                            ptForOrthoPoly.Insert(b, projectedPtB);
                        }
                        else
                        {
                            //ptForOrthoPoly[i] = projectedPtB;
                            ptForOrthoPoly.Insert(b, projectedPtA);
                        }
                    }
                    else
                    {

                        Point2d ptEndA = Point2d.ByCoordinates(polyReg.Points[a].X + eps, polyReg.Points[a].Y);
                        Line2d refLineA = Line2d.ByStartPointEndPoint(polyReg.Points[a], ptEndA);
                        refLineA = LineUtility.extend(refLineA);

                        Point2d ptEndB = Point2d.ByCoordinates(polyReg.Points[b].X + eps, polyReg.Points[b].Y);
                        Line2d refLineB = Line2d.ByStartPointEndPoint(polyReg.Points[b], ptEndB);
                        refLineB = LineUtility.extend(refLineB);

                        Point2d projectedPtA = GraphicsUtility.ProjectedPointOnLine(refLineB, polyReg.Points[a]);
                        Point2d projectedPtB = GraphicsUtility.ProjectedPointOnLine(refLineA, polyReg.Points[b]);

                        Vector2d vecA = new Vector2d(projectedPtA, cenPoly);
                        Vector2d vecB = new Vector2d(projectedPtB, cenPoly);
                        double vecALength = vecA.Length;
                        double vecBLength = vecB.Length;
                        if (vecALength < vecBLength)
                        {
                            //ptForOrthoPoly[i] = projectedPtA;
                            ptForOrthoPoly.Insert(b, projectedPtB);
                        }
                        else
                        {
                            //ptForOrthoPoly[i] = projectedPtB;
                            ptForOrthoPoly.Insert(b, projectedPtB);
                        }
                    }
                    */
                }
            }
            return new Polygon2d(ptForOrthoPoly); 
        }

        //check if a given polygon2d has any of its longer edges aligned with any edge of the containerpolygon2d
        public static bool CheckPolyGetsExternalWall(Polygon2d poly, Polygon2d containerPoly, double shortEdgeDist = 16, bool tag = true)
        {
            bool check = false;
            if (!PolygonUtility.CheckPoly(poly)) return check;
            Polygon2d polyReg = new Polygon2d(null);
            //make given polys reduce number of points
            if (tag) polyReg = new Polygon2d(poly.Points);
            else polyReg = poly;
            Polygon2d containerPolyReg = new Polygon2d(containerPoly.Points);
            for(int i = 0; i < polyReg.Points.Count; i++)
            {
                int a = i, b = i + 1;
                double eps = 0;
                if (i == polyReg.Points.Count-1) b = 0;
                double distance = GraphicsUtility.DistanceBetweenPoints(polyReg.Points[a], polyReg.Points[b]);
                List<double> spansSorted = PolygonUtility.PolySpanCheck(containerPolyReg);
                if (distance <= spansSorted[0]*0.75) continue;
                Line2d lineA = Line2d.ByStartPointEndPoint(polyReg.Points[a], polyReg.Points[b]);
                for(int j = 0; j < containerPolyReg.Points.Count; j++)
                {
                    int c = j, d = j + 1;
                    if (j == containerPolyReg.Points.Count - 1) d = 0;
                    Line2d lineB = Line2d.ByStartPointEndPoint(containerPolyReg.Points[c], containerPolyReg.Points[d]);
                    check = GraphicsUtility.LineAdjacencyCheck(lineA, lineB, eps);
                    if (check) break;
                }
                if (check) break;
            }
            return check;
        }


        //check if a given polygon2d has any of its longer edges aligned with any edge of the containerpolygon2d
        public static bool CheckLineGetsExternalWall(Line2d lineA, Polygon2d containerPoly)
        {
            bool check = false;
            if (!PolygonUtility.CheckPoly(containerPoly)) return check;      
            Polygon2d containerPolyReg = new Polygon2d(containerPoly.Points);
            for (int i = 0; i < containerPoly.Points.Count; i++)
            {
                int a = i, b = i + 1;
                if (i == containerPolyReg.Points.Count - 1) b = 0;
                Line2d lineB = Line2d.ByStartPointEndPoint(containerPolyReg.Points[a], containerPolyReg.Points[b]);
                check = GraphicsUtility.LineAdjacencyCheck(lineA, lineB);
                if (check) break;
            }
            return check;
        }


        //arrange depts on site, till all depts have not been satisfied
        /// <summary>
        /// It arranges the dept on the site based on input from program document
        /// Returns the dept polygon2d and dept  .
        /// </summary>
        /// <param name="Poly">Site Outline where space planning to be placed</param>
        /// <param name="DeptData">Dept Data Object from csv file</param>
        /// <param name="CellsInside">List of Cell Objects inside the site outline</param>
        /// <param name="Offset">Distance of Space Division</param>
        /// <param name="Recompute">Run the function again</param>
        /// <returns name="DeptPolys">Polys for each depts</param>
        /// <returns name="LeftOverPolys">Polys which has not been assigned to any dept</param>
        /// <returns name="CentralStation">Poly for central nurse station or lobby or foyer</param>
        /// <returns name="UpdatedDeptData">Updated Dept Data Object having </param>
        /// <returns name="SpaceDataTree">Space Data Structure - a Binary Tree </param>
        /// <search>
        /// dept data arrangement on sie
        /// </search>
        [MultiReturn(new[] { "DeptPolys", "LeftOverPolys", "CentralStation", "UpdatedDeptData","SpaceDataTree" })]
        public static Dictionary<string, object> DeptArrangeOnSite(Polygon2d poly, List<DeptData> deptData, List<Cell> cellInside, double offset, int recompute = 1)
        {
            Dictionary<string, object> deptArrangement = new Dictionary<string, object>();
            //deptArrangement = DeptSplitRefined(poly, deptData, cellInside, offset, 1);
            double count = 0;            
            Random rand = new Random();
            bool deptPlaced = false;
            while(deptPlaced == false && count < maxCount)
            {
                Trace.WriteLine("Lets Go Again for : " + count);
                int reco = rand.Next();
                deptArrangement = DeptSplitRefined(poly, deptData, cellInside, offset, recompute);
                if(deptArrangement != null)
                {
                    List<List<Polygon2d>> deptAllPolys =(List<List<Polygon2d>>) deptArrangement["DeptPolys"];
                    for(int i = 0; i < deptAllPolys.Count; i++)
                    {
                        List<Polygon2d> eachDeptPoly = deptAllPolys[i];
                        if (PolygonUtility.CheckPolyList(eachDeptPoly)) deptPlaced = true;
                        else { deptPlaced = false; break; }
                    }
                }
                count += 1;
            }
            return deptArrangement;
        }

  
        //get a poly and find rectangular polys inside. then merge them together to form a big poly
        [MultiReturn(new[] { "SplittableLines", "OffsetLines", "SortedIndices", "OffsetMidPts" })]
        public static Dictionary<string, object> ExtLinesAndOffsetsFromBBox(Polygon2d poly, double patientRoomDepth = 16, double recompute = 5)
        {
            if (!PolygonUtility.CheckPoly(poly)) return null;
            Polygon2d polyReg = new Polygon2d(poly.Points);
            List<Line2d> allSplitLines = new List<Line2d>();
            Polygon2d polyBBox = Polygon2d.ByPoints(ReadData.FromPointsGetBoundingPoly(polyReg.Points));
            allSplitLines = polyBBox.Lines;
            double[] splitLineLength = new double[allSplitLines.Count];
            int[] unsortedIndices = new int[allSplitLines.Count];
            for (int i = 0; i < allSplitLines.Count; i++)
            {
                splitLineLength[i] = allSplitLines[i].Length;
                unsortedIndices[i] = i;
            }
            List<int> sortedIndices = BasicUtility.Quicksort(splitLineLength, unsortedIndices, 0, allSplitLines.Count - 1);
            if (sortedIndices != null) sortedIndices.Reverse();

            List<Line2d> offsetLines = new List<Line2d>();
            List<Point2d> midPtsOffsets = new List<Point2d>();
            for (int i = 0; i < allSplitLines.Count; i++)
            {
                offsetLines.Add(LineUtility.Offset(allSplitLines[i], poly, patientRoomDepth));
                midPtsOffsets.Add(LineUtility.NudgeLineMidPt(allSplitLines[i], poly, patientRoomDepth));
            }

            List<Line2d> offsetSortedLines = new List<Line2d>();
            for (int i = 0; i < offsetLines.Count; i++) offsetSortedLines.Add(offsetLines[sortedIndices[i]]);
            return new Dictionary<string, object>
            {
                { "SplittableLines", (allSplitLines) },
                { "OffsetLines", (offsetSortedLines) },
                { "SortedIndices", (sortedIndices) },
                { "OffsetMidPts", (midPtsOffsets) }          
            };
        }


        //get a poly and find rectangular polys inside. then merge them together to form a big poly
        [MultiReturn(new[] { "SplittableLines", "OffsetLines","SortedIndices", "OffsetMidPts", "NonOrthoLines" })]
        public static Dictionary<string, object> FindOuterLinesAndOffsets(Polygon2d poly, double patientRoomDepth = 16, double extension = 8000,double recompute = 5)
        {
            if (!PolygonUtility.CheckPoly(poly)) return null;
            Polygon2d polyReg = new Polygon2d(poly.Points);
            List<Line2d> hLines = new List<Line2d>();
            List<Line2d> vLines = new List<Line2d>();
            List<Point2d> hMidPt = new List<Point2d>();
            List<Point2d> vMidPt = new List<Point2d>();
            List<Line2d> nonOrthoLines = new List<Line2d>();
            int countOrtho = 0, countNonOrtho = 0;
            for (int i = 0; i < polyReg.Points.Count; i++)
            {
                int a = i, b = i + 1;
                if (i == polyReg.Points.Count - 1) b = 0;
                Line2d line = new Line2d(polyReg.Points[a], polyReg.Points[b]);
                int lineType = GraphicsUtility.CheckLineOrient(line);
                if (lineType > -1)
                {
                    if (lineType == 0)
                    {
                        Line2d extendedLine = LineUtility.ExtendLine(line, extension);
                        hLines.Add(extendedLine);
                        hMidPt.Add(LineUtility.LineMidPoint(line));
                    }
                    if (lineType == 1)
                    {
                        Line2d extendedLine = LineUtility.ExtendLine(line, extension);
                        vLines.Add(extendedLine);
                        vMidPt.Add(LineUtility.LineMidPoint(line));
                    }
                    countOrtho += 1;
                }
                else
                {
                    Trace.WriteLine("----------------");
                    Trace.WriteLine("NonOrhto Start Point X : " + line.StartPoint.X + "   NonOrhto Start Point Y : " + line.StartPoint.Y);
                    Trace.WriteLine("NonOrhto End Point X : " + line.EndPoint.X + "   NonOrhto End Point Y : " + line.EndPoint.Y);
                    Trace.WriteLine("----------------");
                    countNonOrtho += 1;
                    nonOrthoLines.Add(line);
                }
            }
            Trace.WriteLine("Orhto lines are : " + countOrtho + "   Non ortho lines are : " + countNonOrtho);
            List<Line2d> selectedHLines = new List<Line2d>();
            List<Line2d> selectedVLines = new List<Line2d>();
            int hIndLow = TestGraphicsUtility.ReturnLowestPointFromList(hMidPt);
            int hIndHigh = TestGraphicsUtility.ReturnHighestPointFromList(hMidPt);
            int vIndLow = GraphicsUtility.ReturnLowestPointFromListNew(vMidPt);
            int vIndHigh = GraphicsUtility.ReturnHighestPointFromListNew(vMidPt);
            if (hIndLow > -1) selectedHLines.Add(hLines[hIndLow]);
            if (hIndHigh > -1) selectedHLines.Add(hLines[hIndHigh]);
            if (vIndLow > -1) selectedVLines.Add(vLines[vIndLow]);
            if (vIndHigh > -1) selectedVLines.Add(vLines[vIndHigh]);

            List<Line2d> allSplitLines = new List<Line2d>();
            allSplitLines.AddRange(selectedHLines);
            allSplitLines.AddRange(selectedVLines);

            double[] splitLineLength = new double[allSplitLines.Count];
            int[] unsortedIndices = new int[allSplitLines.Count];
            for (int i = 0; i < allSplitLines.Count; i++)
            {
                splitLineLength[i] = allSplitLines[i].Length;
                unsortedIndices[i] = i;
            }
            List<int> sortedIndices = BasicUtility.Quicksort(splitLineLength, unsortedIndices, 0, allSplitLines.Count - 1);
            if(sortedIndices != null) sortedIndices.Reverse();

            List<Line2d> offsetLines = new List<Line2d>();
            List<Point2d> midPtsOffsets = new List<Point2d>();
            for (int i = 0; i < allSplitLines.Count; i++)
            {
                offsetLines.Add(LineUtility.Offset(allSplitLines[i], poly, patientRoomDepth));
                midPtsOffsets.Add(LineUtility.NudgeLineMidPt(allSplitLines[i], poly, patientRoomDepth));
            }

            List<Line2d> offsetSortedLines = new List<Line2d>();
            for(int i = 0; i < offsetLines.Count; i++) offsetSortedLines.Add(offsetLines[sortedIndices[i]]);
            return new Dictionary<string, object>
            {
                { "SplittableLines", (allSplitLines) },
                { "OffsetLines", (offsetSortedLines) },
                { "SortedIndices", (sortedIndices) },
                { "OffsetMidPts", (midPtsOffsets) },
                { "NonOrthoLines", (nonOrthoLines) }
            };
        }

         //blocks are assigne based on offset distance, used for inpatient blocks
        [MultiReturn(new[] { "PolyAfterSplit", "LeftOverPoly", "AreaAssignedToBlock","FalseLines", "LineOptions","PointAdded" })]
        public static Dictionary<string, object> AssignBlocksBasedOnDistance(Polygon2d poly, double distance = 16, double area = 0, double thresDistance = 10, double recompute = 5)
        {
           
            if (!PolygonUtility.CheckPoly(poly)) return null;
            if (distance < 1) return null;
            bool externalIncude = false;
            if (recompute > 5) externalIncude = true;
            int count = 0, maxTry = 100;
            poly = new Polygon2d(poly.Points);
            if (area == 0) area = 0.8 * PolygonUtility.AreaCheckPolygon(poly);
            Stack<Polygon2d> polyLeftList = new Stack<Polygon2d>();
            double areaAdded = 0;
            polyLeftList.Push(poly);
            Point2d pointAdd = new Point2d(0, 0);
            List<Polygon2d> blockPolyList = new List<Polygon2d>();
            List<Polygon2d> leftoverPolyList = new List<Polygon2d>();
            List<Line2d> falseLines = new List<Line2d>();
            List<Line2d> lineOptions = new List<Line2d>();
            bool error = false;
            while (polyLeftList.Count > 0 && areaAdded < area && count < recompute) //count<recompute
            {
                Polygon2d currentPoly = polyLeftList.Pop();
                Polygon2d tempPoly = new Polygon2d(currentPoly.Points,0);
                Dictionary<string, object> splitObject = CreateBlocksByLines(currentPoly, poly, distance, thresDistance, externalIncude);
                if (splitObject == null) { count += 1; Trace.WriteLine("Split errored");  continue; }
                Polygon2d blockPoly = (Polygon2d)splitObject["PolyAfterSplit"];
                Polygon2d leftPoly = (Polygon2d)splitObject["LeftOverPoly"];
                lineOptions = (List<Line2d>)splitObject["LineOptions"];
                Dictionary<string, object> addPtObj = AddPointToFitPoly(leftPoly, poly, distance, thresDistance, recompute);
                leftPoly = (Polygon2d)addPtObj["PolyAddedPts"];
                falseLines = (List<Line2d>)addPtObj["FalseLineList"];
                pointAdd = (Point2d)addPtObj["PointAdded"];             
                areaAdded += PolygonUtility.AreaCheckPolygon(blockPoly);
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

        //blocks are assigned based on ratio of split, used for assigning other depts
        [MultiReturn(new[] { "DeptPoly", "LeftOverPoly", "AreaAdded", "AllNodes" })]
        public static Dictionary<string, object> AssignBlocksBasedOnRatio(DeptData deptItem, List<Polygon2d> leftOverPoly, int index)
        {

            Random rn = new Random();
            int count2 = 0, dir = 0;
            double areaCurrentPoly = 0, areaPlaced = 0;
            List<Node> AllNodesList = new List<Node>();
            List<Polygon2d> everyDeptPoly = new List<Polygon2d>();
            double areaAvailable = 0;
            Stack<Polygon2d> leftOverPolyStack = new Stack<Polygon2d>();
            for (int i = 0; i < leftOverPoly.Count; i++)
            {
                areaAvailable += PolygonUtility.AreaCheckPolygon(leftOverPoly[i]);
                leftOverPolyStack.Push(leftOverPoly[i]);
            }
            

            double areaDeptNeeds = deptItem.DeptAreaProportion * areaAvailable;
            //double areaDeptNeeds = deptItem.DeptAreaNeeded;
            double areaAddedToDept = 0;
            double areaLeftOverToAdd = areaDeptNeeds - areaAddedToDept;
            double perc = 0.2;
            double limit = areaDeptNeeds * perc;

            while (areaLeftOverToAdd > limit && leftOverPolyStack.Count > 0 && count2 < maxRound)
            {
                double ratio = BasicUtility.RandomBetweenNumbers(rn, 0.85, 0.15);
                Polygon2d currentPolyObj = leftOverPolyStack.Pop();
                areaCurrentPoly = PolygonUtility.AreaCheckPolygon(currentPolyObj);
                dir = BasicUtility.ToggleInputInt(dir);
                if (areaLeftOverToAdd > areaCurrentPoly)
                {
                    everyDeptPoly.Add(currentPolyObj);
                    areaLeftOverToAdd = areaLeftOverToAdd - areaCurrentPoly;
                    areaPlaced += areaCurrentPoly;
                }
                else
                {
                    Dictionary<string, object> basicSplit = SplitObject.SplitByRatio(currentPolyObj, ratio, dir);
                    if (basicSplit == null) return null;
                    List<Polygon2d> polyS = (List<Polygon2d>)basicSplit["PolyAfterSplit"];
                    double areaA = PolygonUtility.AreaCheckPolygon(polyS[0]);
                    double areaB = PolygonUtility.AreaCheckPolygon(polyS[1]);
                    if (areaA < areaB)
                    {
                        everyDeptPoly.Add(polyS[0]);
                        areaLeftOverToAdd = areaLeftOverToAdd - areaA;
                        areaPlaced += areaA;
                        leftOverPolyStack.Push(polyS[1]);
                    }
                    else
                    {
                        everyDeptPoly.Add(polyS[1]);
                        areaLeftOverToAdd = areaLeftOverToAdd - areaB;
                        areaPlaced += areaB;
                        leftOverPolyStack.Push(polyS[0]);
                    }
                    Node spaceNode = new Node(index, NodeType.Space);
                    Node containerNode = new Node(index, NodeType.Container);
                    AllNodesList.Add(spaceNode);
                    AllNodesList.Add(containerNode);
                }
                count2 += 1;
            } // end of while loop            

            return new Dictionary<string, object>
            {
                { "DeptPoly", (everyDeptPoly) },
                { "LeftOverPoly", (leftOverPolyStack.ToList()) },
                { "AreaAdded", (areaPlaced) },
                { "AllNodes", (AllNodesList)}
            };
        }

        //blocks are assigned based on ratio of split, used for assigning other depts
        [MultiReturn(new[] { "DeptPoly", "LeftOverPoly", "AllPolys", "AreaAdded", "AllNodes" })]
        public static Dictionary<string, object> AssignBlocksBasedOnRatioRecursed(DeptData deptItem, double areaFactor, List<Polygon2d> polyList, double acceptableWidth = 10, double ratio = 0.5)
        {
            //if (!PolygonUtility.CheckPolyList(polyList)) return null;           
            int count = 0, maxTry = 200;
            double areaAvailable = 0;
            for (int i = 0; i < polyList.Count; i++) areaAvailable += PolygonUtility.AreaCheckPolygon(polyList[i]);
            Queue<Polygon2d> polyAvailable = new Queue<Polygon2d>();
            List<Polygon2d> polysToDept = new List<Polygon2d>(), leftOverPoly = new List<Polygon2d>();
            for (int i = 0; i < polyList.Count; i++) polyAvailable.Enqueue(polyList[i]);
            double deptAreaTarget = areaFactor * areaAvailable, areaAssigned = 0;
            //double deptAreaTarget = deptItem.DeptAreaNeeded,areaAssigned = 0;
            while (areaAssigned < deptAreaTarget && polyAvailable.Count > 0)
            {
                Polygon2d currentPoly = polyAvailable.Dequeue();
                areaAssigned += PolygonUtility.AreaCheckPolygon(currentPoly);
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



        //blocks are assigned based on ratio of split, used for assigning other depts
        [MultiReturn(new[] { "DeptPoly", "LeftOverPoly","AllPolys", "AreaAdded", "AllNodes" })]
        public static Dictionary<string, object> AssignBlocksBasedOnMinEdgeLength(DeptData deptItem, List<Polygon2d> polyList, double acceptableWidth = 10, double factor =5)
        {
            bool leftOver = false;
            Queue<Polygon2d> polyAvailable = new Queue<Polygon2d>();
            List<Polygon2d> polysToDept = new List<Polygon2d>(), leftOverPoly = new List<Polygon2d>();
            double areaAvailable = 0;
            for (int i = 0; i < polyList.Count; i++)
            {
                areaAvailable += PolygonUtility.AreaCheckPolygon(polyList[i]);
                polyAvailable.Enqueue(polyList[i]);
            }

            double deptAreaTarget = deptItem.DeptAreaProportion * areaAvailable, areaAssigned = 0;
            while (areaAssigned < deptAreaTarget && polyAvailable.Count>0)
            {
                Polygon2d currentPoly = polyAvailable.Dequeue();
                areaAssigned += PolygonUtility.AreaCheckPolygon(currentPoly);
                polysToDept.Add(currentPoly);
            }
            for (int i = 0; i < polyAvailable.Count; i++)
            {
                leftOver = true;
                leftOverPoly.Add(polyAvailable.Dequeue());
            }
            return new Dictionary<string, object>
            {
                { "DeptPoly", (polysToDept) },
                { "LeftOverPoly", (leftOverPoly) },
                { "AllPolys", (polyList)},
                { "AreaAdded", (areaAssigned) },
                { "AllNodes", (null)}
            };
        }

        //subdivide a given poly into smaller parts till acceptable width is met
        public static List<Polygon2d> SubdivideInputPoly(List<Polygon2d> polyList, double acceptableWidth = 10, double ratio = 0.5)
        {
            //if (!PolygonUtility.CheckPolyList(polyList)) return null;

            int count = 0, maxTry = 200;
            Queue<Polygon2d> polyQueue = new Queue<Polygon2d>();
            List<Polygon2d> polyBrokenList = new List<Polygon2d>();
            for (int i = 0; i < polyList.Count; i++) polyQueue.Enqueue(polyList[i]);
            while (polyQueue.Count > 0)
            {
                Polygon2d currentPoly = polyQueue.Dequeue();
                Dictionary<string, object> splitObj = SplitObject.SplitByRatio(currentPoly, ratio, 0);
                List<Polygon2d> polySplitList = (List<Polygon2d>)splitObj["PolyAfterSplit"];
                if (PolygonUtility.CheckPolyList(polySplitList) && polySplitList.Count > 1)
                {
                    polySplitList = PolygonUtility.SmoothPolygon(polySplitList, 2);
                    Polygon2d bbox1 = Polygon2d.ByPoints(ReadData.FromPointsGetBoundingPoly(polySplitList[0].Points));
                    Polygon2d bbox2 = Polygon2d.ByPoints(ReadData.FromPointsGetBoundingPoly(polySplitList[1].Points));
                    if (bbox1.Lines[0].Length < acceptableWidth || bbox1.Lines[1].Length < acceptableWidth) polyBrokenList.Add(polySplitList[0]);
                    else polyQueue.Enqueue(polySplitList[0]);
                    if (bbox2.Lines[0].Length < acceptableWidth || bbox2.Lines[1].Length < acceptableWidth) polyBrokenList.Add(polySplitList[1]);
                    else polyQueue.Enqueue(polySplitList[1]);
                }

                if (PolygonUtility.CheckPolyList(polySplitList) && polySplitList.Count < 2)
                {
                    Polygon2d bbox1 = Polygon2d.ByPoints(ReadData.FromPointsGetBoundingPoly(polySplitList[0].Points));
                    if (bbox1.Lines[0].Length < acceptableWidth || bbox1.Lines[1].Length < acceptableWidth) polyBrokenList.Add(polySplitList[0]);
                    else polyQueue.Enqueue(polySplitList[0]);
                }
                count += 1;
            }

            for (int i = 0; i < polyQueue.Count; i++) polyBrokenList.Add(polyQueue.Dequeue());
            return polyBrokenList;
            //return polyQueue.ToList();
        }

       
        //adds a point to a line of a poly, such that offsetting places offset line inside the poly
        [MultiReturn(new[] { "PolyAddedPts", "ProblemPoint", "IsAdded","PointAdded", "Trials", "FinalRatio","ProblemLine", "ProblemPtsList", "FalseLineList" })]
        public static Dictionary<string, object> AddPointToFitPoly(Polygon2d poly, Polygon2d containerPoly, double distance = 16, double area = 0, double thresDistance = 10, double recompute = 5)
        {
            if (!PolygonUtility.CheckPoly(poly)) return null;
            if (distance < 1) return null;         

            Dictionary<string, object> lineOffsetCheckObj = PolygonUtility.CheckLinesOffsetInPoly(poly, containerPoly, distance);
            List<int> indicesFalse = (List<int>)lineOffsetCheckObj["IndicesFalse"];
            List<List<Point2d>> pointsFalse = (List<List<Point2d>>)lineOffsetCheckObj["PointsOutside"];
            List<Point2d> probPointList = new List<Point2d>();
            List<Point2d> polyNewPoints = new List<Point2d>();
            List<Line2d> falseLines = new List<Line2d>();
            Point2d ptNewEnd = new Point2d(0, 0);
            Point2d otherPt = new Point2d(0, 0);
            Point2d probPt = new Point2d(0, 0);
            Line2d line = new Line2d(ptNewEnd, otherPt);
            int count = 0, maxTry = 50;
            double ratio = 0, increment = .25;
            bool added = false, checkOffPtNew = false;
            for (int i = 0; i < poly.Points.Count; i++)
            {                               
                int a = i, b = i + 1;
                if (i == poly.Points.Count - 1) b = 0;
                polyNewPoints.Add(poly.Points[a]);
                if (indicesFalse[i] > -1) falseLines.Add(poly.Lines[i]);
                if (poly.Lines[i].Length > thresDistance && 
                    indicesFalse[i] > -1 && pointsFalse[i] != null && pointsFalse[i].Count == 1 && !added && CheckLineGetsExternalWall(poly.Lines[i],containerPoly))
                {
                    probPointList.AddRange(pointsFalse[i]);
                    probPt = pointsFalse[i][0];
                    line = poly.Lines[i];
                    Point2d midPt = LineUtility.LineMidPoint(line);
                    if (line.StartPoint.Compare(probPt)) otherPt = line.EndPoint;
                    else otherPt = line.StartPoint;
                    Vector2d vecToOther = new Vector2d(probPt, otherPt);
                    while (!checkOffPtNew && count < maxTry && ratio < 0.9)
                    {
                        ratio += increment;
                        ptNewEnd = VectorUtility.VectorAddToPoint(probPt, vecToOther, ratio);
                        Point2d offPtNew = LineUtility.OffsetPointInsidePoly(line, ptNewEnd, poly, distance);
                        checkOffPtNew = GraphicsUtility.PointInsidePolygonTest(poly, offPtNew);
                        count += 1;
                    }                  
                    polyNewPoints.Add(ptNewEnd);
                    added = true;                     
                }
            }
            Polygon2d polyAdded = new Polygon2d(polyNewPoints, 0);
            return new Dictionary<string, object>
            {
                { "PolyAddedPts", (polyAdded) },
                { "ProblemPoint", (probPt)},
                { "IsAdded" , (added) },
                { "PointAdded", (ptNewEnd) },
                { "Trials", (count) },
                { "FinalRatio", (ratio) },
                { "ProblemLine", (line)},
                { "ProblemPtsList", ( probPointList) },
                { "FalseLineList", (falseLines) }
            };
        }

    

        //get a poly and find rectangular polys inside. then merge them together to form a big poly
        [MultiReturn(new[] { "InpatientPolys", "SplitablePolys", "LeftOverpolys", "SplittableLines","OffsetLines","Count","SplitLineLast", "CleanedOffsetLines", "UsedLines"})]
        public static Dictionary<string, object> MakeInpatientBlocks(Polygon2d poly,  double patientRoomDepth = 16, double recompute = 5)
        {
            if (!PolygonUtility.CheckPoly(poly)) return null;
            List<Polygon2d> inPatientPolyList = new List<Polygon2d>();
            List<Polygon2d> leftOverPolyList = new List<Polygon2d>();
            List<Line2d> allSplitLinesOut = new List<Line2d>();
            List<Line2d> offsetLinesOut = new List<Line2d>();
            List<Line2d> offsetLinesCleanedOut = new List<Line2d>();
            List<Line2d> usedLineList = new List<Line2d>();
            List<int> sortedIndices = new List<int>();
            List<Cell> latestMergedCells = new List<Cell>();
            bool splitDone = false;
            int maxTry = 500, count = 0, selectLineNum = 0;
            double  threshArea = 500;
            Random ran = new Random();
            List<Line2d> offsetLines = new List<Line2d>();
            List<Line2d> offsetCleanedLines = new List<Line2d>();

            Stack<Polygon2d> splitablePolys = new Stack<Polygon2d>();
            splitablePolys.Push(poly);
      
            while (splitablePolys.Count > 0 && count < recompute)
            {
                Trace.WriteLine("Starting iteration ++++++++++++++++++++++++++++++ : " + count);
                Polygon2d currentPoly = splitablePolys.Pop();
                if(PolygonUtility.AreaCheckPolygon(currentPoly) < threshArea)
                {   count += 1;
                    Trace.WriteLine("Well the area was not enough!");
                    continue;
                }
                Dictionary<string, object> outerLineObj = ExtLinesAndOffsetsFromBBox(currentPoly, patientRoomDepth,count);
                List<Line2d> allSplitLines = (List<Line2d>)outerLineObj["SplittableLines"];
                offsetLines = (List<Line2d>)outerLineObj["OffsetLines"];
                if (offsetLines == null || offsetLines.Count == 0) { count += 1; continue; }
                    
               
                sortedIndices = (List<int>)outerLineObj["SortedIndices"];
                Trace.WriteLine("UsedLineList length is : " + usedLineList.Count);
                Trace.WriteLine("Before cleaning offset lines length : " + offsetLines.Count);
                List<Line2d> newLines = GraphicsUtility.RemoveDuplicateLinesFromAnotherListTry(offsetLines, usedLineList);
                if(newLines.Count == 0)
                {
                    count += 1;
                    continue;
                }
                selectLineNum = (int)Math.Floor(BasicUtility.RandomBetweenNumbers(ran, newLines.Count, 0));
                Trace.WriteLine("After cleaning offset lines length : " + newLines.Count);
             

                Line2d splitLine = newLines[selectLineNum]; // pick any Line
                //splitLine = LineUtility.Move(splitLine, 0.05);
                Dictionary<string, object> splitPolys = SplitObject.SplitByLine(currentPoly, splitLine, 0); //{ "PolyAfterSplit", "SplitLine" })]
                List<Polygon2d> polyAfterSplit = (List<Polygon2d>)splitPolys["PolyAfterSplit"];
                if (PolygonUtility.CheckPolyList(polyAfterSplit))
                {
                    List<Polygon2d> sortedPolys = PolygonUtility.SortPolygonList(polyAfterSplit);
                    if (PolygonUtility.NumberofSidesPoly(sortedPolys[0]) < 5)
                    {
                        Trace.WriteLine("4 sided found");
                        inPatientPolyList.Add(sortedPolys[0]);
                        splitablePolys.Push(sortedPolys[1]);
                        leftOverPolyList.Add(sortedPolys[1]);
                        usedLineList.Add(splitLine);
                    }
                    else
                    {
                        Trace.WriteLine("Not rect or square");
                        splitablePolys.Push(sortedPolys[0]);
                        splitablePolys.Push(sortedPolys[1]);
                    }
                    
                }
                else
                {
                    Trace.WriteLine("Split went wrong");
                    //continue;
                }
                count += 1;                
                //allSplitLinesOut.Add(splitLine);
                allSplitLinesOut.AddRange(allSplitLines);
                offsetLinesOut.AddRange(offsetLines);
                //offsetLinesCleanedOut.AddRange(offsetCleanedLines);
                offsetLinesCleanedOut = newLines;

            }//end of while loop
            
            return new Dictionary<string, object>
            {
                { "InpatientPolys", (inPatientPolyList) },
                { "SplitablePolys", (splitablePolys) },
                { "LeftOverpolys", (leftOverPolyList) },
                { "SplittableLines", (allSplitLinesOut) },
                { "OffsetLines", (offsetLinesOut) },
                { "Count", (count) },
                { "SplitLineLast", (allSplitLinesOut[0]) },
                { "CleanedOffsetLines", (offsetLinesCleanedOut) },
                { "UsedLines", (usedLineList) }
            };
        }

        [MultiReturn(new[] { "PolyAfterSplit", "UpdatedProgramData", "RoomsAdded" })]
        public static Dictionary<string, object> PlaceInpatientPrograms(List<Polygon2d> polyInputList, List<ProgramData> progData, double distance, int recompute = 1)
        {
            
            if (!PolygonUtility.CheckPolyList(polyInputList)) return null;
            if (progData == null || progData.Count == 0) return null;


            int roomCount = 0;
            List<Polygon2d> polyList = new List<Polygon2d>();
            List<Point2d> pointsList = new List<Point2d>();
            Queue<ProgramData> programDataRetrieved = new Queue<ProgramData>();
            List<ProgramData> progDataAddedList = new List<ProgramData>();
            ProgramData copyProgData = new ProgramData(progData[0]);

            for (int i = 0; i < progData.Count; i++) programDataRetrieved.Enqueue(progData[i]);
            for (int i = 0; i < polyInputList.Count; i++)
            {
                Polygon2d poly = polyInputList[i];
                if (!PolygonUtility.CheckPoly(poly)) continue;
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
                while (setSpan > distance) //programDataRetrieved.Count > 0
                {
                    double dist = 0;
                    if (setSpan > fac * distance) dist = distance;
                    else dist = setSpan;
                    Dictionary<string, object> splitReturn = SplitObject.SplitByOffsetFromLine(currentPoly,lineId, dist);
                    polyAfterSplitting = (Polygon2d)splitReturn["PolyAfterSplit"];
                    leftOverPoly = (Polygon2d)splitReturn["LeftOverPoly"];                    
                    if (PolygonUtility.CheckPoly(leftOverPoly))
                    {
                        progItem = programDataRetrieved.Dequeue();
                        currentPoly = leftOverPoly;
                        polyList.Add(polyAfterSplitting);
                        progItem.AreaProvided = GraphicsUtility.AreaPolygon2d(polyAfterSplitting.Points); 
                        setSpan -= dist;
                        progDataAddedList.Add(progItem);
                        count += 1;
                    }
                    else break;
                    if (programDataRetrieved.Count == 0) programDataRetrieved.Enqueue(copyProgData);
                }// end of while loop
                
                progItem = programDataRetrieved.Dequeue();
                polyList.Add(leftOverPoly);
                progItem.AreaProvided = GraphicsUtility.AreaPolygon2d(leftOverPoly.Points); 
                progDataAddedList.Add(progItem);
                if (programDataRetrieved.Count == 0) programDataRetrieved.Enqueue(copyProgData);
                
            }// end of for loop

            roomCount = progDataAddedList.Count;
            List<ProgramData> UpdatedProgramDataList = new List<ProgramData>();
            for (int i = 0; i < progDataAddedList.Count; i++) //progData.Count
            {
                ProgramData progItem = progDataAddedList[i];
                ProgramData progNew = new ProgramData(progItem);
                if (i < polyList.Count) progNew.PolyProgAssigned = polyList[i];
                else progNew.PolyProgAssigned = null;
                UpdatedProgramDataList.Add(progNew);
            }
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyList) },
                { "UpdatedProgramData",(UpdatedProgramDataList) },
                { "RoomsAdded" , (roomCount) }
            };
        }



        //splits a big poly in a single direction
        /// <summary>
        /// Thisnode places the programs in the dept polygon2d's based on the list from the program document
        /// It returns the input number multiplied by 2.
        /// </summary>
        /// <param name="PolyInputList">Dept Polygon2d where programs should be placed</param>
        /// <param name="ProgData">Program Data Object containing program information</param>
        /// <param name="distance">Distance of Program Spaces</param>
        /// <param name="recompute">Compute the function again</param>
        /// <returns name="PolyAfterSplit">Polys after splitting the dept into programs</param>
        /// <returns name="UpdatedProgramData">Updated Program Data Object</param>
        /// <search>
        /// split, divide , partition space based on distance
        /// </search>
        [MultiReturn(new[] { "PolyAfterSplit", "UpdatedProgramData" })]
        public static Dictionary<string, object> RecursiveSplitProgramsOneDirByDistance(List<Polygon2d> polyInputList, List<ProgramData> progData, double distance, int recompute = 1)
        {
            if (!PolygonUtility.CheckPolyList(polyInputList)) return null;
            if (progData == null || progData.Count == 0) return null;
            int dir = 0;
            List<Polygon2d> polyList = new List<Polygon2d>();
            List<double> areaList = new List<double>();
            List<Point2d> pointsList = new List<Point2d>();
            Stack<ProgramData> programDataRetrieved = new Stack<ProgramData>();

            for (int j = 0; j < progData.Count; j++)
            {
                programDataRetrieved.Push(progData[j]);
            }

            for (int i = 0; i < polyInputList.Count; i++)
            {
                Polygon2d poly = polyInputList[i];
                if (!PolygonUtility.CheckPoly(poly)) continue;
                List<double> spans = PolygonUtility.GetSpansXYFromPolygon2d(poly.Points);               
                double setSpan = 1000000000000;                
                if (spans[0] > spans[1])
                {
                    dir = 1;
                    setSpan = spans[0];
                }
                else
                {
                    dir = 0;
                    setSpan = spans[1];
                }
                Polygon2d currentPoly = poly;
                int count = 0;
                Random ran2 = new Random();
                while (setSpan > 0 && programDataRetrieved.Count > 0)
                {
                    
                    Dictionary<string, object> splitReturn = SplitObject.SplitByDistance(currentPoly, ran2, distance, dir);
                    List<Polygon2d> polyAfterSplitting = (List<Polygon2d>)splitReturn["PolyAfterSplit"];
                    if (PolygonUtility.CheckPolyList(polyAfterSplitting))
                    {
                        ProgramData progItem = programDataRetrieved.Pop();
                        double selectedArea = 0;
                        double area1 = GraphicsUtility.AreaPolygon2d(polyAfterSplitting[0].Points);
                        double area2 = GraphicsUtility.AreaPolygon2d(polyAfterSplitting[1].Points);
                        if (area1 > area2)
                        {
                            currentPoly = polyAfterSplitting[0];
                            if (polyAfterSplitting[1] == null) break;
                            polyList.Add(polyAfterSplitting[1]);
                            progItem.AreaProvided = area1;
                            areaList.Add(area2);
                            selectedArea = area2;
                        }
                        else
                        {
                            currentPoly = polyAfterSplitting[1];
                            polyList.Add(polyAfterSplitting[0]);
                            progItem.AreaProvided = area2;
                            areaList.Add(area1);
                            selectedArea = area1;
                        }
                        if (currentPoly.Points == null) break;
                        setSpan -= distance;
                        count += 1;
                    }
                    else
                    {
                        break;
                    }
                }// end of while loop


            }// end of for loop
            List<ProgramData> UpdatedProgramDataList = new List<ProgramData>();
            for (int i = 0; i < progData.Count; i++)
            {
                ProgramData progItem = progData[i];
                ProgramData progNew = new ProgramData(progItem);
                if (i < polyList.Count) progNew.PolyProgAssigned = polyList[i];
                else progNew.PolyProgAssigned = null;
                UpdatedProgramDataList.Add(progNew);
            }
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyList) },
                { "UpdatedProgramData",(UpdatedProgramDataList) }
            };
        }

       
        //used to split Depts into Program Spaces based on recursive poly split grid 
        // USING NOW 
        //splits a ploy into two based on dist and dir, selects the starting pt and side randomly
        /// <summary>
        /// This node places the programs in the dept polygon2d's based on the list from the program document
        /// It returns the input number multiplied by 2.
        /// </summary>
        /// <param name="PolyInputList">Dept Polygon2d where programs should be placed</param>
        /// <param name="ProgData">Program Data Object containing program information</param>
        /// <returns name="AcceptableMinDim">Minimum acceptable dimension of any placed program space</param>
        /// <search>
        /// split, divide , partition space based on distance
        /// </search>
        [MultiReturn(new[] { "PolyAfterSplit", "BigPolysAfterSplit", "CirculationPolygons", "UpdatedProgramData" })]
        public static Dictionary<string, object> RecursivePlaceProgramsSeries(List<Polygon2d> PolyInputList, 
            List<ProgramData> ProgData, double AcceptableMinDim, int factor = 4, int recompute = 0)
        {
            if (!PolygonUtility.CheckPolyList(PolyInputList)) return null;
            if (ProgData == null || ProgData.Count == 0) return null;
            int fac = 5;
            Random ran = new Random();
            List<List<Polygon2d>> polyList = new List<List<Polygon2d>>();
            Stack<ProgramData> programDataRetrieved = new Stack<ProgramData>();
            //push programdata into the stack
            for (int j = 0; j < ProgData.Count; j++) { programDataRetrieved.Push(ProgData[j]); }
            List<Polygon2d> polyOrganizedList = new List<Polygon2d>(), polyCoverList = new List<Polygon2d>();
            double max = AcceptableMinDim + AcceptableMinDim / fac, min = AcceptableMinDim - AcceptableMinDim / fac;
            double acceptWide = BasicUtility.RandomBetweenNumbers(ran, max, min);
            Dictionary<string,object> polySplit = SplitObject.SplitBigPolys(PolyInputList, acceptWide, factor);
            polyOrganizedList = (List<Polygon2d>)polySplit["PolySpaces"];
            polyCoverList = (List<Polygon2d>)polySplit["PolyForCirculation"];

            List<ProgramData> newProgDataList = new List<ProgramData>();
            for (int i = 0; i < ProgData.Count; i++)
            {
                newProgDataList.Add(new ProgramData(ProgData[i]));
            }
            polyList = AssignPolysToProgramData(newProgDataList, polyOrganizedList);            
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyList) },
                { "BigPolysAfterSplit", (polyOrganizedList) },
                { "CirculationPolygons", (polyCoverList) },
                { "UpdatedProgramData",(newProgDataList) }
            };


        }
        
     
        #endregion

        #region - Private Methods  
        //gets and sets the space between points for any smoothened polygon2d
        internal double spacingSetOrig
        {
            get { return spacingSet; }
            set { spacingSet = value; }
        }

        //gets and sets the space between points for any smoothened polygon2d
        internal double spacingSetAnother
        {
            get { return spacingSet2; }
            set { spacingSet2 = value; }
        }


        //makes a space data tree
        [MultiReturn(new[] { "SpaceTree", "NodeList" })]
        internal static Dictionary<string, object> CreateSpaceTree(int numNodes, Point origin, double spaceX, double spaceY, double radius, double recompute = 5)
        {
            Node root = new Node(0, NodeType.Container, true, origin, radius);
            List<Node> nodeList = new List<Node>();
            bool tag = true;
            for (int i = 0; i < numNodes - 1; i++)
            {
                tag = !tag;
                nodeList.Add(new Node(i + 1, SpaceDataTree.GenerateBalancedNodeType(tag)));
            }
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
        public static Dictionary<string, object> CreateBlocksByLines(Polygon2d polyOutline, Polygon2d containerPoly, double distance = 10, double minDist = 20,bool tag = true)
        {
            if (!PolygonUtility.CheckPoly(polyOutline)) return null;
            Polygon2d poly = new Polygon2d(polyOutline.Points,0);
            List<double> lineLength = new List<double>();
            List<Line2d> lineOptions = new List<Line2d>();
            Dictionary<string, object> checkLineOffsetObject = PolygonUtility.CheckLinesOffsetInPoly(poly, containerPoly, distance, tag);
            List<bool> offsetAble = (List<bool>)checkLineOffsetObject["Offsetables"];
            for (int i = 0; i < poly.Points.Count; i++)
            {
                if (offsetAble[i] == true) { lineLength.Add(poly.Lines[i].Length); }
                else lineLength.Add(0);
            }           
            double[] lineLengthArray = new double[lineLength.Count];
            int[] unsortedIndices = new int[lineLength.Count];
            for (int i = 0; i < lineLength.Count; i++)
            {
                lineLengthArray[i] = lineLength[i];
                unsortedIndices[i] = i;
            }
            List<int> sortedIndices = BasicUtility.Quicksort(lineLengthArray, unsortedIndices, 0, lineLength.Count - 1);
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

              
        //makes intersections and returns the two polygon2ds after intersection
        public static Dictionary<string, object> MakeIntersections(List<Point2d> poly, Line2d splitLine, double space)
        {
            List<Point2d> intersectedPoints = GraphicsUtility.LinePolygonIntersection(poly, splitLine);
            //List<Point2d> intersectedPoints = TestGraphicsUtility.LinePolygonIntersectionIndex(poly, splitLine);
            // find all points on poly which are to the left or to the right of the line
            List<int> pIndexA = new List<int>();
            List<int> pIndexB = new List<int>();
            for (int i = 0; i < poly.Count; i++)
            {
                bool check = GraphicsUtility.CheckPointSide(splitLine, poly[i]);
                if (check) pIndexA.Add(i);
                else pIndexB.Add(i);
            }

            //organize the points to make closed poly
            List<Point2d> sortedA = PolygonUtility.DoSortClockwise(poly, intersectedPoints, pIndexA);
            List<Point2d> sortedB = PolygonUtility.DoSortClockwise(poly, intersectedPoints, pIndexB);
            //List<Polygon2d> splittedPoly = PolygonUtility.OptimizePolyPoints(sortedA, sortedB, true, space);
            List<Polygon2d> splittedPoly = new List<Polygon2d> { new Polygon2d(sortedA,0), new Polygon2d(sortedB,0) };
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "IntersectedPoints", (intersectedPoints) },
                { "PointASide", (sortedA) },
                { "PointBSide", (sortedB) }
            };

        }
        // from a list of given polys  it assigns each program a list of polys till its area is satisfied
        internal static List<List<Polygon2d>> AssignPolysToProgramData(List<ProgramData> newProgDataList, List<Polygon2d> polygonLists)
        {
            //reset the area provided to the input progdata
            for (int i = 0; i < newProgDataList.Count; i++)
            {
                newProgDataList[i].AreaProvided = 0;
            }
            List<List<Polygon2d>> polyEachProgramList = new List<List<Polygon2d>>();
            Stack<Polygon2d> polyStack = new Stack<Polygon2d>();
            for (int i = 0; i < polygonLists.Count; i++) { polyStack.Push(polygonLists[i]); }

            for (int i = 0; i < newProgDataList.Count; i++)
            {
                ProgramData progItem = newProgDataList[i];
                //Trace.WriteLine("Starting Porg Data : " + i + "///////////");
                bool added = false;
                //double areaProgram = progData[i].CurrentAreaNeeds;
                List<Polygon2d> polysForProg = new List<Polygon2d>();
                while (progItem.CurrentAreaNeeds > 0 && polyStack.Count > 0)
                {
                    //Trace.WriteLine("  = = now in while = = ");
                    Polygon2d currentPoly = polyStack.Pop();
                    double currentArea = PolygonUtility.AreaCheckPolygon(currentPoly);
                    progItem.AddAreaToProg(currentArea);
                    polysForProg.Add(currentPoly);
                    //Trace.WriteLine("Area Given Now is : " + progItem.AreaAllocatedValue);
                    //Trace.WriteLine("Area Left over to Add :" + progItem.CurrentAreaNeeds);
                    added = true;
                }
                //dummy is just to make sure the function re reuns when slider is hit
                if (added) polyEachProgramList.Add(polysForProg);
                //if (!added) Trace.WriteLine("Did not add.  PolyStack Left : " + polyStack.Count + " | Current area needs were : " + progItem.CurrentAreaNeeds);
                //Trace.WriteLine("++++++++++++++++++++++++++++++");
            }
            return polyEachProgramList;
        }

         
        //gets a poly and recursively splits it till acceptabledimension is met and makes a polyorganized list
        internal static void MakePolysOfProportion(Polygon2d poly, List<Polygon2d> polyOrganizedList,
            List<Polygon2d> polycoverList, double acceptableWidth, double targetArea)
        {
            recurse += 1;
            List<double> spanListXY = PolygonUtility.GetSpansXYFromPolygon2d(poly.Points);
            double spanX = spanListXY[0], spanY = spanListXY[1];
            double aspRatio = spanX / spanY;
            double lowRange = 0.3, highRange = 2;
            double maxValue = 0.70, minValue = 0.35;
            double threshDistanceX = acceptableWidth;
            double threshDistanceY = acceptableWidth;
            Random ran = new Random();
            bool square = true;
            double div;
            div = BasicUtility.RandomBetweenNumbers(ranGenerate, maxValue, minValue);
            if (spanX > threshDistanceX && spanY > threshDistanceY) square = false;
            else {
                if (aspRatio > lowRange && aspRatio < highRange) square = false;
                else square = true;
            }
            if (square) polyOrganizedList.Add(poly);
            else
            {
                Dictionary<string, object> splitResult;
                //poly is rectangle so split it into two and add
                if (spanX > spanY)
                {
                    double dis = spanY * div;
                    int dir = 1;
                    //splitResult = BasicSplitPolyIntoTwo(poly, 0.5, dir);
                    splitResult = SplitObject.SplitByDistance(poly, ran, dis, dir, spacingSet2);
                    //splitResult = SplitByDistanceFromPoint(poly, dis, dir);
                }
                else
                {
                    double dis = spanX * div;
                    int dir = 0;
                    //splitResult = BasicSplitPolyIntoTwo(poly, 0.5, dir);
                    splitResult = SplitObject.SplitByDistance(poly, ran, dis, dir, spacingSet2);
                    //splitResult = SplitByDistanceFromPoint(poly, dis, dir);
                }

                List<Polygon2d> polyAfterSplit = (List<Polygon2d>)splitResult["PolyAfterSplit"];
                if (PolygonUtility.CheckPolyList(polyAfterSplit))
                {
                    double areaPolA = PolygonUtility.AreaCheckPolygon(polyAfterSplit[0]);
                    double areaPolB = PolygonUtility.AreaCheckPolygon(polyAfterSplit[1]);
                    if (areaPolA > targetArea) polycoverList.Add(polyAfterSplit[0]);
                    if (areaPolB > targetArea) polycoverList.Add(polyAfterSplit[1]);

                    List<double> spanA = PolygonUtility.GetSpansXYFromPolygon2d(polyAfterSplit[0].Points);
                    List<double> spanB = PolygonUtility.GetSpansXYFromPolygon2d(polyAfterSplit[1].Points);
                    //Trace.WriteLine("Recurse is : " + recurse);
                    if (recurse < 1500)
                    {
                        if ((spanA[0] > 0 && spanA[1] > 0) || (spanB[0] > 0 && spanB[1] > 0))
                        {
                            if (spanA[0] > acceptableWidth && spanA[1] > acceptableWidth)
                            {
                                MakePolysOfProportion(polyAfterSplit[0], polyOrganizedList, polycoverList, acceptableWidth, targetArea);
                            }
                            else
                            {
                                polyOrganizedList.Add(polyAfterSplit[0]);
                                double areaPoly = PolygonUtility.AreaCheckPolygon(polyAfterSplit[0]);
                            }
                            //end of 1st if
                            if (spanB[0] > acceptableWidth && spanB[1] > acceptableWidth)
                            {
                                MakePolysOfProportion(polyAfterSplit[1], polyOrganizedList, polycoverList, acceptableWidth, targetArea);
                            }
                            else
                            {
                                polyOrganizedList.Add(polyAfterSplit[1]);
                            }
                            //end of 2nd if                        
                        }
                        else
                        {
                            polyOrganizedList.Add(polyAfterSplit[0]);
                            polyOrganizedList.Add(polyAfterSplit[1]);
                        }
                    }
                }// end of if loop , checkingpolylists              
               
            }
        }// end of function

    

        //dept assignment new way
        [MultiReturn(new[] { "DeptPolys", "LeftOverPolys", "OtherDeptMainPoly", "UpdatedDeptData", })]
        public static Dictionary<string, object> DeptPlacer(List<DeptData> deptData, Polygon2d poly, double offset, double acceptableWidth = 20, double recompute = 5)
        {
            if (deptData == null) return null;
           // List<DeptData> sortedDeptData = SortDeptData(deptData);
            List<double> AllDeptAreaAdded = new List<double>();
            List<List<Polygon2d>> AllDeptPolys = new List<List<Polygon2d>>();
            List<Polygon2d> leftOverPoly = new List<Polygon2d>();//changed from stack
            List<Polygon2d> otherDeptPoly = new List<Polygon2d>();
            List<Polygon2d> subDividedPoly = new List<Polygon2d>();

            double totalDeptProp = 0;
            for (int i = 0; i < deptData.Count; i++) if (i > 0) totalDeptProp += deptData[i].DeptAreaProportion;

            for (int i = 0; i < deptData.Count; i++)
            {            
                double areaAssigned = 0;
                DeptData deptItem = deptData[i];
                if (i == 0) // inpatient dept
                {
                    //double areaNeeded = deptItem.DeptAreaNeeded;
                    double areaNeeded = deptItem.DeptAreaProportion*PolygonUtility.AreaCheckPolygon(poly);
                    Dictionary<string, object> inpatientObject = AssignBlocksBasedOnDistance(poly, offset, areaNeeded, 20, recompute);
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
                    leftOverPoly = SubdivideInputPoly(leftOverPoly, acceptableWidth, 0.5);
                }

                if( i > 0 ) // other depts
                {
                    double areaFactor = deptItem.DeptAreaProportion / totalDeptProp;
                    //Dictionary<string, object> assignedByRatioObj = AssignBlocksBasedOnRatio(deptItem, leftOverPoly, i);
                    //Dictionary<string, object> assignedByRatioObj = AssignBlocksBasedOnMinEdgeLength(deptItem, leftOverPoly,20,10);
                    Dictionary<string, object> assignedByRatioObj = AssignBlocksBasedOnRatioRecursed(deptItem, areaFactor, leftOverPoly,acceptableWidth,0.5);
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
                newDeptData.PolyDeptAssigned = AllDeptPolys[i];
                UpdatedDeptData.Add(newDeptData);
            }


            //added to compute area percentage for each dept
            double totalDeptArea = 0;
            for (int i = 0; i < UpdatedDeptData.Count; i++) totalDeptArea += UpdatedDeptData[i].DeptAreaNeeded;
            for (int i = 0; i < UpdatedDeptData.Count; i++) UpdatedDeptData[i].DeptAreaProportionAchieved = Math.Round((UpdatedDeptData[i].AreaProvided / totalDeptArea), 3);


            return new Dictionary<string, object>
            {
                { "DeptPolys", (AllDeptPolys) },
                { "LeftOverPolys", (leftOverPoly) },
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
            List<Point2d> polyPts = PolygonUtility.SmoothPolygon(poly.Points, spacingSet);
            poly = new Polygon2d(polyPts, 0);

            SortedDictionary<double, DeptData> sortedD = new SortedDictionary<double, DeptData>();
            for (int i = 0; i < deptData.Count; i++) sortedD.Add(deptData[i].AreaEachDept(), deptData[i]);

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
                    while (areaLeftOverToAdd > limit && leftOverPoly.Count > 0 && count1 < maxRound)
                    {
                        dir = BasicUtility.ToggleInputInt(dir);
                        currentPolyObj = leftOverPoly.Pop();
                        areaCurrentPoly = PolygonUtility.AreaCheckPolygon(currentPolyObj);
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
                            if (!PolygonUtility.CheckPolyList(edgeSplitted))
                            {
                                Trace.WriteLine("2  - ------- - Returning as splitbydistance did not work : ");
                                //return null;
                                countNew += 1;
                                continue;
                            }
                            areaA = PolygonUtility.AreaCheckPolygon(edgeSplitted[0]);
                            areaB = PolygonUtility.AreaCheckPolygon(edgeSplitted[1]);
                            //make a check on the returned polygon2d, if that gets an external wall or not
                            if (areaA < areaB) checkExternalWallAdj = CheckPolyGetsExternalWall(edgeSplitted[0], poly, offset);
                            else checkExternalWallAdj = CheckPolyGetsExternalWall(edgeSplitted[1], poly, offset);
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
                    while (areaLeftOverToAdd > limit && leftOverPoly.Count > 0 && count2 < maxRound)
                    {
                        double ratio = BasicUtility.RandomBetweenNumbers(rn, 0.85, 0.15);
                        currentPolyObj = leftOverPoly.Pop();
                        areaCurrentPoly = PolygonUtility.AreaCheckPolygon(currentPolyObj);
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
                            double areaA = PolygonUtility.AreaCheckPolygon(polyS[0]);
                            double areaB = PolygonUtility.AreaCheckPolygon(polyS[1]);
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
            //adding left over polys to inpatient blocks, commented out now to reMove inconsistent blocks
            /*
            Random ran2 = new Random();
            double num = ran2.NextDouble();
            if (recompute > 3)
            {    //for any left over poly
                 //double minArea = 10, areaMoreCheck = 0;
                if (leftOverPoly.Count > 0)
                {
                    while (leftOverPoly.Count > 0 && count3 < maxRound)
                    {
                        dir = BasicUtility.ToggleInputInt(dir);
                        Polygon2d currentPolyObj = leftOverPoly.Pop();
                        if (!PolygonUtility.CheckPolyDimension(currentPolyObj))
                        {
                            count3 += 1;
                            continue;
                        }
                        double areaCurrentPoly = PolygonUtility.AreaCheckPolygon(currentPolyObj);
                        Dictionary<string, object> splitReturned = SplitByDistance(currentPolyObj, ran2, offset, dir);
                        List<Polygon2d> edgeSplitted = (List<Polygon2d>)splitReturned["PolyAfterSplit"];
                        if (edgeSplitted == null)
                        {
                            Trace.WriteLine("Returning for leftoverpoly , as  split by distance did not work : ");
                            return null;
                        }
                        if (!PolygonUtility.CheckPolyDimension(edgeSplitted[0]))
                        {
                            count3 += 1;
                            continue;
                        }
                        double areaA = PolygonUtility.AreaCheckPolygon(edgeSplitted[0]);
                        double areaB = PolygonUtility.AreaCheckPolygon(edgeSplitted[1]);
                        if (areaA < areaB)
                        {
                            AllDeptPolys[0].Add(edgeSplitted[0]);
                            areaMoreCheck += areaA;
                            if (areaB > minArea) leftOverPoly.Push(edgeSplitted[1]);
                        }
                        else
                        {
                            AllDeptPolys[0].Add(edgeSplitted[1]);
                            areaMoreCheck += areaB;
                            if (areaA > minArea) { leftOverPoly.Push(edgeSplitted[0]); }
                        }
                        count3 += 1;
                    }// end of while loop
                }// end of if loop for leftover count 
            }


            */
            AllDeptAreaAdded[0] += areaMoreCheck;
            // adding the left over polys to the 2nd highest dept after inpatient
            if (leftOverPoly.Count > 0)
            {
                double areaLeftOver = 0;
                for (int i = 0; i < leftOverPoly.Count; i++)
                {
                    Polygon2d pol = leftOverPoly.Pop();
                    areaLeftOver += GraphicsUtility.AreaPolygon2d(pol.Points);
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
                newDeptData.PolyDeptAssigned = AllDeptPolys[i];
                UpdatedDeptData.Add(newDeptData);
            }

            List<Polygon2d> AllLeftOverPolys = new List<Polygon2d>();
            AllLeftOverPolys.AddRange(leftOverPoly);


            //make the centralStation on second highest dept
            Point2d centerPt = PolygonUtility.CentroidFromPoly(poly.Points);
            Dictionary<string, object> centralPolyLists = MakeCentralStation(AllDeptPolys[1], centerPt);
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

       
        //gets list of polygon2ds and find the most closest polygon2d to the center , to place the central stn
        internal static Dictionary<string, object> MakeCentralStation(List<Polygon2d> polygonsList, Point2d centerPt)
        {
            if (polygonsList == null || polygonsList.Count ==0 ) return null;
            if(polygonsList.Count < 2)
            {
                return new Dictionary<string, object>
            {
                { "PolyCentral", (polygonsList) },
                { "IndexInPatientPoly", (0) }
            };

            }
            List<Polygon2d> newPolyLists = new List<Polygon2d>();
            List<double> distanceList = new List<double>();
            double minArea = 100, ratio = 0.5, dis = 0; ; int dir = 0;
            List<int> indices = PolygonUtility.SortPolygonsFromAPoint(polygonsList, centerPt);
            Polygon2d polyToPlace = polygonsList[indices[0]];
            double area = PolygonUtility.AreaCheckPolygon(polyToPlace);
            if (area > minArea)
            {
                List<double> spans = PolygonUtility.GetSpansXYFromPolygon2d(polyToPlace.Points);
                if (spans[0] > spans[1])
                {
                    dir = 1;
                    dis = spans[0] / 2;
                }
                else {
                    dir = 0;
                    dis = spans[1] / 2;
                }
                Random ran = new Random();
                //Dictionary<string, object> splittedPoly = BasicSplitPolyIntoTwo(polyToPlace, ratio, dir);                
                //Dictionary<string, object> splittedPoly = SplitByDistanceFromPoint(polyToPlace, dis, dir);
                Dictionary<string, object> splittedPoly = SplitObject.SplitByDistance(polyToPlace, ran, dis, dir);
                List<Polygon2d> polyReturnedList = (List<Polygon2d>)splittedPoly["PolyAfterSplit"];
                if (!PolygonUtility.CheckPolyList(polyReturnedList)) return null;
                List<int> ind = PolygonUtility.SortPolygonsFromAPoint(polyReturnedList, centerPt);
                newPolyLists.Add(polyReturnedList[ind[0]]);
                newPolyLists.Add(polyReturnedList[ind[1]]);
            }
            else
            {
                newPolyLists.Add(polyToPlace);
            }
            return new Dictionary<string, object>
            {
                { "PolyCentral", (newPolyLists) },
                { "IndexInPatientPoly", (indices[0]) }
            };

        }

        #endregion

    }
}
