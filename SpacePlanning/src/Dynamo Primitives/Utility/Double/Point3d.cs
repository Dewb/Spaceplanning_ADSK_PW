using System;
using System.Diagnostics;

namespace stuffer
{
  [DebuggerDisplay("({m_x}, {m_y}, {m_z})")]
  public class Point3d
  {
    protected double m_x;
    protected double m_y;
    protected double m_z;

    internal Point3d(double x, double y, double z)
    {
      m_x = x;
      m_y = y;
      m_z = z;
    }

    internal Point3d(Point2d point, double z)
    {
      m_x = point.X;
      m_y = point.Y;
      m_z = z;
    }

    public static Point3d ByCoordinates(double x, double y, double z)
    {
      return new Point3d(x, y, z);
    }

    public double X
    {
      get { return m_x; }
    }

    public double Y
    {
      get { return m_y; }
    }

    public double Z
    {
      get { return m_z; }
    }

    public bool Compare(Point3d other)
    {
      if (!m_x.Compare(other.X))
        return false;
      if (!m_y.Compare(other.Y))
        return false;
      if (!m_z.Compare(other.Z))
        return false;

      return true;
    }

    public override string ToString()
    {
      return String.Format("({0:F3}, {1:F3}, {1:F3})", m_x, m_y, m_z);
    }

    public static Point3d Origin
    {
      get
      {
        return new Point3d(0, 0, 0);
      }
    }
  }
}
