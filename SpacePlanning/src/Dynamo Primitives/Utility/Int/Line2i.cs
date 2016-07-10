namespace stuffer
{
    public class Line2i
  {
    private Point2i m_start;
    private Point2i m_end;

    internal Line2i(Point2i start, Point2i end)
    {
      m_start = start;
      m_end = end;
    }

    public Point2i StartPoint
    {
      get { return m_start; }
    }

    public Point2i EndPoint
    {
      get { return m_end; }
    }
  }
}
