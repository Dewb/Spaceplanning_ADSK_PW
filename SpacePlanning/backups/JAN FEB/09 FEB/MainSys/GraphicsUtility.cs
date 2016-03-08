
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
        internal static bool _pointInsidePolygonTest(List<Point2d> pointsPolygon, Point2d testPoint)
        {
            bool check = false;
            int numPolyPts = pointsPolygon.Count;

            for(int i = 0, j = numPolyPts - 1; i < numPolyPts; j = i++)
            {
                if (((pointsPolygon[i].Y > testPoint.Y) != (pointsPolygon[j].Y > testPoint.Y)) &&
                (testPoint.X < (pointsPolygon[j].X - pointsPolygon[i].X) * (testPoint.Y - pointsPolygon[i].Y) / (pointsPolygon[j].Y - pointsPolygon[i].Y) + pointsPolygon[i].X))
                {

                    //if (((verty[i] > testy) != (verty[j] > testy)) &&
                    //(testx < (vertx[j] - vertx[i]) * (testy - verty[i]) / (verty[j] - verty[i]) + vertx[i]))

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



        //sort point array
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

                 
        

        /*
        // get the next to top point on stack
        Point* beforeTopPoint(stack<Point *> &stackPointer)
        {
            Point *p = stackPointer.top();

            // if there is only one Point on stack, return this point
            if (stackPointer.size() > 1)
            { 
                stackPointer.pop();
                Point *result = stackPointer.top();
                stackPointer.push(p);
                return result;
            }

            return p;
            }
        */


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


    }
}
