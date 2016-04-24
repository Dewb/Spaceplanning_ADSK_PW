using System;
using System.Collections.Generic;
using System.Linq;
using stuffer;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;

namespace SpacePlanning
{
    internal class TestGraphicsUtility
    {
        //################################################################################################################
        //this class stores methods realted to GraphicsUtility class which needs to be tested further for reliability
        //################################################################################################################
        
        // checks if two lines are collinear - not using
        public static bool CheckLineCollinear(Line2d lineA, Line2d lineB)
        {
            Point2d p1 = lineA.StartPoint;
            Point2d p2 = lineA.EndPoint;
            Point2d q1 = lineB.StartPoint;
            Point2d q2 = lineB.EndPoint;

            // Find the four orientations needed for general and special cases
            int o1 = GraphicsUtility.Orientation(p1, q1, p2);
            int o2 = GraphicsUtility.Orientation(p1, q1, q2);
            int o3 = GraphicsUtility.Orientation(p2, q2, p1);
            int o4 = GraphicsUtility.Orientation(p2, q2, q1);

            // General case
            if (o1 != o2 && o3 != o4)
                return false;
            // p1, q1 and p2 are colinear and p2 lies on segment p1q1
            if (o1 == 0 && GraphicsUtility.OnSegment(p1, p2, q1)) return true;

            // p1, q1 and p2 are colinear and q2 lies on segment p1q1
            if (o2 == 0 && GraphicsUtility.OnSegment(p1, q2, q1)) return true;

            // p2, q2 and p1 are colinear and p1 lies on segment p2q2
            if (o3 == 0 && GraphicsUtility.OnSegment(p2, p1, q2)) return true;

            // p2, q2 and q1 are colinear and q1 lies on segment p2q2
            if (o4 == 0 && GraphicsUtility.OnSegment(p2, q1, q2)) return true;
            return false; // Doesn't fall in any of the above cases
        }

        //make points on grids - not using now
        public static List<Point> MakeSquarePointsFromCenterSide(Point2d centerPt, double side)
        {
            List<Point> ptList = new List<Point>();
            double a = centerPt.X - (side / 2);
            double b = centerPt.Y - (side / 2);
            Point pt = Point.ByCoordinates(a, b);
            ptList.Add(pt);

            a = centerPt.X - (side / 2);
            b = centerPt.Y + (side / 2);
            pt = Point.ByCoordinates(a, b);
            ptList.Add(pt);

            a = centerPt.X + (side / 2);
            b = centerPt.Y + (side / 2);
            pt = Point.ByCoordinates(a, b);
            ptList.Add(pt);

            a = centerPt.X + (side / 2);
            b = centerPt.Y - (side / 2);
            pt = Point.ByCoordinates(a, b);
            ptList.Add(pt);
            return ptList;
        }

        //make cell on grids - not using now
        public static Polygon MakeSquareFromCenterSide(Point2d centerPt, double side)
        {
            return Polygon.ByPoints(MakeSquarePointsFromCenterSide(centerPt, side));
        }

        //finds line and line intersection point - not using now
        public static Point2d LineLineIntersectionNew(Line2d s1, Line2d s2)
        {
            Point2d startS1 = s1.StartPoint;
            Point2d endS1 = s1.EndPoint;
            Point2d startS2 = s2.StartPoint;
            Point2d endS2 = s2.EndPoint;

            double As1, Bs1, Cs1;
            double As2, Bs2, Cs2;
            As1 = endS1.Y - startS1.Y;
            Bs1 = startS1.X - endS1.X;
            Cs1 = As1 * startS1.X + Bs1 * startS1.Y;
            As2 = endS2.Y - startS2.Y;
            Bs2 = startS2.X - endS2.X;
            Cs2 = As1 * startS2.X + Bs2 * startS2.Y;

            double det = As1 * Bs2 - As2 * Bs1;
            if (det == 0) return null;
            else
            {
                double x = (Bs2 * Cs1 - Bs1 * Cs2) / det;
                double y = (As1 * Cs2 - As2 * Cs1) / det;
                return new Point2d(x, y);
            }


        }

