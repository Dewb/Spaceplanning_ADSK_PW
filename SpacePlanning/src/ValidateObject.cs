using System;
using System.Collections.Generic;
using System.Linq;
using stuffer;
using Autodesk.DesignScript.Runtime;
using System.Diagnostics;

namespace SpacePlanning
{
    public static class ValidateObject
    {
        #region - Private Methods
        // Given three colinear points p, q, r, the function checks if
        // point q lies on line segment 'pr'
        internal static bool CheckOnSegment(Point2d p, Point2d q, Point2d r)
        {
            if (q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) &&
                q.Y <= Math.Max(p.Y, r.Y) && q.Y >= Math.Min(p.Y, r.Y)) return true;
            return false;
        }

        //method to check a list of polygon2ds if they have all orthogonal lines or not
        public static bool CheckPolygon2dOrthogonality(List<Polygon2d> polyList,double eps =0)
        {
            if (polyList == null) return false;
            for(int i = 0; i < polyList.Count; i++)
            {
                if (!CheckPoly(polyList[i])) return false;
                Polygon2d polySimple = new Polygon2d(polyList[i].Points);               
                List<Line2d> lineList = polySimple.Lines;
                for(int j = 0; j < lineList.Count; j++) if (!CheckLineOrthogonal(lineList[j],eps)) return false;
            }
            return true;

        }

        //


        //this checks a polylist for abnormal polys and returns only those which deem fit
        internal static List<Polygon2d>CheckAndCleanPolygon2dList(List<Polygon2d> polyList)
        {
            List<Polygon2d> cleanPolyList = new List<Polygon2d>();
            for (int i = 0; i < polyList.Count; i++)
            {
                bool lineLength = true;
                for (int j = 0; j < polyList[i].Lines.Count; j++) if (polyList[i].Lines[j].Length < 0.01 && PolygonUtility.AreaPolygon(polyList[i]) < 0.2) lineLength = false;
                if (CheckPoly(polyList[i]) && lineLength) cleanPolyList.Add(polyList[i]);
            }
            return cleanPolyList;
        }

        // Given three colinear points p, q, r, the function checks if
        // point q lies on line segment 'pr'
        internal static bool CheckOnSegment(Line2d givenLine, Point2d q, double eps = 0)
        {
            if (givenLine == null || q == null) return false;
            Point2d p = givenLine.StartPoint, r = givenLine.EndPoint;
            return CheckOnSegment(p, q, r);
        }
        //checks the ratio of the dimension of a poly bbox to be of certain proportion or not
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

        //checks a polygon2d to have min Area and min dimensions
        internal static bool CheckPolyDimension(Polygon2d poly, double minArea = 6, double side = 2)
        {
            if (!CheckPoly(poly)) return false;
            if (PolygonUtility.AreaPolygon(poly) < minArea) return false;
            List<double> spans = PolygonUtility.GetSpansXYFromPolygon2d(poly.Points);
            if (spans[0] < side) return false;
            if (spans[1] < side) return false;
            return true;
        }

        //check if a polygon is null then return false
        internal static bool CheckPoly(Polygon2d poly)
        {
            if (poly == null || poly.Points == null
                || poly.Points.Count == 0 || poly.Lines == null || poly.Lines.Count < 3) return false;
            else return true;
        }

        //checks a polygonlist if any poly is null then return false
        internal static bool CheckPolyList(List<Polygon2d> polyList)
        {
            if (polyList == null || polyList.Count == 0) return false;
            bool check = true;
            for (int i = 0; i < polyList.Count; i++) if (!CheckPoly(polyList[i])) check = false;
            return check;
        }

        //check if a pointlist is null then return false
        internal static bool CheckPointList(List<Point2d> ptList)
        {
            if (ptList == null || ptList.Count == 0) return false;
            else return true;
        }

        //iterate multiple times till theres is no notches in the poly
        [MultiReturn(new[] { "PolyReduced", "HasNotches", "Trials" })]
        internal static Dictionary<string, object> CheckPolyNotches(Polygon2d poly, double distance = 10)
        {
            if (!CheckPoly(poly)) return null;
            bool hasNotches = true;
            int count = 0, maxTry = 2 * poly.Points.Count;
            Polygon2d currentPoly = new Polygon2d(poly.Points);
            while (hasNotches && count < maxTry)
            {
                Dictionary<string, object> notchObject = PolygonUtility.RemoveMultipleNotches(currentPoly, distance);
                if (notchObject == null) continue;
                currentPoly = (Polygon2d)notchObject["PolyReduced"];
                for (int i = 0; i < poly.Lines.Count; i++)
                {
                    int a = i, b = i + 1;
                    if (i == poly.Points.Count - 1) b = 0;
                    if (poly.Lines[a].Length < distance && poly.Lines[b].Length < distance) { hasNotches = true; break; }
                    else hasNotches = false;
                }
                count += 1;
            }

            Dictionary<string, object> singleNotchObj = PolygonUtility.RemoveSingleNotch(currentPoly, distance, currentPoly.Points.Count);
            Polygon2d polyRed = (Polygon2d)singleNotchObj["PolyReduced"];
            if (!CheckPoly(polyRed)) { polyRed = poly; hasNotches = false; }
            return new Dictionary<string, object>
            {
                { "PolyReduced", (polyRed) },
                { "HasNotches", (hasNotches) },
                { "Trials", (count) }
            };
        }

    

