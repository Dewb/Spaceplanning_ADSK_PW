namespace stuffer
{
  public class StrategyArgs
  {
    private JobData m_jobData;
    private BuildData m_buildData;
    private Genome m_genome;
    private Ribosome m_ribosome;
    private Building m_design;

    internal StrategyArgs(JobData jobData, BuildData buildData, Genome genome, Building design)
    {
      m_jobData = jobData;
      m_buildData = buildData;
      m_genome = genome;
      m_ribosome = Ribosome.ByGenome(genome);
      m_design = design;
    }

    /// <summary>
    /// Create a new Args object from the given initial data
    /// </summary>
    /// <param name="jobData">The JobData to use. This will be stored in the Class</param>
    /// <param name="buildData">The BuildData to use. This will be stored in the Class</param>
    /// <param name="genome">The Genome to use. The genome may contain data or may be empty. This will be stored in the Class</param>
    /// <param name="design">The Design to use. The design may contain data or may be empty. This will be stored in the Class</param>
    /// <returns>A newly-constructed Requirements object</returns>
    public static StrategyArgs ByInitialData(JobData jobData, BuildData buildData, Genome genome, Building design)
    {
      return new StrategyArgs(jobData, buildData, genome, design);
    }

    internal void Reset()
    {
      m_design.Reset();
    }

    /// <summary>
    /// Get the JobData used to create this Args object
    /// </summary>
    public JobData JobData
    {
      get { return m_jobData; }
    }

    /// <summary>
    /// Get the BuildData used to create this Args object
    /// </summary>
    public BuildData BuildData
    {
      get { return m_buildData; }
    }

    /// <summary>
    /// Get the Genome used to create this Args object
    /// </summary>
    public Genome Genome
    {
      get { return m_genome; }
    }

    /// <summary>
    /// Get the Ribosome contained in this Args object
    /// </summary>
    public Ribosome Ribosome
    {
      get { return m_ribosome; }
    }

    /// <summary>
    /// Get the Design used to create this Args object
    /// </summary>
    public Building Design
    {
      get { return m_design; }
    }
  }
}
