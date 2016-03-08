using System;
using System.Diagnostics;

namespace stuffer
{
  [DebuggerDisplay("{m_start} - {m_end}")]
  public class Line2d
  {
    private Point2d m_start;
    private Point2d m_end;

    internal Line2d(Point2d start, Point2d end)
    {
      m_start = new Point2d(start);
      m_end = new Point2d(end);
    }

    //ADDED CODE SD : 
    /////////////////////////////////////////////////////////////////////////////////////
    internal Line2d(Point2d centerpt,double extents, double direction)
    {

            if (direction == 0)
            {
                m_start = new Point2d((centerpt.X - extents), centerpt.Y);
                m_end   = new Point2d((centerpt.X + extents), centerpt.Y);

            }
            else
            {
                m_start = new Point2d(centerpt.X, (centerpt.Y - extents));
                m_end = new Point2d(centerpt.X, (centerpt.Y + extents));
            }
        
    }

    internal Line2d(Line2d other)
    {
      m_start = new Point2d(other.StartPoint);
      m_end = new Point2d(other.EndPoint);
    }

    public Point2d StartPoint
    {
      get { return m_start; }
    }

    public Point2d EndPoint
    {
      get { return m_end; }
    }

    public Point2d PointAt(double t)
    {
      return (m_end - m_start)*t + m_start;
    }

    public double Length
    {
      get { return new Vector2d(m_start, m_end).Length; }
    }

    public bool Compare(Line2d other)
    {
      if (other.StartPoint.Compare(m_start) && other.EndPoint.Compare(m_end))
        return true;

      if (other.StartPoint.Compare(m_end) && other.EndPoint.Compare(m_start))
        return true;

      return false;
    }

    public Point2d Intersect(Line2d line, bool clip, bool allowOneFail)
    {
      Vector2d p = new Vector2d(m_start);
      Vector2d q = new Vector2d(line.StartPoint);
      Vector2d r = new Vector2d(p, m_end);
      Vector2d s = new Vector2d(q, line.EndPoint);
      
      double rxs = r.Cross(s);

      // Parallel (could be collinear)
      if (rxs.IsZero())
        return null;

      double t = q.Subtract(p).Cross(s)/rxs;
      if (clip)
      {
        bool tFail = (t.CompareLess(0) || t.CompareGreater(1));
        if (tFail && !allowOneFail)
          return null;

        double u = q.Subtract(p).Cross(r)/rxs;
        bool uFail = (u.CompareLess(0) || u.CompareGreater(1));
        if (uFail && !allowOneFail)
          return null;

        if (allowOneFail && tFail && uFail)
          return null;
      }

      return p.Add(r.Scale(t));
    }

    public Point2d Intersect(Line2d line, bool clip = false)
    {
      return Intersect(line, clip, false);
    }

    public Point2d Heal(Line2d line, bool start)
    {
      Point2d other = start ? m_start : m_end;

      if (new Vector2d(line.StartPoint, other).Length.IsZero())
        return line.StartPoint;

      if (new Vector2d(line.EndPoint, other).Length.IsZero())
        return line.EndPoint;

      return Intersect(line);
    }

    public bool Contains(Point2d point)
    {
      Vector2d vec1 = new Vector2d(point, m_start);
      if (vec1.Length.IsZero())
        return true;

      Vector2d vec2 = new Vector2d(point, m_end);
      if (vec2.Length.IsZero())
        return true;

      //double addLength = vec1.Length + vec2.Length;
      //if (addLength.CompareGreater(Length, Point1d.DotCompare))
      //  return false;

      double area = vec1.Cross(vec2)/0.5;
      if (area.CompareGreater(Length, Point1d.DotCompare))
        return false;

      return true;
    }


    

    internal Line2d Join(Line2d current)
    {
      if (m_start.Compare(current.StartPoint))
        return new Line2d(current.EndPoint, m_end);

      if (m_end.Compare(current.StartPoint))
        return new Line2d(current.EndPoint, m_start);

      if (m_start.Compare(current.EndPoint))
        return new Line2d(current.StartPoint, m_end);

      if (m_end.Compare(current.EndPoint))
        return new Line2d(current.StartPoint, m_start);

      return null;
    }


    //ADDED CODE : SD
    internal void move(double distX, double distY)
        {
            m_start = new Point2d((m_start.X+distX),(m_start.Y+distY));
            m_end = new Point2d((m_end.X + distX), (m_end.Y + distY));

        }

    

    public override string ToString()
    {
      return String.Format("{0} - {1}", m_start, m_end);
    }
  }
}
