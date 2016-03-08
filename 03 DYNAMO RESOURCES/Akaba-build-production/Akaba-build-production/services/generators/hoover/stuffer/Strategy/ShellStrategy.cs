namespace stuffer
{
  public class ShellStrategy : Strategy
  {
    private Shell m_shell;

    private ShellStrategy(StrategyArgs strategyArgs, Shell shell)
    : base(strategyArgs)
    {
      m_shell = shell;
    }

    public static ShellStrategy Create(StrategyArgs strategyArgs, Shell shell)
    {
      return new ShellStrategy(strategyArgs, shell);
    }

    public override StrategyArgs Execute()
    {
      if (m_strategyArgs.BuildData != null)
        m_strategyArgs.BuildData.Shell = m_shell;

      return m_strategyArgs;
    }
  }
}
