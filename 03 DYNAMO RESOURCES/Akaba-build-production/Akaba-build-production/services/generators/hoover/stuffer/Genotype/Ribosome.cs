namespace stuffer
{
  public class Ribosome
  {
    private Genome m_genome;
    //private uint m_position;

    internal Ribosome(Genome genome)
    {
      m_genome = genome;
      //m_position = 0;
    }

    /// <summary>
    /// Create a new Ribosome object attached to the beginning of the given Genome 
    /// </summary>
    /// <param name="genome">The genome to attach to. This will be stored in the Class</param>
    /// <returns>A newly-constructed Ribosome object</returns>
    public static Ribosome ByGenome(Genome genome)
    {
      return new Ribosome(genome);
    }
  }
}
