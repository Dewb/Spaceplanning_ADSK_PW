using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using stuffer;
using Autodesk.DesignScript.Geometry;
using Autodesk.DesignScript.Runtime;
using System.Diagnostics;

namespace SpacePlanning
{
    class CodeToTest
    {

        //****DEF - USING THIS TO SPLIT PROGRAMS--------------------------------
        //RECURSIVE SPLITS A POLY
        [MultiReturn(new[] { "PolyAfterSplit", "AreaEachPoly", "EachPolyPoint", "UpdatedProgramData" })]
        public static Dictionary<string, object> RecursiveSplitPolyPrograms(List<Polygon2d> polyInputList, List<ProgramData> progData, double ratioA = 0.5, int recompute = 1)
        {
            double ratio = 0.5;
            List<Polygon2d> polyList = new List<Polygon2d>();
            List<Point2d> pointsList = new List<Point2d>();
            List<double> areaList = new List<double>();
            Stack<Polygon2d> polyRetrieved = new Stack<Polygon2d>();
            Stack<ProgramData> programDataRetrieved = new Stack<ProgramData>();

            for (int j = 0; j < progData.Count; j++) { programDataRetrieved.Push(progData[j]); }

            for (int i = 0; i < polyInputList.Count; i++)
            {
                Polygon2d currentPoly, poly = polyInputList[i];
                if (poly == null || poly.Points == null || poly.Points.Count == 0) return null;

                polyRetrieved.Push(poly);
                int count = 0, dir = 1;
                double areaThreshold = 1000, maximum = 0.9, minimum = 0.3;

                List<Polygon2d> polyAfterSplit = null;
                Dictionary<string, object> splitReturn = null;
                Random rand = new Random();
                while (polyRetrieved.Count > 0 && programDataRetrieved.Count > 0)
                {
                    ProgramData progItem = programDataRetrieved.Pop();
                    ratio = rand.NextDouble() * (maximum - minimum) + minimum;
                    currentPoly = polyRetrieved.Pop();
                    try
                    {
                        splitReturn = BuildLayout.BasicSplitPolyIntoTwo(currentPoly, ratio, dir);
                        polyAfterSplit = (List<Polygon2d>)splitReturn["PolyAfterSplit"];
                    }
                    catch (Exception)
                    {
                        dir = BasicUtility.toggleInputInt(dir);
                        splitReturn = BuildLayout.BasicSplitPolyIntoTwo(currentPoly, ratio, dir);
                        if (splitReturn == null) { Trace.WriteLine("Could Not Split"); continue; }
                        polyAfterSplit = (List<Polygon2d>)splitReturn["PolyAfterSplit"];
                        //throw;
                    }
                    List<List<Point2d>> pointsOnPoly = (List<List<Point2d>>)splitReturn["EachPolyPoint"];
                    double area1 = GraphicsUtility.AreaPolygon2d(polyAfterSplit[0].Points);
                    double area2 = GraphicsUtility.AreaPolygon2d(polyAfterSplit[1].Points);
                    if (area1 > areaThreshold)
                    {
                        polyRetrieved.Push(polyAfterSplit[0]);
                        polyList.Add(polyAfterSplit[0]);
                        progItem.AreaProvided = area1;
                        pointsList.AddRange(pointsOnPoly[0]);
                        areaList.Add(area1);
                    }
                    if (area2 > areaThreshold)
                    {
                        polyRetrieved.Push(polyAfterSplit[1]);
                        polyList.Add(polyAfterSplit[1]);
                        progItem.AreaProvided = area2;
                        pointsList.AddRange(pointsOnPoly[1]);
                        areaList.Add(area2);
                    }
                    dir = BasicUtility.toggleInputInt(dir);
                    count += 1;
                }// end of while loop
            }//end of for loop

            List<ProgramData> AllProgramDataList = new List<ProgramData>();
            for (int i = 0; i < progData.Count; i++)
            {
                ProgramData progItem = progData[i];
                ProgramData progNew = new ProgramData(progItem);
                if (i < polyList.Count) progNew.PolyProgAssigned = polyList[i];
                else progNew.PolyProgAssigned = null;
                AllProgramDataList.Add(progNew);
            }
            //polyList = Polygon2d.PolyReducePoints(polyList);
            //Trace.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");

            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyList) },
                { "AreaEachPoly", (areaList) },
                { "EachPolyPoint", (pointsList) },
                { "UpdatedProgramData", (AllProgramDataList) }
            };


        }


        //RECURSIVE SPLITS A POLY
        internal static Dictionary<string, object> RecursiveSplitPolyProgramsSingleOut(List<Polygon2d> polyInputList,
            List<ProgramData> progData, double ratioA = 0.5, int recompute = 1)
        {
            return RecursiveSplitPolyPrograms(polyInputList, progData, ratioA, recompute);
        }


        //changes the center of one or both polys to ensure correct intersection line is found
        [MultiReturn(new[] { "CenterPolyA", "CenterPolyB", "PolyA", "PolyB" })]
        internal static Dictionary<string, object> ComputePolyCentersAlign(Polygon2d polyA, Polygon2d polyB)
        {
            double extents = 10000;
            //compute orig centers
            Point2d centerPolyA = GraphicsUtility.CentroidInPointLists(polyA.Points);
            Point2d centerPolyB = GraphicsUtility.CentroidInPointLists(polyB.Points);

            Point2d staticPoint;
            Polygon2d staticPoly;
            Point2d movingPoint;
            Polygon2d movingPoly;

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

            if (IsMovingPoint1)
            {
                movingPoint = movingPoint1;

            }
            else if (IsMovingPoint2)
            {
                movingPoint = movingPoint2;
            }
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




        //changes the center of one or both polys to ensure correct intersection line is found
        [MultiReturn(new[] { "CenterPolyA", "CenterPolyB", "PolyA", "PolyA" })]
        internal static Dictionary<string, object> ComputePolyCenters(Polygon2d polyA, Polygon2d polyB)
        {
            double extents = 10000;
            //compute orig centers
            Point2d centerPolyA = GraphicsUtility.CentroidInPointLists(polyA.Points);
            Point2d centerPolyB = GraphicsUtility.CentroidInPointLists(polyB.Points);

            //make infinite lines via both centers 0 - horizontal line, 1 - vertical line
            Line2d lineAX = new Line2d(centerPolyA, extents, 0);
            Line2d lineAY = new Line2d(centerPolyA, extents, 1);

            Line2d lineBX = new Line2d(centerPolyB, extents, 0);
            Line2d lineBY = new Line2d(centerPolyB, extents, 1);


            //get line line intersection for these lines
            //AX-BY and BX-AY
            Point2d pAXBY = GraphicsUtility.LineLineIntersection(lineAX, lineBY);
            Point2d pBXAY = GraphicsUtility.LineLineIntersection(lineBX, lineAY);

            //check for point containment test for these two
            bool checkA_AXBY = GraphicsUtility.PointInsidePolygonTest(polyA.Points, pAXBY);
            bool checkA_BXAY = GraphicsUtility.PointInsidePolygonTest(polyA.Points, pBXAY);
            bool checkB_AXBY = GraphicsUtility.PointInsidePolygonTest(polyB.Points, pAXBY);
            bool checkB_BXAY = GraphicsUtility.PointInsidePolygonTest(polyB.Points, pBXAY);
            ////////////////////////////////////////////////////
            if (checkA_BXAY == true && checkB_AXBY == true)
            {
                return new Dictionary<string, object>
                {
                { "CenterPolyA", (centerPolyA) },
                { "CenterPolyB", (centerPolyB) },
                { "PolyA", (polyA) },
                { "PolyB", (polyB) }
                };
            }

            if (checkA_AXBY == true && checkB_BXAY == true)
            {
                return new Dictionary<string, object>
                {
                { "CenterPolyA", (centerPolyA) },
                { "CenterPolyB", (centerPolyB) },
                { "PolyA", (polyA) },
                { "PolyB", (polyB) }
                };
            }
            ////////////////////////////////////////////////////


            if (checkA_AXBY == true)
            {

                //centerPolyB.X = centerPolyA.X;

            }
            else if (checkA_BXAY == true)
            {
                //centerPolyB.Y = centerPolyA.Y;

            }
            else if (checkB_AXBY == true)
            {

            }
            else if (checkB_BXAY == true)
            {

            }
            else
            {
                return new Dictionary<string, object>
                {
                { "CenterPolyA", (centerPolyA) },
                { "CenterPolyB", (centerPolyB) },
                { "PolyA", (polyA) },
                { "PolyB", (polyB) }
                };
            }





            return new Dictionary<string, object>
                {
                { "CenterPolyA", (centerPolyA) },
                { "CenterPolyB", (centerPolyB) },
                { "PolyA", (polyA) },
                { "PolyB", (polyB) }
                };
        }





        [MultiReturn(new[] { "Neighbour", "SharedEdgeA", "SharedEdgeB", "LineMoved", "CenterToCenterLine", "CenterPolyPoint", "CenterPolyOtherPoint" })]
        internal static Dictionary<string, object> PolygonPolygonCommonEdgeDict(Polygon2d poly, Polygon2d other)
        {
            /*
            first reduce number of pts in both polys
            find their centers
            make a vec between their center
            get horizontal comp of vec
            get vertical comp of vec
            which length is long will be our vector

            then for both polys
                check line line intersection between line between two centers and each line of the poly
                    if no intersect, no edge
                    find the line intersects 
                    find the perpendicular projection of centers on these linese

            */

            bool check = false;
            if (poly == null || other == null)
            {
                return null;
            }

            double eps = 200;
            //Polygon2d polyReg = poly;
            //Polygon2d otherReg = other;
            //reduce number of points
            Polygon2d polyReg = new Polygon2d(poly.Points);
            Polygon2d otherReg = new Polygon2d(other.Points);
            //reassign centers to each poly
            //Dictionary<string,object> UpdatedCenters  = ComputePolyCentersAlign(polyReg, otherReg);
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
            if (centerToCenX.Length > centerToCenY.Length)
            {
                keyVec = new Vector2d(centerToCenX.X, centerToCenX.Y);
            }
            else
            {
                keyVec = new Vector2d(centerToCenY.X, centerToCenY.Y);
            }

            //check line poly intersection between centertocen vector and each polys            
            Line2d lineInPolyReg = GraphicsUtility.LinePolygonIntersectionReturnLine(polyReg.Points, centerLine, centerOther);
            Line2d lineInOtherReg = GraphicsUtility.LinePolygonIntersectionReturnLine(otherReg.Points, centerLine, centerPoly);


            //find distance d1 and d2 from two centers to linepolyintersection line
            Point2d projectedPtOnPolyReg = GraphicsUtility.ProjectedPointOnLine(lineInPolyReg, centerPoly);
            Point2d projectedPtOnOtherReg = GraphicsUtility.ProjectedPointOnLine(lineInOtherReg, centerOther);

            double dist1 = GraphicsUtility.DistanceBetweenPoints(centerPoly, projectedPtOnPolyReg);
            double dist2 = GraphicsUtility.DistanceBetweenPoints(centerOther, projectedPtOnOtherReg);

            double totalDistance = 2 * (dist1 + dist2);
            //Line2d lineMoved = new Line2d(lineInPolyReg);
            Line2d lineMoved = new Line2d(lineInPolyReg.StartPoint, lineInPolyReg.EndPoint);
            lineMoved.move(centerPoly);
            Point2d projectedPt = GraphicsUtility.ProjectedPointOnLine(lineMoved, centerOther);
            double distance = GraphicsUtility.DistanceBetweenPoints(projectedPt, centerOther);

            bool isNeighbour = false;
            if (totalDistance - eps < distance && distance < totalDistance + eps)
            {
                isNeighbour = true;
            }
            else
            {
                isNeighbour = false;
            }

            //"Neighbour", "SharedEdgeA", "SharedEdgeB" 

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



        //find common edge between two polygons, - works with bugs
        internal static Line2d PolygonPolygonCommonEdge(Polygon2d poly, Polygon2d other)
        {
            /*
            first reduce number of pts in both polys
            find their centers
            make a vec between their center
            get horizontal comp of vec
            get vertical comp of vec
            which length is long will be our vector

            then for both polys
                check line line intersection between line between two centers and each line of the poly
                    if no intersect, no edge
                    find the line intersects 
                    find the perpendicular projection of centers on these linese

            */

            bool check = false;
            if (poly == null || other == null)
            {
                return null;
            }
            double eps = 100;
            //reduce number of points
            Polygon2d polyReg = new Polygon2d(poly.Points);
            Polygon2d otherReg = new Polygon2d(other.Points);

            //find centers
            Point2d centerPoly = GraphicsUtility.CentroidInPointLists(polyReg.Points);
            Point2d centerOther = GraphicsUtility.CentroidInPointLists(otherReg.Points);

            //make vectors
            Vector2d centerToCen = new Vector2d(centerPoly, centerOther);
            Vector2d centerToCenX = new Vector2d(centerToCen.X, 0);
            Vector2d centerToCenY = new Vector2d(0, centerToCen.Y);

            //make centerLine
            Line2d centerLine = new Line2d(centerPoly, centerOther);
            Vector2d keyVec;
            if (centerToCenX.Length > centerToCenY.Length)
            {
                keyVec = new Vector2d(centerToCenX.X, centerToCenX.Y);
            }
            else
            {
                keyVec = new Vector2d(centerToCenY.X, centerToCenY.Y);
            }

            //check line poly intersection between centertocen vector and each polys            
            Line2d lineInPolyReg = GraphicsUtility.LinePolygonIntersectionReturnLine(polyReg.Points, centerLine, centerOther);
            Line2d lineInOtherReg = GraphicsUtility.LinePolygonIntersectionReturnLine(otherReg.Points, centerLine, centerPoly);
            
            Point2d projectedPtOnPolyReg = GraphicsUtility.ProjectedPointOnLine(lineInPolyReg, centerPoly);
            Point2d projectedPtOnOtherReg = GraphicsUtility.ProjectedPointOnLine(lineInOtherReg, centerOther);

            double dist1 = GraphicsUtility.DistanceBetweenPoints(centerPoly, projectedPtOnPolyReg);
            double dist2 = GraphicsUtility.DistanceBetweenPoints(centerOther, projectedPtOnOtherReg);

            double totalDistance = 2 * (dist1 + dist2);
            lineInPolyReg.move(centerPoly);
            Point2d projectedPt = GraphicsUtility.ProjectedPointOnLine(lineInPolyReg, centerOther);
            double distance = GraphicsUtility.DistanceBetweenPoints(projectedPt, centerOther);


            if (totalDistance - eps < distance && distance < totalDistance + eps)
            {
                return lineInOtherReg;
            }
            else
            {
                return null;
            }

        }

        //internal function for Recursive Split By Area to work
        internal static double DistanceEditBasedOnRatio(double distance, double areaPoly, double areaFound, double area, double setSpan, double areaDifference)
        {
            double distanceNew = 0;


            distanceNew = distance * (area / areaFound);
            //Trace.WriteLine("Ratio multiplied to distance is : " + (area / areaFound));
            //distanceNew = distance;
            return distanceNew;
        }

        //RECURSIVE SPLITS A POLY
        [MultiReturn(new[] { "PolyAfterSplit", "AreaEachPoly", "EachPolyPoint" })]
        public static Dictionary<string, object> RecursiveSplitByArea(Polygon2d poly, double area, int dir, int recompute = 1)
        {

            /*PSUEDO CODE:
            get poly's vertical and horizontal span
            based on that get the direction of split
            get polys area and compare with given area ( if area bigger than polyarea then return null )
            based on the proportion calc distance
            bigpoly 
                split that into two
                save distance
                check area of both
                get the smaller poly and compare with asked area
                if area more increase distance by that much
                if area less decrease distance by that much
                split the bigger poly again
                repeat                
            */



            List<Polygon2d> polyList = new List<Polygon2d>();
            List<double> areaList = new List<double>();
            List<Point2d> pointsList = new List<Point2d>();
            List<Point2d> polyBBox = PolygonUtility.FromPointsGetBoundingPoly(poly.Points);
            Range2d polyRange = PolygonUtility.GetRang2DFromBBox(poly.Points);
            double minimumLength = 200;
            double perc = 0.2;
            //set limit of 10%
            double limit = area * perc;

            // increase required area by 10%
            //area += area * perc/4;

            Point2d span = polyRange.Span;
            double horizontalSpan = span.X;
            double verticalSpan = span.Y;
            List<double> spans = new List<double>();
            spans.Add(horizontalSpan);
            spans.Add(verticalSpan);
            double setSpan = 1000000000000;
            //int dir = 0;
            if (horizontalSpan > verticalSpan)
            {
                //dir = 1;
                setSpan = horizontalSpan;

            }
            else
            {
                //dir = 0;
                setSpan = verticalSpan;

            }
            double prop = 0;
            double areaPoly = GraphicsUtility.AreaPolygon2d(poly.Points);
            double areaDifference = 200000;
            if (areaPoly < area)
            {
                return null;
            }
            else
            {
                prop = area / areaPoly;
            }

            List<Polygon2d> polyAfterSplitting = new List<Polygon2d>();
            double distance = prop * setSpan;
            Polygon2d currentPoly = poly;
            int count = 0;
            //Trace.WriteLine("Initial Distance set is : " + distance);
            //Trace.WriteLine("Set Span found is : " + setSpan);
            //Trace.WriteLine("Limit accepted is : " + limit);
            Random ran2 = new Random();
            while (Math.Abs(areaDifference) > limit && count < 300)
            {
                if (currentPoly.Points == null || distance > setSpan)
                {
                    //Trace.WriteLine("Breaking This---------------------------------");
                    break;
                }

                polyAfterSplitting = BuildLayout.EdgeSplitWrapper(currentPoly, ran2, distance, dir);
                double area1 = GraphicsUtility.AreaPolygon2d(polyAfterSplitting[0].Points);


                areaDifference = area - area1;
                distance = DistanceEditBasedOnRatio(distance, areaPoly, area1, area, setSpan, areaDifference);
                //Trace.WriteLine("Updated Distance for 1 is : " + distance);
                //Trace.WriteLine("Area Difference found for 1 is : " + areaDifference);


                if (areaDifference < 0)
                {
                    //Trace.WriteLine("Reducing Distance");
                }






                //reduce number of points
                //currentPoly = new Polygon2d(currentPoly.Points);
                areaList.Add(distance);
                //Trace.WriteLine("Distance Now is : " + distance);
                //Trace.WriteLine("Iteration is : " + count);
                //Trace.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++");
                count += 1;
            }

            polyList.AddRange(polyAfterSplitting);
            pointsList = null;
            //return polyList;
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyList) },
                { "AreaEachPoly", (areaList) },
                { "EachPolyPoint", (pointsList) }
            };
        }



      
        // USING NOW 
        //SPLITS A POLYGON2D INTO TWO POLYS, by a Given Line
        [MultiReturn(new[] { "PolyAfterSplit", "SplitLine" })]
        internal static Dictionary<string, object> SplitByLine(Polygon2d polyOutline, Line2d inputLine, double distance = 5)
        {
            if (polyOutline == null || polyOutline.Points == null || polyOutline.Points.Count == 0)
            {
                return null;
            }


            double spacing = BuildLayout.spacingSet;

            List<Point2d> polyOrig = polyOutline.Points;

            List<Point2d> poly = PolygonUtility.SmoothPolygon(polyOrig, spacing);
            Line2d splitLine = new Line2d(inputLine);
            Point2d centerPoly = GraphicsUtility.CentroidInPointLists(poly);
            bool checkSide = GraphicsUtility.CheckPointSide(splitLine, centerPoly);
            int orient = GraphicsUtility.CheckLineOrient(splitLine);
            if (orient == 0)
            {
                if (checkSide)
                {
                    splitLine.move(0, -1 * distance);
                }
                else
                {
                    splitLine.move(0, 1 * distance);
                }
            }
            else
            {
                if (checkSide)
                {
                    splitLine.move(-1 * distance, 0);
                }
                else
                {
                    splitLine.move(1 * distance, 0);
                }

            }

            //splitLine.move(poly, distance);
            List<Point2d> intersectedPoints = GraphicsUtility.LinePolygonIntersection(poly, splitLine);

            // find all points on poly which are to the left or to the right of the line
            Polygon2d polyA, polyB;

            List<int> pIndexA = new List<int>();
            List<int> pIndexB = new List<int>();
            for (int i = 0; i < poly.Count; i++)
            {
                bool check = GraphicsUtility.CheckPointSide(splitLine, poly[i]);
                if (check)
                {
                    pIndexA.Add(i);
                }
                else
                {
                    pIndexB.Add(i);
                }
            }

            //organize the points to make closed poly
            List<List<Point2d>> twoSets = new List<List<Point2d>>();
            List<Point2d> sortedA = PolygonUtility.DoSortClockwise(poly, intersectedPoints, pIndexA);
            List<Point2d> sortedB = PolygonUtility.DoSortClockwise(poly, intersectedPoints, pIndexB);
            twoSets.Add(sortedA);
            twoSets.Add(sortedB);
            polyA = new Polygon2d(twoSets[0], 0);
            polyB = new Polygon2d(twoSets[1], 0);


            List<Polygon2d> splittedPoly = new List<Polygon2d>();

            splittedPoly.Add(polyA);
            splittedPoly.Add(polyB);
            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "SplitLine", (splitLine) }
            };

        }



        [MultiReturn(new[] { "PolyAfterSplit", "SplitLine", "IntersectedPoints", "SpansBBox", "EachPolyPoint" })]
        public static Dictionary<string, object> SplitByDistanceTest(Polygon2d polyOutline, double distance = 10, int dir = 0, double dummy = 0)
        {
            if (polyOutline == null || polyOutline.Points == null || polyOutline.Points.Count == 0)
            {
                return null;
            }

            double extents = 5000;
            double spacing = BuildLayout.spacingSet;
            List<Point2d> polyOrig = polyOutline.Points;
            List<Point2d> poly = PolygonUtility.SmoothPolygon(polyOrig, spacing);
            if (poly == null || poly.Count == 0)
            {
                return null;
            }
            Random ran = new Random();
            List<double> spans = PolygonUtility.GetSpansXYFromPolygon2d(poly);
            Dictionary<int, object> obj = PolygonUtility.pointSelector(ran, poly);
            Point2d pt = (Point2d)obj[0];
            int orient = (int)obj[1];


            int ind = GraphicsUtility.ReturnLowestPointFromList(poly);
            pt = poly[ind];

            Line2d splitLine = new Line2d(pt, extents, dir);
            // add check such that distanche provided does not exceed the shorter span
            if (dir == 1)
            {
                if (distance > spans[0])
                {
                    //distance = 0.95 * spans[0];
                }

            }
            else
            {
                if (distance > spans[1])
                {
                    //distance = 0.95* spans[1];
                }

            }
            orient = 1;
            // push this line right or left or up or down based on ratio
            if (dir == 1)
            {
                Trace.WriteLine("moved point horizontally : " + orient * distance);
                splitLine.move(0, orient * distance);
            }
            else
            {
                Trace.WriteLine("moved point vertically : " + orient * distance);
                splitLine.move(orient * distance, 0);
            }

            //List<Point2d> intersectedPoints = GraphicsUtility.LinePolygonIntersection(poly, splitLine);
            List<Point2d> intersectedPoints = GraphicsUtility.LinePolygonIntersectionInd(poly, splitLine);

            List<int> pIndexA = new List<int>();
            List<int> pIndexB = new List<int>();
            for (int i = 0; i < poly.Count; i++)
            {
                bool check = GraphicsUtility.CheckPointSide(splitLine, poly[i]);
                if (check)
                {
                    pIndexA.Add(i);
                }
                else
                {
                    pIndexB.Add(i);
                }
            }

            //organize the points to make closed poly
            List<Point2d> sortedA = PolygonUtility.DoSortClockwise(poly, intersectedPoints, pIndexA);
            List<Point2d> sortedB = PolygonUtility.DoSortClockwise(poly, intersectedPoints, pIndexB);

            List<List<Point2d>> twoSets = new List<List<Point2d>>();
            twoSets.Add(sortedA);
            twoSets.Add(sortedB);

            List<Polygon2d> splittedPoly = PolygonUtility.OptimizePolyPoints(sortedA, sortedB);

            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "SplitLine", (splitLine) },
                { "IntersectedPoints", (intersectedPoints) },
                { "SpansBBox", (spans) },
                { "EachPolyPoint", (pt) }
            };

        }

        // USING NOW 
        //SPLITS A POLYGON2D INTO TWO POLYS, BASED ON A DIRECTION AND RATIO
        [MultiReturn(new[] { "PolyAfterSplit", "SplitLine", "IntersectedPoints", "SpansBBox", "EachPolyPoint" })]
        internal static Dictionary<string, object> SplitFromEdgePolyIntoTwo(Polygon2d polyOutline, double distance = 10, int dir = 0)
        {
            double extents = 5000;
            double spacing = BuildLayout.spacingSet;
            double minimumLength = 10;
            double minValue = 10;
            bool horizontalSplit = false;
            bool verticalSplit = true;
            // dir = 0 : horizontal split line
            // dir = 1 : vertical split line

            List<Point2d> polyOrig = polyOutline.Points;
            double eps = 0.1;
            //CHECKS
            /////////////////////////////////////////////////////////////////////////////////////////////////////////////
            //List<Point2d> poly = GraphicsUtility.AddPointsInBetween(polyOrig, 5);
            List<Point2d> poly = PolygonUtility.SmoothPolygon(polyOrig, spacing);
            // compute bounding box ( set of four points ) for the poly
            // find x Range, find y Range
            List<Point2d> polyBBox = PolygonUtility.FromPointsGetBoundingPoly(poly);
            Range2d polyRange = PolygonUtility.GetRang2DFromBBox(poly);

            Point2d span = polyRange.Span;
            double horizontalSpan = span.X;
            double verticalSpan = span.Y;
            List<double> spans = new List<double>();
            spans.Add(horizontalSpan);
            spans.Add(verticalSpan);
            //compute centroid
            //Point2d polyCenter = Polygon2d.CentroidFromPoly(poly);

            //compute lowest point
            int lowInd = GraphicsUtility.ReturnLowestPointFromListNew(poly);
            Point2d lowPt = poly[lowInd];
            //check aspect ratio
            double aspectRatio = 0;



            // check if width or length is enough to make split
            if (horizontalSpan < minimumLength || verticalSpan < minimumLength)
            {
                return null;
            }


            //should check direction of split ( flip dir value )
            if (horizontalSpan > verticalSpan)
            {
                //dir = 1;
                aspectRatio = horizontalSpan / verticalSpan;
            }
            else
            {
                //dir = 0;
                aspectRatio = verticalSpan / horizontalSpan;
            }

            if (aspectRatio > 2)
            {
                //return null;
            }

            //set split style
            if (dir == 0)
            {
                horizontalSplit = true;
            }
            else
            {
                verticalSplit = true;
            }



            // adjust distance if less than some value
            if (distance < minValue)
            {
                distance = minValue;
            }
            // adjust distance if more than total length of split possible
            if (verticalSplit)
            {
                if (distance > verticalSpan)
                {
                    distance = verticalSpan - minValue;
                }
            }

            if (horizontalSplit)
            {
                if (distance > horizontalSpan)
                {
                    distance = horizontalSpan - minValue;
                }
            }


            ////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////

            Line2d splitLine = new Line2d(lowPt, extents, dir);
            //compute vertical or horizontal line via centroid



            // push this line right or left or up or down based on ratio
            if (dir == 0)
            {
                splitLine.move(0, distance);
            }
            else
            {
                splitLine.move(distance, 0);
            }



            List<Point2d> intersectedPoints = GraphicsUtility.LinePolygonIntersection(poly, splitLine);

            ////////////////////////////////////////////////////////////////////////////////////////////



            // find all points on poly which are to the left or to the right of the line
            Polygon2d polyA, polyB;

            List<int> pIndexA = new List<int>();
            List<int> pIndexB = new List<int>();
            for (int i = 0; i < poly.Count; i++)
            {
                bool check = GraphicsUtility.CheckPointSide(splitLine, poly[i]);
                if (check)
                {
                    pIndexA.Add(i);
                    //ptA.Add(poly[i]);
                }
                else
                {
                    pIndexB.Add(i);
                    //ptB.Add(poly[i]);
                }
            }

            //organize the points to make closed poly
            List<List<Point2d>> twoSets = new List<List<Point2d>>();
            List<Point2d> sortedA = PolygonUtility.DoSortClockwise(poly, intersectedPoints, pIndexA);
            List<Point2d> sortedB = PolygonUtility.DoSortClockwise(poly, intersectedPoints, pIndexB);
            twoSets.Add(sortedA);
            twoSets.Add(sortedB);
            List<Polygon2d> splittedPoly = PolygonUtility.OptimizePolyPoints(sortedA, sortedB);

            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "SplitLine", (splitLine) },
                { "IntersectedPoints", (intersectedPoints) },
                { "SpansBBox", (spans) },
                { "EachPolyPoint", (twoSets) }
            };

        }

        //used to split Depts into Program Spaces
        [MultiReturn(new[] { "PolyAfterSplit", "UpdatedProgramData" })]
        public static Dictionary<string, object> RecursivePlaceProgramsSeriesNew(List<Polygon2d> polyInputList,
            List<ProgramData> progData, double minWidth = 5)
        {

            if (polyInputList == null || polyInputList.Count == 0) return null;
            Stack<Polygon2d> polyContainerList = new Stack<Polygon2d>();
            List<Polygon2d> polyList = new List<Polygon2d>();
            for (int i = 0; i < polyInputList.Count; i++)
            {
                if (polyInputList[i] == null || polyInputList[i].Points == null || polyInputList[i].Points.Count == 0) continue;
                polyContainerList.Push(polyInputList[i]);
            }

            for (int i = 0; i < progData.Count; i++)
            {
                if (polyContainerList.Count > 0)
                {
                    while (progData[i].IsAreaSatisfied == false && polyContainerList.Count > 0)
                    {

                        double areaProg = progData[i].AreaNeeded;
                        Polygon2d currentPoly = polyContainerList.Pop();
                        double areaPoly = PolygonUtility.AreaCheckPolygon(currentPoly);
                        Dictionary<string, object> splitResult;
                        if (areaPoly < areaProg)
                        {
                            polyList.Add(currentPoly);
                            //currentPoly = polyContainerList.Pop();
                        }
                        else
                        {
                            double dist = 0; int dir = 1;
                            double ratio = areaPoly / areaProg;


                            List<double> spans = PolygonUtility.GetSpansXYFromPolygon2d(currentPoly.Points);
                            double spanX = spans[0], spanY = spans[1];
                            if (spanX > spanY)
                            {
                                if (spanY < minWidth)
                                {
                                    continue;
                                }
                                dist = spanX / ratio; dir = 1;
                                splitResult = BuildLayout.SplitByDistanceFromPoint(currentPoly, dist, dir);
                            }
                            else
                            {
                                if (spanX < minWidth)
                                {
                                    continue;
                                }
                                dist = spanY / ratio; dir = 0;
                                splitResult = BuildLayout.SplitByDistanceFromPoint(currentPoly, dist, dir);
                            }

                            List<Polygon2d> polyAfterSplit = (List<Polygon2d>)splitResult["PolyAfterSplit"];
                            double areaA = PolygonUtility.AreaCheckPolygon(polyAfterSplit[0]);
                            double areaB = PolygonUtility.AreaCheckPolygon(polyAfterSplit[1]);
                            Polygon2d space, container;
                            double areaSpace;
                            if (areaA < areaB)
                            {
                                space = polyAfterSplit[0];
                                container = polyAfterSplit[1];
                                areaSpace = areaA;
                            }
                            else
                            {
                                space = polyAfterSplit[1];
                                container = polyAfterSplit[0];
                                areaSpace = areaB;
                            }
                            double areaPolyAfterSplit = PolygonUtility.AreaCheckPolygon(polyAfterSplit[0]);
                            progData[i].AreaProvided += areaSpace;
                            polyList.Add(space);
                            polyContainerList.Push(container);

                        }
                    }//end of while loop

                }
            }// end of for loop


            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (polyList) },
                { "UpdatedProgramData",(null) }
            };


        }

     
        [MultiReturn(new[] { "PolyAfterSplit", "SplitLine", "IntersectedPoints", "LowestPoint", "EachPolyPoint" })]
        public static Dictionary<string, object> SplitByDistancePoly(Polygon2d polyOutline, double distance = 10, int dir = 0, double dummy = 0)
        {
            if (polyOutline == null || polyOutline.Points == null || polyOutline.Points.Count == 0)
            {
                return null;
            }
            double fac = 0.999; // 0.999 default
            double extents = 5000;
            double spacing = BuildLayout.spacingSet;
            List<Point2d> polyOrig = polyOutline.Points;
            List<Point2d> poly = PolygonUtility.SmoothPolygon(polyOrig, spacing);
            if (poly == null || poly.Count == 0)
            {
                return null;
            }
            List<double> spans = PolygonUtility.GetSpansXYFromPolygon2d(poly);
            int orient = 1;
            //int ind = GraphicsUtility.ReturnLowestPointFromListNew(poly);
            int ind = GraphicsUtility.ReturnLowestPointFromList(poly);
            Point2d pt = poly[ind];
            Line2d splitLine = new Line2d(pt, extents, dir);

            if (dir == 1)
            {
                if (distance > spans[0])
                {
                    distance = fac * spans[0];
                }

            }
            else
            {
                if (distance > spans[1])
                {
                    distance = fac * spans[1];
                }

            }
            // push this line right or left or up or down based on ratio
            if (dir == 0)
            {
                splitLine.move(0, orient * distance);
            }
            else
            {
                splitLine.move(orient * distance, 0);
            }

            //List<Point2d> intersectedPoints = GraphicsUtility.LinePolygonIntersectionInd(poly, splitLine);
            List<Point2d> intersectedPoints = GraphicsUtility.LinePolygonIntersection(poly, splitLine);

            List<int> pIndexA = new List<int>();
            List<int> pIndexB = new List<int>();
            for (int i = 0; i < poly.Count; i++)
            {
                bool check = GraphicsUtility.CheckPointSide(splitLine, poly[i]);
                if (check)
                {
                    pIndexA.Add(i);
                }
                else
                {
                    pIndexB.Add(i);
                }
            }

            //organize the points to make closed poly
            List<Point2d> sortedA = PolygonUtility.DoSortClockwise(poly, intersectedPoints, pIndexA);
            List<Point2d> sortedB = PolygonUtility.DoSortClockwise(poly, intersectedPoints, pIndexB);

            List<List<Point2d>> twoSets = new List<List<Point2d>>();
            twoSets.Add(sortedA);
            twoSets.Add(sortedB);

            List<Polygon2d> splittedPoly = PolygonUtility.OptimizePolyPoints(sortedA, sortedB, true);

            return new Dictionary<string, object>
            {
                { "PolyAfterSplit", (splittedPoly) },
                { "SplitLine", (splitLine) },
                { "IntersectedPoints", (intersectedPoints) },
                { "LowestPoint", (pt) },
                { "EachPolyPoint", (twoSets) }
            };

        }




    }
}
