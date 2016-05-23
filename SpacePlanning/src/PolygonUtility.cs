using System;
using System.Collections.Generic;
using System.Linq;
using stuffer;
using Autodesk.DesignScript.Runtime;
using System.Diagnostics;

namespace SpacePlanning
{
    public static class PolygonUtility
    {
    

        //gives the highest and lowest point from poly points
        internal static List<Point2d> GetLowestAndHighestPointFromPoly(Polygon2d poly)
        {
            List<Point2d> returnPts = new List<Point2d>();
            List<Point2d> ptList = poly.Points;
            int highPtInd = PointUtility.HighestPointFromList(ptList);
            int lowPtInd = PointUtility.LowestPointFromList(ptList);
            returnPts.Add(ptList[lowPtInd]);
            returnPts.Add(ptList[highPtInd]);
            return returnPts;
        }

        //polygon list cleaner
        internal static List<Polygon2d> CleanPolygonList(List<Polygon2d> polyList)
        {
            // if (!CheckPolyList(polyList)) return null;
            List<Polygon2d> polyNewList = new List<Polygon2d>();
            bool added = false;
            for (int i = 0; i < polyList.Count; i++)
            {
                if (ValidateObject.CheckPoly(polyList[i]))
                {
                    polyNewList.Add(polyList[i]);
                    added = true;
                }

            }
            if (added) return polyNewList;
            else return null;
        }

        // removes polygons which are null from the list
        internal static List<Polygon2d> CleanPolygons(List<Polygon2d> polygonsList)
        {
            List<Polygon2d> cleanPolyList = new List<Polygon2d>();
            for (int i = 0; i < polygonsList.Count; i++)
            {
                if (polygonsList[i] == null || polygonsList[i].Points == null || polygonsList[i].Points.Count == 0)
                    continue;
                cleanPolyList.Add(polygonsList[i]);
            }
            return cleanPolyList;
        }
        
        // sorts a list of polygons from a point and returns the indices 
        internal static List<int> SortPolygonsFromAPoint(List<Polygon2d> polygonsList, Point2d centerPt)
        {
            List<double> distanceList = new List<double>();
            for (int i = 0; i < polygonsList.Count; i++)
            {
                if (polygonsList[i] == null || polygonsList[i].Points == null || polygonsList[i].Points.Count == 0)
                    continue;
                Point2d cen = CentroidOfPoly(polygonsList[i]);
                double distance = PointUtility.DistanceBetweenPoints(cen, centerPt);
                distanceList.Add(distance);
            }
            List<int> indices = BasicUtility.SortIndex(distanceList);
            return indices;
        }



        //orders the points to form a closed polygon2d
        internal static List<Point2d> OrderPolygon2dPoints(List<Point2d> poly, List<Point2d> intersectedPoints, List<int> pIndex)
        {
            if (intersectedPoints.Count < 2) return null;
   
            List<Point2d> pt = new List<Point2d>();
            bool added = false;
            int a = 0, b = 1;
            for (int i = 0; i < pIndex.Count - 1; i++)
            {
                pt.Add(poly[pIndex[i]]);
                if (Math.Abs(pIndex[i] - pIndex[i + 1]) > 1 && added == false)
                {
                    List<Point2d> intersNewList = PointUtility.SortPointsByDistanceFromPoint(intersectedPoints,
                        poly[pIndex[i]]);
                    pt.Add(intersNewList[a]);
                    pt.Add(intersNewList[b]);
                    added = true;
                }

                if (i == (pIndex.Count - 2) && added == false)
                {
                    pt.Add(poly[pIndex[i + 1]]);
                    List<Point2d> intersNewList = PointUtility.SortPointsByDistanceFromPoint(intersectedPoints,
                             poly[pIndex[i + 1]]);
                    pt.Add(intersNewList[a]);
                    pt.Add(intersNewList[b]);
                    added = true;
                }
                else if (i == (pIndex.Count - 2) && added == true) pt.Add(poly[pIndex[i + 1]]);
            }
            return pt;
        }

