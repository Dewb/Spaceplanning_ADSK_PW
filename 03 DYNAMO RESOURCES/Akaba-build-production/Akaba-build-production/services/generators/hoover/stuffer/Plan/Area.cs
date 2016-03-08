using System.Collections.Generic;
using System.Linq;

namespace stuffer
{
  public class Area
  {
    int m_level;
    SortedList<int, SortedList<int, List<Line2i>>> m_gridSegments;
    Rect2i m_gridRect;
    SortedList<int, SortedList<int, Cell>> m_cells;
    List<Cell> m_cellList;
    
    internal Area(int level)
    {
      m_level = level;
    }

    public static Area ByLevel(int level)
    {
      return new Area(level);
    }

    public int Level
    {
      get { return m_level; }
    }

    public SortedList<int, SortedList<int, List<Line2i>>> GridSegments
    {
      get 
      {
        if (m_gridSegments == null)
          m_gridSegments = new SortedList<int, SortedList<int, List<Line2i>>>();

        return m_gridSegments;
      }
    }

    public Rect2i GridRect
    {
      get
      {
        if (m_gridRect == null)
        {
          var xMin = 0;
          var xMax = 0;
          var yMin = 0;
          var yMax = 0;

          if (m_gridSegments != null && m_gridSegments.Count == 2)
          {
            xMin = m_gridSegments[0].ElementAt(0).Key;
            xMax = m_gridSegments[0].ElementAt(m_gridSegments[0].Count - 1).Key;
            yMin = m_gridSegments[1].ElementAt(0).Key;
            yMax = m_gridSegments[1].ElementAt(m_gridSegments[1].Count - 1).Key;
          }

          var tl = new Point2i(xMin, yMin);
          var br = new Point2i(xMax, yMax);
          m_gridRect = new Rect2i(tl, br);
        }

        return m_gridRect;
      }
    }

    internal bool CanCreateCell(int x, int y)
    {
      List<Line2i> ax0;
      if (!m_gridSegments[0].TryGetValue(x, out ax0))
        return false;

      List<Line2i> ax1;
      if (!m_gridSegments[0].TryGetValue(x + 1, out ax1))
        return false;

      List<Line2i> ay0;
      if (!m_gridSegments[1].TryGetValue(y, out ay0))
        return false;

      List<Line2i> ay1;
      if (!m_gridSegments[1].TryGetValue(y + 1, out ay1))
        return false;

      return true;
    }

    public Cell TryCreateCell(int x, int y)
    {
      if (m_cells == null)
        m_cells = new SortedList<int, SortedList<int, Cell>>();

      SortedList<int, Cell> axisCells;
      if (!m_cells.TryGetValue(x, out axisCells))
      {
        axisCells = new SortedList<int, Cell>();
        m_cells[x] = axisCells;
      }

      Cell cell;
      if (!axisCells.TryGetValue(y, out cell))
      {
        if (CanCreateCell(x, y))
        {
          cell = new Cell(x, y);

          if (m_cellList == null)
            m_cellList = new List<Cell>();
          m_cellList.Add(cell);

          // TODO: Connect neighbors here
        }

        axisCells[y] = cell;
      }

      return cell;
    }

    public Cell GetCell(int x, int y)
    {
      if (m_gridRect == null)
        return null;

      if (m_cells == null)
        return null;

      if (x < m_gridRect.TL.X || x > m_gridRect.BR.X)
        return null;

      if (y < m_gridRect.TL.Y || y > m_gridRect.BR.Y)
        return null;

      return m_cells[x][y];
    }

    public List<Cell> CellList
    {
      get { return m_cellList; }
    }
  }
}
