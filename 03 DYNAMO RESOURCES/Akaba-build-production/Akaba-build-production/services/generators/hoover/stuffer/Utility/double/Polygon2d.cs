using System;
using System.Collections.Generic;
using System.Linq;
using SpacePlanning;
using Autodesk.DesignScript.Runtime;
using System.Diagnostics;
using Autodesk.DesignScript.Geometry;

namespace stuffer
    
{
  public class Polygon2d
  {
    private List<Point2d> m_points;
    private Range2d m_bbox;

        internal Polygon2d(List<Point2d> points, int dummy =0)
        {

            if(points == null)
            {
                m_bbox = null;
                m_points = points;
                return;
            }
            if(points.Count < 3)
            {
                m_bbox = null;
                m_points = points;
                return;
            }
            m_points = new List<Point2d>();
            foreach (Point2d point in points)
            {
                m_points.Add(new Point2d(point));
            }

            m_bbox = null;
            foreach (Point2d point in points)
            {
                m_bbox = new Range2d(m_bbox, point);
            }
        }


    internal Polygon2d(List<Point2d> points)
    {  
      if(points == null || points.Count == 0)
            {
                m_bbox = null;
                m_points = points;
                return;
            }
      m_points = new List<Point2d>();
      foreach (Point2d point in points)
      {
        m_points.Add(new Point2d(point));
      }

      bool removed = false;
      do
      {
        removed = false;
        for (int index = 0; index < m_points.Count; ++index)
        {
          int idxPre = (index == 0) ? m_points.Count - 1 : index - 1;
          int idxPost = (index == m_points.Count - 1) ? 0 : index + 1;
          Point2d p0 = m_points[idxPre];
          Point2d p1 = m_points[index];
          Point2d p2 = m_points[idxPost];
          Vector2d v0 = new Vector2d(p0, p1).Normalize();
          Vector2d v1 = new Vector2d(p0, p2).Normalize();
          if (v0.Dot(v1).Compare(1))
          {
            m_points.RemoveAt(index);
            removed = true;
          }
        }
      }
      while (removed);

      m_bbox = null;
      foreach (Point2d point in points)
      {
        m_bbox = new Range2d(m_bbox, point);
      }
    }

    public static Polygon2d ByPoints(List<Point2d> points)
    {
      return new Polygon2d(points);
    }

    public static Polygon2d ByPolygon(Polygon poly)
    {
            Point[] polyPts = poly.Points;
            List<Point2d> ptList = new List<Point2d>();
            for(int i = 0; i < polyPts.Length; i++)
            {
                ptList.Add(Point2d.ByCoordinates(polyPts[i].X, polyPts[i].Y));
            }
        return new Polygon2d(ptList);
    }

        public List<Point2d> Points
    {
      get { return m_points; }
    }

    public Range2d BBox
    {
      get { return m_bbox; }
    }

    public List<Point2d> GetAxisExtents(Vector2d dir)
    {
      double tMin = Double.PositiveInfinity;
      double tMax = Double.NegativeInfinity;
      double dirLen = dir.Dot(dir);
      foreach (Point2d point in m_points)
      {
        Vector2d vecPt = new Vector2d(point);
        double t = vecPt.Dot(dir)/dirLen;
        if (t < tMin)
          tMin = t;
        if (t > tMax)
          tMax = t;
      }

      List<Point2d> extents = new List<Point2d>();
      extents.Add(new Point2d(dir.Scale(tMin)));
      extents.Add(new Point2d(dir.Scale(tMax)));

      return extents;
    }

    public List<Line2d> Lines
    {
      get 
      {
        List<Line2d> lines = new List<Line2d>();
        for (int index = 0; index < m_points.Count; ++index)
        {
          Point2d p0 = m_points[index];
          Point2d p1 = m_points[(index == m_points.Count - 1) ? 0 : index + 1];
          lines.Add(new Line2d(p0, p1));
        }

        if (lines.Count < 3)
          return null;

        return lines;
      }
    }

    public List<Line2d> SelfSlice()
    {
      List<Line2d> lines = this.Lines;
      List<Line2d> sliceLines = new List<Line2d>();
      foreach (Line2d line in this.Lines)
      {
        Vector2d v0 = new Vector2d(line.EndPoint - line.StartPoint);
        SortedDictionary<double, Point2d> points = new SortedDictionary<double, Point2d>();
        foreach (Line2d testLine in this.Lines)
        {
          if (line.Compare(testLine))
            continue;

          Point2d testPoint = line.Intersect(testLine, true, true);
          if (testPoint != null)
          {
            Vector2d v1 = new Vector2d(testPoint - line.StartPoint);
            points[v0.Dot(v1)] = testPoint;
          }
        }

        SortedDictionary<double, Point2d>.ValueCollection values = points.Values;
        Point2d last = null;
        foreach (Point2d value in values)
        {
          if (last != null)
            sliceLines.Add(new Line2d(last, value));
          last = value;
        }
      }

      return sliceLines;
    }



