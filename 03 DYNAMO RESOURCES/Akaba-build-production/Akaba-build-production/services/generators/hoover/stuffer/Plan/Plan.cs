namespace stuffer
{
  public class Plan
  {
    public static StrategyArgs Execute(StrategyArgs strategyArgs, Strategy strategy)
    {
      if (strategyArgs == null)
        return null;

      strategyArgs.Reset();
      if (strategy == null)
        return strategyArgs;

      return strategy.Execute();
    }
  }
}
