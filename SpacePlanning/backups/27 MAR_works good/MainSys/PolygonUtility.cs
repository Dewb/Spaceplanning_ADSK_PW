using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using stuffer;


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

        //can be discarded cleans duplicate points and returns updated list
        internal static List<Point2d> CleanDuplicatePoint2d(List<Point2d> ptListUnclean)
        {
            List<Point2d> cleanList = new List<Point2d>();
            List<double> dummyList = new List<double>();
            List<bool> isDuplicate = new List<bool>();
            double eps = 1;
            bool duplicate = false;



            for (int i = 0; i < ptListUnclean.Count; i++)
            {
                duplicate = false;
                double count = 0;
                for (int j = 0; j < ptListUnclean.Count; j++)
                {

                    if (j == i)
                    {
                        continue;
                    }

                    if (ptListUnclean[i].X - eps < ptListUnclean[j].X && ptListUnclean[j].X < ptListUnclean[i].X + eps)
                    {
                        if (ptListUnclean[i].Y - eps < ptListUnclean[j].Y && ptListUnclean[j].Y < ptListUnclean[i].Y + eps)
                        {
                            count += 1;
                        }
                    }

                    if (count > 1)
                    {
                        duplicate = true;

                        //continue;
                    }


                }

                dummyList.Add(count);



            }
            for (int i = 0; i < ptListUnclean.Count; i++)
            {
                //Trace.WriteLine("count here is : " + dummyList[i]);
                if (dummyList[i] < 2)
                {
                    cleanList.Add(ptListUnclean[i]);
                }

            }
            return cleanList;
        }


        //cleans duplicate points and returns updated list - using this now
        internal static List<Point2d> CleanDuplicatePoint2dNew(List<Point2d> ptListUnclean)
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
        //trying new way to sort points clockwise
        internal static List<Point2d> DoSortClockwise(List<Point2d> poly, List<Point2d> intersectedPoints, List<int> pIndex)
        {
            if (intersectedPoints == null || intersectedPoints.Count == 0)
            {
                return null;

            }
            if (intersectedPoints.Count > 2)
            {

                //Trace.WriteLine("Wow found  " + intersectedPoints.Count + " intersection points!!!!!!!!!!!!!!!");
                List<Point2d> cleanedPtList = CleanDuplicatePoint2dNew(intersectedPoints);
                //Trace.WriteLine("After Cleaning found  " + cleanedPtList.Count + " intersection points!!!!!!!!!!!!!!!");
                //intersectedPoints = GraphicsUtility.CleanDuplicatePoint2d(intersectedPoints);
                //return null;

            }
            /*

            if (intersectedPoints.Count < 2)
            {
                Trace.WriteLine("Returning null as less than 1 points");
                //return null;
            }
            List<Point2d> pointList = new List<Point2d>();
            List<Point2d> mergedPoints = new List<Point2d>();
            
            for (int i = 0; i < pIndex.Count; i++)
            {
                mergedPoints.Add(poly[pIndex[i]]);
            }
            reference = GraphicsUtility.CentroidInPointLists(mergedPoints);
            mergedPoints.AddRange(intersectedPoints);

            //mergedPoints.Sort((a, b) => GraphicsUtility.Angle(a, reference).CompareTo(GraphicsUtility.Angle(b, reference)));
            //mergedPoints.Sort(new Comparison<Point2d>(GraphicsUtility.SortCornersClockwise));
            //return mergedPoints;
            */
            return makePolyPointsStraight(poly, intersectedPoints, pIndex);
        }

        internal static List<Point2d> makePolyPointsStraight(List<Point2d> poly, List<Point2d> intersectedPoints, List<int> pIndex)
        {

            if (intersectedPoints.Count < 2)
            {
                return null;
            }

            //List<Point2d> intersectedPoints = GraphicsUtility.PointUniqueChecker(intersectedPointsUnclean);
            //Trace.WriteLine("Intersected Points Length are : " + intersectedPoints.Count);
            //bool isUnique = intersectedPoints.Distinct().Count() == intersectedPoints.Count();
            //Trace.WriteLine("Intersected Point List is unique ?   " + isUnique);
            List<Point2d> pt = new List<Point2d>();
            bool added = false;

            int a = 0;
            int b = 1;
            if (intersectedPoints.Count > 2)
            {
                //Trace.WriteLine("Intersected pnts are more than one " + intersectedPoints.Count);
                //a = 0;
                //b = intersectedPoints.Count - 1;
            }

            //Trace.WriteLine("Intersected Points Length are : " + intersectedPoints.Count);
            //Trace.WriteLine("a and b are : " + a + "  ,  " + b );
            //Trace.WriteLine("Index Point length is :  " + pIndex.Count);
            for (int i = 0; i < pIndex.Count - 1; i++)
            {
                pt.Add(poly[pIndex[i]]);
                //enter only when indices difference are more than one and intersected points are not added yet
                if (Math.Abs(pIndex[i] - pIndex[i + 1]) > 1 && added == false)
                {
                    List<Point2d> intersNewList = GraphicsUtility.SortPointsByDistanceFromPoint(intersectedPoints,
                        poly[pIndex[i]]);
                    pt.Add(intersNewList[a]);
                    pt.Add(intersNewList[b]);
                    //Trace.WriteLine("Added Intersect Before for PtA");
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

                else if (i == (pIndex.Count - 2) && added == true)
                {
                    pt.Add(poly[pIndex[i + 1]]);
                }
            }
            //Trace.WriteLine("Point Returned Length : " + pt.Count);
            //Trace.WriteLine("I++++++++++++++++++++++++++++++++++++++++++");
            return pt;
        }



        internal static Dictionary<int, object> pointSelector(Random ran, List<Point2d> poly)
        {
            Dictionary<int, object> output = new Dictionary<int, object>();

            double num = ran.NextDouble();
            //Trace.WriteLine("Point Selector Random Found is : " + num);
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


        internal static List<Polygon2d> OptimizePolyPoints(List<Point2d> sortedA, List<Point2d> sortedB,
        bool tag = false)
        {
            Polygon2d polyA, polyB;

            if (tag)
            {
                //added to make sure poly has uniform points
                polyA = new Polygon2d(sortedA);
                polyB = new Polygon2d(sortedB);

                List<Point2d> ptsPolyA = PolygonUtility.SmoothPolygon(polyA.Points, BuildLayout.spacingSet);
                List<Point2d> ptsPolyB = PolygonUtility.SmoothPolygon(polyB.Points, BuildLayout.spacingSet);
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



        internal static List<double> PolySpanCheck(Polygon2d poly)
        {
            List<double> spanList = new List<double>();
            Range2d polyRange = PolygonUtility.GetRang2DFromBBox(poly.Points);

            Point2d span = polyRange.Span;
            double horizontalSpan = span.X;
            double verticalSpan = span.Y;

            //place longer span first
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



        //COMPUTE CENTROID OF A CLOSED POLYGON
        public static Point2d CentroidFromPoly(Polygon2d poly)
        {
            if (poly == null || poly.Points == null || poly.Points.Count == 0) return null;
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
            poly = null;
            return cen;

        }


        //COMPUTE CENTROID OF A CLOSED POLYGON
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



        public static List<Point2d> FromPointsGetBoundingPoly(List<Point2d> pointList)
        {
            if (pointList == null)
            {
                return null;
            }

            if (pointList.Count == 0)
            {
                return null;
            }

            List<Point2d> pointCoordList = new List<Point2d>();
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

            pointCoordList.Add(Point2d.ByCoordinates(xMin, yMin));
            pointCoordList.Add(Point2d.ByCoordinates(xMin, yMax));
            pointCoordList.Add(Point2d.ByCoordinates(xMax, yMax));
            pointCoordList.Add(Point2d.ByCoordinates(xMax, yMin));

            return pointCoordList;
        }


        public static List<double> GetSpansXYFromPolygon2d(List<Point2d> poly)
        {

            if (poly == null || poly.Count == 0)
            {
                List<double> zeroList = new List<double>();
                zeroList.Add(0);
                zeroList.Add(0);
                return zeroList;
            }
            // compute bounding box ( set of four points ) for the poly
            // find x Range, find y Range
            List<Point2d> polyBBox = FromPointsGetBoundingPoly(poly);
            Range2d polyRange = GetRang2DFromBBox(poly);

            Point2d span = polyRange.Span;
            double horizontalSpan = span.X;
            double verticalSpan = span.Y;
            List<double> spans = new List<double>();
            spans.Add(horizontalSpan);
            spans.Add(verticalSpan);

            return spans;
        }

        public static Range2d GetRang2DFromBBox(List<Point2d> pointList)
        {
            if (pointList == null || pointList.Count == 0)
            {
                return null;
            }
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
            if (polyList == null || polyList.Count == 0)
            {
                return null;
            }
            List<Polygon2d> reducedPolyList = new List<Polygon2d>();
            for (int i = 0; i < polyList.Count; i++)
            {
                if (polyList[i] == null || polyList[i].Points == null || polyList[i].Points.Count == 0)
                {
                    continue;
                }
                Polygon2d redPoly = new Polygon2d(polyList[i].Points);
                reducedPolyList.Add(redPoly);
            }


            return reducedPolyList;
        }

        internal static List<Point2d> SmoothPolygon(List<Point2d> pointList, double spacingProvided = 1)
        {
            int threshValue = 20;
            if (pointList == null || pointList.Count == 0) return null;
            if (pointList.Count > threshValue) return pointList;
           
            //added to make sure spacing is set based on poly dimensions-------------------
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


        //can be discarded
        //CHECK IF THE AREA POLYGON WORKS FINE
        public static double AreaCheckPolygon(Polygon2d poly)
        {
            if (poly == null) return 0;           
            return GraphicsUtility.AreaPolygon2d(poly.Points);
        }


        //just for checking can be discarded
        public static List<Point2d> SmoothCheckerForPoly(Polygon2d poly, int spacing)
        {
            List<Point2d> smoothedPoints = SmoothPolygon(poly.Points, spacing);
            //Polygon2d polyNew = new Polygon2d(smoothedPoints);
            return smoothedPoints;
        }



    }
}
