namespace stuffer
{
  public abstract class Strategy
  {
    protected StrategyArgs m_strategyArgs;

    protected Strategy(StrategyArgs strategyArgs)
    {
      m_strategyArgs = strategyArgs;
    }

    public abstract StrategyArgs Execute();
  }
}
