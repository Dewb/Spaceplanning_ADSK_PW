namespace stuffer
{
  public class CreateSpaceStrategy : Strategy
  {
    private int m_level;
    private Rect2i m_rect;
    private string m_usage;

    private CreateSpaceStrategy(StrategyArgs strategyArgs, int level, Rect2i rect, string usage)
    : base(strategyArgs)
    {
      m_level = level;
      m_rect = rect;
      m_usage = usage;
    }

    public static CreateSpaceStrategy Create(StrategyArgs strategyArgs, int level, Rect2i rect, string usage)
    {
      return new CreateSpaceStrategy(strategyArgs, level, rect, usage);
    }

    public override StrategyArgs Execute()
    {
      Building design = m_strategyArgs.Design;
      design.CreateFloor(m_level);
      design.CreateRegion(m_level, m_rect, m_usage);

      // TODO: Add claimed flag to grid after implementing
      //strategyArgs.getBuildData().setClaimed(level, rect);

      return m_strategyArgs;
    }

  }
}
