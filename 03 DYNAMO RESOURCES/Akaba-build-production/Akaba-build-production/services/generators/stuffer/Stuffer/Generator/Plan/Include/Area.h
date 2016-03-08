#pragma once

class Cell;
class CellHelper;

class Area
{
  friend class BuildData;

public:
  Cell* getCell(const Point2i& loc);
  const Cell* getCell(const Point2i& loc) const;
  const map<int, Cell*>& getCells() const;

  Rect2i getBounds() const;

  using area_id = int;
  using sub_area = pair<Rect2i, area_id>;
  using sub_areas = list<sub_area>;
  unique_ptr<sub_areas> calculateSubAreas(Rect2i* pRect, const CellHelper& helper) const;

  void claimSpace(const Rect2i& space);

private:
  int levelNum;
  using area_col = map<int, unique_ptr<Cell>>;
  map<int, area_col> m_cells;
  map<int, Cell*> m_cellMap;
  unique_ptr<Rect2i> m_pBounds;
  BagCache::ref cache;

  Area(int levelNum, BagCache::ref cache);
  Cell& createCell(const Point2i& loc);
  void reset();

  using range_cells = pair<Rangei, list<Cell*>>;
  using range_data = list<range_cells>;
  unique_ptr<range_data> collectRanges(const area_col& data, const CellHelper& helper) const;
};
