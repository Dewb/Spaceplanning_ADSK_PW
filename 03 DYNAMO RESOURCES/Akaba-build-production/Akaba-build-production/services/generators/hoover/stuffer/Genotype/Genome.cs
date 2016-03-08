namespace stuffer
{
  public class Genome
  {
    // In derived class store genome data here
    //private Requirements m_requirements;

    // In derived class enter genome data here, if wanted/needed
    internal Genome(
      //Requirements requirements
      )
    {
      //m_requirements = requirements;
    }

    /// <summary>
    /// Create a new JobData object from the given initial data
    /// </summary>
    /// <param name="requirements">The initial data to use. This will be stored in the Class</param>
    /// <returns>A newly-constructed Genome object</returns>
    public static Genome ByInitialData(
      //Requirements requirements
      )
    {
      return new Genome(
        //requirements
        );
    }
  }
}
