namespace stuffer
{
    internal class Range1i
  {
    protected int m_low;
    protected int m_high;

    internal Range1i(int v0, int v1)
    {
      if (v0 < v1)
      {
        m_low = v0;
        m_high = v1;
      }
      else 
      {
        m_low = v1;
        m_high = v0;
      }
    }

    public int Low
    {
      get { return m_low; }
    }

    public int High
    {
      get { return m_high; }
    }
  }
}
