namespace stuffer
{
    public class Rect2i
  {
    protected Point2i m_tl;
    protected Point2i m_br;

    internal Rect2i(Point2i tl, Point2i br)
    {
      m_tl = tl;
      m_br = br;
    }

    public static Rect2i ByCorners(Point2i tl, Point2i br)
    {
      return new Rect2i(tl, br);
    }

    public Point2i TL
    {
      get { return m_tl; }
    }

    public Point2i BR
    {
      get { return m_br; }
    }

    public int Width
    {
      get { return m_br.X - m_tl.X; }
    }

    public int Height
    {
      get { return m_br.Y - m_tl.Y; }
    }
  }
}
