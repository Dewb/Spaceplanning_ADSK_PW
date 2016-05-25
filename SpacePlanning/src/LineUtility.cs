using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using stuffer;
using Autodesk.DesignScript.Runtime;
using Autodesk.DesignScript.Interfaces;
using Autodesk.DesignScript.Geometry;

namespace SpacePlanning
{
    public static class LineUtility
    {
        #region - Public Methods
        //offsets an input line by a given distance 
        public static Line2d Offset(Line2d lineInp, Polygon2d poly, double distance)
        {
            if (lineInp == null || !ValidateObject.CheckPoly(poly)) return null;
            Point2d ptStart = OffsetLinePoint(lineInp, lineInp.StartPoint, distance);
            Vector2d vec = new Vector2d(lineInp.StartPoint, ptStart);
            Point2d ptEnd = VectorUtility.VectorAddToPoint(lineInp.EndPoint, vec);
            return new Line2d(ptStart, ptEnd);
        }

        //offsets an input line by a given distance inside a poly
        public static Line2d OffsetLineInsidePoly(Line2d lineInp, Polygon2d poly, double distance)
        {
            if (lineInp == null || !ValidateObject.CheckPoly(poly)) return null;
            Point2d ptStart = OffsetLinePointInsidePoly(lineInp, lineInp.StartPoint, poly, distance);
            Vector2d vec = new Vector2d(lineInp.StartPoint, ptStart);
            Point2d ptEnd = VectorUtility.VectorAddToPoint(lineInp.EndPoint, vec);
            return new Line2d(ptStart, ptEnd);
        }

        //returns the midPt of a line
        public static Point2d LineMidPoint(Line2d line)
        {
            double x = (line.StartPoint.X + line.EndPoint.X) / 2;
            double y = (line.StartPoint.Y + line.EndPoint.Y) / 2;
            return new Point2d(x, y);
        }

        //moves a line from its midpoint to a given point
        public static Line2d Move(Line2d line, Point2d point)
        {
            Point2d midPt = LineMidPoint(line);
            double distX = point.X - midPt.X;
            double distY = point.Y - midPt.Y;
            double x1 = line.StartPoint.X + distX, y1 = line.StartPoint.Y + distY;
            double x2 = line.EndPoint.X + distX, y2 = line.EndPoint.Y + distY;
            Point2d start = new Point2d(x1, y1);
            Point2d end = new Point2d(x2, y2);
            return new Line2d(start, end);

        }

        //moves a line by a distance in positive dir 
        public static Line2d Move(Line2d line, double distX, double distY)
        {
            Point2d start = new Point2d((line.StartPoint.X + distX), (line.StartPoint.Y + distY));
            Point2d end = new Point2d((line.EndPoint.X + distX), (line.EndPoint.Y + distY));
            return new Line2d(start, end);
        }

        //extends both  ends of a line 
        public static Line2d ExtendLine(Line2d line, double extend = 0)
        {
            if (extend == 0) extend = 10000;
            double startPtX = 0, startPtY = 0, endPtX = 0, endPtY = 0;
            if (ValidateObject.CheckLineOrient(line) == 1)
            {
                startPtX = line.StartPoint.X;
                startPtY = line.StartPoint.Y - extend;
                endPtX = line.EndPoint.X;
                endPtY = line.EndPoint.Y + extend;
            }
            else
            {
                startPtX = line.StartPoint.X - extend;
                startPtY = line.StartPoint.Y;
                endPtX = line.EndPoint.X + extend;
                endPtY = line.EndPoint.Y;
            }
            return new Line2d(new Point2d(startPtX, startPtY), new Point2d(endPtX, endPtY));
        }

        #endregion


        #region - Private Methods
        //pushes the line midpt towards the center of the given polygon2d
        internal static Point2d NudgeLineMidPt(Line2d line, Polygon2d poly, double scale = 0.2)
        {
            Point2d midPt = LineMidPoint(line);
            Point2d polyCenter = PointUtility.CentroidInPointLists(poly.Points);
            Vector2d vecToCenter = new Vector2d(midPt, polyCenter);
            Vector2d vecNormalized = vecToCenter.Normalize();
            Vector2d vecScaled = vecNormalized.Scale(scale);
            return new Point2d(midPt.X + vecScaled.X, midPt.Y + vecScaled.Y);
        }

        //offsets an input point by a given distance 
        internal static Point2d OffsetLinePoint(Line2d lineInp, Point2d testPoint, double distance)
        {
            double newX1 = 0, newY1 = 0;
            if (ValidateObject.CheckLineOrient(lineInp) == 0) // horizontal line
            {
                newX1 = testPoint.X;
                newY1 = testPoint.Y + distance;
            }
            else // vertical line
            {
                newX1 = testPoint.X + distance;
                newY1 = testPoint.Y;
            }
            return new Point2d(newX1, newY1);
        }

        //offsets an input point by a given distance inside a poly
        internal static Point2d OffsetLinePointInsidePoly(Line2d lineInp, Point2d testPoint, Polygon2d poly, double distance)
        {
            if (lineInp == null || !ValidateObject.CheckPoly(poly)) return null;
            int dir = DirectionForPointInPoly(lineInp, poly, distance);
            if (dir == 0) return null;
            return OffsetLinePoint(lineInp, testPoint, dir * distance);
        }