        //get a polygonlist and sort based on area
        internal static List<Polygon2d> SortPolygonList(List<Polygon2d> polyList)
        {
            if (!ValidateObject.CheckPolyList(polyList)) return null;
            List<double> areaPolyList = new List<double>();
            for (int i = 0; i < polyList.Count; i++) areaPolyList.Add(AreaPolygon(polyList[i]));
            List<int> sortedIndices = BasicUtility.Quicksort(areaPolyList);
            List<Polygon2d> sortedPolys = new List<Polygon2d>();
            for (int i = 0; i < polyList.Count; i++) sortedPolys.Add(polyList[sortedIndices[i]]);
            return sortedPolys;
        }     
 
        //finds the outerlines of a polygon, except the extreme max and min lines in its x and y axis
        public static List<Line2d> GetOuterLines(Polygon2d polyReg)
        {            
            List<Line2d> hLines = new List<Line2d>();
            List<Line2d> vLines = new List<Line2d>();
            List<Point2d> hMidPt = new List<Point2d>();
            List<Point2d> vMidPt = new List<Point2d>();
            for (int i = 0; i < polyReg.Points.Count; i++)
            {
                int a = i, b = i + 1;
                if (i == polyReg.Points.Count - 1) b = 0;
                Line2d line = new Line2d(polyReg.Points[a], polyReg.Points[b]);
                int lineType = ValidateObject.CheckLineOrient(line);
                if (lineType != -1)
                {
                    if (lineType == 0)
                    {
                        hLines.Add(line);
                        hMidPt.Add(LineUtility.LineMidPoint(line));
                    }
                    if (lineType == 1)
                    {
                        vLines.Add(line);
                        vMidPt.Add(LineUtility.LineMidPoint(line));
                    }
                }
            }
            //int hIndLow = GraphicsUtility.ReturnLowestPointFromListNew(hMidPt);
            //int hIndHigh = GraphicsUtility.ReturnHighestPointFromListNew(hMidPt);
            int hIndLow = CodeToBeTested.ReturnLowestPointFromList(hMidPt);
            int hIndHigh = PointUtility.HighestPointFromList(hMidPt);
            int vIndLow = PointUtility.LowestPointFromList(vMidPt);
            int vIndHigh = PointUtility.HighestPointFromList(vMidPt);
            hLines.RemoveAt(hIndLow);
            hLines.RemoveAt(hIndHigh);
            vLines.RemoveAt(vIndLow);
            vLines.RemoveAt(vIndHigh);
            List<Line2d> allSplitLines = new List<Line2d>();
            allSplitLines.AddRange(hLines);
            allSplitLines.AddRange(vLines);
            return allSplitLines;

        }


        //flatten list of polygon2d
        internal static List<Polygon2d> FlattenPolygon2dList(List<List<Polygon2d>> polyList)
        {
            if (polyList == null) return null;
            List<Polygon2d> flatPolyList = new List<Polygon2d>();
            for (int i = 0; i < polyList.Count; i++)
            {
                if (polyList[i] != null)
                    if (polyList[i].Count > 0) flatPolyList.AddRange(polyList[i]);
            }
            return flatPolyList;
        }


