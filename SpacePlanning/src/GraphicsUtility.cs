
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



namespace SpacePlanning
{
    public class GraphicsUtility
    {     

        //checks if a point is inside a polygon or not
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
            return PointInsidePolygonTest(poly.Points, testPoint);
        }
        

        //check order of points 0 = collinear, 1 = a,b,c clockwise, 2 = a,b,c are anti clockwise
        internal static int CheckPointOrder(Point2d a, Point2d b, Point2d c)
        {
            double area = (b.Y - a.Y) * (c.X - b.X) - (b.X - a.X) * (c.Y - b.Y);
            if (area > 0) return 1;
            else if ( area < 0) return 2;
            return 0;
        }
        
        // change two points in the list - used by Grahams Algo
        internal static void ChangePlaces(ref List<Point2d> pointList, int posA, int posB)
        {
            Point2d a = pointList[posA];
            pointList[posA] = pointList[posB];
            pointList[posB] = a;           
        }

        // get element position in a list
        internal static int ElementPosition(List<Point2d> ptList,int size,Point2d pt)
        {
            for(int i =0; i< size; ++i)
            {
                if(ptList[i] == pt) return i;
            }
            return -1;
        }


        //checks a line if horizontal or vertical 0 for horizontal, 1 for vertical
        internal static int CheckLineOrient(Line2d line)
        {
            double x = line.StartPoint.X - line.EndPoint.X;
            double y = line.StartPoint.Y - line.EndPoint.Y;       
            if (x == 0) return 1;
            else if ( y==0) return 0;
            else return -1; // was 0 prev
        }
      
        //joins two collinear lines to make one line - using now
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
             
        //returns the point having lowest x,y value from a list using now
        internal static int ReturnLowestPointFromListNew(List<Point2d> ptList)
        {
            if (!PolygonUtility.CheckPointList(ptList)) return -1;
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
            return index;

        }

