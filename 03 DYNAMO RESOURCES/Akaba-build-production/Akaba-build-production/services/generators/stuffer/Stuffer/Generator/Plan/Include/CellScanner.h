#pragma once

class Area;

class CellScanner : public GridScanner
{
public:
  CellScanner(Area& area, int index);

  bool processLoc(const GridRef& loc, GridVisitor& visitor) const;

private:
  Area* area;
};
