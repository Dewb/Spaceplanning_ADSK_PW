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

        //checks a polygonlist for non orthogonal polys and removes them if any
        public static List<Polygon2d> GetOrthoPolys(List<Polygon2d> nonOrthoPolyList, double eps = 0)
        {
            if (!ValidateObject.CheckPolyList(nonOrthoPolyList)) return null;            
            List<Polygon2d> orthoPolyList = new List<Polygon2d>();
            List<Polygon2d> orthoPoly = new List<Polygon2d>();
            for(int i = 0; i < nonOrthoPolyList.Count; i++)
            {
                Polygon2d polyNew = new Polygon2d(nonOrthoPolyList[i].Points);
                bool result = ValidateObject.CheckPolygon2dOrtho(polyNew, eps);
                if (result) orthoPoly.Add(polyNew);
            }
            return orthoPoly;
        }
    

        //gives the highest and lowest point from poly points
        public static List<Point2d> GetLowestAndHighestPointFromPoly(Polygon2d poly)
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
       
        
        //gets a poly and removes small notches based on agiven min distance
        [MultiReturn(new[] { "PolyNotchRemoved", "NotchFound"})]
        internal static Dictionary<string, object> RemoveAnyNotches(Polygon2d polyInp, double distance = 10)
        {
            if (!ValidateObject.CheckPoly(polyInp)) return null;

            bool  found = false;
            Polygon2d poly = new Polygon2d(polyInp.Points);
            for (int i = 0; i < poly.Points.Count; i++)
            {
                int a = i, b = i + 1, c = i-1, d = i+2;
                if (i == 0) c = poly.Points.Count - 1;
                if (i == poly.Points.Count - 1) { b = 0; d = 1; }
                if (i == poly.Points.Count - 2) { d = 0; }
                if (poly.Lines[a].Length < distance)
                {
                    int orient = ValidateObject.CheckLineOrient(poly.Lines[a]);
                    if (orient == 0)// horizontal line
                    {
                        poly.Points[b] = poly.Points[a];
                        poly.Points[d] = new Point2d(poly.Points[a].X, poly.Points[d].Y);
                    }
                    else // vertical line
                    {
                        poly.Points[b] = poly.Points[a];
                        poly.Points[d] = new Point2d(poly.Points[d].X, poly.Points[a].Y);
                    }
                    found = true;
                }
             
            }
            Polygon2d polyNew = new Polygon2d(poly.Points);         
            return new Dictionary<string, object>
            {
                { "PolyNotchRemoved" , (polyNew) },
                { "NotchFound" , (found) }
            };

        }

        //gets a poly and removes small notches based on agiven min distance
        [MultiReturn(new[] { "PolyNotchRemoved", "NotchFound" })]
        public static Dictionary<string, object> RemoveAllNotches(Polygon2d polyInp, double distance = 10)
        {
            if (!ValidateObject.CheckPoly(polyInp)) return null;
            bool found = true;
            int count = 0, maxTry = polyInp.Lines.Count;
            
            Polygon2d currentPoly = new Polygon2d(polyInp.Points);
            while (found && count <maxTry)
            {
                count += 1;
                Dictionary<string, object> notchObj = RemoveAnyNotches(currentPoly, distance);
                currentPoly = (Polygon2d)notchObj["PolyNotchRemoved"];
                found = (bool)notchObj["NotchFound"];
                Trace.WriteLine("still notches : " + count);
            }
            Polygon2d polyNew = CreateOrthoPoly(currentPoly);
            if (!ValidateObject.CheckPoly(polyNew)) { polyNew = polyInp; found = false; }
            return new Dictionary<string, object>
            {
                { "PolyNotchRemoved" , (polyNew) },
                { "NotchFound" , (found) }
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
        public static double AreaPolygon(Polygon2d poly, bool value = true)
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

        //find if two polys are adjacent, and if yes, then returns the common edge between them
        [MultiReturn(new[] { "Neighbour", "SharedEdge" })]
        internal static Dictionary<string, object> FindPolyAdjacentEdgeEdit(Polygon2d polyA, Polygon2d polyB, double eps = 0)
        {
            if (!ValidateObject.CheckPoly(polyA) || !ValidateObject.CheckPoly(polyB)) return null;
            Line2d joinedLine = null;
            bool isNeighbour = false;
            Polygon2d polyAReg = new Polygon2d(polyA.Points);
            Polygon2d polyBReg = new Polygon2d(polyB.Points);

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
                    if(lineA.StartPoint.Compare(lineB.StartPoint) && lineA.EndPoint.Compare(lineB.EndPoint))
                    {
                        joinedLine = lineA;
                        isNeighbour = true;
                    }
                    else if (lineA.EndPoint.Compare(lineB.StartPoint) && lineA.StartPoint.Compare(lineB.EndPoint))
                    {
                        joinedLine = lineB;
                        isNeighbour = true;
                    }
                   /* bool checkAdj = GraphicsUtility.LineAdjacencyCheck(lineA, lineB, eps);
                    if (checkAdj)
                    {
                        if (lineA.Length > lineB.Length) joinedLine = lineA;
                        else joinedLine = lineB;
                        //joinedLine = GraphicsUtility.JoinCollinearLines(lineA, lineB);
                        isNeighbour = true;
                        break;
                    }
                    */
                }
            }
            return new Dictionary<string, object>
            {
                { "Neighbour", (isNeighbour) },
                { "SharedEdge", (joinedLine) }
            };

        }

        //makes a square polygon2d
        public static Polygon2d SquareByCenter(Point2d center, double side = 5)
        {
            Point2d pt1 = Point2d.ByCoordinates(center.X - side / 2, center.Y - side / 2);
            Point2d pt2 = Point2d.ByCoordinates(center.X + side / 2, center.Y - side / 2);
            Point2d pt3 = Point2d.ByCoordinates(center.X + side / 2, center.Y + side / 2);
            Point2d pt4 = Point2d.ByCoordinates(center.X - side / 2, center.Y + side / 2);
            List<Point2d> ptList = new List<Point2d>() { pt1, pt2, pt3, pt4 };
            return new Polygon2d(ptList);
        }

        //makes a square polygon2d
        public static Polygon2d CircleByRadius(Point2d center, double radius = 5, int segments = 10)
        {
            if (center == null) return null;
            List<Point2d> ptList = new List<Point2d>();
            for (int i = 0; i < segments; i++)
            {
                double x = center.X + radius * Math.Cos((Math.PI/ 180) *(360 * (i + 1) / segments)), y = center.Y + radius * Math.Sin((Math.PI/ 180)*(360 * (i + 1) / segments));
                Point2d pt = new Point2d(x, y);
                //Trace.WriteLine("Points are : " + pt.X + " , " + pt.Y);
                ptList.Add(pt);
            }            
            return new Polygon2d(ptList);
        }

        //makes a square polygon2d
        public static Polygon2d RectangleByLowHighPoint(Point2d low, Point2d high)
        {
            Point2d lowOne = Point2d.ByCoordinates(high.X, low.Y);
            Point2d highOne = Point2d.ByCoordinates(low.X,high.Y);
            List<Point2d> ptList = new List<Point2d>() { low,lowOne,high,highOne};
            return new Polygon2d(ptList);
        }

        //finds a center point with respect to a given poly. Directions are: 0 = right, 1 = up, 2 = left, 3 = down
        public static Point2d FindPointOnPolySide(Polygon2d poly, int dir = 0, double dist = 10)
        {
            if (!ValidateObject.CheckPoly(poly)) return null;
            Point2d centerFound = new Point2d(0,0);
            List<Point2d> lowAndHighPt = GetLowestAndHighestPointFromPoly(poly);
            Point2d low = lowAndHighPt[0];
            Point2d high = lowAndHighPt[1];
            // switch based on dir input
            switch (dir)
            {
                case 0: // go right
                    centerFound = new Point2d(high.X + dist, high.Y - dist);
                    break;
                case 1: // go up
                    centerFound = new Point2d(high.X - dist, high.Y + dist);
                    break;
                case 2: // go left
                    centerFound = new Point2d(low.X - dist, low.Y + dist);
                    break;
                case 3: // go down
                    centerFound = new Point2d(low.X + dist, low.Y - dist);
                    break;
                default:
                    centerFound = new Point2d(high.X + dist, high.Y - dist);
                    break;
            }
            return centerFound;
        }

        //places a point randomly inside a poly | padding should be between 0.2 to 1.0. -1 = anywhere, 0 = towards upper right, 1 = towards lower left
        public static Point2d PlaceRandomPointInsidePoly(Polygon2d poly, int seed = 1, int side = -1)
        {
            if (!ValidateObject.CheckPoly(poly)) return null;
            Point2d centerFound = new Point2d(0, 0), low = new Point2d(0, 0), high = new Point2d(0, 0);
            List<Point2d> lowAndHighPt = GetLowestAndHighestPointFromPoly(poly);
            if (side == 0) // upper right
            {
                low = centerFound;
                high = lowAndHighPt[1];
            }
            else if ( side == 1) // loower left
            {
                low = lowAndHighPt[0];
                high = centerFound;
            }
            else // default, anywhere
            {
                low = lowAndHighPt[0];
                high = lowAndHighPt[1];
            }
           
            Random rnd = new Random(seed);
            double x = BasicUtility.RandomBetweenNumbers(rnd, high.X, low.X);
            double y = BasicUtility.RandomBetweenNumbers(rnd, high.Y, low.Y);
            Point2d pointFound = new Point2d(x, y);
            bool pointInside = false;
            int count = 0, maxTry = 100;
            while (!pointInside && count < maxTry)
            {
                count += 1;
                if (GraphicsUtility.PointInsidePolygonTest(poly, pointFound)) pointInside = true;
            }
            return pointFound;

        }


        //places a point randomly inside a poly | padding should be between 0.2 to 1.0. -1 = anywhere, 0 = towards upper right, 1 = towards lower left
        [MultiReturn(new[] { "RandomPoint", "FoundPointList", "LineSelected"})]
        public static Dictionary<string, object> GetPointOnOneQuadrantTest(Polygon2d poly, int seed = 1, int side = -1, double scale = 0.5)
        {
            if (!ValidateObject.CheckPoly(poly)) return null;
            Point2d centerFound = CentroidOfPoly(poly);
            //Point2d ptA = new Point2d(centerFound.X, centerFound.Y + 2000), ptB = new Point2d(centerFound.X, centerFound.Y - 2000);
            //Line2d centerVertLine = Line2d.ByStartPointEndPoint(ptA, ptB);
            List<Point2d> pointLowerList = new List<Point2d>();
            poly = new Polygon2d(SmoothPolygon(poly.Points, 3),0);
            if (side == 0) // upper right
            {
                for (int i = 0; i < poly.Points.Count; i++)
                {
                    if (poly.Points[i].X < centerFound.X && poly.Points[i].Y > centerFound.Y) pointLowerList.Add(poly.Points[i]);
                }
            }
            else if (side == 1) // lower left
            {
                for (int i = 0; i < poly.Points.Count; i++)
                {
                    if (poly.Points[i].X < centerFound.X && poly.Points[i].Y < centerFound.Y) pointLowerList.Add(poly.Points[i]);
                }
            }
            else // default, anywhere
            {
                pointLowerList = poly.Points;
            }
            int index = (int)BasicUtility.RandomBetweenNumbers(new Random(seed), pointLowerList.Count, 0);
            Line2d lineSelected = new Line2d(centerFound, pointLowerList[index]);
            Vector2d vec = new Vector2d(centerFound, pointLowerList[index]);
            scale = BasicUtility.RandomBetweenNumbers(new Random(seed), 1, 0);
            Point2d selectedPt = VectorUtility.VectorAddToPoint(centerFound, vec, scale);
            return new Dictionary<string, object>
            {
                { "RandomPoint", (selectedPt) },    
                { "FoundPointList", (pointLowerList) },
                { "LineSelected", (lineSelected) }
            };
        }

        //gets the extreme points from cell list, top right, top left, bottom right, bottom left
        [MultiReturn(new[] { "TopRightPoint", "TopLeftPoint", "BottomRightPoint", "BottomLeftPoint" })]
        public static Dictionary<string, object> GetExtremePointsFromCells(List<Cell> cellList)
        {
            if (cellList == null) return null;
            List<Point2d> ptLists = new List<Point2d>();
            for(int i = 0; i < cellList.Count; i++) ptLists.Add(cellList[i].CenterPoint);
            return GetExtremePointsFromPoints(ptLists); 
        }


        //gets the extreme points from apoint list, top right, top left, bottom right, bottom left // needs work later
        [MultiReturn(new[] { "TopRightPoint", "TopLeftPoint", "BottomRightPoint", "BottomLeftPoint", "RightTopPoint", "LeftTopPoint", "RightBottomPoint", "LeftBottomPoint" })]
        public static Dictionary<string, object> GetExtremePointsFromPoints(List<Point2d> ptLists)
        {
            if (!ValidateObject.CheckPointList(ptLists)) return null;
            Point2d center = CentroidOfPoly(new Polygon2d(ptLists, 0));
            Point2d topRight = ptLists[0], topLeft = ptLists[0], bottomRight = ptLists[0], bottomLeft = ptLists[0];
            Point2d rightTop = ptLists[0], leftTop = ptLists[0], rightBottom = ptLists[0], leftBottom = ptLists[0];
            for (int i = 0; i < ptLists.Count; i++)
            {
                if (ptLists[i].X >= topRight.X && ptLists[i].Y >= topRight.Y) topRight = ptLists[i]; //top right corner point 
                if (ptLists[i].X <= topLeft.X && ptLists[i].Y >= topLeft.Y) topLeft = ptLists[i]; //top left corner point 
                if (ptLists[i].X >= bottomRight.X && ptLists[i].Y <= bottomRight.Y) bottomRight = ptLists[i]; //bottom right corner point 
                if (ptLists[i].X <= bottomLeft.X && ptLists[i].Y <= bottomLeft.Y) bottomLeft = ptLists[i]; //bottom right corner point 
           
            }

            return new Dictionary<string, object>
            {
                { "TopRightPoint", (topRight) },
                { "TopLeftPoint", (topLeft) },
                { "BottomRightPoint", (bottomRight) },
                { "BottomLeftPoint", (bottomLeft) },
                { "RightTopPoint", (rightTop) },
                { "LeftTopPoint", (leftTop) },
                { "RightBottomPoint", (rightBottom) },
                { "LeftBottomPoint", (leftBottom) }
            };
        }

    }

}