        //line and polygon intersection - not using now
        public static Line2d LinePolygonIntersectionReturnLine(List<Point2d> poly, Line2d testLine, Point2d centerPt)
        {
            Random ran = new Random();
            double dist = 10000000000000000;
            SortedDictionary<double, Line2d> sortedIntersectionLines = new SortedDictionary<double, Line2d>();
            List<Point2d> ptList = new List<Point2d>();
            double x = (testLine.StartPoint.X + testLine.EndPoint.X) / 2;
            double y = (testLine.StartPoint.Y + testLine.EndPoint.Y) / 2;
            Point2d midPt = new Point2d(x, y);
            Line2d intersectedLineInPoly = null;
            int count = 0;
            for (int i = 0; i < poly.Count - 1; i++)
            {
                Point2d pt1 = poly[i];
                Point2d pt2 = poly[i + 1];
                Line2d edge = new Line2d(pt1, pt2);

                if (LineLineIntersectionNew(edge, testLine) != null)
                {

                    double xE = (edge.StartPoint.X + edge.EndPoint.X) / 2;
                    double yE = (edge.StartPoint.Y + edge.EndPoint.Y) / 2;
                    Point2d EdgeMidPt = new Point2d(xE, yE);
                    double checkDist = GraphicsUtility.DistanceBetweenPoints(centerPt, EdgeMidPt);
                    try
                    {
                        sortedIntersectionLines.Add(checkDist, edge);
                    }
                    catch (Exception)
                    {

                        double eps = ran.NextDouble() * 2;
                        double newDist = checkDist - eps;
                        sortedIntersectionLines.Add(newDist, edge);
                    }
                    intersectedLineInPoly = edge;
                    count += 1;
                }

            }
            if (sortedIntersectionLines.Count > 0)
            {

                foreach (KeyValuePair<double, Line2d> p in sortedIntersectionLines)
                {
                    intersectedLineInPoly = p.Value;
                    break;
                }
            }
            else intersectedLineInPoly = null;
            return intersectedLineInPoly;
        }

        //line and polygon intersection - not using now
        internal static List<Point2d> LinePolygonIntersectionIndex(List<Point2d> poly, Line2d testLine)
        {
            int n = poly.Count;
            double eps = 0.00000001;
            double tE = 0;              // the maximum entering segment parameter
            double tL = 1;              // the minimum leaving segment parameter
            double t, N, D;             // intersect parameter t = N / D
            Vector2d dS = new Vector2d(testLine.StartPoint, testLine.EndPoint);   // the  segment direction vector
            Vector2d e;                 // edge vector
            Vector2d ne;               // edge outward normal (not explicit in code)
            Vector2d ef;


            for (int i = 0; i < n - 1; i++)   // process polygon edge V[i]V[i+1]
            {
                e = new Vector2d(poly[i + 1], poly[i]);
                ne = new Vector2d(-e.Y, e.X);
                ef = new Vector2d(testLine.StartPoint, poly[i]);
                N = ne.Dot(ef); // = -dot(ne, S.P0 - V[i])
                D = dS.Dot(ne);       // = dot(ne, dS)

                if (Math.Abs(D) < eps)
                {  // S is nearly parallel to this edge
                    if (N < 0)              // P0 is outside this edge, so
                        return null;      // S is outside the polygon
                    else                    // S cannot cross this edge, so
                        continue;          // ignore this edge
                }

                t = N / D;
                if (D < 0)
                {            // segment S is entering across this edge
                    if (t > tE)
                    {       // new max tE
                        tE = t;
                        if (tE > tL)   // S enters after leaving polygon
                            return null;
                    }
                }
                else {                  // segment S is leaving across this edge
                    if (t < tL)
                    {       // new min tL
                        tL = t;
                        if (tL < tE)   // S leaves before entering polygon
                            return null;
                    }
                }
            }

            // tE <= tL implies that there is a valid intersection subsegment
            Point2d p1 = testLine.StartPoint + dS.Scale(tE);   // = P(tE) = point where S enters polygon
            Point2d p2 = testLine.StartPoint + dS.Scale(tL);   // = P(tL) = point where S leaves polygon

            List<Point2d> ptList = new List<Point2d>();
            ptList.Add(p1);
            ptList.Add(p2);
            return ptList;
        }



