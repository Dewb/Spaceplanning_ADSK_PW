using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace stuffer
{
    public class PointAxis2d
  {
    [DebuggerDisplay("{m_abscissa}, [{m_ordinate}]")]
    public class Value
    {
      private int m_abscissa;
      private double m_ordinate;

      public Value(int abscissa, double ordinate)
      {
        m_abscissa = abscissa;
        m_ordinate = ordinate;
      }

      public static Value FromPoint2d(GridBasis basis, int ordinal, Point2d point, bool snap)
      {
        if (ordinal == 0)
        {
          int abscissa = basis.ToGrid(ordinal, point.X);
          if (!snap && !point.X.Compare(basis.FromGrid(ordinal, abscissa)))
            return null;

          return new Value(abscissa, point.Y);
        }

        if (ordinal == 1)
        {
          int abscissa = basis.ToGrid(ordinal, point.Y);
          if (!snap && !point.Y.Compare(basis.FromGrid(ordinal, abscissa)))
            return null;

          return new Value(abscissa, point.X);
        }

        return null;
      }

      public Point2d ToPoint(GridBasis basis, int ordinal)
      {
        double v0 = basis.FromGrid(ordinal, m_abscissa);
        double v1 = m_ordinate;

        if (ordinal == 0)
          return new Point2d(v0, v1);

        if (ordinal == 1)
          return new Point2d(v1, v0);

        return null;
      }

      public int Abscissa
      {
        get { return m_abscissa; }
      }

      public double Ordinate
      {
        get { return m_ordinate; }
      }

      public override string ToString()
      {
        return String.Format("{0:F3}, [{1:F3}]", m_abscissa, m_ordinate);
      }
    }

    private GridBasis m_basis;
    private int m_ordinal;
    private SortedList<int, List<double>> m_points;

    public PointAxis2d(GridBasis basis, int ordinal)
    {
      m_basis = basis;
      m_ordinal = ordinal;
      m_points = new SortedList<int, List<double>>();
    }

    public void Add(PointAxis2d.Value value)
    {
      List<double> points;
      if (!m_points.TryGetValue(value.Abscissa, out points))
      {
        points = new List<double>();
        m_points[value.Abscissa] = points;
        points.Add(value.Ordinate);
        return;
      }

      points.Add(value.Ordinate);
      points.Sort();
    }

    public int Ordinal
    {
      get { return m_ordinal; }
    }

    public SortedList<int, List<double>> Points
    {
      get { return m_points; }
    }

    public List<Point2d> CreatePointList()
    {
      List<Point2d> points = new List<Point2d>();
      foreach (KeyValuePair<int, List<double>> point in m_points)
      {
        foreach (double value in point.Value)
        {
          points.Add(new Value(point.Key, value).ToPoint(m_basis, m_ordinal));
        }
      }

            
          

      return points;
    }
  }
}
