namespace stuffer
{
    public class Facet3d
  {
    private Point3d m_p0;
    private Point3d m_p1;
    private Point3d m_p2;

    internal Facet3d(Point3d p0, Point3d p1, Point3d p2)
    {
      m_p0 = p0;
      m_p1 = p1;
      m_p2 = p2;
    }

    public static Facet3d ByPoints(Point3d p0, Point3d p1, Point3d p2)
    {
      return new Facet3d(p0, p1, p2);
    }

    public Point3d P0
    {
      get { return m_p0; }
    }

    public Point3d P1
    {
      get { return m_p1; }
    }

    public Point3d P2
    {
      get { return m_p2; }
    }
  }
}