        //get a poly and find rectangular polys inside. then merge them together to form a big poly 
        [MultiReturn(new[] { "WholesomePolys", "PolysAfterSplit", "AllSplitLines"})]
        public static Dictionary<string, object> MakeWholesomeBlockInPoly(Polygon2d poly, double dim = 10,double recompute = 5)
        {
            if (poly == null || poly.Points == null || poly.Points.Count == 0) return null;
            List<Polygon2d> wholesomePolyList = new List<Polygon2d>();
            Polygon2d polyReg = new Polygon2d(poly.Points);
            List<Line2d> allSplitLines = new List<Line2d>();
            if (dim > 18) // splitlines from gridlines
            {
                Dictionary<string, object> gridLineObj = GridObject.CreateGridLines(poly, dim, 1);
                List<Line2d> gridXLines = (List<Line2d>)gridLineObj["GridXLines"];
                List<Line2d> gridYLines = (List<Line2d>)gridLineObj["GridYLines"];
                allSplitLines.AddRange(gridXLines);
                allSplitLines.AddRange(gridYLines);
            }
            else allSplitLines = GetOuterLines(polyReg);   // splitlines from poly itself        
            List<Line2d> allSplitLinesCopy = allSplitLines.Select(x => new Line2d(x.StartPoint, x.EndPoint)).ToList(); // example of deep copy

            // can replace the allsplitlines with gridlines
            bool splitDone = false;
            Stack<Polygon2d> splittedPolys = new Stack<Polygon2d>();
            Polygon2d currentPoly = new Polygon2d(SmoothPolygon(polyReg.Points, BuildLayout.SPACING));
            splittedPolys.Push(currentPoly);
            Random ran = new Random();
            int countBig = 0, maxRounds = 50;
            List<int> numSidesList = new List<int>();
            List<Polygon2d> allPolyAfterSplit = new List<Polygon2d>();
            while (splittedPolys.Count > 0 && countBig < maxRounds && allSplitLines.Count > 0)
            {
                int count = 0, maxTry = 100;
                int numSides = NumberofSidesPoly(currentPoly);
                numSidesList.Add(numSides);
                //CHECK sides
                if (numSides < 5)
                {
                    wholesomePolyList.Add(currentPoly);
                    currentPoly = splittedPolys.Pop();
                }

                //SPLIT blocks          
                while (splitDone == false && count < maxTry && allSplitLines.Count > 0)
                {
                    //randomly get a line
                    int selectLineNum = (int)Math.Floor(BasicUtility.RandomBetweenNumbers(ran, allSplitLines.Count, 0));
                    Line2d splitLine = allSplitLines[selectLineNum];
                    splitLine = LineUtility.Move(splitLine, 0.05,0.05);
                    Dictionary<string, object> splitPolys = SplitObject.SplitByLine(currentPoly, splitLine, 0);
                    List<Polygon2d> polyAfterSplit = (List<Polygon2d>)splitPolys["PolyAfterSplit"];
                    if (polyAfterSplit == null || polyAfterSplit.Count < 2 ||
                        polyAfterSplit[0] == null || polyAfterSplit[0].Points == null || polyAfterSplit[0].Points.Count == 0 ||
                        polyAfterSplit[1] == null || polyAfterSplit[1].Points == null || polyAfterSplit[1].Points.Count == 0) splitDone = false;
                    else
                    {
                        allSplitLines.RemoveAt(selectLineNum);
                        currentPoly = polyAfterSplit[0];
                        splittedPolys.Push(polyAfterSplit[1]);
                        allPolyAfterSplit.AddRange(polyAfterSplit);
                        splitDone = true;
                    }
                    count += 1;

                } // end of second while loop                    
                splitDone = false;
                countBig += 1;
            }// end of 1st while loop

            List<Polygon2d> cleanWholesomePolyList = new List<Polygon2d>();
            //rationalize the wholesome polys
            for (int i = 0; i < wholesomePolyList.Count; i++)
            {
                if (wholesomePolyList[i] == null || wholesomePolyList[i].Points == null ||
                    wholesomePolyList[i].Points.Count < 4) continue;
                cleanWholesomePolyList.Add(new Polygon2d(wholesomePolyList[i].Points));
            }

            return new Dictionary<string, object>
            {
                { "WholesomePolys", (cleanWholesomePolyList) },
                { "PolysAfterSplit", (allPolyAfterSplit) },
                { "AllSplitLines" , (allSplitLinesCopy) }
            };
        }
            

