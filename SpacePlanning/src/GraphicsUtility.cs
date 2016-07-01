
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.Threading.Tasks;
using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;
using System.IO;
using stuffer;
using System.Collections;
using System.Diagnostics;



namespace SpacePlanning
{
    internal class GraphicsUtility
    {
        #region - Public Methods
        //checks if a point is inside a polygon or not
        public static bool PointInsidePolygonTest(Polygon2d poly, Point2d testPoint)
        {
            if (!ValidateObject.CheckPoly(poly) || testPoint == null) return false;
            bool check = false;
            List<Point2d> pointsPolygon = poly.Points;
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

        //joins two collinear lines to make one line
        public static Line2d JoinCollinearLines(Line2d lineA, Line2d lineB)
        {
            List<Point2d> allPoints = new List<Point2d>();
            allPoints.Add(lineA.StartPoint);
            allPoints.Add(lineA.EndPoint);
            allPoints.Add(lineB.StartPoint);
            allPoints.Add(lineB.EndPoint);
            int p = PointUtility.LowestPointFromList(allPoints);
            int q = PointUtility.HighestPointFromList(allPoints);
            Line2d lineJoined = new Line2d(allPoints[p], allPoints[q]);
            return lineJoined;
        }

        //checks if two lines are collinear or not 
        public static bool LineAdjacencyCheck(Line2d lineA, Line2d lineB, double eps = 0)
        {
            Point2d pA = lineA.StartPoint, qA = lineA.EndPoint;
            Point2d pB = lineB.StartPoint, qB = lineB.EndPoint;

            Vector2d vecA = new Vector2d(pA, qA), vecB = new Vector2d(pB, qB);
            double crossMag = vecA.Cross(vecB);
            if (crossMag != 0) return false;

            bool checkA1 = ValidateObject.CheckOnSegment(lineB, pA), checkA2 = ValidateObject.CheckOnSegment(lineB, qA);
            bool checkB1 = ValidateObject.CheckOnSegment(lineA, pB), checkB2 = ValidateObject.CheckOnSegment(lineA, qB);

            if (checkA1 || checkA2) return true;
            if (checkB1 || checkB2) return true;
            return false;
        }

        // find closest point to a line
        public static Point2d ProjectedPointOnLine(Line2d testline, Point2d P)
        {
            Line2d line = new Line2d(Point2d.ByCoordinates(testline.StartPoint.X, testline.StartPoint.Y),
            Point2d.ByCoordinates(testline.EndPoint.X, testline.EndPoint.Y));
            Point2d A = line.StartPoint;
            Point2d B = line.EndPoint;
            Vector2d vVector1 = new Vector2d(A, P);
            Vector2d vVector2 = new Vector2d(A, B);
            Vector2d vVector2N = vVector2.Normalize();
            double d = line.Length;
            double t = vVector2N.Dot(vVector1);
            Vector2d vVector3 = vVector2N.Scale(t);
            return new Point2d(A.X + vVector3.X, A.Y + vVector3.Y);
        }

        //finds line and line intersection point - using now
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
            double s = (-s1y * (startS1.X - endS1.X) + s1x * (startS1.Y - endS1.Y)) / (-s2x * s1y + s1x * s2y);
            double t = (s2x * (startS1.Y - startS2.Y) - s2y * (startS1.X - startS2.X)) / (-s2x * s1y + s1x * s2y);
            double intersectX = 0, intersectY = 0;
            if (s >= 0 && s <= 1 && t >= 0 && t <= 1)
            {
                // Collision detected
                intersectX = startS1.X + (t * s1x);
                intersectY = startS1.Y + (t * s1y);
                return new Point2d(intersectX, intersectY);
            }
            else return null; // no intersection
        }

