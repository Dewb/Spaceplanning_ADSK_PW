using System;

namespace stuffer
{
  public class Vector3d : Point3d
  {
    public Vector3d(double x, double y, double z)
    : base (x, y, z)
    {
    }

    public Vector3d(Point3d pt)
    : base(pt.X, pt.Y, pt.Y)
    {
    }

    public Vector3d(Point3d p0, Point3d p1)
      : base(p1.X - p0.X, p1.Y - p0.Y, p1.Z - p0.Z)
    {
    }

    public Vector3d Add(Vector3d other)
    {
      return new Vector3d(m_x + other.m_x, m_y + other.m_y, m_z + other.m_z);
    }

    public Vector3d Subtract(Vector3d other)
    {
      return new Vector3d(m_x - other.m_x, m_y - other.m_y, m_z - other.m_z);
    }

    public double Length
    {
      get { return Math.Sqrt(m_x*m_x + m_y*m_y + m_z*m_z); }
    }

    public Vector3d Scale(double scale)
    {
      return new Vector3d(m_x*scale, m_y*scale, m_z*scale);
    }

    public Vector3d Normalize()
    {
      return Scale(1/Length);
    }

    public double Dot(Vector3d other)
    {
      return m_x*other.X + m_y*other.Y + m_z*other.Z;
    }

    public static Vector3d XAxis()
    {
      return new Vector3d(1.0, 0.0, 0.0);
    }

    public static Vector3d YAxis()
    {
      return new Vector3d(0.0, 1.0, 0.0);
    }

    public static Vector3d ZAxis()
    {
      return new Vector3d(0.0, 0.0, 1.0);
    }
  }
}
