using System;
using System.Diagnostics;

namespace stuffer
{
  [DebuggerDisplay("({m_x}, {m_y})")]
  public class Point2d
  {
    protected double m_x;
    protected double m_y;
    int number = 4;

    internal Point2d(double x, double y)
    {  
      m_x = Math.Round(x,number, MidpointRounding.AwayFromZero);//, number, MidpointRounding.AwayFromZero
      m_y = Math.Round(y,number, MidpointRounding.AwayFromZero);//
       //m_x = x;
       //m_y = y;
    }

    internal Point2d(Point2d other)
    {
      m_x = other.X;
      m_y = other.Y;
    }

    public static Point2d ByCoordinates(double x, double y, int number = 4)
    {
      return new Point2d(Math.Round(x,number),  Math.Round(y,number));
      //return new Point2d(x, y);
    }

    public double X
    {
      get { return m_x; }
      set { m_x = value; }
    }

    public double Y
    {
      get { return m_y; }
      set { m_y = value; }
   }


    public double DistanceTo(Point2d other)
        {
            double dist = m_x * other.X + m_y * other.Y;
            return dist;
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


    //ADDED CODE SD 
     public static Point2d AddVector(Point2d a, Vector2d vec, double scale)
        {
            double x = a.X + vec.Length*scale;
            double y = a.Y + vec.Length*scale;
            Point2d pt = new Point2d(x, y);
            return pt;
        }

    public static Point2d operator+(Point2d p0, Point2d p1)
    {
      return new Point2d(p0.m_x + p1.m_x, p0.m_y + p1.m_y);
    }

    public static Point2d operator +(Point2d p0, Vector2d vec)
    {
        return new Point2d(vec.m_x + p0.m_x, vec.m_y + p0.m_y);
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