        //finds the closest point to a line from a point list
        public static Point2d ClosestPointToLine(List<Point2d> pt, Line2d line)
        {
            double a = line.StartPoint.Y - line.EndPoint.Y;
            double b = line.EndPoint.X - line.StartPoint.X;
            double c = line.StartPoint.X * line.EndPoint.Y - line.EndPoint.X * line.StartPoint.Y;
            int index = 0;
            double min = Math.Abs(a * pt[0].X + b * pt[0].Y + c);
            for (int i = 1; i < pt.Count; i++)
            {
                double dist = Math.Abs(a * pt[i].X + b * pt[i].Y + c);
                if (dist < min)
                {
                    index = i;
                    min = dist;
                }
            }
            return pt[index];
        }

        // returns line and polygon intersection using now
        public static List<Point2d> LinePolygonIntersection(List<Point2d> poly, Line2d testLine)
        {
            List<Point2d> ptList = new List<Point2d>();
            for (int i = 0; i < poly.Count - 1; i++)
            {
                Line2d edge = new Line2d(poly[i], poly[i + 1]);
                if (LineLineIntersection(edge, testLine) != null) ptList.Add(LineLineIntersection(edge, testLine));
            }
            return ptList;
        }

        #endregion


        #region - Private Methods
        // Removes the lines which are on the poly lines
        internal static List<Line2d> RemoveDuplicateslinesWithPoly(Polygon2d poly, List<Line2d> lineList)
        {
            List<Line2d> cleanLineList = new List<Line2d>();
            List<bool> duplicateList = new List<bool>();
            for (int i = 0; i < lineList.Count; i++)
            {
                Line2d line = new Line2d(lineList[i].StartPoint, lineList[i].EndPoint);
                cleanLineList.Add(line);
                duplicateList.Add(false);
            }

            for (int i = 0; i < poly.Points.Count; i++)
            {
                int b = i + 1;
                if (i == poly.Points.Count - 1) b = 0;
                Line2d lineA = new Line2d(poly.Points[i], poly.Points[b]);
                for (int j = 0; j < lineList.Count; j++)
                {
                    Line2d lineB = lineList[j];
                    bool checkAdj = GraphicsUtility.LineAdjacencyCheck(lineA, lineB);
                    if (checkAdj)
                    {
                        duplicateList[j] = true;
                        break;
                    }// end of if loop
                } // end of 2nd for loop


            }// end of 1st for loop

            int count = 0;
            for (int i = 0; i < duplicateList.Count; i++)
            {
                if (duplicateList[i] == true)
                {
                    cleanLineList.RemoveAt(i - count);
                    count += 1;
                }
            }
            return cleanLineList;
        }

        // find the closest point to a point from a group of cells
        internal static int FindClosestPointIndex(List<Cell> cellList, Point2d pt)
        {
            List<Point2d> ptList = new List<Point2d>();
            for (int i = 0; i < cellList.Count; i++)
            {
                if (cellList[i].CellAvailable) ptList.Add(cellList[i].CenterPoint);
            }
            return PolygonUtility.FindClosestPointIndex(ptList, pt);
        }


        // find the centroid of a group of  cells
        internal static Point2d CentroidInCells(List<Cell> cellList)
        {
            List<Point2d> ptList = new List<Point2d>();
            for (int i = 0; i < cellList.Count; i++)
            {
                ptList.Add(cellList[i].CenterPoint);
            }
            return PointUtility.CentroidInPointLists(ptList);
        }

        //add a point with a vector
        internal static Point2d PointAddVector2D(Point2d pt, Vector2d vec)
        {
            return new Point2d(pt.X + vec.X, pt.Y + vec.Y);
        }

        // checks collinearity and order of three points
        internal static int Orientation(Point2d p, Point2d q, Point2d r)
        {
            double val = (q.Y - p.Y) * (r.X - q.X) -
                      (q.X - p.X) * (r.Y - q.Y);
            if (val == 0) return 0;  // colinear
            return (val > 0) ? 1 : 2; // clock or counterclock wise
        }
        #endregion




    }






}

