#pragma once

class Cell
{
public:
  static Bag::param_tag claimed;
  static Bag::param_tag areaId;

  Cell(const Point2i& loc, BagCache::ref cache);

  Cell& operator=(const Cell& other) = delete;

  void reset();

  const Point2i& getLoc() const;

  void setHNeighbor(int side, Cell* pNeighbor);
  Cell* getHNeighbor(int side) const;
  void setVNeighbor(bool above, Cell* pNeighbor);
  Cell* getVNeighbor(bool above) const;

  Bag::ref data;

private:
  const Point2i loc;
  map<int, Cell*> hNeighbors;
  Cell* vUpNeighbor;
  Cell* vDnNeighbor;
};
