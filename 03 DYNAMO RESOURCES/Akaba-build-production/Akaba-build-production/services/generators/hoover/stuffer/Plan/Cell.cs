namespace stuffer
{
  public class Cell
  {
    int m_x;
    int m_y;
    Rect2i m_rect;

    internal Cell(int x, int y)
    {
      m_x = x;
      m_y = y;
      m_rect =
        new Rect2i(
          new Point2i(x, y),
          new Point2i(x + 1, y + 1));
    }

    public int X
    {
      get { return m_x; }
    }

    public int Y
    {
      get { return m_y; }
    }

    public Rect2i Rect
    {
      get { return m_rect; }
    }
  }
}
