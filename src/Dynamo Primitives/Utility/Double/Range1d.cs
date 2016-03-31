using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace stuffer
{
  [DebuggerDisplay("[{m_min}, {m_max}]")]
  public class Range1d
  {
    private double m_min;
    private double m_max;

    internal Range1d(double value)
    {
      m_min = value;
      m_max = value;
    }

    internal Range1d(double v0, double v1)
    {
      m_min = Math.Min(v0, v1);
      m_max = Math.Max(v0, v1);
    }

    internal Range1d(Range1d other)
    {
      m_min = other.Min;
      m_max = other.Max;
    }

    internal Range1d(Range1d other, double value)
    {
      m_min = Math.Min(other.m_min, value);
      m_max = Math.Max(other.m_max, value);
    }

    internal Range1d(Range1d r0, Range1d r1)
    {
      if (r0 == null)
      {
        m_min = r1.m_min;
        m_max = r1.m_max;
      }
      else
      {
        m_min = Math.Min(r0.m_min, r1.m_min);
        m_max = Math.Max(r0.m_max, r1.m_max);
      }
    }

    public double Min
    {
      get { return m_min; }
    }

    public double Max
    {
      get { return m_max; }
    }

    public double Span
    {
      get { return m_max - m_min; }
    }

    public double Center
    {
      get { return (m_max + m_min)/2; }
    }

    public bool Compare(Range1d value)
    {
      if (!value.m_min.Compare(m_min))
        return false;
      if (!value.m_max.Compare(m_max))
        return false;

      return true;
    }

    public bool Contains(double value)
    {
      if (value.CompareLess(m_min))
        return false;
      if (value.CompareGreater(m_max))
        return false;

      return true;
    }

    public bool Overlaps(Range1d other)
    {
      if (m_max.CompareLess(other.Min))
        return false;
      if (other.Max.CompareLess(m_min))
        return false;

      return true;
    }

    public override string ToString()
    {
      return String.Format("[{0:F3}, {1:F3}]", m_min, m_max);
    }

    internal class Comparer : IComparer<Range1d>
    {
      public int Compare(Range1d r0, Range1d r1)
      {
        if (r0 == null)
          return 0;

        return (r0.Min < r1.Min) ? -1 : 0;
      }
    }
  }
}