        //finds number of sides in a polygon2d
        public static int NumberofSidesPoly(Polygon2d poly)
        {
            if (poly == null || poly.Points == null || poly.Points.Count == 0) return -1;
            int sides = 0;
            Polygon2d polyReg = new Polygon2d(poly.Points);
            for(int i = 0; i < polyReg.Points.Count; i++)
            {
                int a = i, b = i + 1;
                if (i == polyReg.Points.Count - 1) b = 0;
                sides += 1;                
            }
            return sides;
        }

  
        //gets two point lists , adds or optimizes the number of points and merges the pointlist together
        internal static List<Polygon2d> OptimizePolyPoints(List<Point2d> sortedA, List<Point2d> sortedB,
        bool tag = false, double spacing = 0)
        {
            Polygon2d polyA, polyB;

            if (tag)
            {
                //added to make sure poly has uniform points
                polyA = new Polygon2d(sortedA);
                polyB = new Polygon2d(sortedB);
                double spacingProvided = 3;
                if (spacing == 0) spacingProvided = BuildLayout.SPACING;
                else spacingProvided = spacing;
                List<Point2d> ptsPolyA = SmoothPolygon(polyA.Points, spacingProvided);
                List<Point2d> ptsPolyB = SmoothPolygon(polyB.Points, spacingProvided);
                polyA = new Polygon2d(ptsPolyA, 0);
                polyB = new Polygon2d(ptsPolyB, 0);
            }
            else
            {
                //return the polys as obtained - no smoothing
                polyA = new Polygon2d(sortedA, 0);
                polyB = new Polygon2d(sortedB, 0);
            }

            //added check to see if poly is null
            if (!ValidateObject.CheckPoly(polyA)) polyA = null;
            if (!ValidateObject.CheckPoly(polyB)) polyB = null;

            List<Polygon2d> splittedPoly = new List<Polygon2d>();
            splittedPoly.Add(polyA);
            splittedPoly.Add(polyB);
            return splittedPoly;
        }
        
        //returns the hprizontal and vertical span of a polygon2d , places longer span first
        public static List<double> GetPolySpan(Polygon2d poly)
        {
            List<double> spanList = new List<double>();
            Range2d polyRange = PolygonUtility.GetRang2DFromBBox(poly.Points);

            Point2d span = polyRange.Span;
            double horizontalSpan = span.X;
            double verticalSpan = span.Y;
            if (horizontalSpan > verticalSpan)
            {
                spanList.Add(horizontalSpan);
                spanList.Add(verticalSpan);
            }
            else
            {
                spanList.Add(verticalSpan);
                spanList.Add(horizontalSpan);
            }

            return spanList;
        }

        //offsets a poly 
        public static Polygon2d OffsetPoly(Polygon2d polyOutline, double distance = 0.5)
        {
            if (!ValidateObject.CheckPoly(polyOutline)) return null;
            List<bool> offsetAble = new List<bool>();
            Polygon2d poly = new Polygon2d(polyOutline.Points);
            List<Line2d> linesPoly = poly.Lines;
            for (int i = 0; i < poly.Points.Count; i++)
            {
                int a = i, b = i + 1;
                if (i == poly.Points.Count - 1) b = 0;
                Line2d line = new Line2d(poly.Points[a], poly.Points[b]);
                int dir = LineUtility.DirectionForPointInPoly(line, poly, distance);
                if (dir == 0) return null;
                if(dir == 1)
                {
                    Point2d ptA = LineUtility.OffsetLinePoint(line, line.StartPoint, -1 * distance);
                    Point2d ptB = LineUtility.OffsetLinePoint(line, line.EndPoint, -1 * distance);
                    poly.Points[a] = ptA;
                    poly.Points[b] = ptB;
                }
                else
                {
                    Point2d ptA = LineUtility.OffsetLinePoint(line, line.StartPoint, distance);
                    Point2d ptB = LineUtility.OffsetLinePoint(line, line.EndPoint, distance);
                    poly.Points[a] = ptA;
                    poly.Points[b] = ptB;
                }                
            }
            return new Polygon2d(poly.Points);
        }
       
