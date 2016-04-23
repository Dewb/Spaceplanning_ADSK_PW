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
    internal class LineUtility
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

        //offsets an input line by a given distance towards the centroid of given poly
        internal static Line2d Offset(Line2d lineInp,Polygon2d poly, double distance)
        {
            if (lineInp == null || !PolygonUtility.CheckPoly(poly)) return null;
            Line2d line = extend(lineInp);
            Point2d shiftedMidPt = NudgeLineMidPt(line, poly, distance);
            Point2d startPtNew = Point2d.ByCoordinates(0, 0);
            Point2d endPtNew = Point2d.ByCoordinates(0, 0);
            if (GraphicsUtility.CheckLineOrient(line) == 0)
            {
                startPtNew = Point2d.ByCoordinates(line.StartPoint.X, shiftedMidPt.Y + line.StartPoint.Y);
                endPtNew = Point2d.ByCoordinates(line.EndPoint.X, shiftedMidPt.Y + line.EndPoint.Y);
            }
            else
            {
                startPtNew = Point2d.ByCoordinates(shiftedMidPt.X + line.StartPoint.X,line.StartPoint.Y);
                endPtNew = Point2d.ByCoordinates(shiftedMidPt.X + line.EndPoint.X,line.EndPoint.Y);
            }
            return Line2d.ByStartPointEndPoint(startPtNew, endPtNew); 
        }

        //moves a line from its midpoint to a given point
        internal static Line2d move(Line2d line,Point2d point)
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
        internal static Line2d move(Line2d line, double distance)
        {
            double x1 = line.StartPoint.X + distance, y1 = line.StartPoint.Y + distance;
            double x2 = line.EndPoint.X + distance, y2 = line.EndPoint.Y + distance;
            Point2d start = new Point2d(x1, y1);
            Point2d end = new Point2d(x2, y2);
            return new Line2d(start, end);

        }

        //moves a line by a given distance
        internal static Line2d move(Line2d line, List<Point2d> poly, double distance)
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
        internal static Line2d move(Line2d line, double distX, double distY)
        {
            Point2d start = new Point2d((line.StartPoint.X + distX), (line.StartPoint.Y + distY));
            Point2d end = new Point2d((line.EndPoint.X + distX), (line.EndPoint.Y + distY));
            return new Line2d(start, end);

        }


        //extends both of the ends of a line
        internal static Line2d extend(Line2d line)
        {
            double eps = 1000, extend = 100000;
            Vector2d vecLine = new Vector2d(line.StartPoint, line.EndPoint);
            Vector2d vecX = new Vector2d(line.StartPoint, Point2d.ByCoordinates(line.StartPoint.X + eps, 0));
            Vector2d vecY = new Vector2d(line.StartPoint, Point2d.ByCoordinates(0, line.StartPoint.X + eps));

            double dotX = vecLine.Dot(vecX), dotY = vecLine.Dot(vecY);
            Line2d lineReturn = new Line2d(line);
            if (dotX == 0) lineReturn = move(line, 0, extend); //vertical
            else if (dotY == 0) lineReturn = move(line, extend, 0); //horizontal
            return lineReturn;
        }
    }
}