        //find lines which will not be inside the poly when offset by a distance
        [MultiReturn(new[] { "LinesFalse", "Offsetables", "IndicesFalse", "PointsOutside" })]
        internal static Dictionary<string, object> CheckLinesOffsetInPoly(Polygon2d poly, Polygon2d containerPoly, double distance = 10, bool tag = false)
        {
            if (!CheckPoly(poly)) return null;
            Polygon2d oPoly = PolygonUtility.OffsetPoly(poly, 0.2);
            List<bool> offsetAble = new List<bool>();
            List<List<Point2d>> pointsOutsideList = new List<List<Point2d>>();
            List<Line2d> linesNotOffset = new List<Line2d>();
            List<int> indicesFalse = new List<int>();
            for (int i = 0; i < poly.Points.Count; i++)
            {
                bool offsetAllow = false;
                int a = i, b = i + 1;
                if (i == poly.Points.Count - 1) b = 0;
                Line2d line = poly.Lines[i];
                Point2d offStartPt = LineUtility.OffsetLinePointInsidePoly(line, line.StartPoint, oPoly, distance);
                Point2d offEndPt = LineUtility.OffsetLinePointInsidePoly(line, line.EndPoint, oPoly, distance);
                bool checkStartPt = GraphicsUtility.PointInsidePolygonTest(oPoly, offStartPt);
                bool checkEndPt = GraphicsUtility.PointInsidePolygonTest(oPoly, offEndPt);
                bool checkExtEdge = LayoutUtility.CheckLineGetsExternalWall(line, containerPoly);
                //if (tag) checkExtEdge = true;
                List<Point2d> pointsDefault = new List<Point2d>();
                if (checkStartPt && checkEndPt && checkExtEdge)
                {
                    offsetAllow = true;
                    indicesFalse.Add(-1);
                    pointsDefault.Add(null);
                }
                else
                {
                    if (!checkStartPt) pointsDefault.Add(line.StartPoint);
                    if (!checkEndPt) pointsDefault.Add(line.EndPoint);
                    linesNotOffset.Add(line);
                    indicesFalse.Add(i);
                    offsetAllow = false;
                }
                pointsOutsideList.Add(pointsDefault);
                offsetAble.Add(offsetAllow);
            }
            return new Dictionary<string, object>
            {
                { "LinesFalse", (linesNotOffset) },
                { "Offsetables", (offsetAble) },
                { "IndicesFalse", (indicesFalse) },
                { "PointsOutside", (pointsOutsideList) }
            };
        }

        //checks a line if horizontal or vertical 0 for horizontal, 1 for vertical
        internal static int CheckLineOrient(Line2d line)
        {
            if (line == null) return -1;
            double x = Math.Round((line.StartPoint.X - line.EndPoint.X), 2);
            double y = Math.Round((line.StartPoint.Y - line.EndPoint.Y), 2);
            if (x == 0) return 1;
            else if (y == 0) return 0;
            else return -1; // was 0 prev
        }

        //check order of points 0 = collinear, 1 = a,b,c clockwise, 2 = a,b,c are anti clockwise
        internal static int CheckPointOrder(Point2d a, Point2d b, Point2d c)
        {
            double area = (b.Y - a.Y) * (c.X - b.X) - (b.X - a.X) * (c.Y - b.Y);
            if (area > 0) return 1;
            else if (area < 0) return 2;
            return 0;
        }

        //check to see if a test point is towards the left or right of the point
        //if positive then the point is towards the left of the point
        internal static bool CheckPointSide(Line2d lineSegment, Point2d c)
        {
            Point2d a = lineSegment.StartPoint;
            Point2d b = lineSegment.EndPoint;
            return ((b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X)) > 0;
        }

        //checks if two points are within a certain threshold region
        internal static bool CheckPointsWithinRange(Point2d ptA, Point2d ptB, double threshold = 2)
        {
            List<Point2d> squarePts = new List<Point2d>();
            squarePts.Add(Point2d.ByCoordinates(ptA.X + threshold, ptA.Y - threshold));//LR
            squarePts.Add(Point2d.ByCoordinates(ptA.X + threshold, ptA.Y + threshold));//UR
            squarePts.Add(Point2d.ByCoordinates(ptA.X - threshold, ptA.Y + threshold));//UL
            squarePts.Add(Point2d.ByCoordinates(ptA.X - threshold, ptA.Y - threshold));//LLl
            Polygon2d squarePoly = Polygon2d.ByPoints(squarePts);
            return GraphicsUtility.PointInsidePolygonTest(squarePoly, ptB);
        }


        //finds if line is horizontal or vertical
        public static bool CheckLineOrthogonal(Line2d line, double eps = 0)
        {
            bool check = false;
            Point2d p1 = line.StartPoint;
            Point2d p2 = line.EndPoint;
            double xDiff = p1.X - p2.X;
            double yDiff = p1.Y - p2.Y;
            if (eps == 0)
            {
                if (xDiff == 0 || yDiff == 0) check = true;
            }
            else
            {
                if (BasicUtility.CheckWithinRange(0, xDiff, eps) == -1 || BasicUtility.CheckWithinRange(0, yDiff, eps) == -1) check = true;
                else check = false;
            }         
            return check;
        }

        #endregion


    }
}