        //gets a poly and removes a single notch if it meets criteria
        [MultiReturn(new[] {  "PolyReduced", "FoundSmall" })]
        public static Dictionary<string,object> RemoveSingleNotch(Polygon2d polyInp, double distance = 10, int recompute = 2)
        {
            if (!ValidateObject.CheckPoly(polyInp)) return null;
            Polygon2d poly = new Polygon2d(polyInp.Points);
            int startIndex = poly.Points.Count - 1, endIndex = 0, inititalIndex = startIndex;
            bool check = false, found = false;
            int count = 0;

            for(int i = 0; i < poly.Points.Count; i++)
            {
                int a = i, b = i + 1;
                if (i == poly.Points.Count - 1) b = 0;
                if (poly.Lines[a].Length < distance && count<recompute)
                {
                    Point2d midPtLLine = LineUtility.LineMidPoint(poly.Lines[a]);
                    int c = i - 1;
                    int d = b + 1;
                    if (i == 0) c = poly.Points.Count - 1;
                    if (b == poly.Points.Count - 1) d = 0;               
                    if(ValidateObject.CheckLineOrient(poly.Lines[c]) == 0) // horizontal
                    {
                        poly.Points[a] = midPtLLine;
                        poly.Points[c] = new Point2d(poly.Points[c].X, midPtLLine.Y);
                        poly.Points[b] = midPtLLine;
                        poly.Points[d] = new Point2d(poly.Points[d].X, midPtLLine.Y);
                    }
                    else //vertical
                    {
                        poly.Points[a] = midPtLLine;
                        poly.Points[c] = new Point2d(midPtLLine.X, poly.Points[c].Y);
                        poly.Points[b] = midPtLLine;
                        poly.Points[d] = new Point2d(midPtLLine.X, poly.Points[d].Y);
                    }
                    found = true;
                    count += 1;
                }// end of if loop        
            }


            Polygon2d polyReduced = CreateOrthoPoly(new Polygon2d(poly.Points));
            return new Dictionary<string, object>
            {
                { "PolyReduced" , (polyReduced) },
                { "FoundSmall" , (found) }
            };

        }


        //gets a poly and removes small notches based on agiven min distance
        [MultiReturn(new[] { "PointStart", "PointEnd", "ConnectingLine", "PointsProjected", "PolyReduced", "FoundSmall" })]
        public static Dictionary<string, object> RemoveMultipleNotches(Polygon2d poly, double distance = 10)
        {
            if (!ValidateObject.CheckPoly(poly)) return null;
            int startIndex = poly.Points.Count - 1, endIndex = 0, inititalIndex = startIndex;
            bool check = false, found = false;

            for (int i = 0; i < poly.Points.Count; i++)
            {
                int a = i, b = i + 1;
                if (i == poly.Points.Count - 1) b = 0;
                if (poly.Lines[a].Length < distance && poly.Lines[b].Length < distance && !check)
                {
                    if (startIndex > a) startIndex = a;
                    endIndex = b;
                    found = true;
                }
                else if (found) check = true;
            }

            double eps = 100, num = 1000;
            Point2d ptStart = poly.Points[startIndex];
            Point2d ptEnd = poly.Points[endIndex];
            Vector2d vecStartEnd = new Vector2d(ptStart, ptEnd);
            Line2d line = new Line2d(ptStart, ptEnd);

            Point2d midPt = LineUtility.LineMidPoint(line);
            Line2d vertLine = LineUtility.ExtendLine(new Line2d(midPt, new Point2d(midPt.X, midPt.Y + eps)), num);
            Line2d horzLine = LineUtility.ExtendLine(new Line2d(midPt, new Point2d(midPt.X + eps, midPt.Y)), num);
            List<Line2d> lineFormed = new List<Line2d> { line, vertLine, horzLine };

            Point2d projPtStart = new Point2d(0, 0);
            Point2d projPtEnd = new Point2d(0, 0);
            if (vecStartEnd.Y > vecStartEnd.X) // pick vertLine
            {
                projPtStart = new Point2d(midPt.X, ptStart.Y);
                projPtEnd = new Point2d(midPt.X, ptEnd.Y);
            }
            else // pick horzLine
            {
                projPtStart = new Point2d(ptStart.X, midPt.Y);
                projPtEnd = new Point2d(ptEnd.X, midPt.Y);
            }

            List<Point2d> ptFormed = new List<Point2d> { projPtStart, projPtEnd };
            List<Point2d> ptList = new List<Point2d>();
            for (int i = 0; i < poly.Points.Count; i++)
            {
                if (i == startIndex) { ptList.Add(ptStart); ptList.Add(projPtStart); }
                else if (i == endIndex) { ptList.Add(projPtEnd); ptList.Add(ptEnd); }
                else if (i > startIndex && i < endIndex) continue;
                else ptList.Add(poly.Points[i]);
            }
            Polygon2d polyReduced = new Polygon2d(ptList);
            List<int> indices = new List<int> { startIndex, endIndex, inititalIndex };
            return new Dictionary<string, object>
            {
                { "PointStart", (ptStart) },
                { "PointEnd", (check) },
                { "ConnectingLine", (lineFormed) },
                { "PointsProjected", (indices) },
                { "PolyReduced" , (polyReduced) },
                { "FoundSmall" , (found) }
            };

        }

