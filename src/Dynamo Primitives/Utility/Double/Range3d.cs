using System;
using System.Diagnostics;

namespace stuffer
{
  [DebuggerDisplay("[({m_x.Min}, {m_y.Min}, {m_z.Min}) - ({m_x.Max}, {m_y.Max}, {m_z.Max})]")]
  public class Range3d
  {
    private Range1d m_x;
    private Range1d m_y;
    private Range1d m_z;

    public Range3d(Range3d other, Point3d point)
    {
      if (other == null)
      {
        m_x = new Range1d(point.X);
        m_y = new Range1d(point.Y);
        m_z = new Range1d(point.Z);
      }
      else
      {
        m_x = new Range1d(other.m_x, point.X);
        m_y = new Range1d(other.m_y, point.Y);
        m_z = new Range1d(other.m_z, point.Z);
      }
    }

    public Point3d Min
    {
      get
      {
        if (m_x == null || m_y == null || m_z == null)
          return Point3d.Origin;

        return new Point3d(m_x.Min, m_y.Min, m_z.Min);
      }
    }

    public Point3d Max
    {
      get
      {
        if (m_x == null || m_y == null || m_z == null)
          return Point3d.Origin;

        return new Point3d(m_x.Max, m_y.Max, m_z.Max);
      }
    }

    public Point3d Span
    {
      get
      {
        return new Point3d(m_x.Span, m_y.Span, m_z.Span);
      }
    }

    public double Volume
    {
      get
      {
        Point3d span = Span;
        return span.X*span.Y*span.Z;
      }
    }

    public Point3d Center
    {
      get
      {
        return new Point3d(m_x.Center, m_y.Center, m_z.Center);
      }
    }

    public bool Contains(Point3d point)
    {
      if (!m_x.Contains(point.X))
        return false;
      if (!m_y.Contains(point.Y))
        return false;
      if (!m_z.Contains(point.Z))
        return false;

      return true;
    }

    public override string ToString()
    {
      return String.Format("[({0:F3}, {1:F3}, {2:F3}) - ({3:F3}, {4:F3}, {5:F3})]", m_x.Min, m_y.Min, m_z.Min, m_x.Max, m_y.Max, m_z.Max);
    }
  }
}
