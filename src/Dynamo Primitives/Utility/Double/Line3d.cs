using System;
using System.Diagnostics;

namespace stuffer
{
  [DebuggerDisplay("{m_start} - {m_end}")]
  public class Line3d
  {
    private Point3d m_start;
    private Point3d m_end;

    internal Line3d(Point3d start, Point3d end)
    {
      m_start = start;
      m_end = end;
    }

    public Point3d StartPoint
    {
      get { return m_start; }
    }

    public Point3d EndPoint
    {
      get { return m_end; }
    }

    public bool Compare(Line3d other)
    {
      if (other.StartPoint.Compare(m_start) && other.EndPoint.Compare(m_end))
        return true;

      if (other.StartPoint.Compare(m_end) && other.EndPoint.Compare(m_start))
        return true;

      return false;
    }

    public override string ToString()
    {
      return String.Format("{0} - {1}", m_start, m_end);
    }
  }
}
