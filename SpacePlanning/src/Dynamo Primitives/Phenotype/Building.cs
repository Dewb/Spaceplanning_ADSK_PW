using System.Collections.Generic;

namespace stuffer
{
  internal class Building
  {
    private SortedList<int, Floor> m_floors;

    internal Building()
    {
      m_floors = new SortedList<int, Floor>();
    }

    internal void Reset()
    {
      m_floors.Clear();
    }

    internal void CreateFloor(int level)
    {
      Floor floor;
      if (!m_floors.TryGetValue(level, out floor))
      {
        floor = new Floor(level);
        m_floors.Add(level, floor);
      }
    }

    internal void CreateRegion(int level, Rect2i rect, string usage)
    {
      Floor floor;
      if (m_floors.TryGetValue(level, out floor))
      {
        // TODO: Add basis and cache use after implementation
        Region region = floor.CreateRegion(rect); //, basis, cache);
        region.Bag.Add("usage", usage);
        
        // TODO: Accumulate to design usages here
        //usages.add(tag, section->getRect().area());
      }
    }

    public static Building Empty()
    {
      return new Building();
    }

    public IList<Floor> Floors
    {
      get { return m_floors.Values; }
    }
  }
}