        //returns the point having highest x,y value from a list - using now
        internal static int ReturnHighestPointFromList(List<Point2d> ptList)
        {
            if (!PolygonUtility.CheckPointList(ptList)) return -1;
            Point2d highestPoint = ptList[0];
            int size = ptList.Count;
            int index = 0;
            for (int i = 0; i < size; i++)
            {
                if ((highestPoint.Y < ptList[i].Y) || (highestPoint.Y == ptList[i].Y && highestPoint.X > ptList[i].X))
                {
                    highestPoint = ptList[i];
                    index = i;
                }
            }
            return index;
        }

        //returns the point having lowest x,y value from a list
        internal static int ReturnLowestPointFromList(List<Point2d> ptList)
        {
            if (!PolygonUtility.CheckPointList(ptList)) return -1;
            Point2d lowestPoint = ptList[0];
            int size = ptList.Count;
            int index = 0;
            for (int i = 0; i < size; i++)
            {
                if (lowestPoint.Y > ptList[0].Y || (lowestPoint.Y == ptList[i].Y && lowestPoint.X > ptList[i].X))
                {
                    lowestPoint = ptList[i];
                    index = i;
                }
            }
            return index;

        }
        
        //changes the center of one or both polys to ensure correct intersection line is found
        [MultiReturn(new[] { "CenterPolyA", "CenterPolyB", "PolyA", "PolyB" })]
        internal static Dictionary<string, object> ComputePolyCentersAlign(Polygon2d polyA, Polygon2d polyB)
        {
            //compute orig centers
            Point2d centerPolyA = GraphicsUtility.CentroidInPointLists(polyA.Points);
            Point2d centerPolyB = GraphicsUtility.CentroidInPointLists(polyB.Points);

            Point2d staticPoint, movingPoint;
            Polygon2d staticPoly, movingPoly;

            double areaPolyA = GraphicsUtility.AreaPolygon2d(polyA.Points);
            double areaPolyB = GraphicsUtility.AreaPolygon2d(polyB.Points);
            if (areaPolyA > areaPolyB)
            {
                staticPoint = centerPolyB;
                staticPoly = polyB;
                movingPoint = centerPolyA;
                movingPoly = polyA;
            }
            else
            {
                staticPoint = centerPolyA;
                staticPoly = polyA;
                movingPoint = centerPolyB;
                movingPoly = polyB;
            }

            //shift the other points
            Point2d movingPoint1 = new Point2d(staticPoint.X, movingPoint.Y);
            Point2d movingPoint2 = new Point2d(movingPoint.X, staticPoint.Y);
            bool IsMovingPoint1 = GraphicsUtility.PointInsidePolygonTest(movingPoly.Points, movingPoint1);
            bool IsMovingPoint2 = GraphicsUtility.PointInsidePolygonTest(movingPoly.Points, movingPoint2);

            if (IsMovingPoint1) movingPoint = movingPoint1;
            else if (IsMovingPoint2) movingPoint = movingPoint2;
            else
            {
                staticPoint = centerPolyA;
                staticPoly = polyA;
                movingPoint = movingPoint1;
                movingPoly = polyB;
            }

            return new Dictionary<string, object>
                {
                { "CenterPolyA", (staticPoint) },
                { "CenterPolyB", (movingPoint) },
                { "PolyA", (staticPoly) },
                { "PolyB", (movingPoly) }
                };
        }
        

