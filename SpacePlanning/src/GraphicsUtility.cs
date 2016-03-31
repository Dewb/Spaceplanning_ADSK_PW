
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;
//using Excel = Microsoft.Office.Interop.Excel;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;
using System.IO;
using stuffer;
using System.Collections;
using System.Diagnostics;

///////////////////////////////////////////////////////////////////
/// NOTE: This project requires references to the ProtoInterface
/// and ProtoGeometry DLLs. These are found in the Dynamo install
/// directory.
///////////////////////////////////////////////////////////////////

namespace SpacePlanning
{
    internal class GraphicsUtility
    {
     

        //internal methods
        internal static bool PointInsidePolygonTest(List<Point2d> pointsPolygon, Point2d testPoint)
        {
            bool check = false;
            int numPolyPts = pointsPolygon.Count;

            for (int i = 0, j = numPolyPts - 1; i < numPolyPts; j = i++)
            {
                if (((pointsPolygon[i].Y > testPoint.Y) != (pointsPolygon[j].Y > testPoint.Y)) &&
                (testPoint.X < (pointsPolygon[j].X - pointsPolygon[i].X) * (testPoint.Y - pointsPolygon[i].Y) / (pointsPolygon[j].Y - pointsPolygon[i].Y) + pointsPolygon[i].X))
                {

                    check = !check;

                }

            }
            return check;
        }

        //checks if point is inside polygon
        internal static bool PointInsidePolygonTest(Polygon2d poly, Point2d testPoint)
        {
            List<Point2d> pointsPolygon = poly.Points;
            bool check = false;
            int numPolyPts = pointsPolygon.Count;

            for(int i = 0, j = numPolyPts - 1; i < numPolyPts; j = i++)
            {
                if (((pointsPolygon[i].Y > testPoint.Y) != (pointsPolygon[j].Y > testPoint.Y)) &&
                (testPoint.X < (pointsPolygon[j].X - pointsPolygon[i].X) * (testPoint.Y - pointsPolygon[i].Y) / (pointsPolygon[j].Y - pointsPolygon[i].Y) + pointsPolygon[i].X))
                {
                    
                    check = !check;

                }

            }
            return check;
        }



        //check order of points 0 = collinear, 1 = a,b,c clockwise, 2 = a,b,c are anti clockwise
        internal static int CheckPointOrder(Point2d a, Point2d b, Point2d c)
        {
           
            double area = 0;
            area = (b.Y - a.Y) * (c.X - b.X) - (b.X - a.X) * (c.Y - b.Y);
            if (area > 0)
            {
                // 1 = clockwise
                return 1;
            }else if ( area < 0)
            {
                // 2 = anti clockwise
                return 2;
            }
            
            // 0 = collinear
            return 0;

        }


        // change two points in the list
        internal static void ChangePlaces(ref List<Point2d> pointList, int posA, int posB)
        {
            Point2d a = pointList[posA];
            pointList[posA] = pointList[posB];
            pointList[posB] = a;           
        }



        // get element position in the Array
        internal static int ElementPosition(List<Point2d> ptList,int size,Point2d pt)
        {
            for(int i =0; i< size; ++i)
            {
                if(ptList[i] == pt)
                {
                    return i;
                }
            }

            return -1;
        }


        //checks line if horizontal or vertical 0 for horizontal, 1 for vertical
        internal static int CheckLineOrient(Line2d line)
        {
            double x = line.StartPoint.X - line.EndPoint.X;
            double y = line.StartPoint.Y - line.EndPoint.Y;       
            if (x == 0) return 1;
            else if ( y==0) return 0;
            else return 0;
        }

      

        //joins two collinear lines to make one line
        public static Line2d JoinCollinearLines(Line2d lineA, Line2d lineB)
        {
            List<Point2d> allPoints = new List<Point2d>();
            allPoints.Add(lineA.StartPoint);
            allPoints.Add(lineA.EndPoint);
            allPoints.Add(lineB.StartPoint);
            allPoints.Add(lineB.EndPoint);

            int p = ReturnLowestPointFromListNew(allPoints);
            int q = ReturnHighestPointFromListNew(allPoints);
            Line2d lineJoined = new Line2d(allPoints[p],allPoints[q]);
            return lineJoined;
        }


        //finds and returns the lowest position also
        internal static int ReturnLowestPointFromList(List<Point2d> ptList)
        {
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
            //return
            return index;           

        }
        //finds and returns the lowest position
        internal static int ReturnLowestPointFromListNew(List<Point2d> ptList)
        {
            Point2d lowestPoint = ptList[0];
            int size = ptList.Count;
            int index = 0;
            for (int i = 0; i < size; i++)
            {
                if((lowestPoint.X > ptList[i].X) || (lowestPoint.X == ptList[i].X && lowestPoint.Y > ptList[i].Y))
                {
                    lowestPoint = ptList[i];
                    index = i;
                }
            }
            //return index
            return index;

        }

