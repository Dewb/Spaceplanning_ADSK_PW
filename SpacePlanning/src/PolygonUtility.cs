using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using stuffer;
using Autodesk.DesignScript.Runtime;
using System.Diagnostics;

namespace SpacePlanning
{
    public class PolygonUtility
    {

        //checks a polygon2d to have min Area and min dimensions
        internal static bool CheckPolyDimension(Polygon2d poly, double minArea = 6, double side = 2)
        {
            if (!CheckPoly(poly)) return false;
            if (AreaCheckPolygon(poly) < minArea) return false;
            List<double> spans = GetSpansXYFromPolygon2d(poly.Points);
            if (spans[0] < side) return false;
            if (spans[1] < side) return false;
            return true;
        }

        //check if a polygon is null then return false
        internal static bool CheckPoly(Polygon2d poly)
        {
            if (poly == null || poly.Points == null || poly.Points.Count == 0) return false;
            else return true;
        }

        //check if a pointlist is null then return false
        internal static bool CheckPointList(List<Point2d> ptList)
        {
            if (ptList == null || ptList.Count == 0) return false;
            else return true;
        }

        //checker function - can be discarded later
        internal static List<Point2d> CheckLowest_HighestPoint(Polygon2d poly)
        {
            List<Point2d> returnPts = new List<Point2d>();
            List<Point2d> ptList = poly.Points;
            int highPtInd = GraphicsUtility.ReturnHighestPointFromListNew(ptList);
            int lowPtInd = GraphicsUtility.ReturnLowestPointFromListNew(ptList);
            returnPts.Add(ptList[lowPtInd]);
            returnPts.Add(ptList[highPtInd]);

            return returnPts;

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
                Point2d cen = PolygonUtility.CentroidFromPoly(polygonsList[i]);
                double distance = GraphicsUtility.DistanceBetweenPoints(cen, centerPt);
                distanceList.Add(distance);
            }
            List<int> indices = BasicUtility.SortIndex(distanceList);
            return indices;
        }


        // not using now can be discarded
        internal static List<Point2d> OrganizePointToMakePoly(List<Point2d> poly, List<Point2d> intersectedPoints, List<int> pIndex)
        {
            List<Point2d> sortedPoint = new List<Point2d>();
            List<Point2d> unsortedPt = new List<Point2d>();
            // make two unsorted point lists
            for (int i = 0; i < pIndex.Count; i++)
            {
                unsortedPt.Add(poly[pIndex[i]]);
            }
            unsortedPt.AddRange(intersectedPoints);
            //compute lowest and highest pts
            Point2d lowPt = unsortedPt[GraphicsUtility.ReturnLowestPointFromListNew(unsortedPt)];
            Point2d hiPt = unsortedPt[GraphicsUtility.ReturnHighestPointFromListNew(unsortedPt)];
            //form a line2d between them
            Line2d lineHiLo = new Line2d(lowPt, hiPt);

            //make left and right points based on the line
            List<Point2d> ptOnA = new List<Point2d>();
            List<Point2d> ptOnB = new List<Point2d>();
            for (int i = 0; i < unsortedPt.Count; i++)
            {
                bool check = GraphicsUtility.CheckPointSide(lineHiLo, unsortedPt[i]);
                if (check)
                {
                    //pIndexA.Add(i);
                    ptOnA.Add(unsortedPt[i]);
                }
                else
                {
                    //pIndexB.Add(i);
                    ptOnB.Add(unsortedPt[i]);
                }
            }

            //sort ptOnA and ptOnB individually
            List<Point2d> SortedPtA = GraphicsUtility.SortPointsByDistanceFromPoint(ptOnA,
                         lowPt);
            List<Point2d> SortedPtB = GraphicsUtility.SortPointsByDistanceFromPoint(ptOnB,
                         lowPt);
            SortedPtB.Reverse();
            //add the sorted ptOnA and ptOnB
            sortedPoint.AddRange(SortedPtA);
            sortedPoint.AddRange(SortedPtB);
            return sortedPoint;
        }

    
        //cleans duplicate points and returns updated list - using this now
        internal static List<Point2d> CleanDuplicatePoint2d(List<Point2d> ptListUnclean)
        {
            List<Point2d> cleanList = new List<Point2d>();
            List<double> exprList = new List<double>();
            double a = 45, b = 65;
            for (int i = 0; i < ptListUnclean.Count; i++)
            {
                double expr = a * ptListUnclean[i].X + b * ptListUnclean[i].Y;
                exprList.Add(expr);
            }

            var dups = exprList.GroupBy(x => x)
            .Where(x => x.Count() > 1)
            .Select(x => x.Key)
            .ToList();

            List<double> distinct = exprList.Distinct().ToList();
            for (int i = 0; i < distinct.Count; i++)
            {
                double dis = distinct[i];
                for (int j = 0; j < exprList.Count; j++)
                {
                    if (dis == exprList[j])
                    {
                        cleanList.Add(ptListUnclean[j]);
                        break;
                    }
                }
            }
            return cleanList;

        }
        
