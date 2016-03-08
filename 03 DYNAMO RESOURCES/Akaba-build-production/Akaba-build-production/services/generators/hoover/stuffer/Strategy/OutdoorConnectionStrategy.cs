namespace stuffer
{
  public class OutdoorConnectionStrategy : Strategy
  {
    private OutdoorConnectionStrategy(StrategyArgs strategyArgs)
    : base(strategyArgs)
    {
    }

    public static OutdoorConnectionStrategy Create(StrategyArgs strategyArgs)
    {
      return new OutdoorConnectionStrategy(strategyArgs);
    }

    public override StrategyArgs Execute()
    {
      if (m_strategyArgs.BuildData != null)
      {
        Area area = m_strategyArgs.BuildData.GetLevelArea(0);
      }

      return m_strategyArgs;
    }

  }
}