        //finds and returns the highest position
        internal static int ReturnHighestPointFromListNew(List<Point2d> ptList)
        {
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


        //removes duplicates from a list based on a list
        internal static List<Line2d>RemoveDuplicateLines(List<double> exprList, List<Line2d> lineList)
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


        //removes duplicates lines from a list of lines
        public static List<Line2d> CleanLines(List<Line2d> lineList)
        {
            List<Line2d> cleanList = new List<Line2d>();
            List<bool> taggedList = new List<bool>();
            //set a new copy list of lines and initial tagged list
            for(int i = 0; i < lineList.Count; i++)
            {
                Line2d lin = new Line2d(lineList[i].StartPoint, lineList[i].EndPoint);
                cleanList.Add(lin);
                taggedList.Add(false);
            }

            for(int i = 0; i < lineList.Count; i++)
            {
                double eps = 1;
                Line2d lineA = lineList[i];
                for(int j = i + 1; j < lineList.Count; j++)
                {
                    Line2d lineB = lineList[j];
                    int orientA = CheckLineOrient(lineA);
                    int orientB = CheckLineOrient(lineB);
                    if(orientA != orientB)
                    {
                        
                        continue;
                    }
                    else
                    {
                        Point2d midA = lineA.midPt();
                        Point2d midB = lineB.midPt();
                        if (orientA == 0)
                        {
                            //lines are horizontal                           
                            if((midA.Y - eps < midB.Y && midB.Y < midA.Y + eps) || 
                                (midB.Y - eps < midA.Y && midA.Y < midB.Y + eps))
                            {
                                // lines are duplicate check length, whichever has longer length will be added to list
                                double lenA = lineA.Length;
                                double lenB = lineB.Length;
                                if(lenA > lenB)
                                {
                                    taggedList[i] = true;
                                    //cleanList.Add(lineA);
                                }
                                else
                                {
                                    taggedList[j] = true;
                                    //cleanList.Add(lineB);
                                }

                            }// end of if statement
                        }
                        else
                        {
                            //lines are vertical
                            if ((midA.X - eps < midB.X && midB.X < midA.X + eps) ||
                               (midB.X - eps < midA.X && midA.X < midB.X + eps))
                            {
                                // lines are duplicate check length, whichever has longer length will be added to list
                                double lenA = lineA.Length;
                                double lenB = lineB.Length;
                                if (lenA > lenB)
                                {
                                    cleanList.Add(lineA);
                                }
                                else
                                {
                                    cleanList.Add(lineB);
                                }

                            }// end of if statement

                        }
                    }
                }
            }
            // check of the line is horizontal or vertical
            return cleanList;
        }

        internal static List<Point2d> PointUniqueChecker(List<Point2d> testList)
        {
            List<Point2d> cleanList = new List<Point2d>();
            List<double> Xlist = new List<double>();
            List<double> Ylist = new List<double>();
            for (int i = 0; i < testList.Count; i++)
            {
                Point2d pt = testList[i];
                Xlist.Add(pt.X);
                Ylist.Add(pt.Y);
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
            if (duplicateIndexesX.Count() == 0 || duplicateIndexesY.Count() == 0)
            {
                Trace.WriteLine(" +++++++ No Duplicate Found ++++++++ ");
                return testList;
            }

            List<int> dupX = (List<int>)duplicateIndexesX;
            List<int> dupY = (List<int>)duplicateIndexesX;
            List<int> commonIndex = new List<int>();
            for (int i = 0; i < dupX.Count; i++)
            {
                for (int j = 0; j < dupY.Count; j++)
                {
                    if(dupX[i] == dupY[j])
                    {
                        commonIndex.Add(dupX[i]);
                    }

                }

            }

            for (int i=0; i< testList.Count; i++)
            {
                for(int j = 0; j < commonIndex.Count; j++)
                {
                    if (i != commonIndex[j])
                    {
                        cleanList.Add(testList[i]);
                        break;
                    }
                    Trace.WriteLine(" +++++++ Alert Duplicate Found ++++++++ " + commonIndex[j]);
                }
            }
            
            return cleanList;
        }

 

        //find lowest position
        internal static void FindLowestPointFromList(ref List<Point2d> ptList, int size)
        {
            Point2d lowestPoint = ptList[0];
            for (int i = 0; i < size; i++)
            {
                if (lowestPoint.Y > ptList[0].Y || (lowestPoint.Y == ptList[i].Y && lowestPoint.X > ptList[i].X))
                {
                    lowestPoint = ptList[i];
                }
            }
            int rootPosition = ElementPosition(ptList, size, lowestPoint);
            if(rootPosition != -1)
            {
                    ChangePlaces(ref ptList, 0, rootPosition);
            }
          
        }

        //returns angle between two vectors
        //input two vectors u and v
        //for 'returndegrees' enter true for an answer in degrees, false for radians
        internal static double AngleBetween(Vector2d u, Vector2d v, bool returndegrees)
        {
            double toppart = u.X * v.X + u.Y * v.Y;

            double u2 = u.X * u.X + u.Y * u.Y; //u squared
            double v2 = v.X * v.X + v.Y * v.Y; //v squared
            double bottompart = 0;
            bottompart = Math.Sqrt(u2 * v2);
            double rtnval = Math.Acos(toppart / bottompart);
            if (returndegrees) rtnval *= 360.0 / (2 * Math.PI);
            return rtnval;
        }




        //sort point array for Grahams scan algo to find convex hull
        internal static void SortedPoint2dList(ref List<Point2d> ptList, int size)
        {
           
            for (int i = 1; i < size; ++i)
            {
                for (int y = i + 1; y < size; ++y)
                {
                    Point2d ptA = ptList[i];
                    Point2d ptB = ptList[y];
                    int order = CheckPointOrder(ptList[0], ptA, ptB);
                    // collinear
                    if (order == 0)
                    {
                        if (DistanceBetweenPoints(ptList[0], ptA) <= DistanceBetweenPoints(ptList[0], ptB))
                        {
                            ChangePlaces(ref ptList, i, y);

                        }
                    }
                    else if (order == 1)
                    {
                        ChangePlaces(ref ptList, i, y);
                    }
                    }

                }
            }

                 
        

        //get next to top point on Stack
        internal static Point2d BeforeTopPoint(ref Stack myStack)
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

        // find the closest point to a point from a point list
        internal static int FindClosestPointIndex(List<Cell> cellList, Point2d pt)
        {
            int index = 0;
            double minDist = 100000000;
            for (int i=0; i< cellList.Count; i++)
            {
                if (cellList[i].CellAvailable)
                {
                Point2d centerPt = cellList[i].CenterPoint;
                double calcDist = DistanceBetweenPoints(centerPt, pt);
                if (calcDist < minDist)
                {
                    minDist = calcDist;
                    index = i;
                    //Trace.WriteLine("For Cells, Index is : " + index);
                }

                }
            }
            return index;
        }

        // find the closest point to a point from a point list
        internal static int FindClosestPointIndex(List<Point2d> ptList, Point2d pt)
        {
            int index = 0;
            double minDist = 100000000;
            //Trace.WriteLine(" Total Points in the poly line is  : " + ptList.Count);
            for (int i = 0; i < ptList.Count; i++)
            {
                Point2d centerPt = ptList[i];
                double calcDist = DistanceBetweenPoints(centerPt, pt);
                if (calcDist < minDist)
                {
                    minDist = calcDist;
                    index = i;
                    //Trace.WriteLine(" For Points, Index is : " + index);
                }
            }
            return index;
        }


        //distance between points - correct one
        internal static double DistanceBetweenPoints(Point2d ptA, Point2d ptB)
        {
            if (ptA == null || ptB == null) return 0;
            double xLen = ptA.X - ptB.X;
            double yLen = ptA.Y - ptB.Y;

            return xLen * xLen + yLen * yLen;
        }

        internal static List<Point2d> AddPointsInBetween(List<Point2d> pointList, int number = 3)
        {
            List<Point2d> ptList = new List<Point2d>();

            for(int i=0;i< pointList.Count; i++)
            {

                Point2d ptA = pointList[i];
                Point2d ptB = null;
                if (i == pointList.Count - 1)
                {
                     ptB = pointList[0];
                }
                else
                {
                     ptB = pointList[i + 1];
                }
                
                Vector2d vec = new Vector2d(ptA, ptB);
                double dist = vec.Length;
                //Trace.WriteLine("Distance is : " + dist);
                double increment = dist / number;
                ptList.Add(pointList[i]);
                
                for(int j = 0; j < number-1; j++)
                {
                    double value = (j+1)*increment / dist;
                    //Trace.WriteLine("Value is : " + value);

                    //p = (1 - t) * p1 + t * p2
                    double x = ((1 - value) * ptA.X) + (value * ptB.X);
                    double y = ((1 - value) * ptA.Y) + (value * ptB.Y);
                    //Point2d ptAdd = Point2d.AddVector(ptA, vec, value);
                    Point2d ptAdd = new Point2d(x, y);
                    ptList.Add(ptAdd);
                }
                

            }
            return ptList;
        }


       


        // COMPUTES AREA OF A CLOSED POLYGON ( SET OF POINTS )  IF AREA IS POS , POLY VERTICES ARE COUNTER CLOCKWISE AND VICE VERSA
        internal static double AreaPolygon2d(List<Point2d> polyPoints)
        {
            if(polyPoints == null)
            {
                return 0;
            }
            /*
            double area = 0;
            for(int i = 0; i < polyPoints.Count - 1; i++)
            {
                int j = (i + 1) % polyPoints.Count;
                area += polyPoints[i].X * polyPoints[j].Y;
                area -= polyPoints[i].Y + polyPoints[j].X;
            }
            return area/2;
            */
            double area = 0;
            int j = polyPoints.Count - 1;  // The last vertex is the 'previous' one to the first

            for (int i = 0; i < polyPoints.Count; i++)
            {
                area += (polyPoints[j].X + polyPoints[i].X) * (polyPoints[j].Y - polyPoints[i].Y);
                j = i;  //j is previous vertex to i

            }
                return Math.Abs(area / 2);
        }


        public static List<Point2d> SortPointsByDistanceFromPoint(List<Point2d> ptList, Point2d testPoint)
        {
            List<Point2d> sortedPtList = new List<Point2d>();
            List<double> distanceList = new List<double>();
            List<int> indexList = new List<int>();

            for (int i = 0; i < ptList.Count; i++)
            {
                double dist = DistanceBetweenPoints(ptList[i], testPoint);
                distanceList.Add(dist);
            }

            indexList = BasicUtility.SortIndex(distanceList);
           
            for(int i = 0; i < indexList.Count; i++)
            {
                sortedPtList.Add(ptList[indexList[i]]);
                //Trace.WriteLine("Indices are :" + indexList[i]);
            }
            //Trace.WriteLine("++++++++++++++++++++++++++++++++++++++++++++++++++++++");
            return sortedPtList;
        }


        // find the centroid of group of  cells
        internal static Point2d CentroidInCells(List<Cell> cellList)
        {
            double x = 0, y = 0;
            for(int i = 0; i < cellList.Count; i++)
            {
                x += cellList[i].CenterPoint.X;
                y += cellList[i].CenterPoint.Y;
            }
            x = x / cellList.Count;
            y = y / cellList.Count;
            Point2d centroid = new Point2d(x, y);
            return centroid;

        }

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

    
        //add a point with a vector
        internal static Point2d PointAddVector2D(Point2d pt, Vector2d vec)
        {
            Point2d ptNew = new Point2d(pt.X+vec.X, pt.Y+vec.Y);
            return ptNew;
        }


        //MAKE CELLS ON THE GRIDS
        public static Polygon MakeSquareFromCenterSide(Point2d centerPt,double side)
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

                Polygon pol = Polygon.ByPoints(ptList);
            return pol;
        }


        //MAKE CELLS ON THE GRIDS
        public static Polygon2d MakeSquarePolygonFromCenterSide(Point2d centerPt, double side)
        {

            List<Point2d> ptList = new List<Point2d>();

            double a = centerPt.X - (side / 2);
            double b = centerPt.Y - (side / 2);
            Point2d pt = new Point2d(a, b);
            ptList.Add(pt);

            a = centerPt.X - (side / 2);
            b = centerPt.Y + (side / 2);
            pt = new Point2d(a, b);
            ptList.Add(pt);

            a = centerPt.X + (side / 2);
            b = centerPt.Y + (side / 2);
            pt = new Point2d(a, b);
            ptList.Add(pt);

            a = centerPt.X + (side / 2);
            b = centerPt.Y - (side / 2);
            pt = new Point2d(a, b);
            ptList.Add(pt);

            Polygon2d pol = Polygon2d.ByPoints(ptList);
            
            return pol;
        }



        //QUICKSORT IMPLEMENTATION
        public static void Quicksort(ref IComparable[] elements, int left, int right)
        {
            int i = left, j = right;
            IComparable pivot = elements[(left + right) / 2];

            while (i <= j)
            {
                while (elements[i].CompareTo(pivot) < 0)
                {
                    i++;
                }

                while (elements[j].CompareTo(pivot) > 0)
                {
                    j--;
                }

                if (i <= j)
                {
                    // Swap
                    IComparable tmp = elements[i];
                    elements[i] = elements[j];
                    elements[j] = tmp;

                    i++;
                    j--;
                }
            }

            // Recursive calls
            if (left < j)
            {
                Quicksort(ref elements, left, j);
            }

            if (i < right)
            {
                Quicksort(ref elements, i, right);
            }
        }

        //check to see if a test point is towards the left or right of the point
        //if positive then the point is towards the left of the point
        public static bool CheckPointSide(Line2d lineSegment, Point2d c)
        {
            Point2d a = lineSegment.StartPoint;
            Point2d b = lineSegment.EndPoint;

            return ((b.X - a.X) * (c.Y - a.Y) - (b.Y - a.Y) * (c.X - a.X)) > 0;
        }




        // finds the angle between two points
        internal static double Angle(Point2d A, Point2d center)
        {
            double angle = Math.Acos((A.X - center.X) / DistanceBetweenPoints(center, A));
            if (A.Y < center.Y)
            {
                angle = Math.PI + Math.PI - angle;//360-angle
            }
            return angle; //return angle*180/Math.PI;
        }




        // sorts two points clockwise, with respect to a reference point
        internal static int SortCornersClockwise(Point2d A, Point2d B)
        {
            //  Variables to Store the atans
            double aTanA, aTanB;
            

            //  Fetch the atans
            aTanA = Math.Atan2(A.Y - SplitObject.reference.Y, A.X - SplitObject.reference.X);
            aTanB = Math.Atan2(B.Y - SplitObject.reference.Y, B.X - SplitObject.reference.X);

            //  Determine next point in Clockwise rotation
            if (aTanA < aTanB) return -1;
            else if (aTanA > aTanB) return 1;
            return 0;
        }







        // to be discarded
        //SORT A GROUP OF POINTS TO FORM A POLYLINE ( NO SELF INTERSECTIONS)
        public static List<Point2d> SortPoints(List<Point2d> pointList)
        {
            //center point
            Point2d cen = PolygonUtility.CentroidFromPoly(pointList);
            Vector2d vecX = new Vector2d(0, 100);
            //double[] angList = new double[pointList.Count];
            List<double> angList = new List<double>();
            int[] indList = new int[pointList.Count];
            for (int i = 0; i < pointList.Count; i++)
            {
                //Vector2d vecX = new Vector2d(0, 0);

                Vector2d CenterToPt = new Vector2d(cen, pointList[i]);
                double dotValue = vecX.Dot(CenterToPt);
                //double angValue = dotValue / (vecX.Length * CenterToPt.Length);
                double angValue = Math.Atan2(CenterToPt.Y - vecX.Y, CenterToPt.X - vecX.X);
                //angList[i] = angValue;
                angList.Add(angValue);
                indList[i] = i;
            }

            List<int> newIndexList = new List<int>();
            //newIndexList = BasicUtility.quicksort(angList, indList, 0, pointList.Count);
            newIndexList = BasicUtility.SortIndex(angList);
            List<Point2d> sortedPointList = new List<Point2d>();
            for (int i = 0; i < pointList.Count; i++)
            {
                sortedPointList.Add(pointList[newIndexList[i]]);
            }

            return sortedPointList;
        }

        // can be discarded
        internal static bool AreLinesCollinear(Line2d lineA, Line2d lineB, double threshold = 0)
        {

            Vector2d a = new Vector2d(lineA.StartPoint, lineA.EndPoint);
            Vector2d b = new Vector2d(lineB.StartPoint, lineB.EndPoint);
            double dotProduct = a.X * b.X + a.Y * b.Y;
            double magA = Math.Sqrt(a.X * a.X + a.Y * a.Y); //sub your own sqrt
            double magB = Math.Sqrt(b.X * b.X + b.Y * b.Y); //sub your own sqrt

            double angle = Math.Acos(dotProduct / (magA * magB)); //sub your own arc-cosine

            if (angle <= threshold)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        // Given three colinear points p, q, r, the function checks if
        // point q lies on line segment 'pr'
        internal static bool onSegment(Point2d p, Point2d q, Point2d r)
        {
            if (q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) &&
                q.Y <= Math.Max(p.Y, r.Y) && q.Y >= Math.Min(p.Y, r.Y))
                return true;

            return false;
        }

        // checks collinearity and order of three points
        internal static int orientation(Point2d p, Point2d q, Point2d r)
        {
           
            double val = (q.Y - p.Y) * (r.X - q.X) -
                      (q.X - p.X) * (r.Y - q.Y);

            if (val == 0) return 0;  // colinear

            return (val > 0) ? 1 : 2; // clock or counterclock wise
        }



        //GIVEN THREE POINTS CHECKS IF POINT Q IS INSIDE THE LINE PR
        // Given three colinear points p, q, r, the function checks if
        // point q lies on line segment 'pr'
        public static bool onSegment(Line2d givenLine, Point2d q, double eps = 0)
        {
            if(givenLine == null || q == null) return false; 
            Point2d p = givenLine.StartPoint, r = givenLine.EndPoint;
            if (q.X <= (Math.Max(p.X, r.X) + eps) && q.X >= (Math.Min(p.X, r.X) - eps) &&
                q.Y <= (Math.Max(p.Y, r.Y) + eps) && q.Y >= (Math.Min(p.Y, r.Y))- eps) return true;
            return false;
        }


        //checks if two lines are collinear or not , works good
        public static bool LineAdjacencyCheck(Line2d lineA, Line2d lineB, double eps = 0)
        {
            Point2d pA = lineA.StartPoint, qA = lineA.EndPoint;
            Point2d pB = lineB.StartPoint, qB = lineB.EndPoint;

            Vector2d vecA = new Vector2d(pA, qA), vecB = new Vector2d(pB, qB);
            double crossMag = vecA.Cross(vecB);
            if (crossMag != 0) return false;

            bool checkA1 = onSegment(lineB, pA, eps), checkA2 = onSegment(lineB, qA, eps);
            bool checkB1 = onSegment(lineA, pB, eps), checkB2 = onSegment(lineA, qB, eps);

            if (checkA1 || checkA2) return true;
            if (checkB1 || checkB2) return true;
            return false;
        }


        // WORKS SOMEWHAT - has bugs
        public static bool CheckLineCollinear(Line2d lineA, Line2d lineB)
        {

            // The main function that returns true if line segment 'p1q1'
            // and 'p2q2' intersect.


            Point2d p1 = lineA.StartPoint;
            Point2d p2 = lineA.EndPoint;
            Point2d q1 = lineB.StartPoint;
            Point2d q2 = lineB.EndPoint;
           
            // Find the four orientations needed for general and
            // special cases
            int o1 = orientation(p1, q1, p2);
            int o2 = orientation(p1, q1, q2);
            int o3 = orientation(p2, q2, p1);
            int o4 = orientation(p2, q2, q1);

            // General case
            if (o1 != o2 && o3 != o4)
                    return false;

            // Special Cases
            // p1, q1 and p2 are colinear and p2 lies on segment p1q1
            if (o1 == 0 && onSegment(p1, p2, q1)) return true;

            // p1, q1 and p2 are colinear and q2 lies on segment p1q1
            if (o2 == 0 && onSegment(p1, q2, q1)) return true;

            // p2, q2 and p1 are colinear and p1 lies on segment p2q2
            if (o3 == 0 && onSegment(p2, p1, q2)) return true;

            // p2, q2 and q1 are colinear and q1 lies on segment p2q2
            if (o4 == 0 && onSegment(p2, q1, q2)) return true;

            return false; // Doesn't fall in any of the above cases
}
            
 

    

        //FIND PERP PROJECTION ON A LINE FROM A POINT
        public static Point2d PerpProjectionPointOnLine(Line line, Point2d C)
        {

            Line2d testLine = new Line2d(Point2d.ByCoordinates(line.StartPoint.X,line.StartPoint.Y), Point2d.ByCoordinates(line.EndPoint.X, line.EndPoint.Y));

            /*Point2d A = testLine.StartPoint;
            Point2d B = testLine.EndPoint;
            double x1 = A.X, 
                   y1 = A.Y, 
                   x2 = B.X, 
                   y2 = B.Y,
                   x3 = C.X,
                   y3 = C.Y;

            double px = x2 - x1, 
                   py = y2 - y1, 
                   dAB = px * px + py * py;

            double u = ((x3 - x1) * px + (y3 - y1) * py) / dAB;
            double x = x1 + (u * px), y = y1 + (u * py);
            return new Point2d(x, y);
            */
            Point2d A = testLine.StartPoint;
            Point2d B = testLine.EndPoint;
            Vector2d b = new Vector2d(A, B);
            Vector2d a = new Vector2d(A, C);

            double a1 = a.Dot(b) / b.Length;

            Vector2d bScaled = b.Scale(a1);

            Point2d finpt = new Point2d(A.X + bScaled.X, A.Y + bScaled.Y);
            return finpt;

        }





        // CORRECT IMPLEMENTATION OF CLOSEST PT TO LINE
        public static Point2d ProjectedPointOnLine(Line2d testline, Point2d P)
        {

            Line2d line = new Line2d(Point2d.ByCoordinates(testline.StartPoint.X, testline.StartPoint.Y), Point2d.ByCoordinates(testline.EndPoint.X, testline.EndPoint.Y));

            Point2d A = line.StartPoint;
            Point2d B = line.EndPoint;


            Vector2d vVector1 = new Vector2d(A, P);
            Vector2d vVector2 = new Vector2d(A, B);
            Vector2d vVector2N = vVector2.Normalize();
         

            double d = line.Length;
            double t = vVector2N.Dot(vVector1);
            /*
            if (t <= 0)
                //return A;

            if (t >= d)
                    //return B;
            */
            Vector2d vVector3 = vVector2N.Scale(t);
            return new Point2d(A.X+vVector3.X, A.Y+vVector3.Y);
        }




        //FINDS VERTICAL OR HORIZONTAL COMPONENT LENGTH DEPENDING ON REQUIREMENT
        internal static double OrthogonalDistance(Point2d p1, Point2d p2)
        {
            double eps = 1000;
            double extend = 100000;
            double dist = 0;
            Vector2d vecLine = new Vector2d(p1, p2);
            Vector2d vecX = new Vector2d(p1, Point2d.ByCoordinates(p1.X + eps, 0));
            Vector2d vecY = new Vector2d(p1, Point2d.ByCoordinates(0, p1.X + eps));

            double dotX = vecLine.Dot(vecX);
            double dotY = vecLine.Dot(vecY);

            if (dotX == 0)
            {
                //line is vertical
             

            }
            else if (dotY == 0)
            {
                //line is horizontal
                

            }
            return dist;
        }


        //FINDS IF LINE IS VERTICAL OR HORIZONTAL
        internal static bool IsLineOrthogonal(Line2d line)
        {
            double eps = 1000;
            double extend = 100000;
            double dist = 0;
            bool check = false;
            Point2d p1 = line.StartPoint;
            Point2d p2 = line.EndPoint;
            Vector2d vecLine = new Vector2d(p1,p2);
            Vector2d vecX = new Vector2d(p1, Point2d.ByCoordinates(p1.X + eps, 0));
            Vector2d vecY = new Vector2d(p1, Point2d.ByCoordinates(0, p1.X + eps));

            double dotX = vecLine.Dot(vecX);
            double dotY = vecLine.Dot(vecY);

            if (dotX == 0 )
            {
                //line is vertical
                check = true;

            }
            if (dotY == 0)
            {
                //line is horizontal
                check = true;


            }
            return check;
        }

        //FINDS IF LINE IS VERTICAL OR HORIZONTAL
        internal static bool IsLineOrthogonalCheck(Line2d line)
        {
            double eps = 1000;
            double extend = 100000;
            double dist = 0;
            bool check = false;
            Point2d p1 = line.StartPoint;
            Point2d p2 = line.EndPoint;


            double xDiff = p1.X - p2.X;
            double yDiff = p1.Y - p2.Y;
            if(xDiff == 0 || yDiff == 0)
            {
                check = true;
            }


            return check;
        }

        /////// - using now
        //IMPLEMENTS LINE AND LINE INTERSECTION
        public static Point2d LineLineIntersectionNew(Line2d s1, Line2d s2)
        {
            Point2d startS1 = s1.StartPoint;
            Point2d endS1 = s1.EndPoint;
            Point2d startS2 = s2.StartPoint;
            Point2d endS2 = s2.EndPoint;

            //make line equations

            double As1, Bs1, Cs1;
            double As2, Bs2, Cs2;

            As1 = endS1.Y - startS1.Y;
            Bs1 = startS1.X - endS1.X;
            Cs1 = As1 * startS1.X + Bs1 * startS1.Y;

            As2 = endS2.Y - startS2.Y;
            Bs2 = startS2.X - endS2.X;
            Cs2 = As1 * startS2.X + Bs2 * startS2.Y;

            double det = As1 * Bs2 - As2 * Bs1;
            if (det == 0)
            {
                return null;
            }
            else
            {
                double x = (Bs2 * Cs1 - Bs1 * Cs2) / det;
                double y = (As1 * Cs2 - As2 * Cs1) / det;
                return new Point2d(x, y);
            }


        }



        //IMPLEMENTS LINE AND LINE INTERSECTION
        public static Point2d LineLineIntersection(Line2d s1, Line2d s2)
        {

            Point2d startS1 = s1.StartPoint;
            Point2d endS1 = s1.EndPoint;
            Point2d startS2 = s2.StartPoint;
            Point2d endS2 = s2.EndPoint;
            double s1x, s1y, s2x, s2y;

            s1x = endS1.X - startS1.X;
            s1y = endS1.Y - startS1.Y;
            s2x = endS2.X - startS2.X;
            s2y = endS2.Y - startS2.Y;

            double s, t;
            s = (-s1y * (startS1.X - endS1.X) + s1x * (startS1.Y - endS1.Y)) / (-s2x * s1y + s1x * s2y);
            t = (s2x * (startS1.Y - startS2.Y) - s2y * (startS1.X - startS2.X)) / (-s2x * s1y + s1x * s2y);

            double intersectX = 0, intersectY = 0;
            if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
            {
                // Collision detected
                intersectX = startS1.X + (t * s1x);
                intersectY = startS1.Y + (t * s1y);
                return new Point2d(intersectX, intersectY);
            }
            else
            {
                return null; // no intersection
            }


        }


        //CLOSEST POINT TO A LINE FROM A POINT LIST
        public static Point2d ClosestPointToLine(List<Point2d> pt, Line2d line)
        {
            // Get coefficients of the implicit line equation.
            // Do NOT normalize since scaling by a constant
            // is irrelevant for just comparing distances.
            double a = line.StartPoint.Y - line.EndPoint.Y;
            double b = line.EndPoint.X - line.StartPoint.X;
            double c = line.StartPoint.X * line.EndPoint.Y - line.EndPoint.X * line.StartPoint.Y;
            // initialize min index and distance to P[0]
            int index = 0;
            double min = a * pt[0].X + b * pt[0].Y + c;
            if (min < 0) min = -min;     // absolute value


            // loop through Point array testing for min distance to L
            for (int i = 1; i < pt.Count; i++)
            {
                // just use dist squared (sqrt not  needed for comparison)
                double dist = a * pt[i].X + b * pt[i].Y + c;
                if (dist < 0) dist = -dist;    // absolute value
                if (dist < min)
                {      // this point is closer
                    index = i;              // so have a new minimum
                    min = dist;
                }
            }

                return pt[index];
        } 

        // IMPLEMENTS LINE AND POLYGON INTERSECTION
        public static List<Point2d> LinePolygonIntersection(List<Point2d> poly, Line2d testLine)
        {
            List<Point2d> ptList = new List<Point2d>();
            for (int i = 0; i < poly.Count - 1; i++)
            {
                Point2d pt1 = poly[i];
                Point2d pt2 = poly[i + 1];
                Line2d edge = new Line2d(pt1, pt2);

                if (LineLineIntersection(edge, testLine) != null)
                {
                    ptList.Add(LineLineIntersection(edge, testLine));
                }

            }
            return ptList;
        }


        //CHECKS IF TWO LINES ARE DUPLICATE
        public static bool IsLineDuplicate(Line2d A, Line2d B)
        {
            bool check = false;
            double eps = 0.1;
            double mA = (A.EndPoint.Y - A.StartPoint.Y) / (A.EndPoint.X - A.StartPoint.X);
            double mB = (B.EndPoint.Y - B.StartPoint.Y) / (B.EndPoint.X - B.StartPoint.X);
            if((mB-eps < mA && mA < mB + eps) || (mA - eps < mB && mB < mA + eps))
            {
                double intercA = A.StartPoint.Y - mA * A.StartPoint.X;
                double intercB = B.StartPoint.Y - mB * B.StartPoint.X;

                if ((intercA-eps < intercA && intercA < intercA + eps) || (intercB - eps < intercB && intercB < intercB + eps))
                {
                    check = true;
                }
                else
                {
                    check = false;
                }
            }

            return check;
        }
        
        //CHECKS IF TWO LINES ARE DUPLICATE
        public static List<double> LineSlopeIntercept(List<Line2d> lineList)
        {
            double eps = 0.1;
            double u = 0.23;
            double v = 0.16;
            List<double> slopeInterceptList = new List<double>();
            for(int i = 0; i < lineList.Count; i++)
            {
                Line2d A = lineList[i];
                double N = (A.EndPoint.Y - A.StartPoint.Y);
                double D = (A.EndPoint.X - A.StartPoint.X);
                if (D == 0)
                    D = A.EndPoint.X;
                double mA = N / D;
                double intercA = A.StartPoint.Y - (mA * A.StartPoint.X);
                double val = Math.Round(u * mA + v * intercA);
                //double val = mA + intercA
                slopeInterceptList.Add(Math.Abs(mA));
                //Trace.WriteLine("Value is : " + val);
                //Trace.WriteLine("Slope is : " + Math.Round(mA));
                //Trace.WriteLine("Intercept is : " + Math.Round(intercA));
            }
     
            return slopeInterceptList;
        }


        //FLATTEN LIST OF LINE2D
        internal static List<Line2d> FlattenLine2dList(List<List<Line2d>> lineList)
        {
            //flatten the list
            List<Line2d> flatLineList = new List<Line2d>();
            for (int i = 0; i < lineList.Count; i++)
            {
                if (lineList[i] != null)
                {
                    if (lineList[i].Count > 0)
                    {
                        flatLineList.AddRange(lineList[i]);
                    }
                }

            }

            return flatLineList;
        }

        //LINE AND POLY INTERSECTION TEST AND RETURNS THE LINE INTERSECTED IN THE POLY
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
                    Trace.WriteLine("CheckDist is : " + checkDist + " . Dist is :  " + dist);

                    try
                    {
                        sortedIntersectionLines.Add(checkDist, edge);
                    }
                    catch (Exception)
                    {
                        
                        double eps = ran.NextDouble() * 2;
                        double newDist = checkDist - eps;
                        sortedIntersectionLines.Add(newDist, edge);
                        //throw;
                    }
                    intersectedLineInPoly = edge;
                    count += 1;
                    Trace.WriteLine(" Instersection : " + count + " . Dist is :  " + dist);
                    //Trace.WriteLine(" Good ! Intersections found : ");
                    }
                else
                {
                    //intersectedLineInPoly = null;
                    Trace.WriteLine("No Intersections found : ");
                }

            }

