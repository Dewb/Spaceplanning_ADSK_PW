using System.Collections.Generic;

namespace stuffer
{
  public class BuildData
  {
    // Primary build data
    private GridBasis m_basis;

    // Secondary build data
    private Shell m_shell;
    private SortedList<int, Outline2d> m_outlines;
    private SortedList<int, Grid2d> m_grids;
    private SortedList<int, Area> m_levels;

    internal BuildData(GridBasis basis)
    {
      // Set primary data
      m_basis = basis;

      // Default initialize secondary data
      m_shell = null;
      m_outlines = null;
      m_grids = null;
      m_levels = null;
    }

    public static BuildData ByBasis(GridBasis basis)
    {
      return new BuildData(basis);
    }

    /// <summary>
    /// Get the Shell contained in this BuildData object. The value may be null.
    /// </summary>
    public GridBasis GridBasis
    {
      get { return m_basis; }
    }

    /// <summary>
    /// Get the Shell contained in this BuildData object. The value may be null.
    /// </summary>
    public Shell Shell
    {
      set { m_shell = value; }
      get { return m_shell; }
    }

    public Outline2d FloorOutline(int level)
    {
      if (m_shell == null)
        return null;

      if (m_outlines == null)
        m_outlines = new SortedList<int, Outline2d>();

      Outline2d outline;
      if (!m_outlines.TryGetValue(level, out outline))
      {
        double height = m_basis.FloorHeight(level);
        outline = m_shell.GetFloorOutline(height);
        if (outline == null)
          return null;

        m_outlines[level] = outline;
      }

      return outline;
    }

    public Grid2d FloorGrid(int level)
    {
      if (m_shell == null)
        return null;

      if (m_grids == null)
        m_grids = new SortedList<int, Grid2d>();

      Grid2d grid;
      if (!m_grids.TryGetValue(level, out grid))
      {
        Outline2d outline = FloorOutline(level);
        if (outline == null)
          return null;

        grid = new Grid2d(outline, m_basis);
        m_grids[level] = grid;
      }

      return grid;
    }

    internal int GetTag(int ordinal, Line2d segment)
    {
      // NOTE: This will need to be updated if another coordinate system is ever used
      double value = (ordinal == 0) ? segment.StartPoint.X : segment.StartPoint.Y;
      return m_basis.ToGrid(ordinal, value);
    }

    internal void StoreGridSegment(int ordinal, SortedList<int, SortedList<int, List<Line2i>>> gridSegments, Line2d segment)
    {
      SortedList<int, List<Line2i>> axisSegments;
      if (!gridSegments.TryGetValue(ordinal, out axisSegments))
      {
        axisSegments = new SortedList<int, List<Line2i>>();
        gridSegments[ordinal] = axisSegments;
      }

      int majorTag = GetTag(ordinal, segment);
      List<Line2i> valueSegments;
      if (!axisSegments.TryGetValue(majorTag, out valueSegments))
      {
        valueSegments = new List<Line2i>();
        axisSegments[majorTag] = valueSegments;
      }

      valueSegments.Add(m_basis.ToGrid(segment));
    }

    public Area GetLevelArea(int level)
    {
      if (m_levels == null)
        m_levels = new SortedList<int, Area>();

      Area area;
      if (m_levels.TryGetValue(level, out area))
        return area;

      area = Area.ByLevel(level);
      if (m_shell != null)
      {
        // Map of index (x and y axis) -> map int -> list of lines
        SortedList<int, SortedList<int, List<Line2i>>> gridSegments = area.GridSegments;

        // List of axis (x or y)
        List<List<Line2d>> lines = FloorGrid(level).GridLines;
        for (int ordinal = 0; ordinal < lines.Count; ++ordinal)
        {
          List<Line2d> axis = lines[ordinal];

          // List of axis segments
          foreach (Line2d segment in axis)
          {
            StoreGridSegment(
              ordinal,
              gridSegments, 
              new Line2d(
                new Point2d(
                  segment.StartPoint.X,
                  segment.StartPoint.Y),
                new Point2d(
                  segment.EndPoint.X,
                  segment.EndPoint.Y)));
          }
        }

        Rect2i gridRect = area.GridRect;
        for (int x = gridRect.TL.X; x < gridRect.BR.X; ++x)
        {
          for (int y = gridRect.TL.Y; y < gridRect.BR.Y; ++y)
          {
            Cell cell = area.TryCreateCell(x, y);
          }
        }
      }

      m_levels[level] = area;

      return area;
    }
  }
}
