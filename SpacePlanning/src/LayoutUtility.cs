using System;
using System.Collections.Generic;
using stuffer;
using Autodesk.DesignScript.Runtime;
using System.Diagnostics;
using Autodesk.DesignScript.Geometry;
using System.Linq;

namespace SpacePlanning
{
    public static class LayoutUtility
    {
        #region - Private Methods

        //gets list of polygon2ds and find the most closest polygon2d to the center , to place the central stn
        internal static Dictionary<string, object> MakeCentralStation(List<Polygon2d> polygonsList, Point2d centerPt)
        {
            if (polygonsList == null || polygonsList.Count == 0) return null;
            if (polygonsList.Count < 2)
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
            double area = PolygonUtility.AreaPolygon(polyToPlace);
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
                if (!ValidateObject.CheckPolyList(polyReturnedList)) return null;
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

        // to be reconsidered this func
        //get a poly and find rectangular polys inside. then merge them together to form a big poly
        [MultiReturn(new[] { "SplittableLines", "OffsetLines", "SortedIndices", "OffsetMidPts", "NonOrthoLines" })]
        internal static Dictionary<string, object> FindOuterLinesAndOffsets(Polygon2d poly, double patientRoomDepth = 16, double extension = 8000, double recompute = 5)
        {
            if (!ValidateObject.CheckPoly(poly)) return null;
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
                int lineType = ValidateObject.CheckLineOrient(line);
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
                    countNonOrtho += 1;
                    nonOrthoLines.Add(line);
                }
            }
            List<Line2d> selectedHLines = new List<Line2d>();
            List<Line2d> selectedVLines = new List<Line2d>();
            int hIndLow = CodeToBeTested.ReturnLowestPointFromList(hMidPt);
            int hIndHigh = CodeToBeTested.ReturnHighestPointFromList(hMidPt);
            int vIndLow = PointUtility.LowestPointFromList(vMidPt);
            int vIndHigh = PointUtility.HighestPointFromList(vMidPt);
            if (hIndLow > -1) selectedHLines.Add(hLines[hIndLow]);
            if (hIndHigh > -1) selectedHLines.Add(hLines[hIndHigh]);
            if (vIndLow > -1) selectedVLines.Add(vLines[vIndLow]);
            if (vIndHigh > -1) selectedVLines.Add(vLines[vIndHigh]);

            List<Line2d> allSplitLines = new List<Line2d>();
            allSplitLines.AddRange(selectedHLines);
            allSplitLines.AddRange(selectedVLines);
            List<double> splitLineLength = new List<double>();
            for (int i = 0; i < allSplitLines.Count; i++) splitLineLength.Add(allSplitLines[i].Length);
            List<int> sortedIndices = BasicUtility.Quicksort(splitLineLength);
            if (sortedIndices != null) sortedIndices.Reverse();

            List<Line2d> offsetLines = new List<Line2d>();
            List<Point2d> midPtsOffsets = new List<Point2d>();
            for (int i = 0; i < allSplitLines.Count; i++)
            {
                offsetLines.Add(LineUtility.Offset(allSplitLines[i], patientRoomDepth));
                midPtsOffsets.Add(LineUtility.NudgeLineMidPt(allSplitLines[i], poly, patientRoomDepth));
            }

            List<Line2d> offsetSortedLines = new List<Line2d>();
            for (int i = 0; i < offsetLines.Count; i++) offsetSortedLines.Add(offsetLines[sortedIndices[i]]);
            return new Dictionary<string, object>
            {
                { "SplittableLines", (allSplitLines) },
                { "OffsetLines", (offsetSortedLines) },
                { "SortedIndices", (sortedIndices) },
                { "OffsetMidPts", (midPtsOffsets) },
                { "NonOrthoLines", (nonOrthoLines) }
            };
        }

