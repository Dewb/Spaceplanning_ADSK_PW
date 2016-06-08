﻿using System;
using System.Collections.Generic;
using stuffer;
using Autodesk.DesignScript.Runtime;
using System.Diagnostics;
using Autodesk.DesignScript.Geometry;
using System.Linq;

namespace SpacePlanning
{
    public static class SplitObject
    {

        #region - Public Methods
        //subdivide a given poly into smaller parts till acceptable width is met, returns list of polydept grids and list of polys to compute circulation
        public static List<List<Polygon2d>> SplitRecursivelyToSubdividePoly(List<Polygon2d> polyList, double acceptableWidth = 10, double circulationFreq = 10, double ratio = 0.5, bool tag = false)
        {
            if (!ValidateObject.CheckPolyList(polyList)) return null;

            int count = 0;
            Queue<Polygon2d> polyQueue = new Queue<Polygon2d>();
            List<List<Polygon2d>> polyAllReturn = new List<List<Polygon2d>>();
            List<Polygon2d> polyBrokenList = new List<Polygon2d>(), polyCirculationList = new List<Polygon2d>();
            double totalArea = 0; // cirFac = Math.Ceiling(acceptableWidth/ circulationFreq);
            double cirFac = circulationFreq;
            for (int i = 0; i < polyList.Count; i++)
            {
                totalArea += PolygonUtility.AreaPolygon(polyList[i]);
                polyQueue.Enqueue(polyList[i]);
            }
            Random rand = new Random();
            double targetArea = totalArea / cirFac;
            while (polyQueue.Count > 0)
            {
                Polygon2d currentPoly = polyQueue.Dequeue();
                Dictionary<string, object> splitObj = SplitByRatio(currentPoly, ratio, 0);
                if (tag) ratio = BasicUtility.RandomBetweenNumbers(rand, 0.7, 0.35);
                if (splitObj == null) continue;
                List<Polygon2d> polySplitList = (List<Polygon2d>)splitObj["PolyAfterSplit"];
                if (ValidateObject.CheckPolyList(polySplitList) && polySplitList.Count > 1)
                {
                    polySplitList = PolygonUtility.SmoothPolygonList(polySplitList, 2);
                    Polygon2d bbox1 = Polygon2d.ByPoints(ReadData.FromPointsGetBoundingPoly(polySplitList[0].Points));
                    Polygon2d bbox2 = Polygon2d.ByPoints(ReadData.FromPointsGetBoundingPoly(polySplitList[1].Points));
                    if (!ValidateObject.CheckPoly(bbox1) || !ValidateObject.CheckPoly(bbox2)) continue;

                    if (PolygonUtility.AreaPolygon(polySplitList[0]) > targetArea) polyCirculationList.Add(polySplitList[0]);
                    if (PolygonUtility.AreaPolygon(polySplitList[1]) > targetArea) polyCirculationList.Add(polySplitList[1]);

                    if (bbox1.Lines[0].Length < acceptableWidth || bbox1.Lines[1].Length < acceptableWidth) polyBrokenList.Add(polySplitList[0]);
                    else polyQueue.Enqueue(polySplitList[0]);
                    if (bbox2.Lines[0].Length < acceptableWidth || bbox2.Lines[1].Length < acceptableWidth) polyBrokenList.Add(polySplitList[1]);
                    else polyQueue.Enqueue(polySplitList[1]);
                }
                if (ValidateObject.CheckPolyList(polySplitList) && polySplitList.Count < 2)
                {
                    Polygon2d bbox1 = Polygon2d.ByPoints(ReadData.FromPointsGetBoundingPoly(polySplitList[0].Points));
                    if (!ValidateObject.CheckPoly(bbox1)) continue;
                    if (bbox1.Lines[0].Length < acceptableWidth || bbox1.Lines[1].Length < acceptableWidth) polyBrokenList.Add(polySplitList[0]);
                    if (PolygonUtility.AreaPolygon(polySplitList[0]) > targetArea) polyCirculationList.Add(polySplitList[0]);
                    else polyQueue.Enqueue(polySplitList[0]);
                }
                count += 1;
            }

            for (int i = 0; i < polyQueue.Count; i++) polyBrokenList.Add(polyQueue.Dequeue());
            polyAllReturn.Add(polyBrokenList);
            polyAllReturn.Add(polyCirculationList);
            return polyAllReturn;
        }

