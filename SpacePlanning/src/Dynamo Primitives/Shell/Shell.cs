using System;
using System.Collections.Generic;

namespace stuffer
{
    public class Shell
  {
    private STL m_stl;

    internal Shell(STL stl)
    {
      m_stl = stl;
    }

    public Range3d BBox
    {
      get 
      {
        if (m_stl == null)
          return null;

        return m_stl.BBox;
      }
    }

    public static Shell BySTL(STL stl)
    {
      return new Shell(stl);
    }

    // NOTE: This is the main 3d to 2d transformation method
    public List<Line2d> GetRawOutline(double height, double offset = 0.0)
    {
      if (m_stl == null)
        return null;

      Plane3d plane = new Plane3d(new Point3d(0, 0, height + offset), Vector3d.ZAxis());

      List<Line2d> lines = new List<Line2d>();
      foreach (Facet3d facet in m_stl.Facets)
      {
        double minLineLength = 0.25;
        Line3d rawLine = plane.Intersect(facet, minLineLength);
        if (rawLine != null)
        {
          Line2d line = 
          new Line2d(
              new Point2d(rawLine.StartPoint.X, rawLine.StartPoint.Y),
              new Point2d(rawLine.EndPoint.X, rawLine.EndPoint.Y));
          lines.Add(line);
        }
      }

      if (lines.Count < 3)
        return null;

      return lines;
    }

    public Outline2d GetFloorOutline(double height)
    {
      return new Outline2d(GetRawOutline(height, 0.001));
    }

    public Outline2d GetCeilingOutline(double height)
    {
      return new Outline2d(GetRawOutline(height, -0.001));
    }
  }
}
