using System;
using System.Collections.Generic;

namespace stuffer
{
  public class GridBasis
  {
    ///////////////////////////////////////////////////////////////////////////////////////
    // Internals

    private List<Vector2d> m_axes;
    private List<double> m_gridSize;
    private double m_floorHeight;

    internal GridBasis(double gridSize, double floorHeight)
    {
      // 2D Grid with equal spacing
      m_gridSize = new List<double>();
      m_gridSize.Add(gridSize);
      m_gridSize.Add(gridSize);

      m_floorHeight = floorHeight;

      m_axes = new List<Vector2d>();
      m_axes.Add(Vector2d.XAxis());
      m_axes.Add(Vector2d.YAxis());
    }

    public static GridBasis BySize(double gridSize, double floorHeight)
    {
      return new GridBasis(gridSize, floorHeight);
    }

    public List<Vector2d> Vectors
    {
      get { return m_axes; }
    }

    public double GridSize(int ordinal)
    {
      return m_gridSize[ordinal];
    }

    public double FloorHeight(int level)
    {
      // TODO: Remove this, for rendering purposes only...
      if (level == 0)
        return 0.01;

      return level*m_floorHeight;
    }

    public double CeilingHeight(int level)
    {
      return (level + 1)*m_floorHeight;
    }

    public int Level(double height)
    {
      return (int)Math.Floor(height/m_floorHeight);
    }

    ///////////////////////////////////////////////////////////////////////////////////////
    // Grid Utility

    private double InflateToGrid(int ordinal, double value)
    {
      if (value > 0.0)
        return Ceiling(ordinal, value);

      return Floor(ordinal, value);
    }

    private Point2d InflateToGrid(Point2d point)
    {
      return new Point2d(InflateToGrid(0, point.X), InflateToGrid(1, point.Y));
    }

    public Range2d Extend(Range2d extents)
    {
      if (extents == null)
        return null;

      Range2d extended = null;
      extended = new Range2d(extended, InflateToGrid(extents.Min));
      extended = new Range2d(extended, InflateToGrid(extents.Max));

      return extended;
    }

    public RangeAxis2d TrimToGrid(RangeAxis2d other)
    {
      RangeAxis2d axis = new RangeAxis2d(this, other.Ordinal);
      foreach (KeyValuePair<int, List<Range1d>> segments in other.Lines)
      {
        foreach (Range1d segment in segments.Value)
        {
          double p0 = Ceiling(axis.Ordinal, segment.Min);
          double p1 = Floor(axis.Ordinal, segment.Max);
          axis.Add(segments.Key, new Range1d(p0, p1));
        }
      }

      return axis;
    }

    ///////////////////////////////////////////////////////////////////////////////////////
    // Conversions

    // Single value conversions; Floor, Ceiling, ToGrid (Round)

    private static double SnapThreshold = 1.0/8.0;

    public double Floor(int ordinal, double value)
    {
      double gridValue = FromGrid(ordinal, ToGrid(ordinal, value));
      if (value.Compare(gridValue, SnapThreshold))
        return value;

      return Math.Floor(value/GridSize(ordinal))*GridSize(ordinal);
    }

    public double Ceiling(int ordinal, double value)
    {
      double gridValue = FromGrid(ordinal, ToGrid(ordinal, value));
      if (value.Compare(gridValue, SnapThreshold))
        return value;

      return Math.Ceiling(value/GridSize(ordinal))*GridSize(ordinal);
    }

    public int ToGrid(int ordinal, double value)
    {
      return (int)Math.Round(value/GridSize(ordinal));
    }

    public double FromGrid(int ordinal, int value)
    {
      return value*GridSize(ordinal);
    }

    // Point conversion

    public Point2i ToGrid(Point2d point)
    {
      if (point == null)
        return null;

      return new Point2i(ToGrid(0, point.X), ToGrid(1, point.Y));
    }

    public Point2d FromGrid(Point2i point)
    {
      if (point == null)
        return null;

      return new Point2d(FromGrid(0, point.X), FromGrid(1, point.Y));
    }

    // Line conversion

    public Line2i ToGrid(Line2d line)
    {
      if (line == null)
        return null;

      return new Line2i(
        ToGrid(line.StartPoint),
        ToGrid(line.EndPoint));
    }

    // Space conversion

    public Space FromGrid(Region region)
    {
      if (region == null)
        return null;

      int level = region.Floor.Level;
      Point2d tl = FromGrid(region.Rect.TL);
      Point2d br = FromGrid(region.Rect.BR);
      double h0 = FloorHeight(level);
      double h1 = CeilingHeight(level);

      Point3d position = new Point3d(tl + (br - tl)/2.0, h0);
      Point3d dimensions = new Point3d(br - tl, h1 - h0);
      double rotation = 0.0;

      return
        new Space(
          position,
          dimensions,
          region.Bag.Get<string>("usage", "unknown"),
          rotation,
          region.Bag.Get<bool>("isCirculation", false));
    }
  }
}
