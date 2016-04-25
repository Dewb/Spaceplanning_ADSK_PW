using System;
using System.Collections.Generic;
using stuffer;
using Autodesk.DesignScript.Runtime;
using System.Diagnostics;
using Autodesk.DesignScript.Geometry;


namespace SpacePlanning
{
    public class BuildLayout
    {
        
        internal static double spacingSet = 10; //higher value makes code faster, 6 was good too
        internal static double spacingSet2 = 1;
        internal static Random ranGenerate = new Random();
        internal static double recurse = 0;
        internal static Point2d reference = new Point2d(0,0);
        internal static int maxCount = 2000, maxRound = 50;

      

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
        public static bool CheckPolyGetsExternalWall(Polygon2d poly, Polygon2d containerPoly, double shortEdgeDist = 16)
        {
            bool check = false;
            if (!PolygonUtility.CheckPoly(poly)) return check;
            //make given polys reduce number of points
            Polygon2d polyReg = new Polygon2d(poly.Points);
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
            //Trace.WriteLine("Well lines are found to be adjacent ? ++++++++++++++++++++   " + check);
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
                        if(eachDeptPoly != null)
                        {                        
                            for(int j = 0; j < eachDeptPoly.Count; j++)
                            {
                                Polygon2d polyItem = eachDeptPoly[j];
                                if(polyItem == null || polyItem.Points == null)
                                {
                                    deptPlaced = false;
                                    break;
                                } else if (polyItem.Points.Count == 0)
                                {
                                    deptPlaced = false;
                                    break;
                                } else
                                {
                                    deptPlaced = true;
                                }     
                            }
                        }
                        else
                        {
                            deptPlaced = false;
                            break;
                        }
                    }
                }
                count += 1;
            }
            return deptArrangement;
        }

  
        //dept arrange on site till it returns not a null value  
        internal static Dictionary<string, object> DeptArrangeOnSiteSingleOut(Polygon2d poly, List<DeptData> deptData, List<Cell> cellInside, double offset, int recompute = 1)
        {
            Dictionary<string, object> deptArrangement = null;
            double count = 0;
            int maxCount = 10;
            Random rand = new Random();
            while (deptArrangement == null && count < maxCount)
            {
                Trace.WriteLine("Lets Go Again for : " + count);
                int reco = rand.Next();
                deptArrangement = DeptSplitRefined(poly, deptData, cellInside, offset, reco);
                count += 1;
            }
            return deptArrangement;

        }
        
        //get a poly and find rectangular polys inside. then merge them together to form a big poly
        [MultiReturn(new[] { "SplittableLines", "OffsetLines","SortedIndices", "OffsetMidPts" })]
        public static Dictionary<string, object> FindOuterLinesAndOffsets(Polygon2d poly, double patientRoomDepth = 16, double extension = 8000,double recompute = 5)
        {
            if (!PolygonUtility.CheckPoly(poly)) return null;
            Polygon2d polyReg = new Polygon2d(poly.Points);
            List<Line2d> hLines = new List<Line2d>();
            List<Line2d> vLines = new List<Line2d>();
            List<Point2d> hMidPt = new List<Point2d>();
            List<Point2d> vMidPt = new List<Point2d>();
            for (int i = 0; i < polyReg.Points.Count; i++)
            {
                int a = i, b = i + 1;
                if (i == polyReg.Points.Count - 1) b = 0;
                Line2d line = new Line2d(polyReg.Points[a], polyReg.Points[b]);
                int lineType = GraphicsUtility.CheckLineOrient(line);
                if (lineType != -1)
                {
                    if (lineType == 0)
                    {
                        Line2d extendedLine = LineUtility.ExtendLine(line, extension);
                        hLines.Add(line);
                        hMidPt.Add(LineUtility.LineMidPoint(line));
                    }
                    if (lineType == 1)
                    {
                        Line2d extendedLine = LineUtility.ExtendLine(line, extension);
                        vLines.Add(line);
                        vMidPt.Add(LineUtility.LineMidPoint(line));
                    }
                }
            }
            List<Line2d> selectedHLines = new List<Line2d>();
            List<Line2d> selectedVLines = new List<Line2d>();
            int hIndLow = TestGraphicsUtility.ReturnLowestPointFromList(hMidPt);
            int hIndHigh = TestGraphicsUtility.ReturnHighestPointFromList(hMidPt);
            int vIndLow = GraphicsUtility.ReturnLowestPointFromListNew(vMidPt);
            int vIndHigh = GraphicsUtility.ReturnHighestPointFromListNew(vMidPt);
            selectedHLines.Add(hLines[hIndLow]); selectedHLines.Add(hLines[hIndHigh]);
            selectedVLines.Add(vLines[vIndLow]); selectedVLines.Add(vLines[vIndHigh]);
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
            sortedIndices.Reverse();

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
                { "OffsetMidPts", (midPtsOffsets) }
            };
        }
        

        //get a poly and find rectangular polys inside. then merge them together to form a big poly
        [MultiReturn(new[] { "InpatientPolys", "SplitablePolys", "LeftOverPolys", "SplittableLines","OffsetLines","Count","SplitLineLast", "CleanedOffsetLines", "UsedLines"})]
        public static Dictionary<string, object> MakeInpatientBlocks(Polygon2d poly, List<Cell> cellList, double patientRoomDepth = 16, double recompute = 5)
        {
            if (!PolygonUtility.CheckPoly(poly)) return null;
            List<Polygon2d> inPatientPolyList = new List<Polygon2d>();
            List<Polygon2d> leftOverPolyList = new List<Polygon2d>();
            List<Line2d> allSplitLinesOut = new List<Line2d>();
            List<List<Line2d>> offsetLinesOut = new List<List<Line2d>>();
            List<List<Line2d>> offsetLinesCleanedOut = new List<List<Line2d>>();
            List<Line2d> usedLineList = new List<Line2d>();
            List<int> sortedIndices = new List<int>();
            bool splitDone = false;
            int maxTry = 500, count = 0;
            double dim = cellList[0].DimX;
            Random ran = new Random();
            
            
            Stack<Polygon2d> splitablePolys = new Stack<Polygon2d>();
            splitablePolys.Push(poly);
      
            while (splitablePolys.Count > 0 && count < recompute)
            {                
                Polygon2d currentPoly = splitablePolys.Pop();
                Dictionary<string, object> outerLineObj = FindOuterLinesAndOffsets(currentPoly, patientRoomDepth,1500, count);
                List<Line2d> allSplitLines = (List<Line2d>)outerLineObj["SplittableLines"];
                List<Line2d> offsetLines = (List<Line2d>)outerLineObj["OffsetLines"];
                List<Line2d> offsetCleanedLines = new List<Line2d>();
                sortedIndices = (List<int>)outerLineObj["SortedIndices"];
                Trace.WriteLine("Before cleaning offset lines length : " + offsetLines.Count);
                Trace.WriteLine("UsedLineList length is : " + usedLineList.Count);
                if (usedLineList.Count > 0) offsetCleanedLines = GraphicsUtility.RemoveDuplicateLinesBasedOnMidPt(offsetLines, usedLineList);
                else offsetCleanedLines = offsetLines;
                Trace.WriteLine("After cleaning offset lines length : " + offsetCleanedLines.Count);
                int selectLineNum = (int)Math.Floor(BasicUtility.RandomBetweenNumbers(ran, allSplitLines.Count, 0));

                Line2d splitLine = offsetCleanedLines[0]; // pick the longest Line
                //splitLine = LineUtility.Move(splitLine, 0.05);
                Dictionary<string, object> splitPolys = SplitByLine(currentPoly, splitLine, 0); //{ "PolyAfterSplit", "SplitLine" })]
                List<Polygon2d> polyAfterSplit = (List<Polygon2d>)splitPolys["PolyAfterSplit"];
                if (PolygonUtility.CheckPolyList(polyAfterSplit))
                {
                    List<Polygon2d> sortedPolys = PolygonUtility.SortPolygonList(polyAfterSplit);
                    if (PolygonUtility.NumberofSidesPoly(sortedPolys[0]) < 5)
                    {
                        inPatientPolyList.Add(sortedPolys[0]);
                        splitablePolys.Push(sortedPolys[1]);
                        leftOverPolyList.Add(sortedPolys[1]);
                    }
                    else
                    {
                        List<Cell> cellInsideList = GridObject.CellsInsidePoly(currentPoly.Points, dim);
                        Dictionary<string, object> mergeObj = GridObject.MergePoly(sortedPolys, cellInsideList);
                        Polygon2d mergedPoly = (Polygon2d)mergeObj["MergedPoly"];
                        splitablePolys.Push(mergedPoly);
                    }
                    
                    
                }
                else
                {
                    continue;
                }
                count += 1;
                usedLineList.Add(splitLine);
                //allSplitLinesOut.Add(splitLine);
                allSplitLinesOut.AddRange(allSplitLines);
                offsetLinesOut.Add(offsetLines);
                offsetLinesCleanedOut.Add(offsetCleanedLines);

            }//end of while loop
            
            return new Dictionary<string, object>
            {
                { "InpatientPolys", (inPatientPolyList) },
                { "SplitablePolys", (splitablePolys) },
                { "LeftOverPolys", (leftOverPolyList) },
                { "SplittableLines", (allSplitLinesOut) },
                { "OffsetLines", (offsetLinesOut) },
                { "Count", (count) },
                { "SplitLineLast", (allSplitLinesOut[0]) },
                { "CleanedOffsetLines", (offsetLinesCleanedOut) },
                { "UsedLines", (usedLineList) }
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
                    
                    Dictionary<string, object> splitReturn = SplitByDistance(currentPoly, ran2, distance, dir);
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
            int fac = 5;
            Random ran = new Random();
            List<List<Polygon2d>> polyList = new List<List<Polygon2d>>();
            Stack<ProgramData> programDataRetrieved = new Stack<ProgramData>();
            //push programdata into the stack
            for (int j = 0; j < ProgData.Count; j++) { programDataRetrieved.Push(ProgData[j]); }
            List<Polygon2d> polyOrganizedList = new List<Polygon2d>(), polyCoverList = new List<Polygon2d>();
            double max = AcceptableMinDim + AcceptableMinDim / fac, min = AcceptableMinDim - AcceptableMinDim / fac;
            double acceptWide = BasicUtility.RandomBetweenNumbers(ran, max, min);
            Dictionary<string,object> polySplit = SplitBigPolys(PolyInputList, acceptWide, factor);
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
        
        //splits a polygon by a line 
        [MultiReturn(new[] { "PolyAfterSplit", "SplitLine" })]
        public static Dictionary<string, object> SplitByLine(Polygon2d polyOutline, Line2d inputLine, double distance = 5)
        {

            if (!PolygonUtility.CheckPoly(polyOutline)) return null;
            List<Point2d> polyOrig = polyOutline.Points;
            List<Point2d> poly = PolygonUtility.SmoothPolygon(polyOrig, spacingSet);
            Line2d splitLine = new Line2d(inputLine);
            Point2d centerPoly = GraphicsUtility.CentroidInPointLists(poly);
            bool checkSide = GraphicsUtility.CheckPointSide(splitLine, centerPoly);
            int orient = GraphicsUtility.CheckLineOrient(splitLine);
            if (orient == 0)
            {
                if (!checkSide) splitLine = LineUtility.Move(splitLine, 0, -1 * distance);
                else splitLine = LineUtility.Move(splitLine, 0, 1 * distance);
            } else
            {
                if (checkSide) splitLine = LineUtility.Move(splitLine, -1 * distance, 0);
                else splitLine = LineUtility.Move(splitLine, 1 * distance, 0);
            }         

            Dictionary<string, object> intersectionReturn = MakeIntersections(poly, splitLine, spacingSet);
            List<Polygon2d> splittedPoly = (List<Polygon2d>)intersectionReturn["PolyAfterSplit"];
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "SplitLine", (splitLine) }
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


        //splits a polygon based on distance and random direction
        [MultiReturn(new[] { "PolyAfterSplit", "SplitLine", "IntersectedPoints" })]
        internal static Dictionary<string, object> SplitByDistance(Polygon2d polyOutline, Random ran, double distance = 10, int dir = 0, double spacing = 0)
        {
            if (!PolygonUtility.CheckPoly(polyOutline)) return null;
            double extents = 5000, spacingProvided;
            List<Point2d> polyOrig = polyOutline.Points;
            if (spacing == 0) spacingProvided = spacingSet;
            else spacingProvided = spacing;

            List<Point2d> poly = PolygonUtility.SmoothPolygon(polyOrig, spacingProvided);
            if (!PolygonUtility.CheckPointList(poly)) return null;
            Dictionary<int, object> obj = PolygonUtility.PointSelector(ran, poly);
            Point2d pt = (Point2d)obj[0];
            int orient = (int)obj[1];
            Line2d splitLine = new Line2d(pt, extents, dir);

            // push this line right or left or up or down based on ratio
            if (dir == 0) splitLine = LineUtility.Move(splitLine, 0, orient * distance);
            else splitLine = LineUtility.Move(splitLine, orient * distance, 0);

            Dictionary<string, object> intersectionReturn = MakeIntersections(poly, splitLine, spacingProvided);
            List<Point2d> intersectedPoints = (List<Point2d>)intersectionReturn["IntersectedPoints"];
            List<Polygon2d> splittedPoly = (List<Polygon2d>)intersectionReturn["PolyAfterSplit"];

            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "SplitLine", (splitLine) },
                { "IntersectedPoints", (intersectedPoints) }
            };

        }

        //splits a polygon into two based on direction and distance from the lowest pt in the poly
        [MultiReturn(new[] { "PolyAfterSplit", "SplitLine", "IntersectedPoints" })]
        internal static Dictionary<string, object> SplitByDistanceFromPoint(Polygon2d polyOutline, double distance = 10, int dir = 0)
        {
            if (polyOutline == null || polyOutline.Points == null || polyOutline.Points.Count == 0) return null;
            double extents = 5000;
            int threshValue = 20;
            List<Point2d> polyOrig = polyOutline.Points;
            List<Point2d> poly = new List<Point2d>();
            if (polyOrig.Count > threshValue) { poly = polyOrig; }
            else { poly = PolygonUtility.SmoothPolygon(polyOrig, spacingSet2); }

            if (poly == null || poly.Count == 0) return null;
            int lowInd = GraphicsUtility.ReturnLowestPointFromListNew(poly);// THIS IS BETTER THAN THE OTHER VER
            Point2d lowPt = poly[lowInd];
            Line2d splitLine = new Line2d(lowPt, extents, dir);
            if (dir == 0) splitLine = LineUtility.Move(splitLine, 0, 1 * distance);
            else splitLine = LineUtility.Move(splitLine, 1 * distance, 0);



            Dictionary<string, object> intersectionReturn = MakeIntersections(poly, splitLine, spacingSet2);
            List<Point2d> intersectedPoints = (List<Point2d>)intersectionReturn["IntersectedPoints"];
            List<Polygon2d> splittedPoly = (List<Polygon2d>)intersectionReturn["PolyAfterSplit"];

            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "SplitLine", (splitLine) },
                { "IntersectedPoints", (intersectedPoints) }
            };

        }

        //makes intersections and returns the two polygon2ds after intersection
        internal static Dictionary<string, object> MakeIntersections(List<Point2d> poly, Line2d splitLine, double space)
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
            List<Polygon2d> splittedPoly = PolygonUtility.OptimizePolyPoints(sortedA, sortedB, true, space);

            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "IntersectedPoints", (intersectedPoints) }
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


        //used to split Depts into Program Spaces based on cell grids
        [MultiReturn(new[] { "PolyAfterSplit", "BigPolysAfterSplit", "UpdatedProgramData" })]
        internal static Dictionary<string, object> RecursivePlaceProgramsGridCells(List<Polygon2d> polyInputList,
            List<ProgramData> progData, double dimX = 3, double dimY = 3)
        {

            List<List<Polygon2d>> polyList = new List<List<Polygon2d>>();
            List<double> areaList = new List<double>();
            Stack<ProgramData> programDataRetrieved = new Stack<ProgramData>();
            List<ProgramData> newProgDataList = new List<ProgramData>();
            for (int i = 0; i < progData.Count; i++) newProgDataList.Add(new ProgramData(progData[i]));
            List<Polygon2d> polyOrganizedList = new List<Polygon2d>();
            for (int i = 0; i < polyInputList.Count; i++)
            {
                Polygon2d pol = polyInputList[i];
                List<Point2d> bbox = GraphicsUtility.FromPointsGetBoundingPoly(pol.Points);
                Dictionary<string, object> cellObject = GridObject.GridPointsInsideOutline(bbox, pol.Points, dimX, dimY);
                List<Point2d> point2dList = (List<Point2d>)cellObject["PointsInsideOutline"];
                List<Polygon2d> polyGridList = GridObject.MakeCellsFromGridPoints2d(point2dList, dimX, dimY);
                polyOrganizedList.AddRange(polyGridList);
            }
            polyList = AssignPolysToProgramData(newProgDataList, polyOrganizedList);

            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyList) },
                { "BigPolysAfterSplit", (polyOrganizedList) },
                { "UpdatedProgramData",(newProgDataList) }
            };


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
                    splitResult = SplitByDistance(poly, ran, dis, dir, spacingSet2);
                    //splitResult = SplitByDistanceFromPoint(poly, dis, dir);
                }
                else
                {
                    double dis = spanX * div;
                    int dir = 0;
                    //splitResult = BasicSplitPolyIntoTwo(poly, 0.5, dir);
                    splitResult = SplitByDistance(poly, ran, dis, dir, spacingSet2);
                    //splitResult = SplitByDistanceFromPoint(poly, dis, dir);
                }

                List<Polygon2d> polyAfterSplit = (List<Polygon2d>)splitResult["PolyAfterSplit"];
                double areaPolA = PolygonUtility.AreaCheckPolygon(polyAfterSplit[0]);
                double areaPolB = PolygonUtility.AreaCheckPolygon(polyAfterSplit[1]);
                if (areaPolA > targetArea) polycoverList.Add(polyAfterSplit[0]);
                if (areaPolB > targetArea) polycoverList.Add(polyAfterSplit[1]);

                List<double> spanA = PolygonUtility.GetSpansXYFromPolygon2d(polyAfterSplit[0].Points);
                List<double> spanB = PolygonUtility.GetSpansXYFromPolygon2d(polyAfterSplit[1].Points);
                Trace.WriteLine("Recurse is : " + recurse);
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
            }
        }// end of function

        //uses makepolysofproportion function to split one big poly into sub components
        internal static Dictionary<string, object> SplitBigPolys(List<Polygon2d> polyInputList, double acceptableWidth, double factor = 4)
        {
            List<Polygon2d> polyOrganizedList = new List<Polygon2d>(), polyCoverList = new List<Polygon2d>();
            int count = 0;
            for (int i = 0; i < polyInputList.Count; i++)
            {
                Polygon2d poly = polyInputList[i];
                if (poly == null || poly.Points == null || poly.Points.Count == 0)
                {
                    count += 1;
                    continue;
                }
                double targetArea = PolygonUtility.AreaCheckPolygon(poly) / factor;
                MakePolysOfProportion(poly, polyOrganizedList, polyCoverList, acceptableWidth, targetArea);
            }
            recurse = 0;
            return new Dictionary<string, object>
            {
                { "PolySpaces", (polyOrganizedList) },
                { "PolyForCirculation", (polyCoverList) }
            };
        }


        //splits a poly in a single direction
        internal static Dictionary<string, object> RecursiveSplitProgramsOneDirByDistanceSingleOut(List<Polygon2d> polyInputList,
            List<ProgramData> progData, double distance, int recompute = 1)
        {
            return RecursiveSplitProgramsOneDirByDistance(polyInputList, progData, distance, recompute);

        }


        //splits a polygon into two based on ratio and dir
        [MultiReturn(new[] { "PolyAfterSplit", "SplitLine", "IntersectedPoints" })]
        internal static Dictionary<string, object> SplitByRatio(Polygon2d polyOutline, double ratio = 0.5, int dir = 0)
        {
            if (polyOutline == null) return null;
            if (polyOutline != null && polyOutline.Points == null) return null;

            double extents = 5000;
            double minimumLength = 2, minWidth = 10, aspectRatio = 0, eps = 0.1;
            List<Point2d> polyOrig = polyOutline.Points;
            List<Point2d> poly = PolygonUtility.SmoothPolygon(polyOrig, spacingSet);
            List<double> spans = PolygonUtility.GetSpansXYFromPolygon2d(poly);
            double horizontalSpan = spans[0], verticalSpan = spans[1];
            Point2d polyCenter = PolygonUtility.CentroidFromPoly(poly);
            if (horizontalSpan < minimumLength || verticalSpan < minimumLength) return null;

            if (horizontalSpan > verticalSpan) { dir = 1; aspectRatio = horizontalSpan / verticalSpan; }
            else { dir = 0; aspectRatio = verticalSpan / horizontalSpan; }

            // adjust ratio
            if (ratio < 0.15) ratio = ratio + eps;
            if (ratio > 0.85) ratio = ratio - eps;

            if (horizontalSpan < minWidth || verticalSpan < minWidth) ratio = 0.5;
            Line2d splitLine = new Line2d(polyCenter, extents, dir);
            double shift = ratio - 0.5;
            if (dir == 0) splitLine = LineUtility.Move(splitLine, 0, shift * verticalSpan);
            else splitLine = LineUtility.Move(splitLine, shift * horizontalSpan, 0);

            Dictionary<string, object> intersectionReturn = MakeIntersections(poly, splitLine, spacingSet);
            List<Point2d> intersectedPoints = (List<Point2d>)intersectionReturn["IntersectedPoints"];
            List<Polygon2d> splittedPoly = (List<Polygon2d>)intersectionReturn["PolyAfterSplit"];

            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "SplitLine", (splitLine) },
                { "IntersectedPoints", (intersectedPoints) }
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
                            Dictionary<string, object> splitReturned = SplitByDistance(currentPolyObj, ran, offset, dir);
                            if (splitReturned == null) return null;
                            edgeSplitted = (List<Polygon2d>)splitReturned["PolyAfterSplit"];
                            if (!PolygonUtility.CheckPolyList(edgeSplitted)) return null;
                            areaA = PolygonUtility.AreaCheckPolygon(edgeSplitted[0]);
                            areaB = PolygonUtility.AreaCheckPolygon(edgeSplitted[1]);
                            //make a check on the returned polygon2d, if that gets an external wall or not
                            if (areaA < areaB)
                            {
                                checkExternalWallAdj = CheckPolyGetsExternalWall(edgeSplitted[0], poly, offset);
                            }
                            else
                            {
                                checkExternalWallAdj = CheckPolyGetsExternalWall(edgeSplitted[1], poly, offset);
                            }
                            //Trace.WriteLine("trying to have an external edge " + countNew);
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
                            Dictionary<string, object> basicSplit = SplitByRatio(currentPolyObj, ratio, dir);
                            if (basicSplit == null) return null;
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
            ///*
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
                        if (edgeSplitted == null) return null;
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


            //*/
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
            int index = (int)centralPolyLists["IndexInPatientPoly"];
            List<Polygon2d> polyReturned = (List<Polygon2d>)centralPolyLists["PolyCentral"];

            if (polyReturned.Count > 1)
            {
                AllDeptPolys[1].RemoveAt(index);
                AllDeptPolys[1].Add(polyReturned[1]);
            }
            else AllDeptPolys[1][index] = polyReturned[0];

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
            if (polygonsList == null) return null;
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
                Dictionary<string, object> splittedPoly = SplitByDistance(polyToPlace, ran, dis, dir);
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
