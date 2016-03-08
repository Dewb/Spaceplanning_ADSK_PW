using System.Collections.Generic;

namespace stuffer
{
  public class ListStrategy : Strategy
  {
    private List<Strategy> m_strategies;

    private ListStrategy(StrategyArgs strategyArgs, List<Strategy> strategies)
    : base(strategyArgs)
    {
      m_strategies = strategies;
    }

    public static ListStrategy Create(StrategyArgs strategyArgs, List<Strategy> strategies)
    {
      return new ListStrategy(strategyArgs, strategies);
    }

    public override StrategyArgs Execute()
    {
      if (m_strategies != null)
      {
        foreach (Strategy strategy in m_strategies)
        {
          strategy.Execute();
        }
      }

      return m_strategyArgs;
    }
  }
}
