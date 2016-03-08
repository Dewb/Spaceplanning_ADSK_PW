#include <stdafx.h>
#include <CellScanner.h>
#include <Area.h>
#include <CellVisitor.h>

CellScanner::CellScanner(Area& area, int index)
: GridScanner(area.getBounds(), index),
  area(&area)
{
}

bool CellScanner::processLoc(const GridRef& loc, GridVisitor& visitor) const
{
  Cell* cell(area->getCell(loc));
  if (!cell)
    return dynamic_cast<CellVisitor&>(visitor).noCell();
  else
    return dynamic_cast<CellVisitor&>(visitor).processCell(loc, *cell);
}
