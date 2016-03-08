using System;
using System.Diagnostics;

namespace stuffer
{
  [DebuggerDisplay("[({m_x.Min}, {m_y.Min}) - ({m_x.Max}, {m_y.Max})]")]
  public class Range2d
  {
    private Range1d m_x;
    private Range1d m_y;

    public Range2d(Point2d point0, Point2d point1)
    {
      m_x = new Range1d(point0.X, point1.X);
      m_y = new Range1d(point0.Y, point1.Y);
    }

    public Range2d(Range2d other, Point2d point)
    {
      if (other == null)
      {
        m_x = new Range1d(point.X, point.X);
        m_y = new Range1d(point.Y, point.Y);
      }
      else
      {
        m_x = new Range1d(other.m_x, point.X);
        m_y = new Range1d(other.m_y, point.Y);
      }
    }

    public Range2d(Range2d r0, Range2d r1)
    {
      if (r0 == null)
      {
        m_x = new Range1d(r1.m_x);
        m_y = new Range1d(r1.m_y);
      }
      else
      {
        m_x = new Range1d(r0.m_x, r1.m_x);
        m_y = new Range1d(r0.m_y, r1.m_y);
      }
    }

    public Point2d Min
    {
      get
      {
        return new Point2d(m_x.Min, m_y.Min);
      }
    }

    public Point2d Max
    {
      get
      {
        return new Point2d(m_x.Max, m_y.Max);
      }
    }

    public Point2d Span
    {
      get
      {
        return new Point2d(m_x.Span, m_y.Span);
      }
    }

    public double Area
    {
      get
      {
        Point2d span = Span;
        return span.X*span.Y;
      }
    }

    public Point2d Center
    {
      get
      {
        return new Point2d(m_x.Center, m_y.Center);
      }
    }

    public bool Compare(Range2d value)
    {
      if (!value.m_x.Compare(m_x))
        return false;
      if (!value.m_y.Compare(m_y))
        return false;

      return true;
    }

    public bool Contains(Point2d point)
    {
      if (!m_x.Contains(point.X))
        return false;
      if (!m_y.Contains(point.Y))
        return false;

      return true;
    }

    public override string ToString()
    {
      return String.Format("[({0:F3}, {1:F3}) - ({2:F3}, {3:F3})]", m_x.Min, m_y.Min, m_x.Max, m_y.Max);
    }

    ///////////////////////////////////////////////////////////
    // Axis (major) specific methods
    public double LongestSpan
    {
      get
      {
        return Math.Max(Span.X, Span.Y);
      }
    }

    public Line2d MajorCenterline
    {
      get
      {
        if (m_x.Span > m_y.Span)
        {
          return new Line2d(
            new Point2d(m_x.Min, m_y.Center),
            new Point2d(m_x.Max, m_y.Center));
        }
        else
        {
          return new Line2d(
            new Point2d(m_x.Center, m_y.Min),
            new Point2d(m_x.Center, m_y.Max));
        }
      }
    }
  }
}
