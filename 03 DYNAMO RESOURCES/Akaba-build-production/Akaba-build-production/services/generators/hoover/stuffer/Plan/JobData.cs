namespace stuffer
{
  public class JobData
  {
    private Requirements m_requirements;

    internal JobData(Requirements requirements)
    {
      m_requirements = requirements;
    }

    /// <summary>
    /// Create a new JobData object from the given Requirements
    /// </summary>
    /// <param name="requirements">The Requirements data to use. This will be stored in the Class</param>
    /// <returns>A newly-constructed Requirements object</returns>
    public static JobData ByRequirements(Requirements requirements)
    {
      return new JobData(requirements);
    }

    /// <summary>
    /// Get the Requirements used to create this JobData
    /// </summary>
    public Requirements Requirements
    {
      get { return m_requirements; }
    }
  }
}
