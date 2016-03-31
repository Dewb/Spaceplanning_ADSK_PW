using System.Collections.Generic;

namespace stuffer
{
  public class Bag
  {
    private Dictionary<string, object> m_data;

    internal Bag()
    {
    }

    /// <summary>
    /// Create a new empty Bag object
    /// </summary>
    /// <returns>A newly-constructed Bag object</returns>
    public static Bag Empty()
    {
      return new Bag();
    }

    /// <summary>
    /// Add a data item to the Bag
    /// </summary>
    /// <param name="tag">The string tag used to identify the particular data item</param>
    /// <param name="data">The data item to store</param>
    public void Add(string tag, object data)
    {
      if (m_data == null)
        m_data = new Dictionary<string, object>();
      
      m_data.Add(tag, data);
    }

    /// <summary>
    /// Gets a data item from the Bag, returning the default if the tag is not found
    /// </summary>
    /// <param name="tag">The string tag used to identify the particular data item</param>
    /// <param name="defaultValue">The default value to return if the tag is not found</param>
    /// <returns>The stored value, or the default value if the tag is not found</returns>
    public T Get<T>(string tag, T defaultValue)
    {
      if (m_data == null)
        return defaultValue;

      object foundObject;
      if (!m_data.TryGetValue(tag, out foundObject))
        return defaultValue;

      if (foundObject.GetType() != typeof(T))
        return defaultValue;

      return (T)foundObject;
    }
  }
}
