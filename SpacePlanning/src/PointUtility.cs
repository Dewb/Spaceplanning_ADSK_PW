using System;
using System.Collections.Generic;
using System.Linq;
using stuffer;
using Autodesk.DesignScript.Runtime;
using System.Diagnostics;
using System.Collections;

namespace SpacePlanning
{
    class PointUtility
    {
        #region - Public Methods
        // find the centroid of list of point2d
        public static Point2d CentroidInPointLists(List<Point2d> ptList)
        {
            if (ptList == null || ptList.Count == 0) return null;
            double x = 0, y = 0;
            for (int i = 0; i < ptList.Count; i++)
            {
                x += ptList[i].X;
                y += ptList[i].Y;
            }
            x = x / ptList.Count;
            y = y / ptList.Count;
            return new Point2d(x, y);

        }

        //get the bounding box from input points
        public static List<Point2d> FromPointsGetBoundingPoly(List<Point2d> pointList)
        {
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

        //generate points on a circle and then randomize their sequence
        public static List<Point2d> PointGenerator(int tag, int size)
        {
            List<Point2d> pointGen = new List<Point2d>();
            double rad = 1000;
            double t = 0;
            for (int i = 0; i < size; i++)
            {
                t += 15;
                pointGen.Add(new Point2d(rad * Math.Cos(t), rad * Math.Sin(t)));
            }
            var rnd = new Random();
            var newSequence = pointGen.OrderBy(item => rnd.Next());
            List<Point2d> finalPts = newSequence.ToList();
            return finalPts;
        }

        //sorts a list of points form a polyline
        public static List<Point2d> SortPoints(List<Point2d> pointList)
        {
            if (!ValidateObject.CheckPointList(pointList)) return null;
            Point2d cen = PolygonUtility.CentroidOfPoly(Polygon2d.ByPoints(pointList));
            Vector2d vecX = new Vector2d(0, 100);
            List<double> angList = new List<double>();
            int[] indList = new int[pointList.Count];
            for (int i = 0; i < pointList.Count; i++)
            {
                Vector2d CenterToPt = new Vector2d(cen, pointList[i]);
                double dotValue = vecX.Dot(CenterToPt);
                double angValue = Math.Atan2(CenterToPt.Y - vecX.Y, CenterToPt.X - vecX.X);
                angList.Add(angValue);
                indList[i] = i;
            }
            List<int> newIndexList = new List<int>();
            newIndexList = BasicUtility.SortIndex(angList);
            List<Point2d> sortedPointList = new List<Point2d>();
            for (int i = 0; i < pointList.Count; i++)
            {
                sortedPointList.Add(pointList[newIndexList[i]]);
            }
            return sortedPointList;
        }

        //sorts list of points by distance from a given point
        public static List<Point2d> SortPointsByDistanceFromPoint(List<Point2d> ptList, Point2d testPoint)
        {
            List<Point2d> sortedPtList = new List<Point2d>();
            List<double> distanceList = new List<double>();
            List<int> indexList = new List<int>();
            for (int i = 0; i < ptList.Count; i++) distanceList.Add(PointUtility.DistanceBetweenPoints(ptList[i], testPoint));
            indexList = BasicUtility.SortIndex(distanceList);
            for (int i = 0; i < indexList.Count; i++) sortedPtList.Add(ptList[indexList[i]]);
            return sortedPtList;
        }


        //random point selector from a list
        public static Dictionary<int, object> PointSelector(Random ran, List<Point2d> poly)
        {
            Dictionary<int, object> output = new Dictionary<int, object>();
            double num = ran.NextDouble();
            int highInd = HighestPointFromList(poly);
            Point2d hiPt = poly[highInd];
            int lowInd = LowestPointFromList(poly);
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

        #endregion

        #region - Private Methods
        //returns the point having lowest x,y value from a list using now
        internal static int LowestPointFromList(List<Point2d> ptList)
        {
            if (!ValidateObject.CheckPointList(ptList)) return -1;
            Point2d lowestPoint = ptList[0];
            int size = ptList.Count;
            int index = 0;
            for (int i = 0; i < size; i++)
            {
                if ((lowestPoint.X > ptList[i].X) || (lowestPoint.X == ptList[i].X && lowestPoint.Y > ptList[i].Y))
                {
                    lowestPoint = ptList[i];
                    index = i;
                }
            }
            return index;

        }

        //returns the highest and lowest point along with indices from a pointlist
        internal static Dictionary<string, object> ReturnHighestAndLowestPointofBBox(Polygon2d poly)
        {
            Range2d range = PolygonUtility.GetRang2DFromBBox(ReadData.FromPointsGetBoundingPoly(poly.Points));
            double minX = range.Xrange.Min;
            double maxX = range.Xrange.Max;
            double minY = range.Yrange.Min;
            double maxY = range.Yrange.Max;
            Point2d lowPt = new Point2d(minX, minY), hipt = new Point2d(maxX, maxY);


            return new Dictionary<string, object>
            {
                { "LowerPoint", (lowPt) },
                { "HigherPoint", (hipt) },
            };
        }

        //returns the point having highest x,y value from a list - using now
        internal static int HighestPointFromList(List<Point2d> ptList)
        {
            if (!ValidateObject.CheckPointList(ptList)) return -1;
            Point2d highestPoint = ptList[0];
            int size = ptList.Count;
            int index = 0;
            for (int i = 0; i < size; i++)
            {
                if ((highestPoint.X < ptList[i].X) || (highestPoint.X == ptList[i].X && highestPoint.Y < ptList[i].Y))
                {
                    highestPoint = ptList[i];
                    index = i;
                }
            }
            return index;
        }

        //sort points clockwise direction
        internal static List<Point2d> DoSortClockwise(List<Point2d> poly, List<Point2d> intersectedPoints, List<int> pIndex)
        {
            if (intersectedPoints == null || intersectedPoints.Count == 0) return null;
            List<Point2d> cleanedPtList = new List<Point2d>();
            if (intersectedPoints.Count > 2) cleanedPtList = CleanDuplicatePoint2d(intersectedPoints);
            else cleanedPtList = intersectedPoints;
            return PolygonUtility.OrderPolygon2dPoints(poly, cleanedPtList, pIndex); //intersectedPoints
        }

        //find lowest point from a list ( only used in the Grahams Scan Convex Hull Algo )
        internal static void GetLowestPointForGrahamsScan(ref List<Point2d> ptList, int size)
        {
            Point2d lowestPoint = ptList[0];
            for (int i = 0; i < size; i++)
            {
                if (lowestPoint.Y > ptList[0].Y || (lowestPoint.Y == ptList[i].Y && lowestPoint.X > ptList[i].X)) lowestPoint = ptList[i];

            }
            int rootPosition = ElementPosition(ptList, size, lowestPoint);
            if (rootPosition != -1) ChangePlaces(ref ptList, 0, rootPosition);

        }

        //distance between points
        internal static double DistanceBetweenPoints(Point2d ptA, Point2d ptB)
        {
            if (ptA == null || ptB == null) return 0;
            double xLen = ptA.X - ptB.X;
            double yLen = ptA.Y - ptB.Y;
            return Math.Sqrt(xLen * xLen + yLen * yLen);
        }

        //distance between points
        internal static double DistanceBetweenPointsByLine(Point2d ptA, Point2d ptB)
        {
            if (ptA == null || ptB == null) return 0;
            Line2d line = new Line2d(ptA, ptB);
            return line.Length;
        }


        //sort point array for Grahams scan algo to find convex hull
        internal static void SortedPoint2dListForGrahamScan(ref List<Point2d> ptList, int size)
        {
            for (int i = 1; i < size; ++i)
            {
                for (int j = i + 1; j < size; ++j)
                {
                    int order = ValidateObject.CheckPointOrder(ptList[0], ptList[i], ptList[j]);
                    // collinear
                    if (order == 0)
                    {
                        if (DistanceBetweenPoints(ptList[0], ptList[0]) <= DistanceBetweenPoints(ptList[0], ptList[j])) ChangePlaces(ref ptList, i, j);
                    }
                    else if (order == 1) ChangePlaces(ref ptList, i, j);
                }
            }
        }

        // change two points in the list - used by Grahams Algo
        internal static void ChangePlaces(ref List<Point2d> pointList, int posA, int posB)
        {
            Point2d a = pointList[posA];
            pointList[posA] = pointList[posB];
            pointList[posB] = a;
        }

        // get element position in a list
        internal static int ElementPosition(List<Point2d> ptList, int size, Point2d pt)
        {
            for (int i = 0; i < size; ++i)
            {
                if (ptList[i] == pt) return i;
            }
            return -1;
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

        //returns only unique point2d from a list of points
        internal static List<Point2d> ReturnUniquePoints(List<Point2d> testList)
        {
            List<Point2d> cleanList = new List<Point2d>();
            List<double> Xlist = new List<double>();
            List<double> Ylist = new List<double>();
            for (int i = 0; i < testList.Count; i++)
            {
                Xlist.Add(testList[i].X);
                Ylist.Add(testList[i].Y);
            }

            var duplicateIndexesX = Xlist
            .Select((t, i) => new { Index = i, Text = t })
            .GroupBy(g => g.Text)
            .Where(g => g.Count() > 1)
            .SelectMany(g => g, (g, x) => x.Index);

            var duplicateIndexesY = Xlist
           .Select((t, i) => new { Index = i, Text = t })
           .GroupBy(g => g.Text)
           .Where(g => g.Count() > 1)
           .SelectMany(g => g, (g, x) => x.Index);
            if (duplicateIndexesX.Count() == 0 || duplicateIndexesY.Count() == 0) return testList;


            List<int> dupX = (List<int>)duplicateIndexesX;
            List<int> dupY = (List<int>)duplicateIndexesX;
            List<int> commonIndex = new List<int>();
            for (int i = 0; i < dupX.Count; i++)
            {
                for (int j = 0; j < dupY.Count; j++)
                {
                    if (dupX[i] == dupY[j])
                    {
                        commonIndex.Add(dupX[i]);
                    }
                }

            }

            for (int i = 0; i < testList.Count; i++)
            {
                for (int j = 0; j < commonIndex.Count; j++)
                {
                    if (i != commonIndex[j])
                    {
                        cleanList.Add(testList[i]);
                        break;
                    }
                }
            }
            return cleanList;
        }
        //get next to top point on Stack for Graham Scan algo
        internal static Point2d BeforeTopPointForGrahamScan(ref Stack myStack)
        {
            Point2d pt = new Point2d(0, 0);
            pt = (Point2d)myStack.Peek();
            if (myStack.Count > 1)
            {
                myStack.Pop();
                Point2d result = (Point2d)myStack.Peek();
                myStack.Push(pt);
                return result;
            }
            return pt;
        }

        // sorts two points clockwise, with respect to a reference point
        internal static int SortCornersClockwise(Point2d A, Point2d B)
        {
            double aTanA, aTanB;
            aTanA = Math.Atan2(A.Y - BuildLayout.REFERENCEPOINT.Y, A.X - BuildLayout.REFERENCEPOINT.X);
            aTanB = Math.Atan2(B.Y - BuildLayout.REFERENCEPOINT.Y, B.X - BuildLayout.REFERENCEPOINT.X);
            //  Determine next point in Clockwise rotation
            if (aTanA < aTanB) return -1;
            else if (aTanA > aTanB) return 1;
            return 0;
        }

        // finds the angle between two points wrt Origin - not sure
        internal static double AngleBetweenPoint2d(Point2d A, Point2d center)
        {
            double angle = Math.Acos((A.X - center.X) / PointUtility.DistanceBetweenPoints(center, A));
            if (A.Y < center.Y) angle = Math.PI + Math.PI - angle;//360-angle
            return angle; //return angle*180/Math.PI;
        }

        //from point2d list get the range2d
        internal static Range2d FromPoint2dGetRange2D(List<Point2d> point2dList)
        {
            List<double> xCordList = new List<double>();
            List<double> yCordList = new List<double>();
            double xMax = 0, xMin = 0, yMax = 0, yMin = 0;
            for (int i = 0; i < point2dList.Count; i++)
            {
                xCordList.Add(point2dList[i].X);
                yCordList.Add(point2dList[i].Y);
            }

            xMax = xCordList.Max();
            yMax = yCordList.Max();

            xMin = xCordList.Min();
            yMin = yCordList.Min();

            Range1d x = new Range1d(xMin, xMax);
            Range1d y = new Range1d(yMin, yMax);
            Range2d xyRange = new Range2d(x, y);

            return xyRange;
        }
        #endregion



    }
}
