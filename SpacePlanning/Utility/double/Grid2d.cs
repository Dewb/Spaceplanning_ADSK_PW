using System;
using System.Collections.Generic;

namespace stuffer
{
  public class Grid2d
  {
    private Outline2d m_outline;
    private GridBasis m_basis;
    private Range2d m_extents;
    List<RangeAxis2d> m_extentsAxes;
    List<RangeAxis2d> m_axes;

    internal Grid2d(Outline2d outline, GridBasis basis)
    {
      m_outline = outline;
      m_basis = basis;
      m_extents = m_basis.Extend(m_outline.BBox);
      m_axes = null;
    }

    public static Grid2d ByOutline(Outline2d outline, GridBasis basis)
    {
      return new Grid2d(outline, basis);
    }

    public GridBasis Basis
    {
      get { return m_basis; }
    }

    public List<RangeAxis2d> GetExtentsAxes()
    {
      if (m_extents == null)
        return null;

      if (m_extentsAxes == null)
      {
        RangeAxis2d yLines = new RangeAxis2d(m_basis, 0);
        int minX = m_basis.ToGrid(0, m_extents.Min.X);
        int maxX = m_basis.ToGrid(0, m_extents.Max.X);
        for (int x = minX; x <= maxX; ++x)
          yLines.Add(x, new Range1d(m_extents.Min.Y, m_extents.Max.Y));

        RangeAxis2d xLines = new RangeAxis2d(m_basis, 1);
        int minY = m_basis.ToGrid(0, m_extents.Min.Y);
        int maxY = m_basis.ToGrid(0, m_extents.Max.Y);
        for (int y = minY; y <= maxY; ++y)
          xLines.Add(y, new Range1d(m_extents.Min.X, m_extents.Max.X));

        m_extentsAxes = new List<RangeAxis2d>();
        m_extentsAxes.Add(yLines);  // Ordinal 0
        m_extentsAxes.Add(xLines);  // Ordinal 1
      }

      return m_extentsAxes;
    }

    internal bool PointIsValid(Point2d point, Line2d segment)
    {
      if (point == null)
        return false;

      if (!m_extents.Contains(point))
        return false;

      return segment.Contains(point);
    }

    internal void GetAxisIntersections(RangeAxis2d gridAxis, Polygon2d polygon, ref PointAxis2d pointAxis)
    {
      if (polygon == null)
        return;

      List<Line2d> lines = polygon.Lines;
      if (lines == null)
        return;

      foreach (RangeAxis2d.Value axisValue in gridAxis.CreateExtentsList())
      {
        for (int index = 0; index < lines.Count; ++index)
        {
          Line2d test = axisValue.ToLine(m_basis, gridAxis.Ordinal);
          Line2d other = lines[index];
          Point2d point = test.Intersect(other, true);
          if (PointIsValid(point, other))
          {
            pointAxis.Add(PointAxis2d.Value.FromPoint2d(m_basis, gridAxis.Ordinal, point, true));
          }
        }
      }
    }

    internal RangeAxis2d GetAxisSegments(PointAxis2d intersections)
    {
      int ordinal = intersections.Ordinal;
      RangeAxis2d collinearSegments = new RangeAxis2d(m_basis, ordinal);
      foreach (Polygon2d polygon in m_outline.Polygons)
      {
        foreach (Line2d line in polygon.Lines)
        {
          RangeAxis2d.Value value = RangeAxis2d.Value.FromLine2d(m_basis, ordinal, line, false);
          if (value != null)
            collinearSegments.Add(value);
        }
      }

      RangeAxis2d segments = new RangeAxis2d(m_basis, ordinal);
      foreach (KeyValuePair<int, List<double>> value in intersections.Points)
      {
        int abscissa = value.Key;
        List<double> ordinates = value.Value;

        if (ordinates.Count < 2)
          continue;

        for (int index = 0; index < ordinates.Count - 1; )
        {
          double first = ordinates[index++];
          double next = ordinates[index++];

          if (index < ordinates.Count)
          {
            Range1d segment = collinearSegments.Contains(new PointAxis2d.Value(abscissa, next));
            if (segment != null && ordinates.Count % 3 == 0)
              next = ordinates[index++];
          }

          segments.Add(abscissa, new Range1d(first, next));
        }
      }

      return segments;
    }

    public List<PointAxis2d> GetGridLineIntersections(List<RangeAxis2d> gridAxes)
    {
      if (gridAxes == null)
        return null;

      List<PointAxis2d> pointAxes = new List<PointAxis2d>();

      foreach (RangeAxis2d gridAxis in gridAxes)
      {
        PointAxis2d pointAxis = new PointAxis2d(m_basis, gridAxis.Ordinal);
        pointAxes.Add(pointAxis);

        foreach (Polygon2d polygon in m_outline.Polygons)
        {
          if (polygon != null)
            GetAxisIntersections(gridAxis, polygon, ref pointAxis);
        }
      }

      return pointAxes;
    }

    internal List<RangeAxis2d> GetGridAxes()
    {
      if (m_axes == null)
      {
        List<RangeAxis2d> basicGridAxes = GetExtentsAxes();
        List<PointAxis2d> gridIntersections = GetGridLineIntersections(basicGridAxes);
        if (gridIntersections == null)
          return null;

        m_axes = new List<RangeAxis2d>();
        foreach (PointAxis2d axisIntersections in gridIntersections)
        {
          RangeAxis2d segments = GetAxisSegments(axisIntersections);
          m_axes.Add(m_basis.TrimToGrid(segments));
        }
      }

      return m_axes;
    }

    public List<List<Line2d>> GridLines
    {
      get
      {
        if (m_outline == null)
          return null;

        if (m_extents == null)
          return null;

        List<List<Line2d>> lines = new List<List<Line2d>>();
        foreach (var axis in GetGridAxes())
          lines.Add(axis.CreateLineList());

        return lines;
      }
    }
  }
}
