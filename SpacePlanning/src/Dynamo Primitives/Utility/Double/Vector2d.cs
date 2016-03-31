using System;

namespace stuffer
{
  public class Vector2d : Point2d
  {
    public Vector2d(double x, double y)
    : base (x, y)
    {
    }

    public Vector2d(Point2d pt)
    : base(pt.X, pt.Y)
    {
    }

    public Vector2d(Point2d p0, Point2d p1)
      : base(p1.X - p0.X, p1.Y - p0.Y)
    {
    }

    public Vector2d Add(Vector2d other)
    {
      return new Vector2d(m_x + other.m_x, m_y + other.m_y);
    }

    public Vector2d Subtract(Vector2d other)
    {
      return new Vector2d(m_x - other.m_x, m_y - other.m_y);
    }

    public double Length
    {
      get { return Math.Sqrt(m_x*m_x + m_y*m_y);  }
    }

    public Vector2d Scale(double scale)
    {
      return new Vector2d(m_x*scale, m_y*scale);
    }

    public Vector2d Normalize()
    {
      return Scale(1/Length);
    }

    public double Dot(Vector2d other)
    {
      return m_x*other.X + m_y*other.Y;
    }

    public double Cross(Vector2d other)
    {
      return m_x*other.Y - m_y*other.X;
    }

    public static Vector2d XAxis()
    {
      return new Vector2d(1.0, 0.0);
    }

    public static Vector2d YAxis()
    {
      return new Vector2d(0.0, 1.0);
    }



       
    }
}