        //returns the point having highest x,y value from a list - using now
        internal static int ReturnHighestPointFromListNew(List<Point2d> ptList)
        {
            if (!PolygonUtility.CheckPointList(ptList)) return -1;
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

        //removes duplicate lines from the list
        internal static List<Line2d> RemoveDuplicateLines(List<Line2d> networkLine)
        {
            List<Line2d> dummyLineList = new List<Line2d>();
            List<bool> duplicateList = new List<bool>();
            for (int i = 0; i < networkLine.Count; i++)
            {
                Line2d line = new Line2d(networkLine[i].StartPoint, networkLine[i].EndPoint);
                dummyLineList.Add(line);
                duplicateList.Add(false);
            }
            List<Line2d> cleanLines = new List<Line2d>();
            for (int i = 0; i < networkLine.Count; i++)
            {
                Line2d lineA = networkLine[i];
                for (int j = i + 1; j < networkLine.Count; j++)
                {
                    Line2d lineB = networkLine[j];
                    bool checkDuplicacy = GraphicsUtility.LineAdjacencyCheck(lineA, lineB); ;
                    if (checkDuplicacy)
                    {
                        double lenA = lineA.Length;
                        double lenB = lineB.Length;
                        if (lenA > lenB) duplicateList[j] = true;
                        else duplicateList[i] = true;
                    }
                }
            }
            int count = 0;
            for (int i = 0; i < duplicateList.Count; i++)
            {
                if (duplicateList[i] == true)
                {
                    dummyLineList.RemoveAt(i - count);
                    count += 1;
                }
            }
            return dummyLineList;
        }

        //removes duplicate lines from a list, based on the lines from another list
        public static List<Line2d> RemoveDuplicateLinesFromAnotherList(List<Line2d> lineListOrig, List<Line2d> otherLineList)
        {
            //List<Line2d> lineEditedList = new List<Line2d>();
            //for (int i = 0; i < lineListOrig.Count; i++) lineEditedList.Add(lineListOrig[i]);
            List<Line2d> lineEditedList = lineListOrig.Select(x => new Line2d(x.StartPoint,x.EndPoint)).ToList(); // example of deep copy

            List<bool> duplicateTagList = new List<bool>();
            for (int i = 0; i < lineListOrig.Count; i++)
            {
                bool duplicate = false;
                for (int j = 0; j < otherLineList.Count; j++)
                {
                    if (lineListOrig[i].Compare(otherLineList[j])) { duplicate = true; break; }
                }
                duplicateTagList.Add(duplicate);
            }
            int count = 0;
            for (int i = 0; i < duplicateTagList.Count; i++)
            {
                if (duplicateTagList[i])
                {
                    lineEditedList.RemoveAt(i-count);
                    count += 1;
                }
            }                      
            return lineEditedList;
        }


        //removes duplicate lines from a list, based on the lines from another list
        public static List<Line2d> RemoveDuplicateLinesBasedOnMidPt(List<Line2d> lineListOrig, List<Line2d> otherLineList)
        {
            List<Line2d> lineEditedList = new List<Line2d>();
            for (int i = 0; i < lineListOrig.Count; i++) lineEditedList.Add(lineListOrig[i]);
            List<bool> duplicateTagList = new List<bool>();
            for (int i = 0; i < lineListOrig.Count; i++)
            {
                bool duplicate = false;
                for (int j = 0; j < otherLineList.Count; j++)
                {
                    Point2d midPtOrig = LineUtility.LineMidPoint(lineListOrig[i]);
                    Point2d midPtOther = LineUtility.LineMidPoint(otherLineList[j]);
                    //if (midPtOrig.Compare(midPtOther)) { duplicate = true; break; }
                    if (PolygonUtility.CheckPointsWithinRange(midPtOrig,midPtOther,4)) { duplicate = true; break; }
                }
                duplicateTagList.Add(duplicate);
            }
            int count = 0;
            for (int i = 0; i < duplicateTagList.Count; i++)
            {
                if (duplicateTagList[i])
                {
                    lineEditedList.RemoveAt(i - count);
                    count += 1;
                }
            }
            return lineEditedList;
        }

        //removes duplicate lines from a list, based on line adjacency check
        public static List<Line2d> RemoveDuplicateLinesBasedOnAdjacency(List<Line2d> lineListOrig, List<Line2d> otherLineList)
        {
            List<Line2d> lineEditedList = new List<Line2d>();
            for (int i = 0; i < lineListOrig.Count; i++) lineEditedList.Add(lineListOrig[i]);
            List<bool> duplicateTagList = new List<bool>();
            for (int i = 0; i < lineListOrig.Count; i++)
            {
                bool duplicate = false;
                for (int j = 0; j < otherLineList.Count; j++)
                {
                    Point2d midPtOrig = LineUtility.LineMidPoint(lineListOrig[i]);
                    Point2d midPtOther = LineUtility.LineMidPoint(otherLineList[j]);
                    if (LineAdjacencyCheck(lineListOrig[i],otherLineList[j])) { duplicate = true; break; }
                }
                duplicateTagList.Add(duplicate);
            }
            int count = 0;
            for (int i = 0; i < duplicateTagList.Count; i++)
            {
                if (duplicateTagList[i])
                {
                    lineEditedList.RemoveAt(i - count);
                    count += 1;
                }
            }
            return lineEditedList;
        }

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
        
        //removes duplicates lines from a list of lines
        public static List<Line2d> CleanLines(List<Line2d> lineList)
        {
            List<Line2d> cleanList = new List<Line2d>();
            List<bool> taggedList = new List<bool>();
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
                    if(orientA != orientB) continue;
                    else
                    {
                        Point2d midA = LineUtility.LineMidPoint(lineA);
                        Point2d midB = LineUtility.LineMidPoint(lineB);
                        if (orientA == 0)
                        {
                            //lines are horizontal                           
                            if((midA.Y - eps < midB.Y && midB.Y < midA.Y + eps) || 
                                (midB.Y - eps < midA.Y && midA.Y < midB.Y + eps))
                            {
                                // lines are duplicate check length, whichever has longer length will be added to list
                                double lenA = lineA.Length;
                                double lenB = lineB.Length;
                                if(lenA > lenB) taggedList[i] = true;
                                else taggedList[j] = true;
                            }// end of if statement
                        }
                        else
                        {
                            //lines are vertical
                            if ((midA.X - eps < midB.X && midB.X < midA.X + eps) ||
                               (midB.X - eps < midA.X && midA.X < midB.X + eps))
                            {
                                double lenA = lineA.Length;
                                double lenB = lineB.Length;
                                if (lenA > lenB) cleanList.Add(lineA);
                                else  cleanList.Add(lineB);
                            }// end of if statement

                        }
                    }
                }
            }
            return cleanList;
        }

        //returns only unique point2d from a list of points
        internal static List<Point2d> MakeUniquePoints(List<Point2d> testList)
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
                }
            }            
            return cleanList;
        }
         
        //find lowest point from a list ( only used in the Grahams Scan Convex Hull Algo )
        internal static void FindLowestPointFromList(ref List<Point2d> ptList, int size)
        {
            Point2d lowestPoint = ptList[0];
            for (int i = 0; i < size; i++)
            {
                if (lowestPoint.Y > ptList[0].Y || (lowestPoint.Y == ptList[i].Y && lowestPoint.X > ptList[i].X)) lowestPoint = ptList[i];
               
            }
            int rootPosition = ElementPosition(ptList, size, lowestPoint);
            if(rootPosition != -1) ChangePlaces(ref ptList, 0, rootPosition);
          
        }

        //returns angle between two vectors
        //for 'returndegrees' enter true for an answer in degrees, false for radians
        internal static double AngleBetween(Vector2d u, Vector2d v, bool returndegrees)
        {
            double numerator = u.X * v.X + u.Y * v.Y;
            double u2 = u.X * u.X + u.Y * u.Y;
            double v2 = v.X * v.X + v.Y * v.Y;
            double denominator = 0;
            denominator = Math.Sqrt(u2 * v2);
            double rtnval = Math.Acos(numerator / denominator);
            if (returndegrees) rtnval *= 360.0 / (2 * Math.PI);
            return rtnval;
        }
                

        //sort point array for Grahams scan algo to find convex hull
        internal static void SortedPoint2dList(ref List<Point2d> ptList, int size)
        {           
            for (int i = 1; i < size; ++i)
            {
                for (int j = i + 1; j < size; ++j)
                {
                    int order = CheckPointOrder(ptList[0], ptList[i], ptList[j]);
                    // collinear
                    if (order == 0)
                    {
                        if (DistanceBetweenPoints(ptList[0], ptList[0]) <= DistanceBetweenPoints(ptList[0], ptList[j])) ChangePlaces(ref ptList, i, j);                       
                    }
                    else if (order == 1) ChangePlaces(ref ptList, i, j);
                    }
                }
            }
        

        //get next to top point on Stack for Graham Scan algo
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

        // find the closest point to a point from a group of cells
        internal static int FindClosestPointIndex(List<Cell> cellList, Point2d pt)
        {
            List<Point2d> ptList = new List<Point2d>();
            for (int i=0; i< cellList.Count; i++)
            {
                if (cellList[i].CellAvailable) ptList.Add(cellList[i].CenterPoint);
            }
            return FindClosestPointIndex(ptList, pt);
        }

        // find the closest point to a point from a point list
        internal static int FindClosestPointIndex(List<Point2d> ptList, Point2d pt)
        {
            int index = 0;
            double minDist = 100000000;
            for (int i = 0; i < ptList.Count; i++)
            {
                Point2d centerPt = ptList[i];
                double calcDist = DistanceBetweenPoints(centerPt, pt);
                if (calcDist < minDist)
                {
                    minDist = calcDist;
                    index = i;
                }
            }
            return index;
        }


        //distance between points
        internal static double DistanceBetweenPoints(Point2d ptA, Point2d ptB)
        {
            if (ptA == null || ptB == null) return 0;
            double xLen = ptA.X - ptB.X;
            double yLen = ptA.Y - ptB.Y;
            return xLen * xLen + yLen * yLen;
        }
               
        // returns area of a closed polygon, if area is positive, poly points are counter clockwise and vice versa
        internal static double AreaPolygon2d(List<Point2d> polyPoints, bool value = true)
        {
            if(polyPoints == null) return 0;
           
            double area = 0;
            int j = polyPoints.Count - 1; 
            for (int i = 0; i < polyPoints.Count; i++)
            {
                area += (polyPoints[j].X + polyPoints[i].X) * (polyPoints[j].Y - polyPoints[i].Y);
                j = i;  
            }
            //if true return absolute value, else return normal value
            if (value) return Math.Abs(area / 2);
            else return area / 2;
        }
        
        //sorts list of points by distance from a given point
        public static List<Point2d> SortPointsByDistanceFromPoint(List<Point2d> ptList, Point2d testPoint)
        {
            List<Point2d> sortedPtList = new List<Point2d>();
            List<double> distanceList = new List<double>();
            List<int> indexList = new List<int>();
            for (int i = 0; i < ptList.Count; i++)
            {
                distanceList.Add(DistanceBetweenPoints(ptList[i], testPoint));
            }

            indexList = BasicUtility.SortIndex(distanceList);           
            for(int i = 0; i < indexList.Count; i++)
            {
                sortedPtList.Add(ptList[indexList[i]]);
            }
            return sortedPtList;
        }


        // find the centroid of a group of  cells
        internal static Point2d CentroidInCells(List<Cell> cellList)
        {
            List<Point2d> ptList = new List<Point2d>();
            for (int i = 0; i < cellList.Count; i++)
            {
                ptList.Add(cellList[i].CenterPoint);
            }
            return CentroidInPointLists(ptList);
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
            return new Point2d(pt.X + vec.X, pt.Y + vec.Y); 
        }
        
        //make polygon2d on grids - notusing now
        public static Polygon2d MakeSquarePolygon2dFromCenterSide(Point2d centerPt, double side)
        {
            List<Point> ptList = TestGraphicsUtility.MakeSquarePointsFromCenterSide(centerPt, side);
            List<Point2d> pt2dList = new List<Point2d>();
            for (int i = 0; i < ptList.Count; i++)
            {
                pt2dList.Add(new Point2d(ptList[i].X, ptList[i].Y));
            }
            return Polygon2d.ByPoints(pt2dList);
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
            if (A.Y < center.Y) angle = Math.PI + Math.PI - angle;//360-angle
            return angle; //return angle*180/Math.PI;
        }
        
        // sorts two points clockwise, with respect to a reference point not using now
        internal static int SortCornersClockwise(Point2d A, Point2d B)
        {
            double aTanA, aTanB; 
            aTanA = Math.Atan2(A.Y - BuildLayout.reference.Y, A.X - BuildLayout.reference.X);
            aTanB = Math.Atan2(B.Y - BuildLayout.reference.Y, B.X - BuildLayout.reference.X);
            //  Determine next point in Clockwise rotation
            if (aTanA < aTanB) return -1;
            else if (aTanA > aTanB) return 1;
            return 0;
        }

        
        //sorts a list of points to form a polyline - not using
        public static List<Point2d> SortPoints(List<Point2d> pointList)
        {
            Point2d cen = PolygonUtility.CentroidFromPoly(pointList);
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

    
        // Given three colinear points p, q, r, the function checks if
        // point q lies on line segment 'pr'
        internal static bool OnSegment(Point2d p, Point2d q, Point2d r)
        {
            if (q.X <= Math.Max(p.X, r.X) && q.X >= Math.Min(p.X, r.X) &&
                q.Y <= Math.Max(p.Y, r.Y) && q.Y >= Math.Min(p.Y, r.Y)) return true;
            return false;
        }

      
        // Given three colinear points p, q, r, the function checks if
        // point q lies on line segment 'pr'
        public static bool OnSegment(Line2d givenLine, Point2d q, double eps = 0)
        {
            if(givenLine == null || q == null) return false; 
            Point2d p = givenLine.StartPoint, r = givenLine.EndPoint;
            if (q.X <= (Math.Max(p.X, r.X) + eps) && q.X >= (Math.Min(p.X, r.X) - eps) &&
                q.Y <= (Math.Max(p.Y, r.Y) + eps) && q.Y >= (Math.Min(p.Y, r.Y))- eps) return true;
            return false;
        }

        // checks collinearity and order of three points
        internal static int Orientation(Point2d p, Point2d q, Point2d r)
        {

            double val = (q.Y - p.Y) * (r.X - q.X) -
                      (q.X - p.X) * (r.Y - q.Y);

            if (val == 0) return 0;  // colinear

            return (val > 0) ? 1 : 2; // clock or counterclock wise
        }


        //checks if two lines are collinear or not , works good
        public static bool LineAdjacencyCheck(Line2d lineA, Line2d lineB, double eps = 0)
        {
            Point2d pA = lineA.StartPoint, qA = lineA.EndPoint;
            Point2d pB = lineB.StartPoint, qB = lineB.EndPoint;

            Vector2d vecA = new Vector2d(pA, qA), vecB = new Vector2d(pB, qB);
            double crossMag = vecA.Cross(vecB);
            if (crossMag != 0) return false;

            bool checkA1 = OnSegment(lineB, pA, eps), checkA2 = OnSegment(lineB, qA, eps);
            bool checkB1 = OnSegment(lineA, pB, eps), checkB2 = OnSegment(lineA, qB, eps);

            if (checkA1 || checkA2) return true;
            if (checkB1 || checkB2) return true;
            return false;
        }

        
        //find perp projection on a line from a point
        public static Point2d PerpProjectionPointOnLine(Line line, Point2d C)
        {
            Line2d testLine = new Line2d(Point2d.ByCoordinates(line.StartPoint.X,line.StartPoint.Y), Point2d.ByCoordinates(line.EndPoint.X, line.EndPoint.Y));
            Point2d A = testLine.StartPoint;
            Point2d B = testLine.EndPoint;
            Vector2d b = new Vector2d(A, B);
            Vector2d a = new Vector2d(A, C);
            double a1 = a.Dot(b) / b.Length;
            Vector2d bScaled = b.Scale(a1);
            return new Point2d(A.X + bScaled.X, A.Y + bScaled.Y);

        }
        

        // find closest point to a line - correct
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
            return new Point2d(A.X+vVector3.X, A.Y+vVector3.Y);
        }
     

        //finds if line is horizontal or vertical
        internal static bool IsLineOrthogonal(Line2d line)
        {
            bool check = false;
            Point2d p1 = line.StartPoint;
            Point2d p2 = line.EndPoint;
            double xDiff = p1.X - p2.X;
            double yDiff = p1.Y - p2.Y;
            if(xDiff == 0 || yDiff == 0) check = true;        
            return check;
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


        //checks if two lines are same - not using now
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
                if ((intercA-eps < intercA && intercA < intercA + eps) || (intercB - eps < intercB && intercB < intercB + eps)) check = true;
                else  check = false;
            }
            return check;
        }
        

        //flatten list of line2d
        internal static List<Line2d> FlattenLine2dList(List<List<Line2d>> lineList)
        {
            if (lineList == null || lineList.Count == 0) return null;
            List<Line2d> flatLineList = new List<Line2d>();
            for (int i = 0; i < lineList.Count; i++)
            {
                if (lineList[i] != null)
                    if (lineList[i].Count > 0) flatLineList.AddRange(lineList[i]);
            }
            return flatLineList;
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




    }






}