        //subdivide a given poly into smaller parts till acceptable width is met, returns list of polydept grids and list of polys to compute circulation
        public static List<List<Polygon2d>> SplitRecursivelyRandomizedToSubdividePoly(List<Polygon2d> polyList, double acceptableWidth = 10, double circulationFreq = 10, double ratio = 0.5, double eps = 0, double iterationCount = 100)
        {
            if (!ValidateObject.CheckPolyList(polyList)) return null;
            bool subDivResult = false;
            int count = 0;
            List<List<Polygon2d>> polySubdivs = new List<List<Polygon2d>>();
            double acceptableWidthMax = acceptableWidth * 1.5, acceptableWidthMin = acceptableWidth * 0.75;
            double ratioMax = ratio * 1.8, ratioMin = ratio * 0.60;
            Random ran1 = new Random();
            Random ran2 = new Random();
            while (!subDivResult && count < iterationCount)
            {
                count += 1;
                polySubdivs = SplitRecursivelyToSubdividePoly(polyList, acceptableWidth, circulationFreq, ratio);
                acceptableWidth = BasicUtility.RandomBetweenNumbers(ran1, acceptableWidthMax, acceptableWidthMin);
                ratio = BasicUtility.RandomBetweenNumbers(ran2, ratioMax, ratioMin);
                if (ratio > 0.7 || ratio < 0.30) ratio = BasicUtility.RandomBetweenNumbers(ran2, 0.75, 0.25);
                if (polySubdivs == null) { subDivResult = false; }
                else if (!ValidateObject.CheckPolyList(polySubdivs[0]) || !ValidateObject.CheckPolygon2dListOrtho(polySubdivs[0])) { subDivResult = false; }
                else if (!ValidateObject.CheckPolyList(polySubdivs[1]) || !ValidateObject.CheckPolygon2dListOrtho(polySubdivs[1])) { subDivResult = false; }
                else subDivResult = true;
            }     
            return polySubdivs;
        }