        [MultiReturn(new[] { "Neighbour", "SharedEdgeA", "SharedEdgeB", "LineMoved", "CenterToCenterLine", "CenterPolyPoint", "CenterPolyOtherPoint" })]
        internal static Dictionary<string, object> PolygonPolygonCommonEdgeDict(Polygon2d poly, Polygon2d other)
        {

            bool check = false;
            if (poly == null || other == null) return null;

            double eps = 200;
            Polygon2d polyReg = new Polygon2d(poly.Points);
            Polygon2d otherReg = new Polygon2d(other.Points);
            Dictionary<string, object> UpdatedCenters = ComputePolyCentersAlign(polyReg, otherReg);

            Point2d centerPoly = (Point2d)UpdatedCenters["CenterPolyA"];
            Point2d centerOther = (Point2d)UpdatedCenters["CenterPolyB"];
            polyReg = (Polygon2d)UpdatedCenters["PolyA"];
            otherReg = (Polygon2d)UpdatedCenters["PolyB"];
            //make vectors
            Vector2d centerToCen = new Vector2d(centerPoly, centerOther);
            Vector2d centerToCenX = new Vector2d(centerToCen.X, 0);
            Vector2d centerToCenY = new Vector2d(0, centerToCen.Y);
            //make centerLine
            Line2d centerLine = new Line2d(centerPoly, centerOther);
            Vector2d keyVec;
            if (centerToCenX.Length > centerToCenY.Length) keyVec = new Vector2d(centerToCenX.X, centerToCenX.Y);
            else keyVec = new Vector2d(centerToCenY.X, centerToCenY.Y);
            //check line poly intersection between centertocen vector and each polys            
            Line2d lineInPolyReg = TestGraphicsUtility.LinePolygonIntersectionReturnLine(polyReg.Points, centerLine, centerOther);
            Line2d lineInOtherReg = TestGraphicsUtility.LinePolygonIntersectionReturnLine(otherReg.Points, centerLine, centerPoly);

            //find distance d1 and d2 from two centers to linepolyintersection line
            Point2d projectedPtOnPolyReg = GraphicsUtility.ProjectedPointOnLine(lineInPolyReg, centerPoly);
            Point2d projectedPtOnOtherReg = GraphicsUtility.ProjectedPointOnLine(lineInOtherReg, centerOther);

            double dist1 = GraphicsUtility.DistanceBetweenPoints(centerPoly, projectedPtOnPolyReg);
            double dist2 = GraphicsUtility.DistanceBetweenPoints(centerOther, projectedPtOnOtherReg);

            double totalDistance = 2 * (dist1 + dist2);
            Line2d lineMoved = new Line2d(lineInPolyReg.StartPoint, lineInPolyReg.EndPoint);
            lineMoved = LineUtility.Move(lineMoved, centerPoly);
            Point2d projectedPt = GraphicsUtility.ProjectedPointOnLine(lineMoved, centerOther);
            double distance = GraphicsUtility.DistanceBetweenPoints(projectedPt, centerOther);

            bool isNeighbour = false;
            if (totalDistance - eps < distance && distance < totalDistance + eps) isNeighbour = true;
            else isNeighbour = false;

            return new Dictionary<string, object>
            {
                { "Neighbour", (isNeighbour) },
                { "SharedEdgeA", (lineInPolyReg) },
                { "SharedEdgeB", (lineInOtherReg) },
                { "LineMoved", (lineMoved) },
                { "CenterToCenterLine", (centerLine) },
                { "CenterPolyPoint", (centerPoly) },
                { "CenterPolyOtherPoint", (centerOther) },
            };

        }

        //sort a list with Quicksort algorithm
        public static void Quicksort2(ref IComparable[] elements, int left, int right)
        {
            int i = left, j = right;
            IComparable pivot = elements[(left + right) / 2];
            while (i <= j)
            {
                while (elements[i].CompareTo(pivot) < 0) i++;
                while (elements[j].CompareTo(pivot) > 0) j--;

                if (i <= j)
                {
                    IComparable tmp = elements[i];
                    elements[i] = elements[j];
                    elements[j] = tmp;
                    i++;
                    j--;
                }
            }
            if (left < j) Quicksort2(ref elements, left, j);
            if (i < right) Quicksort2(ref elements, i, right);
        }

        //removes duplicates lines from a list of line
        internal static List<Line2d> RemoveDuplicateLinesOld(List<double> exprList, List<Line2d> lineList)
        {
            List<Line2d> cleanLineList = new List<Line2d>();
            List<double> distinct = exprList.Distinct().ToList();
            for (int i = 0; i < distinct.Count; i++)
            {
                double dis = distinct[i];
                for (int j = 0; j < exprList.Count; j++)
                {
                    if (dis == exprList[j])
                    {
                        cleanLineList.Add(lineList[j]);
                        break;
                    }
                }
            }
            return cleanLineList;
        }
    }
}
