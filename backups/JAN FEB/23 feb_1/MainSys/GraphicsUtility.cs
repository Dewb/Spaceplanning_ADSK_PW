
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
        // private variables

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

        //finds and returns the lowest position
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

            return index;           

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


     

        //distance between points
        internal static double DistanceBetweenPoints(Point2d ptA, Point2d ptB)
        {
            double xLen = ptA.X - ptB.X;
            double yLen = ptA.Y - ptB.Y;

            return xLen * xLen + yLen * yLen;
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
                double calcDist = DistanceBetweenPoint2d(centerPt, pt);
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
            Trace.WriteLine(" Total Points in the poly line is  : " + ptList.Count);
            for (int i = 0; i < ptList.Count; i++)
            {
                Point2d centerPt = ptList[i];
                double calcDist = DistanceBetweenPoint2d(centerPt, pt);
                if (calcDist < minDist)
                {
                    minDist = calcDist;
                    index = i;
                    Trace.WriteLine(" For Points, Index is : " + index);
                }
            }
            return index;
        }

        internal static double DistanceBetweenPoint2d(Point2d ptA, Point2d ptB)
        {
            double distance = ptA.X * ptB.X + ptA.Y * ptB.Y;
            return distance;
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

        // find the centroid of group of  cells
        internal static Point2d CentroidInPointLists(List<Point2d> ptList)
        {
            double x = 0, y = 0;
            for (int i = 0; i < ptList.Count; i++)
            {
                x += ptList[i].X;
                y += ptList[i].Y;
            }
            x = x / ptList.Count;
            y = y / ptList.Count;
            Point2d centroid = new Point2d(x, y);
            return centroid;

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

    }






}

