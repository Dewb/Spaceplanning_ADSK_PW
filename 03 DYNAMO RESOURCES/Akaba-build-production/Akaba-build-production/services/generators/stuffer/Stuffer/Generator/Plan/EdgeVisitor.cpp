#include <stdafx.h>
#include <EdgeVisitor.h>
#include <Cell.h>
#include <CellHelper.h>

EdgeVisitor::EdgeVisitor(int amount, bool atEdgeOnly, unique_ptr<CellHelper> helper)
: amount(amount),
  atEdgeOnly(atEdgeOnly),
  helper(move(helper))
{
  stripReset();
}

void EdgeVisitor::setAmount(int amount_)
{
  amount = amount_;
}

const list<Rect2i>& EdgeVisitor::getSpaces() const
{
  return spaces;
}

void EdgeVisitor::outerLoopReset()
{
  spaces.clear();
  stripReset();
}

void EdgeVisitor::innerLoopComplete()
{
  stripReset();
}

bool EdgeVisitor::noCell()
{
  stripReset();

  return false;
}

bool EdgeVisitor::processCell(const GridRef& loc, Cell& cell)
{
  // Do not start counting until a non-claimed space is encountered
  if (cell.data->getBoolParam(Cell::claimed, false))
  {
    stripReset();
    return atEdgeOnly;
  }

  // Only count until the given amount
  if (++count > amount)
    return true;

  // Add the cell to the collection of rects
  addLoc(loc);

  return false;
}

namespace
{
  bool extendsV(const Rect2i& space, const Rect2i& strip)
  {
    if (strip.l() != space.l())
      return false;
    if (strip.r() != space.r())
      return false;
    
    if (strip.b() == space.t() - 1)
      return true;
    if (strip.t() == space.b() + 1)
      return true;

    return false;
  }

  bool extendsH(const Rect2i& space, const Rect2i& strip)
  {
    if (strip.t() != space.t())
      return false;
    if (strip.b() != space.b())
      return false;

    if (strip.r() == space.l() - 1)
      return true;
    if (strip.l() == space.r() + 1)
      return true;

    return false;
  }
}

void EdgeVisitor::stripReset()
{
  unique_ptr<Rect2i> strip(move(inStrip));
  inStrip.reset();
  count = 0;

  if (strip)
  {
    for (auto& space : spaces)
    {
      if (extendsV(*strip, space) || extendsH(*strip, space))
      {
        space.inflate(*strip);
        return;
      }
    }

    spaces.push_back(*strip);
  }
}

void EdgeVisitor::addLoc(const GridRef& loc)
{
  if (inStrip)
  {
    // Space found, inflate it
    inStrip->inflate(loc);
  }
  else
  {
    // Start a new strip
    inStrip = make_unique<Rect2i>(loc, loc);
  }
}
