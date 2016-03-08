#include <stdafx.h>
#include <BuildData.h>
#include <Shell.h>
#include <SpaceLayout.h>
#include <Area.h>
#include <Cell.h>
#include <Phenotype.h>

BuildData::BuildData(const GridBasis& basis)
: basis(basis),
  cache(new BagCache())
{
}

BuildData::~BuildData() = default;

const GridBasis& BuildData::getBasis() const
{
  return basis;
}

void BuildData::setExistingDesign(const Design& design, Phenotype& phenotype)
{
  for (auto& space : design.spaces)
  {
    const Point3f& origin3d(space.origin);
    const Point3f& dim3d(space.dimensions);
    const Point2f origin2d(origin3d.x(), origin3d.y());
    const Point2f dim2d(dim3d.x(), dim3d.y());
    const Point2f dim2dDiv2(dim2d/2.0f);
    const Rangef heightRange(origin3d.z(), origin3d.z() + dim3d.z());

    const Point2i pi0(basis.toGrid(origin2d - dim2dDiv2));
    const Point2i pi1(basis.toGrid(origin2d + dim2dDiv2) - 1);
    const Rect2i rect(pi0, pi1);
    Rangei levelRange(basis.levelNum(heightRange.l()), basis.levelNum(heightRange.h()) - 1);

    phenotype.createFloor(levelRange);
    phenotype.createSection(levelRange, rect, Usage::tag(space.usage));
    setClaimed(levelRange, rect);
  }
}

void BuildData::setXYShell(unique_ptr<const Shell> shell_, Phenotype& phenotype)
{
  shell = move(shell_);

  const Shell::level_map& levels(shell->getLevels());
  for (const auto& levelMapIt : levels)
  {
    Area& levelArea(createLevelArea(levelMapIt.first));
    const Grid& grid(levelMapIt.second->getGrid());

    // TODO: Possibly do something about hard-coded axis?
    const Grid::line_map& xLines = grid.getLines(U("x"));
    const Grid::line_map& yLines = grid.getLines(U("y"));

    const vector<Rangei>* xPrev = nullptr;
    for (const auto& xIt : xLines)
    {
      if (xPrev)
      {
        const vector<Rangei>* yPrev = nullptr;
        for (const auto& yIt : yLines)
        {
          if (yPrev)
            createCellIfNeeded(
            levelArea,
            Point2i(xIt.first - 1, yIt.first - 1),
            *xPrev, xIt.second,
            *yPrev, yIt.second);

          yPrev = &yIt.second;
        }
      }

      xPrev = &xIt.second;
    }
  }

  for (const auto& levelIt : shell->getLevels())
    phenotype.createFloor(levelIt.first);
}

const Shell& BuildData::getShell() const
{
  return *shell;
}

void BuildData::reset()
{
  for (auto& areaIt : getAreas())
  {
    areaIt.second->reset();
  }
}

BuildData& BuildData::createSnapshot()
{
  cache->createSnapshot();

  return *this;
}

void BuildData::mergeSnapshot()
{
  cache->mergeSnapshot();
}

void BuildData::discardSnapshot()
{
  cache->discardSnapshot();
}

Area& BuildData::createLevelArea(int levelNum)
{
  levelAreas[levelNum].reset(new Area(levelNum, cache));
  return *levelAreas[levelNum];
}

void BuildData::createCellIfNeeded(
  Area& levelArea,
  const Point2i& loc,
  const vector<Rangei>& xLines1,
  const vector<Rangei>& xLines2,
  const vector<Rangei>& yLines1,
  const vector<Rangei>& yLines2)
{
  // NOTE: xLines are x-axis gridlines that are in the y-direction
  for (const auto& x1 : xLines1)
  {
    for (const auto& x2 : xLines2)
    {
      int yMin = max(x1.l(), x2.l());
      int yMax = min(x1.h(), x2.h());
      if (yMin <= loc.y() && yMax > loc.y())
      {
        // NOTE: yLines are y-axis gridlines that are in the x-direction
        for (const auto& y1 : yLines1)
        {
          for (const auto& y2 : yLines2)
          {
            int xMin = max(y1.l(), y2.l());
            int xMax = min(y1.h(), y2.h());
            if (xMin <= loc.x() && xMax > loc.x())
            {
              createCell(levelArea, loc);
              return;
            }
          }
        }
      }
    }
  }
}

void BuildData::createCell(
  Area& levelArea,
  const Point2i& loc)
{
  Cell& cell(levelArea.createCell(loc));

  Area* levelUp(getLevelArea(levelArea.levelNum + 1));
  if (levelUp)
  {
    Cell* neighbor(levelUp->getCell(loc));
    if (neighbor)
      cell.setVNeighbor(true, neighbor);
  }

  Area* levelDn(getLevelArea(levelArea.levelNum - 1));
  if (levelDn)
  {
    Cell* neighbor(levelDn->getCell(loc));
    if (neighbor)
      cell.setVNeighbor(false, neighbor);
  }
}

Rangei BuildData::getLevelRange() const
{
  if (levelAreas.size() == 0)
    return Rangei();

  Rangei levels(levelAreas.begin()->first);
  for (auto& areaIt : levelAreas)
    levels.inflate(areaIt.first);

  return levels;
}

Point2i BuildData::getMaxAreaSize() const
{
  if (levelAreas.size() == 0)
    return Point2i();

  Rect2i maxBounds(levelAreas.begin()->second->getBounds());
  for (auto& areaIt : levelAreas)
    maxBounds.inflate(areaIt.second->getBounds());
  Point2i cellCount(maxBounds.size());
  cellCount.x() += 1;
  cellCount.y() += 1;

  return cellCount;
}

const Area* BuildData::getLevelArea(int level) const
{
  const auto& areaIt = levelAreas.find(level);
  if (areaIt == levelAreas.end())
    return nullptr;

  return areaIt->second.get();
}

Area* BuildData::getLevelArea(int level)
{
  auto areaIt(levelAreas.find(level));
  if (areaIt == levelAreas.end())
    return nullptr;

  return areaIt->second.get();
}

const BuildData::level_map& BuildData::getAreas() const
{
  return levelAreas;
}

void BuildData::setClaimed(int level, const Rect2i& rect)
{
  Area* area(getLevelArea(level));
  if (!area)
    return;

  for (auto x(rect.l()); x < rect.r() + 1; ++x)
  {
    for (auto y(rect.t()); y < rect.b() + 1; ++y)
    {
      Cell* cell(area->getCell(Point2i(x, y)));
      if (cell)
        cell->data->setBoolParam(Cell::claimed, true);
    }
  }
}

void BuildData::setClaimed(const Rangei& levels, const Rect2i& rect)
{
  for (auto level(levels.l()); level <= levels.h(); ++level)
    setClaimed(level, rect);
}