            Trace.WriteLine("Sorted Dictionary length is : " + sortedIntersectionLines.Count);
            if (sortedIntersectionLines.Count > 0)
            {
                
                foreach (KeyValuePair<double, Line2d> p in sortedIntersectionLines)
                {
                    intersectedLineInPoly = p.Value;
                    break;
                }
            }
            else
            {
                intersectedLineInPoly = null;
            }
            

            Trace.WriteLine("++++++++++++++++++++++++++++++++++++++++++");
            return intersectedLineInPoly;
        }



        //split by line function using it now
        internal static List<Point2d> LinePolygonIntersectionInd(List<Point2d> poly, Line2d testLine)
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


                for (int i = 0; i < n-1; i++)   // process polygon edge V[i]V[i+1]
                {
                    e = new Vector2d(poly[i+1],poly[i]);
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
                Point2d p1  = testLine.StartPoint + dS.Scale(tE);   // = P(tE) = point where S enters polygon
                Point2d p2 = testLine.StartPoint + dS.Scale(tL);   // = P(tL) = point where S leaves polygon

            List<Point2d> ptList = new List<Point2d>();
            ptList.Add(p1);
            ptList.Add(p2);
            return ptList;
}


        

        //GENERATE POINTS ON A CIRCLE AND THEN RANDOMIZE THEM
        public static List<Point2d> PointGenerator(int tag, int size)
        {
            List<Point2d> pointGen = new List<Point2d>();
            Random randomX = new Random();
            Random randomY = new Random();
            double max = 3000;
            double min = 1500;
            double rad = 1000;
            double t = 0;
            for (int i = 0; i < size; i++)
            {

                //double x = randomX.NextDouble() * (max - min) + min;
                //double y = randomY.NextDouble() * (max - min) + min;
                //


                double x = rad * Math.Cos(t);
                double y = rad * Math.Sin(t);
                t += 15;
                Point2d pt = new Point2d(x, y);
                pointGen.Add(pt);
            }

            var rnd = new Random();
            var newSequence = pointGen.OrderBy(item => rnd.Next());

            List<Point2d> finalPts = newSequence.ToList();

            return finalPts;
        }


    }






}