        // to be reconsidered this func
        //get a poly and find rectangular polys inside. then merge them together to form a big poly
        [MultiReturn(new[] { "SplittableLines", "OffsetLines", "SortedIndices", "OffsetMidPts" })]
        internal static Dictionary<string, object> ExtLinesAndOffsetsFromBBox(Polygon2d poly, double patientRoomDepth = 16, double recompute = 5)
        {
            if (!ValidateObject.CheckPoly(poly)) return null;
            Polygon2d polyReg = new Polygon2d(poly.Points);
            List<Line2d> allSplitLines = new List<Line2d>();
            Polygon2d polyBBox = Polygon2d.ByPoints(ReadData.FromPointsGetBoundingPoly(polyReg.Points));
            allSplitLines = polyBBox.Lines;
            List<double> splitLineLength = new List<double>();
            for (int i = 0; i < allSplitLines.Count; i++) splitLineLength.Add(allSplitLines[i].Length);
            List<int> sortedIndices = BasicUtility.Quicksort(splitLineLength);
            if (sortedIndices != null) sortedIndices.Reverse();

            List<Line2d> offsetLines = new List<Line2d>();
            List<Point2d> midPtsOffsets = new List<Point2d>();
            for (int i = 0; i < allSplitLines.Count; i++)
            {
                offsetLines.Add(LineUtility.Offset(allSplitLines[i], patientRoomDepth));
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

        //use makePolypointsOrtho multiple times
        internal static Polygon2d FitPolyToBeOrtho(Polygon2d inputPoly, int times = 5)
        {
            Polygon2d polyReturn = MakePolyPointsOrtho(inputPoly);
            for (int i = 0; i < times; i++)
            {
                polyReturn = MakePolyPointsOrtho(inputPoly);
            }
            return polyReturn;
        }

        //make the given poly all points orthonogonal to each other
        internal static Polygon2d MakePolyPointsOrtho(Polygon2d poly)
        {
            Polygon2d polyReg = new Polygon2d(poly.Points);
            List<Point2d> ptForOrthoPoly = new List<Point2d>();
            for (int i = 0; i < polyReg.Points.Count; i++)
            {
                Point2d pt = Point2d.ByCoordinates(polyReg.Points[i].X, polyReg.Points[i].Y);
                ptForOrthoPoly.Add(pt);
            }

            for (int i = 0; i < polyReg.Points.Count; i++)
            {
                int a = i, b = i + 1;
                double eps = 50;
                if (i == polyReg.Points.Count - 1) b = 0;
                Line2d line = new Line2d(polyReg.Points[a], polyReg.Points[b]);
                if (ValidateObject.CheckLineOrient(line) == -1)
                {
                    //double diffX = Math.Abs(line.StartPoint.X - line.EndPoint.X);
                    //double diffY = Math.Abs(line.StartPoint.Y - line.EndPoint.Y);
                    Point2d cenPoly = PolygonUtility.CentroidOfPoly(polyReg);
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
        internal static bool CheckPolyGetsExternalWall(Polygon2d poly, Polygon2d containerPoly, double shortEdgeDist = 16, bool tag = true)
        {
            bool check = false;
            if (!ValidateObject.CheckPoly(poly)) return check;
            Polygon2d polyReg = new Polygon2d(null);
            //make given polys reduce number of points
            if (tag) polyReg = new Polygon2d(poly.Points);
            else polyReg = poly;
            Polygon2d containerPolyReg = new Polygon2d(containerPoly.Points);
            for (int i = 0; i < polyReg.Points.Count; i++)
            {
                int a = i, b = i + 1;
                double eps = 0;
                if (i == polyReg.Points.Count - 1) b = 0;
                double distance = PointUtility.DistanceBetweenPoints(polyReg.Points[a], polyReg.Points[b]);
                List<double> spansSorted = PolygonUtility.GetPolySpan(containerPolyReg);
                if (distance <= spansSorted[0] * 0.75) continue;
                Line2d lineA = Line2d.ByStartPointEndPoint(polyReg.Points[a], polyReg.Points[b]);
                for (int j = 0; j < containerPolyReg.Points.Count; j++)
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
        internal static bool CheckLineGetsExternalWall(Line2d lineA, Polygon2d containerPoly)
        {
            bool check = false;
            if (!ValidateObject.CheckPoly(containerPoly)) return check;
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

        //adds a point to a line of a poly, such that offsetting places offset line inside the poly
        [MultiReturn(new[] { "PolyAddedPts", "ProblemPoint", "IsAdded", "PointAdded", "Trials", "FinalRatio", "ProblemLine", "ProblemPtsList", "FalseLineList" })]
        internal static Dictionary<string, object> AddPointToFitPoly(Polygon2d poly, Polygon2d containerPoly, double distance = 16, double area = 0, double thresDistance = 10, double recompute = 5)
        {
            if (!ValidateObject.CheckPoly(poly)) return null;
            if (distance < 1) return null;

            Dictionary<string, object> lineOffsetCheckObj = ValidateObject.CheckLinesOffsetInPoly(poly, containerPoly, distance);
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
                    indicesFalse[i] > -1 && pointsFalse[i] != null && pointsFalse[i].Count == 1 && !added && LayoutUtility.CheckLineGetsExternalWall(poly.Lines[i], containerPoly))
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
                        Point2d offPtNew = LineUtility.OffsetLinePointInsidePoly(line, ptNewEnd, poly, distance);
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
                    double currentArea = PolygonUtility.AreaPolygon(currentPoly);
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


        #endregion



    }
}
