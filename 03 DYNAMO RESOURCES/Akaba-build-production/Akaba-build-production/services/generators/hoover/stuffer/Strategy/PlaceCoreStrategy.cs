namespace stuffer
{
  public class PlaceCoreStrategy : Strategy
  {
    private PlaceCoreStrategy(StrategyArgs strategyArgs)
    : base(strategyArgs)
    {
    }

    public static PlaceCoreStrategy Create(StrategyArgs strategyArgs)
    {
      return new PlaceCoreStrategy(strategyArgs);
    }

    public override StrategyArgs Execute()
    {
      if (m_strategyArgs.BuildData != null)
      {
        Area area = m_strategyArgs.BuildData.GetLevelArea(0);
        Rect2i areaRect = area.GridRect;

        GridBasis basis = m_strategyArgs.BuildData.GridBasis;
        double stairX = 3;
        double stairY = 4;
        Point2i stairSize = new Point2i(basis.ToGrid(0, stairX), basis.ToGrid(1, stairY));
        double hallwayWidth = 4;

        // TODO: Fix this for hallway orientation
        int hallwayGridWidth = basis.ToGrid(0, hallwayWidth);

        Point2i coreSize = new Point2i(
          hallwayGridWidth*2 + stairSize.X,
          hallwayGridWidth*2 + stairSize.Y);

        Point2i coreTL = new Point2i(
          areaRect.TL.X + (areaRect.Width - coreSize.X)/2,
          areaRect.TL.X + (areaRect.Height - coreSize.Y)/2);
        Point2i coreBR = new Point2i(
          coreTL.X + coreSize.X,
          coreTL.Y + coreSize.Y);
        Rect2i coreRect = new Rect2i(coreTL, coreBR);
        CreateSpaceStrategy.Create(m_strategyArgs, 0, coreRect, "Core").Execute();
      }

      return m_strategyArgs;
    }

  }
}