        //ADDED CODE : SD
        /////////////////////////////////////////////////////////////////////////////////////////////////


        //COMPUTE CENTROID OF A CLOSED POLYGON
        public static Point2d CentroidFromPoly(Polygon2d poly)
        {
            List<Point2d> ptList = poly.Points;
            double x = 0, y= 0;
            for(int i = 0; i < ptList.Count; i++)
            {
                x += ptList[i].X;
                y += ptList[i].Y;
            }
            x = x / ptList.Count;
            y = y / ptList.Count;
            Point2d cen = new Point2d(x, y);
            poly = null;
            return cen;

        }


        //COMPUTE CENTROID OF A CLOSED POLYGON
        public static Point2d CentroidFromPoly(List<Point2d> ptList)
        {
           
            double x = 0, y = 0;
            for (int i = 0; i < ptList.Count; i++)
            {
                x += ptList[i].X;
                y += ptList[i].Y;
            }
            x = x / ptList.Count;
            y = y / ptList.Count;
            Point2d cen = new Point2d(x, y);
            ptList = null;
            return cen;

        }

        

        public static List<Point2d> FromPointsGetBoundingPoly(List<Point2d> pointList)
        {
            if(pointList == null)
            {
                return null;
            }

            if (pointList.Count == 0)
            {
                return null;
            }

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


        public static List<double> GetSpansXYFromPolygon2d(List<Point2d> poly)
        {

            if(poly == null || poly.Count ==0)
            {
                List<double> zeroList = new List<double>();
                zeroList.Add(0);
                zeroList.Add(0);
                return zeroList ;
            }
            // compute bounding box ( set of four points ) for the poly
            // find x Range, find y Range
            List<Point2d> polyBBox = Polygon2d.FromPointsGetBoundingPoly(poly);
            Range2d polyRange = Polygon2d.GetRang2DFromBBox(poly);

            Point2d span = polyRange.Span;
            double horizontalSpan = span.X;
            double verticalSpan = span.Y;
            List<double> spans = new List<double>();
            spans.Add(horizontalSpan);
            spans.Add(verticalSpan);

            return spans;
        }
        
        public static Range2d GetRang2DFromBBox(List<Point2d> pointList)
        {    
            if(pointList == null || pointList.Count ==0)
            {
                return null;
            }       
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

            Range1d ran1X = new Range1d(xMin, xMax);
            Range1d ran1Y = new Range1d(yMin, yMax);
            Range2d ran2D = new Range2d(ran1X, ran1Y);

            return ran2D;
        }


        internal static List<Point2d> SmoothPolygon(List<Point2d> pointList, double spacingProvided = 5)
        {
            if(pointList == null || pointList.Count == 0)
            {
                return null;
            }

              
             //added to make sure spacing is set based on poly dimensions-------------------
             List<double> spans = GetSpansXYFromPolygon2d(pointList);
             double spanX = spans[0];
             double spanY = spans[1];

             double distanceConsidered = 0;

             if(spanX > spanY)
             {
                 distanceConsidered = spanY;
             }
             else
             {
                 distanceConsidered = spanX;
             }

             double spacing = distanceConsidered / spacingProvided;
            
            List<Point2d> ptList = new List<Point2d>();

            for (int i = 0; i < pointList.Count; i++)
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
                int numPointsNeeded = (int)(dist / spacingProvided);
                //int numPointsNeeded = (int)spacingProvided;
                double increment = dist / numPointsNeeded;
                ptList.Add(pointList[i]);

                for (int j = 0; j < numPointsNeeded - 1; j++)
                {
                    double value = (j + 1) * increment / dist;
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


        //can be discarded
        //CHECK IF THE AREA POLYGON WORKS FINE
        public static double AreaCheckPolygon(Polygon2d poly)
        {
            if(poly == null)
            {
                return 0;
            }
            double area = GraphicsUtility.AreaPolygon2d(poly.Points);
            return area;
        }


        //just for checking can be discarded
        public static List<Point2d> SmoothCheckerForPoly(Polygon2d poly, int spacing)
        {
            List<Point2d> smoothedPoints = SmoothPolygon(poly.Points, spacing);
            //Polygon2d polyNew = new Polygon2d(smoothedPoints);
            return smoothedPoints;
        }



        /////////////////////////////////////////////////////////////////////////////////////////////////
    }







}
