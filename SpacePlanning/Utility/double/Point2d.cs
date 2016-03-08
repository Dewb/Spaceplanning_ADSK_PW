using System;
using System.Diagnostics;

namespace stuffer
{
  [DebuggerDisplay("({m_x}, {m_y})")]
  public class Point2d
  {
    protected double m_x;
    protected double m_y;

    internal Point2d(double x, double y)
    {
      m_x = x;
      m_y = y;
    }

    internal Point2d(Point2d other)
    {
      m_x = other.X;
      m_y = other.Y;
    }

    public static Point2d ByCoordinates(double x, double y)
    {
      return new Point2d(x, y);
    }

    public double X
    {
      get { return m_x; }
    }

    public double Y
    {
      get { return m_y; }
    }

    public bool Compare(Point2d other)
    {
      if (!m_x.Compare(other.X))
        return false;
      if (!m_y.Compare(other.Y))
        return false;

      return true;
    }

    public override string ToString()
    {
      return String.Format("({0:F3}, {1:F3})", m_x, m_y);
    }

    public static Point2d Origin()
    {
      return new Point2d(0, 0);
    }

    public static Point2d operator+(Point2d p0, Point2d p1)
    {
      return new Point2d(p0.m_x + p1.m_x, p0.m_y + p1.m_y);
    }

    public static Point2d operator-(Point2d p0, Point2d p1)
    {
      return new Point2d(p0.m_x - p1.m_x, p0.m_y - p1.m_y);
    }

    public static Point2d operator/(Point2d pt, double scale)
    {
      return new Point2d(pt.m_x/scale, pt.m_y/scale);
    }

    public static Point2d operator*(Point2d pt, double scale)
    {
      return new Point2d(pt.m_x*scale, pt.m_y*scale);
    }
  }
}