        //calc centroid of a closed polygon2d
        public static Point2d CentroidOfPoly(Polygon2d poly)
        {
            if (!ValidateObject.CheckPoly(poly)) return null;
            List<Point2d> ptList = poly.Points;
            double x = 0, y = 0;
            for (int i = 0; i < ptList.Count; i++)
            {
                x += ptList[i].X;
                y += ptList[i].Y;
            }
            x = x / ptList.Count;
            y = y / ptList.Count;
            Point2d cen = new Point2d(x, y);
            ptList = null;
            return cen;

        }        
      
        //gets the spans in both dir for a polygon2d - repeated 
        internal static List<double> GetSpansXYFromPolygon2d(List<Point2d> poly)
        {
            if (poly == null || poly.Count == 0)
            {
                List<double> zeroList = new List<double>();
                zeroList.Add(0);
                zeroList.Add(0);
                return zeroList;
            }
            List<Point2d> polyBBox = ReadData.FromPointsGetBoundingPoly(poly);
            Range2d polyRange = GetRang2DFromBBox(poly);
            Point2d span = polyRange.Span;
            double horizontalSpan = span.X;
            double verticalSpan = span.Y;
            List<double> spans = new List<double>();
            spans.Add(horizontalSpan);
            spans.Add(verticalSpan);
            return spans;
        }

        //gets the range2d obj for a bounding box
        internal static Range2d GetRang2DFromBBox(List<Point2d> pointList)
        {
            if (pointList == null || pointList.Count == 0) return null;
          
            List<double> xCordList = new List<double>();
            List<double> yCordList = new List<double>();
            double xMax = 0, xMin = 0, yMax = 0, yMin = 0;
            for (int i = 0; i < pointList.Count; i++)
            {
                xCordList.Add(pointList[i].X);
                yCordList.Add(pointList[i].Y);
            }

            xMax = xCordList.Max();
            yMax = yCordList.Max();

            xMin = xCordList.Min();
            yMin = yCordList.Min();

            Range1d ran1X = new Range1d(xMin, xMax);
            Range1d ran1Y = new Range1d(yMin, yMax);
            Range2d ran2D = new Range2d(ran1X, ran1Y);
            return ran2D;
        }        

        //checks all lines of a polyline, if orthogonal or not, if not makes the polyline orthogonal
        public static Polygon2d CreateOrthoPoly(Polygon2d nonOrthoPoly)
        {
            if (!ValidateObject.CheckPoly(nonOrthoPoly)) return null;
            List<Point2d> pointFoundList = new List<Point2d>();
            for(int i = 0; i < nonOrthoPoly.Points.Count; i++)
            {
                int a = i, b = i + 1;
                if (i == nonOrthoPoly.Points.Count - 1) b = 0;
                Line2d line = new Line2d(nonOrthoPoly.Points[a], nonOrthoPoly.Points[b]);
                if (ValidateObject.CheckLineOrient(line)  == -1) // found non ortho
                {
                    nonOrthoPoly.Points[b] = new Point2d(nonOrthoPoly.Points[a].X, nonOrthoPoly.Points[b].Y);
                    pointFoundList.Add(nonOrthoPoly.Points[b]);
                }
                pointFoundList.Add(nonOrthoPoly.Points[b]);
            }

            Polygon2d orthoPoly = new Polygon2d(pointFoundList,0);
            return orthoPoly;
        }

        // reduces points in polygon2d list
        internal static List<Polygon2d> PolyReducePoints(List<Polygon2d> polyList)
        {
            if (polyList == null || polyList.Count == 0) return null;
            List<Polygon2d> reducedPolyList = new List<Polygon2d>();
            for (int i = 0; i < polyList.Count; i++)
            {
                if (polyList[i] == null || polyList[i].Points == null || polyList[i].Points.Count == 0) continue;
                Polygon2d redPoly = new Polygon2d(polyList[i].Points);
                reducedPolyList.Add(redPoly);
            }
            return reducedPolyList;
        }

