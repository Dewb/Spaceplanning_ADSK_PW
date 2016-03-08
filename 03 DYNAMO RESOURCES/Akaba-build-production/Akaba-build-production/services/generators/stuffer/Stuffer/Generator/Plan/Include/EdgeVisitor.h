#pragma once

#include <CellVisitor.h>

class CellHelper;

class EdgeVisitor : public CellVisitor
{
public:
  EdgeVisitor(int amount, bool atEdgeOnly, unique_ptr<CellHelper> helper);

  void setAmount(int amount_);

  const list<Rect2i>& getSpaces() const;

  // GridVisitor methods
  void outerLoopReset();
  void innerLoopComplete();

  // CellVisitor methods
  bool noCell();
  bool processCell(const GridRef& loc, Cell& cell);

private:
  int amount;
  bool atEdgeOnly;
  unique_ptr<CellHelper> helper;

  int count;
  unique_ptr<Rect2i> inStrip;
  list<Rect2i> spaces;

  void stripReset();
  void addLoc(const GridRef& loc);
};