        //sort points clockwise direction
        internal static List<Point2d> DoSortClockwise(List<Point2d> poly, List<Point2d> intersectedPoints, List<int> pIndex)
        {
            if (intersectedPoints == null || intersectedPoints.Count == 0) return null;
            List<Point2d> cleanedPtList = new List<Point2d>();
            if (intersectedPoints.Count > 2) cleanedPtList = CleanDuplicatePoint2d(intersectedPoints);
            else cleanedPtList = intersectedPoints;
            Trace.WriteLine("Found Intersections : " + cleanedPtList.Count);
            return OrderPolygon2dPoints(poly, cleanedPtList, pIndex); //intersectedPoints
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
                    List<Point2d> intersNewList = GraphicsUtility.SortPointsByDistanceFromPoint(intersectedPoints,
                        poly[pIndex[i]]);
                    pt.Add(intersNewList[a]);
                    pt.Add(intersNewList[b]);
                    added = true;
                }

                if (i == (pIndex.Count - 2) && added == false)
                {
                    pt.Add(poly[pIndex[i + 1]]);
                    List<Point2d> intersNewList = GraphicsUtility.SortPointsByDistanceFromPoint(intersectedPoints,
                             poly[pIndex[i + 1]]);
                    pt.Add(intersNewList[a]);
                    pt.Add(intersNewList[b]);
                    added = true;
                }

