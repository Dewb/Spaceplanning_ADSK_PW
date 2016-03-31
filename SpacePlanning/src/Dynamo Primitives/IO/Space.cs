namespace stuffer
{
  public class Space
  {
    private Point3d m_position;
    private Point3d m_dimensions;
    private string m_usage;
    private double m_rotation;
    private bool m_isCirculation;

    internal Space(Point3d position, Point3d dimensions, string usage, double rotation, bool isCirculation)
    {
      m_position = position;
      m_dimensions = dimensions;
      m_usage = usage;
      m_rotation = rotation;
      m_isCirculation = isCirculation;
    }

    public Point3d Position
    {
      get { return m_position; }
    }

    public Point3d Dimensions
    {
      get { return m_dimensions; }
    }

    public string Usage
    {
      get { return m_usage; }
    }

    public double Rotation
    {
      get { return m_rotation; }
    }

    public bool IsCirculation
    {
      get { return m_isCirculation; }
    }
  }
}
