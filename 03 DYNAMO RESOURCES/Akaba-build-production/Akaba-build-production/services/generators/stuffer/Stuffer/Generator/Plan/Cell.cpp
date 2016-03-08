#include <stdafx.h>
#include <Cell.h>

Bag::param_tag Cell::claimed(BagCache::reserve());
Bag::param_tag Cell::areaId(BagCache::reserve());

Cell::Cell(const Point2i& loc, BagCache::ref cache)
: loc(loc),
  vUpNeighbor(nullptr),
  vDnNeighbor(nullptr),
  data(cache->createBag())
{
}

void Cell::reset()
{
  data->setBoolParam(claimed, false);
  data->setIntParam(areaId, -1);
}

const Point2i& Cell::getLoc() const
{
  return loc;
}

void Cell::setHNeighbor(int side, Cell* pNeighbor)
{
  hNeighbors[side] = pNeighbor;

  int opposite = (side + 2) % 4;
  Cell* pThis = pNeighbor->getHNeighbor(opposite);
  if (pThis != this)
    pNeighbor->setHNeighbor(opposite, this);
}

Cell* Cell::getHNeighbor(int side) const
{
  auto it = hNeighbors.find(side);
  if (it == hNeighbors.end())
    return nullptr;

  return it->second;
}

void Cell::setVNeighbor(bool above, Cell* pNeighbor)
{
  if (above)
    vUpNeighbor = pNeighbor;
  else
    vDnNeighbor = pNeighbor;

  Cell* pThis = pNeighbor->getVNeighbor(!above);
  if (pThis != this)
    pNeighbor->setVNeighbor(!above, this);
}

Cell* Cell::getVNeighbor(bool above) const
{
  return (above ? vUpNeighbor : vDnNeighbor);
}
