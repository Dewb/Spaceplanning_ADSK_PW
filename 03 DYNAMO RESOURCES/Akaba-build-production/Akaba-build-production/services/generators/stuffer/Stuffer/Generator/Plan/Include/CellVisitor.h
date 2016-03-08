#pragma once

class Cell;

class CellVisitor : public GridVisitor
{
public:
  virtual bool noCell() { return false; }
  virtual bool processCell(const GridRef& loc, Cell& cell) = 0;
};