        //splits a polygon into two based on ratio and dir
        [MultiReturn(new[] { "PolyAfterSplit", "SplitLine", "IntersectedPoints" })]
        public static Dictionary<string, object> SplitByRatio(Polygon2d polyOutline, double ratio = 0.5, int dir = 0)
        {
            if (polyOutline == null) return null;
            if (polyOutline != null && polyOutline.Points == null) return null;

            double extents = 5000;
            double minimumLength = 2, minWidth = 10, aspectRatio = 0, eps = 0.1;
            List<Point2d> polyOrig = polyOutline.Points;
            List<Point2d> poly = PolygonUtility.SmoothPolygon(polyOrig, BuildLayout.SPACING);
            List<double> spans = PolygonUtility.GetSpansXYFromPolygon2d(poly);
            double horizontalSpan = spans[0], verticalSpan = spans[1];
            Point2d polyCenter = PolygonUtility.CentroidOfPoly(Polygon2d.ByPoints(poly));
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

            Dictionary<string, object> intersectionReturn = MakeIntersections(poly, splitLine, BuildLayout.SPACING);
            List<Point2d> intersectedPoints = (List<Point2d>)intersectionReturn["IntersectedPoints"];
            List<Polygon2d> splittedPoly = (List<Polygon2d>)intersectionReturn["PolyAfterSplit"];

            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "SplitLine", (splitLine) },
                { "IntersectedPoints", (intersectedPoints) }
            };
        }
    
        //splits a polygon based on distance and random direction
        [MultiReturn(new[] { "PolyAfterSplit", "SplitLine", "IntersectedPoints", "PointASide", "PointBSide" })]
        public static Dictionary<string, object> SplitByDistance(Polygon2d polyOutline, Random ran, double distance = 10, int dir = 0, double spacing = 0)
        {
            if (!ValidateObject.CheckPoly(polyOutline)) return null;
            double extents = 5000, spacingProvided;
            List<Point2d> polyOrig = polyOutline.Points;
            if (spacing == 0) spacingProvided = BuildLayout.SPACING;
            else spacingProvided = spacing;

            List<Point2d> poly = PolygonUtility.SmoothPolygon(polyOrig, spacingProvided);
            if (!ValidateObject.CheckPointList(poly)) return null;
            Dictionary<int, object> obj = PointUtility.PointSelector(ran, poly);
            Point2d pt = (Point2d)obj[0];
            int orient = (int)obj[1];
            Line2d splitLine = new Line2d(pt, extents, dir);

            // push this line right or left or up or down based on ratio
            if (dir == 0) splitLine = LineUtility.Move(splitLine, 0, orient * distance);
            else splitLine = LineUtility.Move(splitLine, orient * distance, 0);

            Dictionary<string, object> intersectionReturn = MakeIntersections(poly, splitLine, spacingProvided);
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

        //splits a polygon into two based on direction and distance from the lowest point in the poly
        [MultiReturn(new[] { "PolyAfterSplit", "SplitLine", "IntersectedPoints", "PointASide", "PointBSide" })]
        public static Dictionary<string, object> SplitByDistanceFromPoint(Polygon2d polyOutline, double distance = 10, int dir = 0, double space =0)
        {
            if (polyOutline == null || polyOutline.Points == null || polyOutline.Points.Count == 0) return null;
            if (space == 0) space = BuildLayout.SPACING2;
            double extents = 5000;
            int threshValue = 50;
            List<Point2d> polyOrig = polyOutline.Points;
            List<Point2d> poly = new List<Point2d>();
            if (polyOrig.Count > threshValue) { poly = polyOrig; }
            else { poly = PolygonUtility.SmoothPolygon(polyOrig, space); }

            if (poly == null || poly.Count == 0) return null;
            int lowInd = PointUtility.LowestPointFromList(poly);
            Point2d lowPt = poly[lowInd];
            Line2d splitLine = new Line2d(lowPt, extents, dir);
            if (dir == 0) splitLine = LineUtility.Move(splitLine, 0, 1 * distance);
            else splitLine = LineUtility.Move(splitLine, 1 * distance, 0);

            Dictionary<string, object> intersectionReturn = MakeIntersections(poly, splitLine, BuildLayout.SPACING2);
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
            if (!ValidateObject.CheckPoly(polyOutline)) return null;
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
                    int orientPrev = ValidateObject.CheckLineOrient(prevLine);
                    int orientCurr = ValidateObject.CheckLineOrient(currLine);
                    int orientNext = ValidateObject.CheckLineOrient(nextLine);

                    // case 1
                    if (orientPrev == orientCurr && orientCurr == orientNext)
                    {
                        polyPtsCopy.Insert(b, offsetLine.EndPoint);
                        polyPtsCopy.Insert(b, offsetLine.StartPoint);
                    }

                    // case 2
                    if (orientPrev != orientCurr && orientCurr == orientNext)
                    {
                        polyPtsCopy[a] = offsetLine.StartPoint;
                        polyPtsCopy.Insert(b, offsetLine.EndPoint);
                    }

                    // case 3
                    if (orientPrev == orientCurr && orientCurr != orientNext)
                    {
                        polyPtsCopy.Insert(b, offsetLine.StartPoint);
                        polyPtsCopy[b + 1] = offsetLine.EndPoint;
                    }

                    // case 4
                    if (orientPrev != orientCurr && orientCurr != orientNext)
                    {
                        polyPtsCopy[a] = offsetLine.StartPoint;
                        polyPtsCopy[b] = offsetLine.EndPoint;
                    }
                }
            }
            Polygon2d polySplit = new Polygon2d(pointForBlock, 0);
            Polygon2d leftPoly = new Polygon2d(polyPtsCopy, 0); //poly.Points
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

            if (!ValidateObject.CheckPoly(polyOutline)) return null;
            List<Point2d> polyOrig = polyOutline.Points;
            List<Point2d> poly = PolygonUtility.SmoothPolygon(polyOrig, BuildLayout.SPACING);
            Line2d splitLine = new Line2d(inputLine);
            Point2d centerPoly = PointUtility.CentroidInPointLists(poly);
            bool checkSide = ValidateObject.CheckPointSide(splitLine, centerPoly);
            int orient = ValidateObject.CheckLineOrient(splitLine);
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

            Dictionary<string, object> intersectionReturn = MakeIntersections(poly, splitLine, BuildLayout.SPACING);
            List<Polygon2d> splittedPoly = (List<Polygon2d>)intersectionReturn["PolyAfterSplit"];
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "SplitLine", (splitLine) }
            };
        }
        #endregion


        #region - Private Methods
        //gets a poly and recursively splits it till acceptabledimension is met and makes a polyorganized list
        internal static void MakePolysOfProportion(Polygon2d poly, List<Polygon2d> polyOrganizedList,
            List<Polygon2d> polycoverList, double acceptableWidth, double targetArea)
        {
            BuildLayout.RECURSE += 1;
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
            div = BasicUtility.RandomBetweenNumbers(BuildLayout.RANGENERATE, maxValue, minValue);
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
                    splitResult = SplitObject.SplitByDistance(poly, ran, dis, dir, BuildLayout.SPACING2);
                    //splitResult = SplitByDistanceFromPoint(poly, dis, dir);
                }
                else
                {
                    double dis = spanX * div;
                    int dir = 0;
                    //splitResult = BasicSplitPolyIntoTwo(poly, 0.5, dir);
                    splitResult = SplitObject.SplitByDistance(poly, ran, dis, dir, BuildLayout.SPACING2);
                    //splitResult = SplitByDistanceFromPoint(poly, dis, dir);
                }

                List<Polygon2d> polyAfterSplit = (List<Polygon2d>)splitResult["PolyAfterSplit"];
                if (ValidateObject.CheckPolyList(polyAfterSplit))
                {
                    double areaPolA = PolygonUtility.AreaPolygon(polyAfterSplit[0]);
                    double areaPolB = PolygonUtility.AreaPolygon(polyAfterSplit[1]);
                    if (areaPolA > targetArea) polycoverList.Add(polyAfterSplit[0]);
                    if (areaPolB > targetArea) polycoverList.Add(polyAfterSplit[1]);

                    List<double> spanA = PolygonUtility.GetSpansXYFromPolygon2d(polyAfterSplit[0].Points);
                    List<double> spanB = PolygonUtility.GetSpansXYFromPolygon2d(polyAfterSplit[1].Points);
                    //Trace.WriteLine("RECURSE is : " + RECURSE);
                    if (BuildLayout.RECURSE < 1500)
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
                                double areaPoly = PolygonUtility.AreaPolygon(polyAfterSplit[0]);
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

        //makes intersections and returns the two polygon2ds after intersection
        internal static Dictionary<string, object> MakeIntersections(List<Point2d> poly, Line2d splitLine, double space = 0)
        {
            List<Point2d> intersectedPoints = GraphicsUtility.LinePolygonIntersection(poly, splitLine);
            //List<Point2d> intersectedPoints = TestGraphicsUtility.LinePolygonIntersectionIndex(poly, splitLine);
            // find all points on poly which are to the left or to the right of the line
            List<int> pIndexA = new List<int>();
            List<int> pIndexB = new List<int>();
            for (int i = 0; i < poly.Count; i++)
            {
                bool check = ValidateObject.CheckPointSide(splitLine, poly[i]);
                if (check) pIndexA.Add(i);
                else pIndexB.Add(i);
            }
            //organize the points to make closed poly
            List<Point2d> sortedA = PointUtility.DoSortClockwise(poly, intersectedPoints, pIndexA);
            List<Point2d> sortedB = PointUtility.DoSortClockwise(poly, intersectedPoints, pIndexB);
            List<Polygon2d> splittedPoly = new List<Polygon2d>();
            if (space == 0)
            {
              splittedPoly = new List<Polygon2d> { new Polygon2d(sortedA, 0), new Polygon2d(sortedB, 0) };
            }
            else
            {
                sortedA = PolygonUtility.SmoothPolygon(new Polygon2d(sortedA,0).Points, space);
                sortedB = PolygonUtility.SmoothPolygon(new Polygon2d(sortedB,0).Points, space);
                splittedPoly = new List<Polygon2d> { new Polygon2d(sortedA, 0), new Polygon2d(sortedB, 0) };
            }

            //List<Polygon2d> splittedPoly = new List<Polygon2d> { new Polygon2d(sortedA, 0), new Polygon2d(sortedB, 0) };
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "IntersectedPoints", (intersectedPoints) },
                { "PointASide", (sortedA) },
                { "PointBSide", (sortedB) }
            };

        }

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
                double targetArea = PolygonUtility.AreaPolygon(poly) / factor;
                MakePolysOfProportion(poly, polyOrganizedList, polyCoverList, acceptableWidth, targetArea);
            }
            BuildLayout.RECURSE = 0;
            return new Dictionary<string, object>
            {
                { "PolySpaces", (polyOrganizedList) },
                { "PolyForCirculation", (polyCoverList) }
            };
        }
        #endregion


    }




}

