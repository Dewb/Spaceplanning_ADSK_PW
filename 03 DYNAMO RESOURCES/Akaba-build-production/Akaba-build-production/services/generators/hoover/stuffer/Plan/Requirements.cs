namespace stuffer
{
  public class Requirements
  {
    internal Requirements(string json)
    {
      // TODO: Process the JSON data here...
    }

    /// <summary>
    /// Create a new Requirements object using the given JSON data
    /// </summary>
    /// <param name="json">The JSON string to use to create the Requirements object.</param>
    /// <returns>A newly-constructed Requirements object</returns>
    public static Requirements ByJSON(string json)
    {
      return new Requirements(json);
    }
  }
}
