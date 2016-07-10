namespace stuffer
{
    public class Region
  {
    private Floor m_floor;
    private Rect2i m_rect;
    private Bag m_bag;

    internal Region(Floor floor, Rect2i rect)
    {
      m_floor = floor;
      m_rect = rect;
    }

    /// <summary>
    /// Property returning the Floor of the Region
    /// </summary>
    public Floor Floor
    {
      get { return m_floor; }
    }

    /// <summary>
    /// Property returning the GridRect of the Region
    /// </summary>
    public Rect2i Rect
    {
      get { return m_rect; }
    }

    /// <summary>
    /// Property returning the Bag inside the Region
    /// </summary>
    public Bag Bag
    {
      get
      {
        if (m_bag == null)
          m_bag = new Bag();

        return m_bag;
      }
    }
  }
}
