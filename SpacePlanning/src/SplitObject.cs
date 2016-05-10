using System;
using System.Collections.Generic;
using stuffer;
using Autodesk.DesignScript.Runtime;
using System.Diagnostics;
using Autodesk.DesignScript.Geometry;
using System.Linq;

namespace SpacePlanning
{
    public class SplitObject
    {


        //splits a polygon into two based on ratio and dir
        [MultiReturn(new[] { "PolyAfterSplit", "SplitLine", "IntersectedPoints" })]
        public static Dictionary<string, object> SplitByRatio(Polygon2d polyOutline, double ratio = 0.5, int dir = 0)
        {
            if (polyOutline == null) return null;
            if (polyOutline != null && polyOutline.Points == null) return null;

            double extents = 5000;
            double minimumLength = 2, minWidth = 10, aspectRatio = 0, eps = 0.1;
            List<Point2d> polyOrig = polyOutline.Points;
            List<Point2d> poly = PolygonUtility.SmoothPolygon(polyOrig, BuildLayout.spacingSet);
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

            Dictionary<string, object> intersectionReturn = BuildLayout.MakeIntersections(poly, splitLine, BuildLayout.spacingSet);
            List<Point2d> intersectedPoints = (List<Point2d>)intersectionReturn["IntersectedPoints"];
            List<Polygon2d> splittedPoly = (List<Polygon2d>)intersectionReturn["PolyAfterSplit"];

            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "SplitLine", (splitLine) },
                { "IntersectedPoints", (intersectedPoints) }
            };
        }

        //uses makepolysofproportion function to split one big poly into sub components
        public static Dictionary<string, object> SplitBigPolys(List<Polygon2d> polyInputList, double acceptableWidth, double factor = 4)
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
                BuildLayout.MakePolysOfProportion(poly, polyOrganizedList, polyCoverList, acceptableWidth, targetArea);
            }
            BuildLayout.recurse = 0;
            return new Dictionary<string, object>
            {
                { "PolySpaces", (polyOrganizedList) },
                { "PolyForCirculation", (polyCoverList) }
            };
        }
        
        //splits a polygon based on distance and random direction
        [MultiReturn(new[] { "PolyAfterSplit", "SplitLine", "IntersectedPoints", "PointASide", "PointBSide" })]
        internal static Dictionary<string, object> SplitByDistance(Polygon2d polyOutline, Random ran, double distance = 10, int dir = 0, double spacing = 0)
        {
            if (!PolygonUtility.CheckPoly(polyOutline)) return null;
            double extents = 5000, spacingProvided;
            List<Point2d> polyOrig = polyOutline.Points;
            if (spacing == 0) spacingProvided = BuildLayout.spacingSet;
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

            Dictionary<string, object> intersectionReturn = BuildLayout.MakeIntersections(poly, splitLine, spacingProvided);
            List<Point2d> intersectedPoints = (List<Point2d>)intersectionReturn["IntersectedPoints"];
            List<Polygon2d> splittedPoly = (List<Polygon2d>)intersectionReturn["PolyAfterSplit"];
            List<Point2d> ptA = (List<Point2d>)intersectionReturn["PointASide"];
            List<Point2d> ptB = (List<Point2d>)intersectionReturn["PointBSide"];
            Polygon2d polyA = new Polygon2d(ptA, 0), polyB = new Polygon2d(ptB, 0);
            //List<Polygon2d> splittedPoly = new List<Polygon2d>();
            //splittedPoly.Add(polyA); splittedPoly.Add(polyB);
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "SplitLine", (splitLine) },
                { "IntersectedPoints", (intersectedPoints) },
                { "PointASide", (ptA) },
                { "PointBSide", (ptB) }
            };

        }

        //splits a polygon into two based on direction and distance from the lowest pt in the poly
        [MultiReturn(new[] { "PolyAfterSplit", "SplitLine", "IntersectedPoints", "PointASide", "PointBSide" })]
        internal static Dictionary<string, object> SplitByDistanceFromPoint(Polygon2d polyOutline, double distance = 10, int dir = 0)
        {
            if (polyOutline == null || polyOutline.Points == null || polyOutline.Points.Count == 0) return null;
            double extents = 5000;
            int threshValue = 20;
            List<Point2d> polyOrig = polyOutline.Points;
            List<Point2d> poly = new List<Point2d>();
            if (polyOrig.Count > threshValue) { poly = polyOrig; }
            else { poly = PolygonUtility.SmoothPolygon(polyOrig, BuildLayout.spacingSet2); }

            if (poly == null || poly.Count == 0) return null;
            int lowInd = GraphicsUtility.ReturnLowestPointFromListNew(poly);// THIS IS BETTER THAN THE OTHER VER
            Point2d lowPt = poly[lowInd];
            Line2d splitLine = new Line2d(lowPt, extents, dir);
            if (dir == 0) splitLine = LineUtility.Move(splitLine, 0, 1 * distance);
            else splitLine = LineUtility.Move(splitLine, 1 * distance, 0);



            Dictionary<string, object> intersectionReturn = BuildLayout.MakeIntersections(poly, splitLine, BuildLayout.spacingSet2);
            List<Point2d> intersectedPoints = (List<Point2d>)intersectionReturn["IntersectedPoints"];
            List<Polygon2d> splittedPoly = (List<Polygon2d>)intersectionReturn["PolyAfterSplit"];
            List<Point2d> ptA = (List<Point2d>)intersectionReturn["PointASide"];
            List<Point2d> ptB = (List<Point2d>)intersectionReturn["PointBSide"];
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "SplitLine", (splitLine) },
                { "IntersectedPoints", (intersectedPoints) },
                { "PointASide", (ptA) },
                { "PointBSide", (ptB) }
            };

        }

        //splits a polygon based on offset direction from a given line id
        [MultiReturn(new[] { "PolyAfterSplit", "LeftOverPoly" })]
        public static Dictionary<string, object> SplitByOffsetFromLine(Polygon2d polyOutline, int lineId, double distance = 10, double minDist = 0)
        {
            if (!PolygonUtility.CheckPoly(polyOutline)) return null;
            Polygon2d poly = new Polygon2d(polyOutline.Points, 0);

            List<Point2d> pointForBlock = new List<Point2d>();
            List<Point2d> polyPtsCopy = poly.Points.Select(pt => new Point2d(pt.X, pt.Y)).ToList();//deep copy
            for (int i = 0; i < poly.Points.Count; i++)
            {
                int a = i, b = i + 1, c = i - 1;
                if (c < 0) c = poly.Points.Count - 1;
                if (i == poly.Points.Count - 1) b = 0;
                Line2d prevLine = poly.Lines[c];
                Line2d currLine = poly.Lines[a];
                Line2d nextLine = poly.Lines[b];
                if (i == lineId)
                {
                    Line2d line = new Line2d(poly.Points[a], poly.Points[b]);
                    if (line.Length < minDist) continue;
                    Line2d offsetLine = LineUtility.OffsetLineInsidePoly(line, poly, distance);
                    pointForBlock.Add(poly.Points[a]);
                    pointForBlock.Add(poly.Points[b]);
                    pointForBlock.Add(offsetLine.EndPoint);
                    pointForBlock.Add(offsetLine.StartPoint);
                    int orientPrev = GraphicsUtility.CheckLineOrient(prevLine);
                    int orientCurr = GraphicsUtility.CheckLineOrient(currLine);
                    int orientNext = GraphicsUtility.CheckLineOrient(nextLine);

                    // case 1
                    if (orientPrev == orientCurr && orientCurr == orientNext)
                    {
                        polyPtsCopy.Insert(b, offsetLine.EndPoint);
                        polyPtsCopy.Insert(b, offsetLine.StartPoint);
                        //Trace.WriteLine("Case 1 : inserted two pts");
                    }

                    // case 2
                    if (orientPrev != orientCurr && orientCurr == orientNext)
                    {
                        polyPtsCopy[a] = offsetLine.StartPoint;
                        polyPtsCopy.Insert(b, offsetLine.EndPoint);
                        //Trace.WriteLine("Case 2 : inserted 1 pt, replaced 1 pt----------------");
                    }

                    // case 3
                    if (orientPrev == orientCurr && orientCurr != orientNext)
                    {
                        polyPtsCopy.Insert(b, offsetLine.StartPoint);
                        polyPtsCopy[b + 1] = offsetLine.EndPoint;
                        //Trace.WriteLine("Case 3 : inserted 1 pt, replaced 1 pt");
                    }

                    // case 4
                    if (orientPrev != orientCurr && orientCurr != orientNext)
                    {
                        polyPtsCopy[a] = offsetLine.StartPoint;
                        polyPtsCopy[b] = offsetLine.EndPoint;
                        //Trace.WriteLine("Case 4 : replaced 2 pts");
                    }
                }
            }
            Polygon2d polySplit = new Polygon2d(pointForBlock, 0);
            Polygon2d leftPoly = new Polygon2d(polyPtsCopy, 0); //poly.Points
            //Trace.WriteLine("Ret ++++++++++++++++++++++++++++++++++++++++++++");
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polySplit) },
                { "LeftOverPoly", (leftPoly) },
            };

        }

        //splits a polygon by a line 
        [MultiReturn(new[] { "PolyAfterSplit", "SplitLine" })]
        public static Dictionary<string, object> SplitByLine(Polygon2d polyOutline, Line2d inputLine, double distance = 5)
        {

            if (!PolygonUtility.CheckPoly(polyOutline)) return null;
            List<Point2d> polyOrig = polyOutline.Points;
            List<Point2d> poly = PolygonUtility.SmoothPolygon(polyOrig, BuildLayout.spacingSet);
            Line2d splitLine = new Line2d(inputLine);
            Point2d centerPoly = GraphicsUtility.CentroidInPointLists(poly);
            bool checkSide = GraphicsUtility.CheckPointSide(splitLine, centerPoly);
            int orient = GraphicsUtility.CheckLineOrient(splitLine);
            if (orient == 0)
            {
                if (!checkSide) splitLine = LineUtility.Move(splitLine, 0, -1 * distance);
                else splitLine = LineUtility.Move(splitLine, 0, 1 * distance);
            }
            else
            {
                if (checkSide) splitLine = LineUtility.Move(splitLine, -1 * distance, 0);
                else splitLine = LineUtility.Move(splitLine, 1 * distance, 0);
            }

            Dictionary<string, object> intersectionReturn = BuildLayout.MakeIntersections(poly, splitLine, BuildLayout.spacingSet);
            List<Polygon2d> splittedPoly = (List<Polygon2d>)intersectionReturn["PolyAfterSplit"];
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "SplitLine", (splitLine) }
            };
        }


    }




}

