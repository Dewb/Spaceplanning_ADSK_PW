namespace stuffer
{
  internal class Mesh
  {
    private string m_name;
    private string m_url;
    private string m_inlineData;
    private Point3d m_position;
    private Point3d m_dimensions;
    private double m_rotation;

    internal Mesh(
      string name,
      string url,
      string inlineData,
      Point3d position,
      Point3d dimensions,
      double rotation)
    {
      m_name = name;
      m_url = url;
      m_inlineData = inlineData;
      m_position = position;
      m_dimensions = dimensions;
      m_rotation = rotation;
    }

    public static Mesh ByInlineData(
      string name,
      string inlineData,
      Point3d position = null)
    {
      if (position == null)
        position = Point3d.Origin;

      STL stl = STL.ByASCII(inlineData);

      return new Mesh(
        name,
        "",
        inlineData,
        position,
        stl.BBox.Span,
        0);
    }

    public string Name
    {
      get { return m_name; }
    }

    public string URL
    {
      get { return m_url; }
    }

    public string InlineData
    {
      get { return m_inlineData; }
    }

    public Point3d Position
    {
      get { return m_position; }
    }

    public Point3d Dimensions
    {
      get { return m_dimensions; }
    }

    public double Rotation
    {
      get { return m_rotation; }
    }
  }
}
