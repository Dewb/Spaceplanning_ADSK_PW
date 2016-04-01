using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using stuffer;
using Autodesk.DesignScript.Geometry;

namespace SpacePlanning
{
    internal class TestGraphicsUtility
    {
        // checks if two lines are collinear - not using
        public static bool CheckLineCollinear(Line2d lineA, Line2d lineB)
        {
            Point2d p1 = lineA.StartPoint;
            Point2d p2 = lineA.EndPoint;
            Point2d q1 = lineB.StartPoint;
            Point2d q2 = lineB.EndPoint;

            // Find the four orientations needed for general and special cases
            int o1 = GraphicsUtility.Orientation(p1, q1, p2);
            int o2 = GraphicsUtility.Orientation(p1, q1, q2);
            int o3 = GraphicsUtility.Orientation(p2, q2, p1);
            int o4 = GraphicsUtility.Orientation(p2, q2, q1);

            // General case
            if (o1 != o2 && o3 != o4)
                return false;
            // p1, q1 and p2 are colinear and p2 lies on segment p1q1
            if (o1 == 0 && GraphicsUtility.OnSegment(p1, p2, q1)) return true;

            // p1, q1 and p2 are colinear and q2 lies on segment p1q1
            if (o2 == 0 && GraphicsUtility.OnSegment(p1, q2, q1)) return true;

            // p2, q2 and p1 are colinear and p1 lies on segment p2q2
            if (o3 == 0 && GraphicsUtility.OnSegment(p2, p1, q2)) return true;

            // p2, q2 and q1 are colinear and q1 lies on segment p2q2
            if (o4 == 0 && GraphicsUtility.OnSegment(p2, q1, q2)) return true;
            return false; // Doesn't fall in any of the above cases
        }

        //make points on grids - not using now
        public static List<Point> MakeSquarePointsFromCenterSide(Point2d centerPt, double side)
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
            return ptList;
        }

        //make cell on grids - not using now
        public static Polygon MakeSquareFromCenterSide(Point2d centerPt, double side)
        {
            return Polygon.ByPoints(MakeSquarePointsFromCenterSide(centerPt, side));
        }

        

    }
}
