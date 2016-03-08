#include <stdafx.h>
#include <Area.h>
#include <Cell.h>
#include <CellHelper.h>

Area::Area(int levelNum, BagCache::ref cache)
: levelNum(levelNum),
  cache(cache)
{
}

void Area::reset()
{
  for (auto cell : m_cellMap)
    cell.second->reset();
}

namespace
{
  int pointToIndex(const Point2i& loc)
  {
    return loc.x() * 0x7FFF + loc.y();
  }
}

Cell& Area::createCell(const Point2i& loc)
{
  if (!m_pBounds)
    m_pBounds.reset(new Rect2i(loc, loc));
  else
    m_pBounds->inflate(loc);

  unique_ptr<Cell> newCell(new Cell(loc, cache));
  Cell& cell(*newCell);
  m_cells[loc.x()][loc.y()] = move(newCell);
  m_cellMap[pointToIndex(loc)] = &cell;

  list<Point2i> sOff{ { -1, 0 }, { 0, -1 }, { 1, 0 }, { 0, 1 } };
  int side(0);
  for (auto& off : sOff)
  {
    Point2i testLoc(loc.x() + off.x(), loc.y() + off.y());
    Cell* pNeighbor(getCell(testLoc));
    if (pNeighbor)
      cell.setHNeighbor(side, pNeighbor);
    ++side;
  }

  return cell;
}

Cell* Area::getCell(const Point2i& loc)
{
  auto it(m_cellMap.find(pointToIndex(loc)));
  if (it == m_cellMap.end())
    return nullptr;

  return it->second;
}

const Cell* Area::getCell(const Point2i& loc) const
{
  auto it(m_cellMap.find(pointToIndex(loc)));
  if (it == m_cellMap.end())
    return nullptr;

  return it->second;
}

const map<int, Cell*>& Area::getCells() const
{
  return m_cellMap;
}

Rect2i Area::getBounds() const
{
  if (!m_pBounds)
    return Rect2i();

  return *m_pBounds;
}

unique_ptr<Area::range_data> Area::collectRanges(const area_col& data, const CellHelper& helper) const
{
  unique_ptr<range_data> pData(new range_data());
  unique_ptr<range_cells> pWorking;
  for (const auto& it : data)
  {
    int value = it.first;
    if (pWorking && value == pWorking->first.h() + 1)
    {
      if (helper.test(*it.second))
      {
        pWorking->first.inflate(value);
        pWorking->second.push_back(it.second.get());
      }
    }
    else
    {
      if (helper.test(*it.second))
      {
        if (pWorking)
          pData->push_back(*pWorking);

        pWorking.reset(new range_cells());
        pWorking->first = Rangei(value, value);
        pWorking->second.push_back(it.second.get());
      }
    }
  }

  if (pWorking)
    pData->push_back(*pWorking);

  return move(pData);
}

namespace
{
  int areaId = 0;
}

unique_ptr<Area::sub_areas> Area::calculateSubAreas(Rect2i* pRect, const CellHelper& helper) const
{
  map<int, list<Cell*>> subAreas;
  list<pair<Rangei, int>> lastRanges;

  Rect2i bounds;
  if (pRect)
    bounds = *pRect;
  else if (m_pBounds)
    bounds = *m_pBounds;

  Point2i loc;
  for (const auto& xIt : m_cells)
  {
    list<pair<Rangei, int>> currentRanges;
    if (loc.x() >= bounds.l() && loc.x() <= bounds.r())
    {
      unique_ptr<range_data> pCurrentRangeCells(collectRanges(xIt.second, helper));
      for (auto& currentIt : *pCurrentRangeCells)
      {
        int lastId = -1;
        if (lastRanges.size() == 0)
        {
          int newId = areaId++;
          currentRanges.push_back(make_pair(currentIt.first, newId));
          subAreas[newId] = move(currentIt.second);
        }
        else
        {
          bool overlapFound(false);
          for (auto& lastIt : lastRanges)
          {
            if (currentIt.first.overlap(lastIt.first))
            {
              overlapFound = true;
              if (lastId == -1 || lastId == lastIt.second)
              {
                lastId = lastIt.second;
                subAreas[lastId].splice(subAreas[lastId].end(), currentIt.second);
                currentRanges.push_back(make_pair(currentIt.first, lastId));
              }
              else
              {
                subAreas[lastId].splice(subAreas[lastId].end(), subAreas[lastIt.second]);
                subAreas.erase(lastIt.second);
              }
            }
          }

          if (!overlapFound)
          {
            int newId = areaId++;
            currentRanges.push_back(make_pair(currentIt.first, newId));
            subAreas[newId] = move(currentIt.second);
          }
        }
      }
    }

    lastRanges = currentRanges;
    ++loc.x();
  }

  unique_ptr<Area::sub_areas> pSubAreas(new Area::sub_areas);
  for (const auto& subAreaIt : subAreas)
  {
    unique_ptr<Rect2i> pRect;
    for (auto& pCell : subAreaIt.second)
    {
      pCell->data->setIntParam(Cell::areaId, subAreaIt.first);
      if (!pRect)
      {
        pRect.reset(new Rect2i(pCell->getLoc(), pCell->getLoc()));
      }
      else
      {
        pRect->inflate(pCell->getLoc());
      }
    }

    pSubAreas->push_back(make_pair(pRect ? *pRect : Rect2i(), subAreaIt.first));
  }

  return pSubAreas;
}

void Area::claimSpace(const Rect2i& space)
{
  Point2i loc;
  for (loc.x() = space.l(); loc.x() <= space.r(); ++loc.x())
  {
    for (loc.y() = space.t(); loc.y() <= space.b(); ++loc.y())
    {
      Cell* pCell(getCell(loc));
      if (pCell)
        pCell->data->setBoolParam(Cell::claimed, true);
    }
  }
}
