namespace stuffer
{
    public class Point2i
  {
    protected int m_x;
    protected int m_y;

    internal Point2i(int x, int y)
    {
      m_x = x;
      m_y = y;
    }

    public static Point2i ByCoordinates(int x, int y)
    {
      return new Point2i(x, y);
    }

    public int X
    {
      get { return m_x; }
    }

    public int Y
    {
      get { return m_y; }
    }
  }
}