                else if (i == (pIndex.Count - 2) && added == true) pt.Add(poly[pIndex[i + 1]]);
            }
            return pt;
        }

     

        //get a poly and find rectangular polys inside. then merge them together to form a big poly
        public static Dictionary<string, object> MakeWholesomeBlockInPoly(Polygon2d poly, double recompute = 5)
        {
            if (poly == null || poly.Points == null || poly.Points.Count == 0) return null;
            List<Polygon2d> wholesomePolyList = new List<Polygon2d>();
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
            int hIndLow = TestGraphicsUtility.ReturnLowestPointFromList(hMidPt);
            int hIndHigh = GraphicsUtility.ReturnHighestPointFromListNew(hMidPt);
            int vIndLow = GraphicsUtility.ReturnLowestPointFromListNew(vMidPt);
            int vIndHigh = GraphicsUtility.ReturnHighestPointFromListNew(vMidPt);
            hLines.RemoveAt(hIndLow);
            hLines.RemoveAt(hIndHigh);
            vLines.RemoveAt(vIndLow);
            vLines.RemoveAt(vIndHigh);
            List<Line2d> allSplitLines = new List<Line2d>();
            allSplitLines.AddRange(hLines);
            allSplitLines.AddRange(vLines);


            bool splitDone = false;
            Stack<Polygon2d> splittedPolys = new Stack<Polygon2d>();
            Polygon2d currentPoly = new Polygon2d(SmoothPolygon(polyReg.Points, BuildLayout.spacingSet));
            //Polygon2d currentPoly = polyReg;
            splittedPolys.Push(currentPoly);
            Random ran = new Random();
            int countBig = 0, maxRounds = 200;
            List<int> numSidesList = new List<int>();
            List<Polygon2d> allPolyAfterSplit = new List<Polygon2d>();
            while (splittedPolys.Count > 0 && countBig < maxRounds && allSplitLines.Count > 0)
            {
                int count = 0, maxTry = 100;
                int numSides = NumberofSidesPoly(currentPoly);
                numSidesList.Add(numSides);
                //CHECK sides-------------------------------------------------------------------------------
                if (numSides < 5)
                {
                    //ADD to wholesomeblocklist
                    wholesomePolyList.Add(currentPoly);
                    currentPoly = splittedPolys.Pop();
                    Trace.WriteLine("WholeSomeBlock Found " + wholesomePolyList.Count);
                    //continue;
                }
                Trace.WriteLine("================Current Number side Number is  " + numSides);            
                    
                //SPLIT blocks-----------------------------------------------------------------------------                
                while (splitDone == false && count < maxTry && allSplitLines.Count > 0)
                {
                    //randomly get a line
                    int selectLineNum = (int)Math.Floor(BasicUtility.RandomBetweenNumbers(ran, allSplitLines.Count, 0));
                    Line2d splitLine = allSplitLines[selectLineNum];
                    splitLine = LineUtility.move(splitLine, 0.05);
                    Dictionary<string, object> splitPolys = BuildLayout.SplitByLine(currentPoly, splitLine, 0); //{ "PolyAfterSplit", "SplitLine" })]
                    List<Polygon2d> polyAfterSplit = (List<Polygon2d>)splitPolys["PolyAfterSplit"];
                    if (polyAfterSplit == null || polyAfterSplit.Count < 2 || 
                        polyAfterSplit[0] == null || polyAfterSplit[0].Points == null || polyAfterSplit[0].Points.Count == 0 ||
                        polyAfterSplit[1] == null || polyAfterSplit[1].Points == null || polyAfterSplit[1].Points.Count == 0)
                    {
                        Trace.WriteLine("!!!!!!!!!!!!!!!!!!! Drat !!!!!!!!!!!!!!!!");
                        splitDone = false;
                    }
                    else
                    {
                        allSplitLines.RemoveAt(selectLineNum);
                        currentPoly = polyAfterSplit[0];
                        //splittedPolys.Push(polyAfterSplit[0]);
                        splittedPolys.Push(polyAfterSplit[1]);
                        allPolyAfterSplit.AddRange(polyAfterSplit);
                        splitDone = true;
                        Trace.WriteLine("SplitWorked well");
                    }
                    count += 1;                   

                } // end of second while loop      
                Trace.WriteLine("++++Still this many blocks left++++++++++ : " + splittedPolys.Count);
              
                splitDone = false;
                countBig += 1;
                Trace.WriteLine("===============Whiles are going for : " + countBig);
            }// end of 1st while loop

            List<Polygon2d> cleanWholesomePolyList = new List<Polygon2d>();
            //rationalize the wholesome polys
            for(int i = 0; i < wholesomePolyList.Count; i++)
            {
                if (wholesomePolyList[i] == null || wholesomePolyList[i].Points == null || 
                    wholesomePolyList[i].Points.Count < 4 ) continue;
                cleanWholesomePolyList.Add(new Polygon2d(wholesomePolyList[i].Points));
            }

            return new Dictionary<string, object>
            {
                { "WholesomePolys", (cleanWholesomePolyList) },
                { "PolysAfterSplit", (allPolyAfterSplit) }
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
        //random point selector from a list
        internal static Dictionary<int, object> PointSelector(Random ran, List<Point2d> poly)
        {
            Dictionary<int, object> output = new Dictionary<int, object>();
            double num = ran.NextDouble();
            int highInd = GraphicsUtility.ReturnHighestPointFromListNew(poly);
            Point2d hiPt = poly[highInd];
            int lowInd = GraphicsUtility.ReturnLowestPointFromListNew(poly);
            Point2d lowPt = poly[lowInd];
            if (num < 0.5)
            {
                output[0] = lowPt;
                output[1] = 1;
            }
            else
            {
                output[0] = hiPt; //hiPt
                output[1] = -1; //lowPt
            }
            return output;
        }

        //optimizes the points in a list of two points
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
                if (spacing == 0) spacingProvided = BuildLayout.spacingSet;
                else spacingProvided = spacing;
                List<Point2d> ptsPolyA = PolygonUtility.SmoothPolygon(polyA.Points, spacingProvided);
                List<Point2d> ptsPolyB = PolygonUtility.SmoothPolygon(polyB.Points, spacingProvided);
                polyA = new Polygon2d(ptsPolyA, 0);
                polyB = new Polygon2d(ptsPolyB, 0);
            }
            else
            {
                //return the polys as obtained - no smoothing
                polyA = new Polygon2d(sortedA, 0);
                polyB = new Polygon2d(sortedB, 0);
            }
            List<Polygon2d> splittedPoly = new List<Polygon2d>();
            splittedPoly.Add(polyA);
            splittedPoly.Add(polyB);
            return splittedPoly;
        }
        
        //returns the hprizontal and vertical span of a polygon2d , places longer span first
        internal static List<double> PolySpanCheck(Polygon2d poly)
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
        
        //calc centroid of a closed polygon2d
        public static Point2d CentroidFromPoly(Polygon2d poly)
        {
            if (poly == null || poly.Points == null || poly.Points.Count == 0) return null;
            return CentroidFromPoly(poly.Points);
        }
        
        //calc centroid of a closed polygon2d
        public static Point2d CentroidFromPoly(List<Point2d> ptList)
        {
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
        
      
        //gets the spans in both dir for a polygon2d
        public static List<double> GetSpansXYFromPolygon2d(List<Point2d> poly)
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
        public static Range2d GetRang2DFromBBox(List<Point2d> pointList)
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

        //smoothens a polygon2d by adding point2d in between
        internal static List<Point2d> SmoothPolygon(List<Point2d> pointList, double spacingProvided = 1)
        {
            int threshValue = 50;
            if (pointList == null || pointList.Count == 0) return null;
            if (pointList.Count > threshValue) return pointList;
           
            List<double> spans = GetSpansXYFromPolygon2d(pointList);
            double spanX = spans[0];
            double spanY = spans[1];
            List<Point2d> ptList = new List<Point2d>();

            for (int i = 0; i < pointList.Count; i++)
            {
                Point2d ptA = pointList[i];
                Point2d ptB = null;
                if (i == pointList.Count - 1) ptB = pointList[0];                
                else ptB = pointList[i + 1];
              
                Vector2d vec = new Vector2d(ptA, ptB);
                double dist = vec.Length;
                int numPointsNeeded = (int)(dist / spacingProvided);
                double increment = dist / numPointsNeeded;
                ptList.Add(pointList[i]);

                for (int j = 0; j < numPointsNeeded - 1; j++)
                {
                    double value = (j + 1) * increment / dist;
                    double x = ((1 - value) * ptA.X) + (value * ptB.X);
                    double y = ((1 - value) * ptA.Y) + (value * ptB.Y);
                    Point2d ptAdd = new Point2d(x, y);
                    ptList.Add(ptAdd);
                }
            }
            return ptList;
        }
        
        //checks area of a polygon2d
        public static double AreaCheckPolygon(Polygon2d poly)
        {
            if (poly == null) return 0;           
            return GraphicsUtility.AreaPolygon2d(poly.Points);
        }
        
        //to check smoothened polygon2d
        public static List<Point2d> SmoothCheckerForPoly(Polygon2d poly, int spacing)
        {
            List<Point2d> smoothedPoints = SmoothPolygon(poly.Points, spacing);
            return smoothedPoints;
        }

        //checks the ratio of the dimension of a poly bbox of certain proportion or not
        internal static bool CheckPolyBBox(Polygon2d poly, double num = 3)
        {
            bool check = false;
            Range2d range = poly.BBox;
            double X = range.Xrange.Span;
            double Y = range.Yrange.Span;
            if (Y < X)
            {
                double div1 = X / Y;
                if (div1 > num) check = true;
            }
            else
            {
                double div1 = Y / X;
                if (div1 > num) check = true;
            }
            return check;
        }


        //find if two polys are adjacent, and if yes, then returns the common edge between them
        [MultiReturn(new[] { "Neighbour", "SharedEdge" })]
        public static Dictionary<string, object> FindPolyAdjacentEdge(Polygon2d polyA, Polygon2d polyB, double eps = 0)
        {
            if (polyA == null || polyB == null) return null;
            if (polyA.Points == null || polyB.Points == null) return null;
            if (polyA.Points.Count == 0 || polyB.Points.Count == 0) return null;

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