        //offsets an input line by a given distance 
        internal static int DirectionForPointInPoly(Line2d lineInp, Polygon2d poly, double distance)
        {
            if (lineInp == null || !ValidateObject.CheckPoly(poly)) return 0;
            Point2d midPt = LineMidPoint(lineInp);
            Point2d pt1 = OffsetLinePoint(lineInp, midPt, distance);
            Point2d pt2 = OffsetLinePoint(lineInp, midPt, -1 * distance);
            if (GraphicsUtility.PointInsidePolygonTest(poly, pt1)) return 1;
            else return -1;
        }

        //moves a line by a given distance inside a given poly
        internal static Line2d Move(Line2d line, List<Point2d> poly, double distance)
        {
            Point2d midPt = LineMidPoint(line);
            Point2d centerPoly = PointUtility.CentroidInPointLists(poly);
            Vector2d vecToCenter = new Vector2d(midPt, centerPoly);
            Vector2d vecToCenterN = vecToCenter.Normalize();
            Vector2d vectScaled = vecToCenter.Scale(distance);

            Point2d start = new Point2d((line.StartPoint.X + vectScaled.X), (line.StartPoint.Y + vectScaled.Y));
            Point2d end = new Point2d((line.EndPoint.X + vectScaled.X), (line.EndPoint.Y + vectScaled.Y));
            return new Line2d(start, end);
        }

        //removes duplicate lines from a list, based on the lines from another list
        internal static List<Line2d> RemoveDuplicateLinesBasedOnMidPt(List<Line2d> lineListOrig, List<Line2d> otherLineList)
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
                    if (ValidateObject.CheckPointsWithinRange(midPtOrig, midPtOther, 4)) { duplicate = true; break; }
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
        internal static List<Line2d> RemoveDuplicateLinesBasedOnAdjacency(List<Line2d> lineListOrig, List<Line2d> otherLineList)
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
                    if (GraphicsUtility.LineAdjacencyCheck(lineListOrig[i], otherLineList[j])) { duplicate = true; break; }
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
            List<Line2d> lineEditedList = lineListOrig.Select(x => new Line2d(x.StartPoint, x.EndPoint)).ToList(); // example of deep copy
            if (otherLineList == null || otherLineList.Count == 0) return lineEditedList;

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
                    lineEditedList.RemoveAt(i - count);
                    count += 1;
                }
            }
            return lineEditedList;
        }

        //checks if two lines are same 
        public static bool IsLineDuplicate(Line2d A, Line2d B)
        {
            bool check = false;
            double eps = 0.1;
            double mA = (A.EndPoint.Y - A.StartPoint.Y) / (A.EndPoint.X - A.StartPoint.X);
            double mB = (B.EndPoint.Y - B.StartPoint.Y) / (B.EndPoint.X - B.StartPoint.X);
            if ((mB - eps < mA && mA < mB + eps) || (mA - eps < mB && mB < mA + eps))
            {
                double intercA = A.StartPoint.Y - mA * A.StartPoint.X;
                double intercB = B.StartPoint.Y - mB * B.StartPoint.X;
                if ((intercA - eps < intercA && intercA < intercA + eps) || (intercB - eps < intercB && intercB < intercB + eps)) check = true;
                else check = false;
            }
            return check;
        }

        //removes duplicates lines from a list of lines
        public static List<Line2d> CleanLines(List<Line2d> lineList)
        {
            List<Line2d> cleanList = new List<Line2d>();
            List<bool> taggedList = new List<bool>();
            for (int i = 0; i < lineList.Count; i++)
            {
                Line2d lin = new Line2d(lineList[i].StartPoint, lineList[i].EndPoint);
                cleanList.Add(lin);
                taggedList.Add(false);
            }

            for (int i = 0; i < lineList.Count; i++)
            {
                double eps = 1;
                Line2d lineA = lineList[i];
                for (int j = i + 1; j < lineList.Count; j++)
                {
                    Line2d lineB = lineList[j];
                    int orientA = ValidateObject.CheckLineOrient(lineA);
                    int orientB = ValidateObject.CheckLineOrient(lineB);
                    if (orientA != orientB) continue;
                    else
                    {
                        Point2d midA = LineUtility.LineMidPoint(lineA);
                        Point2d midB = LineUtility.LineMidPoint(lineB);
                        if (orientA == 0)
                        {
                            //lines are horizontal                           
                            if ((midA.Y - eps < midB.Y && midB.Y < midA.Y + eps) ||
                                (midB.Y - eps < midA.Y && midA.Y < midB.Y + eps))
                            {
                                // lines are duplicate check length, whichever has longer length will be added to list
                                double lenA = lineA.Length;
                                double lenB = lineB.Length;
                                if (lenA > lenB) taggedList[i] = true;
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
                                else cleanList.Add(lineB);
                            }// end of if statement

                        }
                    }
                }
            }
            return cleanList;
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
        #endregion
    }
}