        //smoothens a polygon2d list by adding points in each poly
        public static List<Polygon2d> SmoothPolygonList(List<Polygon2d> polyList, double spacingProvided = 1)
        {
            if (!ValidateObject.CheckPolyList(polyList)) return null;
            List<Polygon2d> smoothPolyList = new List<Polygon2d>();
            for(int i = 0; i < polyList.Count; i++)
                smoothPolyList.Add(new Polygon2d(SmoothPolygon(polyList[i].Points, spacingProvided), 0));
            return smoothPolyList;
        }

        //smoothens a polygon2d by adding point2d to a given poly
        public static List<Point2d> SmoothPolygon(List<Point2d> pointList, double spacingProvided = 1)
        {
            int threshValue = 50;
            if (pointList == null || pointList.Count == 0) return null;
            //if (pointList.Count > threshValue) return pointList;           
            List<Point2d> ptList = new List<Point2d>();

            for (int i = 0; i < pointList.Count; i++)
            {
                Point2d ptA = pointList[i];
                Point2d ptB = null;
                if (i == pointList.Count - 1) ptB = pointList[0];                
                else ptB = pointList[i + 1];
              
                double dist = new Vector2d(ptA, ptB).Length;
                int numPointsNeeded = (int)(dist / spacingProvided);
                double increment = dist / numPointsNeeded;
                ptList.Add(pointList[i]);

                for (int j = 0; j < numPointsNeeded - 1; j++)
                {
                    double value = (j + 1) * increment / dist;
                    double x = ((1 - value) * ptA.X) + (value * ptB.X);
                    double y = ((1 - value) * ptA.Y) + (value * ptB.Y);
                    ptList.Add(new Point2d(x, y));
                }
            }
            return ptList;
        }

        // returns area of a closed polygon, if area is positive, poly points are counter clockwise and vice versa
        internal static double AreaPolygon(Polygon2d poly, bool value = true)
        {
            if (!ValidateObject.CheckPoly(poly)) return -1;
            List<Point2d> polyPoints = poly.Points;
            double area = 0;
            int j = polyPoints.Count - 1;
            for (int i = 0; i < polyPoints.Count; i++)
            {
                area += (polyPoints[j].X + polyPoints[i].X) * (polyPoints[j].Y - polyPoints[i].Y);
                j = i;
            }
            //if true return absolute value, else return normal value
            if (value) return Math.Abs(area / 2);
            else return area / 2;
        }

        //find if two polys are adjacent, and if yes, then returns the common edge between them
        [MultiReturn(new[] { "Neighbour", "SharedEdge" })]
        internal static Dictionary<string, object> FindPolyAdjacentEdge(Polygon2d polyA, Polygon2d polyB, double eps = 0)
        {
            if (!ValidateObject.CheckPoly(polyA) || !ValidateObject.CheckPoly(polyB)) return null;
            Line2d joinedLine = null;
            bool isNeighbour = false;
            Polygon2d polyAReg = new Polygon2d(polyA.Points, 0);
            Polygon2d polyBReg = new Polygon2d(polyB.Points, 0);

            for (int i = 0; i < polyAReg.Points.Count; i++)
            {
                int a = i + 1;
                if (i == polyAReg.Points.Count - 1) a = 0;
                Line2d lineA = new Line2d(polyAReg.Points[i], polyAReg.Points[a]);
                for (int j = 0; j < polyBReg.Points.Count; j++)
                {
                    int b = j + 1;
                    if (j == polyBReg.Points.Count - 1) b = 0;
                    Line2d lineB = new Line2d(polyBReg.Points[j], polyBReg.Points[b]);
                    bool checkAdj = GraphicsUtility.LineAdjacencyCheck(lineA, lineB, eps);
                    if (checkAdj)
                    {
                        joinedLine = GraphicsUtility.JoinCollinearLines(lineA, lineB);
                        isNeighbour = true;
                        break;
                    }
                }
            }
            return new Dictionary<string, object>
            {
                { "Neighbour", (isNeighbour) },
                { "SharedEdge", (joinedLine) }
            };

        }

    }

}
