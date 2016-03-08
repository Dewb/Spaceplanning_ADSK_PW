using System;
using System.Collections.Generic;

namespace stuffer
{
  public class Polygon2d
  {
    private List<Point2d> m_points;
    private Range2d m_bbox;

    internal Polygon2d(List<Point2d> points)
    {
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
  }
}
