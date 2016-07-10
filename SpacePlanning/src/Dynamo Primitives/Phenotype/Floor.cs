using System.Collections.Generic;

namespace stuffer
{
    public class Floor
  {
    private int m_level;
    private List<Region> m_regions;

    internal Floor(int level)
    {
      m_level = level;
      m_regions = new List<Region>();
    }

    internal Region CreateRegion(Rect2i rect)
    {
      Region region = new Region(this, rect);
      m_regions.Add(region);

      return region;
    }

    /// <summary>
    /// Create a new empty Floor object at the given level
    /// </summary>
    /// <returns>A newly-constructed Floor object</returns>
    public static Floor ByLevel(int level)
    {
      return new Floor(level);
    }

    /// <summary>
    /// Get the level of this Floor object
    /// </summary>
    public int Level
    {
      get { return m_level; }
    }

    /// <summary>
    /// Get the Region objects in this Floor object
    /// </summary>
    public List<Region> Regions
    {
      get { return m_regions; }
    }
  }
}
