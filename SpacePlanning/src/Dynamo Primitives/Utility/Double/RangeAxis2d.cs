using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace stuffer
{
  public class RangeAxis2d
  {
    [DebuggerDisplay("{m_abscissa}, [({m_ordinate.Min}, {m_ordinate.Max}]")]
    public class Value
    {
      private int m_abscissa;
      private Range1d m_ordinate;

      public Value(int abscissa, Range1d ordinate)
      {
        m_abscissa = abscissa;
        m_ordinate = ordinate;
      }

      public static Value FromLine2d(GridBasis basis, int ordinal, Line2d line, bool snap)
      {
        PointAxis2d.Value p0 = PointAxis2d.Value.FromPoint2d(basis, ordinal, line.StartPoint, snap);
        if (p0 == null)
          return null;

        PointAxis2d.Value p1 = PointAxis2d.Value.FromPoint2d(basis, ordinal, line.EndPoint, snap);
        if (p1 == null)
          return null;

        if (p0.Abscissa != p1.Abscissa)
          return null;

        return new Value(p0.Abscissa, new Range1d(p0.Ordinate, p1.Ordinate));
      }

      public Line2d ToLine(GridBasis basis, int ordinal)
      {
        return
          new Line2d(
            new PointAxis2d.Value(m_abscissa, m_ordinate.Min).ToPoint(basis, ordinal),
            new PointAxis2d.Value(m_abscissa, m_ordinate.Max).ToPoint(basis, ordinal));
      }

      public int Abscissa
      {
        get { return m_abscissa; }
      }

      public Range1d Ordinate
      {
        get { return m_ordinate; }
      }

      public override string ToString()
      {
        return String.Format("{0:F3}, [({1:F3}, {1:F3})]", m_abscissa, m_ordinate.Min, m_ordinate.Max);
      }
    }

    private GridBasis m_basis;
    private int m_ordinal;
    private SortedList<int, List<Range1d>> m_lines;

    public RangeAxis2d(GridBasis basis, int ordinal)
    {
      m_basis = basis;
      m_ordinal = ordinal;
      m_lines = new SortedList<int, List<Range1d>>();
    }

    public int Ordinal
    {
      get { return m_ordinal; }
    }

    public SortedList<int, List<Range1d>> Lines
    {
      get { return m_lines; }
    }

    public void Add(int value, Range1d range)
    {
      List<Range1d> segments;
      if (!m_lines.TryGetValue(value, out segments))
      {
        segments = new List<Range1d>();
        m_lines[value] = segments;
        segments.Add(range);
        return;
      }

      segments.Add(range);
      segments.Sort(new Range1d.Comparer());

      for (int index = 0; index < segments.Count - 1; ++index)
      {
        if (segments[index].Overlaps(segments[index + 1]))
        {
          segments[index] = new Range1d(segments[index], segments[index + 1]);
          segments.RemoveAt(index + 1);
          --index;
        }
      }
    }

    public void Add(RangeAxis2d.Value value)
    {
      Add(value.Abscissa, value.Ordinate);
    }

    public Range1d Contains(PointAxis2d.Value value)
    {
      List<Range1d> segments;
      if (!m_lines.TryGetValue(value.Abscissa, out segments))
        return null;

      foreach (Range1d segment in segments)
      {
        if (segment.Contains(value.Ordinate))
          return segment;
      }

      return null;
    }

    public List<RangeAxis2d.Value> CreateExtentsList()
    {
      List<RangeAxis2d.Value> extentLines = new List<RangeAxis2d.Value>();
      foreach (KeyValuePair<int, List<Range1d>> line in m_lines)
      {
        Range1d extents = null;
        foreach (Range1d range in line.Value)
          extents = new Range1d(extents, range);

        extentLines.Add(new RangeAxis2d.Value(line.Key, extents));
      }

      return extentLines;
    }

    public List<Line2d> CreateLineList()
    {
      List<Line2d> lines = new List<Line2d>();
      foreach (KeyValuePair<int, List<Range1d>> line in m_lines)
      {
        foreach (Range1d range in line.Value)
        {
          lines.Add(new RangeAxis2d.Value(line.Key, range).ToLine(m_basis, m_ordinal));
        }
      }

      return lines;
    }
  }
}
