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
    public class LineUtility
    {
        //returns the midPt of a line
        internal static Point2d LineMidPoint(Line2d line)
        {
            double x = (line.StartPoint.X + line.EndPoint.X) / 2;
            double y = (line.StartPoint.Y + line.EndPoint.Y) / 2;
            return new Point2d(x, y);
        }

        //pushes the line midpt towards the center of the given polygon2d
        internal static Point2d NudgeLineMidPt(Line2d line, Polygon2d poly, double scale = 0.2)
        {
            Point2d midPt = LineMidPoint(line);
            Point2d polyCenter = GraphicsUtility.CentroidInPointLists(poly.Points);
            Vector2d vecToCenter = new Vector2d(midPt, polyCenter);
            Vector2d vecNormalized = vecToCenter.Normalize();
            Vector2d vecScaled = vecNormalized.Scale(scale);
            return new Point2d(midPt.X + vecScaled.X, midPt.Y + vecScaled.Y); 
        }

        
        //offsets an input line by a given distance 
        public static Line2d Offset(Line2d lineInp, Polygon2d poly, double distance)
        {
            if (lineInp == null || !PolygonUtility.CheckPoly(poly)) return null;
            Point2d ptStart = OffsetAPoint(lineInp, lineInp.StartPoint, poly, distance);
            Vector2d vec = new Vector2d(lineInp.StartPoint, ptStart);
            Point2d ptEnd = VectorUtility.VectorAddToPoint(lineInp.EndPoint, vec);
            //Point2d ptEnd = OffsetAPoint(lineInp, lineInp.EndPoint, poly, distance);
            return new Line2d(ptStart, ptEnd);
        }

        //offsets an input line by a given distance 
        public static Point2d OffsetAPointBoth(Line2d lineInp, Point2d testPoint, Polygon2d poly, double distance)
        {
            if (lineInp == null || !PolygonUtility.CheckPoly(poly)) return null;
            //Point2d testPoint = LineMidPoint(lineInp);
            double newX1 = 0, newY1 = 0;
            if (GraphicsUtility.CheckLineOrient(lineInp) == 0)
            {
                newX1 = testPoint.X;
                newY1 = testPoint.Y + distance;
            }
            else
            {
                newX1 = testPoint.X + distance;
                newY1 = testPoint.Y;
            }
            Point2d pt1 = new Point2d(newX1, newY1);
            return pt1;
        }

        //offsets an input line by a given distance 
        public static Point2d OffsetAPoint(Line2d lineInp, Point2d testPoint, Polygon2d poly, double distance)
        {
            if (lineInp == null || !PolygonUtility.CheckPoly(poly)) return null;
            //Point2d testPoint = LineMidPoint(lineInp);
            double newX1 = 0, newY1 = 0, newX2 = 0, newY2 = 0;
            if (GraphicsUtility.CheckLineOrient(lineInp) == 0)
            {
                newX1 = testPoint.X;
                newY1 = testPoint.Y + distance;
                newX2 = testPoint.X;
                newY2 = testPoint.Y - distance;
            }
            else
            {
                newX1 = testPoint.X + distance;
                newY1 = testPoint.Y;
                newX2 = testPoint.X - distance;
                newY2 = testPoint.Y;
            }
            Point2d pt1 = new Point2d(newX1, newY1);
            Point2d pt2 = new Point2d(newX2, newY2);
            if (GraphicsUtility.PointInsidePolygonTest(poly.Points, pt1)) return pt1;
            else return pt2;
        }

        //moves a line from its midpoint to a given point
        internal static Line2d Move(Line2d line,Point2d point)
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

        //moves a line from its midpoint to a given point
        internal static Line2d Move(Line2d line, double distance)
        {
            double x1 = line.StartPoint.X + distance, y1 = line.StartPoint.Y + distance;
            double x2 = line.EndPoint.X + distance, y2 = line.EndPoint.Y + distance;
            Point2d start = new Point2d(x1, y1);
            Point2d end = new Point2d(x2, y2);
            return new Line2d(start, end);

        }

        //moves a line by a given distance
        internal static Line2d Move(Line2d line, List<Point2d> poly, double distance)
        {
            Point2d midPt = LineMidPoint(line);
            Point2d centerPoly = GraphicsUtility.CentroidInPointLists(poly);
            Vector2d vecToCenter = new Vector2d(midPt, centerPoly);
            Vector2d vecToCenterN = vecToCenter.Normalize();
            Vector2d vectScaled = vecToCenter.Scale(distance);

            Point2d start = new Point2d((line.StartPoint.X + vectScaled.X), (line.StartPoint.Y + vectScaled.Y));
            Point2d end  = new Point2d((line.EndPoint.X + vectScaled.X), (line.EndPoint.Y + vectScaled.Y));
            return new Line2d(start, end);
        }

        //moves a line by a distance
        internal static Line2d Move(Line2d line, double distX, double distY)
        {
            Point2d start = new Point2d((line.StartPoint.X + distX), (line.StartPoint.Y + distY));
            Point2d end = new Point2d((line.EndPoint.X + distX), (line.EndPoint.Y + distY));
            return new Line2d(start, end);
        }


        //extends both of the ends of a line
        public static Line2d Extend(Line2d line, double extend =0)
        {
            double eps = 1000;
            if(extend == 0) extend = 100000;
            Line2d lineReturn = new Line2d(line);
            if (GraphicsUtility.CheckLineOrient(line) == 0) lineReturn = Move(line, 0, extend); //vertical
            else lineReturn = Move(line, extend, 0); //horizontal
            return lineReturn;
        }


        //extends both of the ends of a line - correct implementation
        public static Line2d ExtendLine(Line2d line, double extend = 0)
        {
            if (extend == 0) extend = 10000;
            double startPtX = 0, startPtY = 0, endPtX = 0, endPtY = 0;
            if(GraphicsUtility.CheckLineOrient(line) == 1)
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
    }
}
