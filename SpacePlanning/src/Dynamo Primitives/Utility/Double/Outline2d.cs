using System;
using System.Collections.Generic;

namespace stuffer
{
    public class Outline2d
  {
    List<Polygon2d> m_polygons;
    Range2d m_bbox;

    internal Outline2d(List<Polygon2d> polygons)
    {
      m_polygons = polygons;
      m_bbox = null;
      foreach (Polygon2d polygon in polygons)
        m_bbox = new Range2d(m_bbox, polygon.BBox);
    }

    public static Outline2d ByPolygons(List<Polygon2d> polygons)
    {
      return new Outline2d(polygons);
    }

    internal Outline2d(List<Line2d> rawLines)
    {
      if (rawLines == null || rawLines.Count < 1)
      {
        m_polygons = null;
        m_bbox = null;
        return;
      }

      m_polygons = new List<Polygon2d>();
      m_bbox = null;

      int nextIndex = 0;
      Line2d firstLine = rawLines[nextIndex];

      List<Point2d> points = new List<Point2d>();
      points.Add(firstLine.StartPoint);

      Line2d currentLine = firstLine;
      bool start = true;
      while (rawLines.Count > 0)
      {
        currentLine = rawLines[nextIndex];
        rawLines.RemoveAt(nextIndex);
        nextIndex = 0;

        Point2d testPoint = GetPoint(currentLine, !start);
        int index = GetClosestLine(testPoint, rawLines, (currentLine == firstLine) ? null : firstLine, ref start);
        if (index == -1)
        {
          Polygon2d polygon = new Polygon2d(points);
          m_polygons.Add(polygon);
          m_bbox = new Range2d(m_bbox, polygon.BBox.Min);
          m_bbox = new Range2d(m_bbox, polygon.BBox.Max);

          points.Clear();
          if (rawLines.Count > 0)
          {
            firstLine = rawLines[nextIndex];
            points.Add(firstLine.StartPoint);
          }
        }
        else
        {
          Point2d point = currentLine.Heal(rawLines[index], false);
          if (point != null)
            points.Add(point);
          nextIndex = index;
        }
      }
    }

    internal Point2d GetPoint(Line2d line, bool start)
    {
      return start ? line.StartPoint : line.EndPoint;
    }

    internal double GetClosestPoint(Point2d point, Line2d line, ref bool start)
    {
      if (line == null)
        return Double.PositiveInfinity;

      double distance = new Vector2d(line.StartPoint, point).Length;
      start = true;

      double endDistance = new Vector2d(line.EndPoint, point).Length;
      if (endDistance < distance)
      {
        start = false;
        distance = endDistance;
      }

      return distance;
    }

    internal int GetClosestLine(Point2d point, List<Line2d> lines, Line2d first, ref bool start)
    {
      int closestIndex = -1;
      start = false;
      double distance = Double.PositiveInfinity;
      for (int index = -1; index < lines.Count; ++index)
      {
        Line2d testLine = (index == -1) ? first : lines[index];
        if (testLine != null)
        {
          bool testIsStart = false;
          double testDistance = GetClosestPoint(point, testLine, ref testIsStart);

          if (testDistance < distance)
          {
            closestIndex = index;
            start = testIsStart;
            distance = testDistance;
          }
        }
      }

      return closestIndex;
    }

    public List<Polygon2d> Polygons
    {
      get { return m_polygons; }
    }

    public Range2d BBox
    {
      get { return m_bbox; }
    }

    public List<Line2d> SelfSlice()
    {
      if (m_polygons == null || m_polygons.Count == 0)
        return null;

      List<Line2d> lines = new List<Line2d>();
      foreach (Polygon2d polygon in m_polygons)
        lines.AddRange(polygon.SelfSlice());

      return lines;
    }
  }
}
