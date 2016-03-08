#include <stdafx.h>
#include <AreaDivisionStrategy.h>
#include <Args.h>
#include <JobData.h>
#include <JobRequest.h>
#include <Phenotype.h>
#include <BuildData.h>
#include <Cell.h>
#include <CellHelperNotClaimed.h>
#include <Floor.h>

AreaDivisionStrategy::AreaDivisionStrategy(int minSize, const Rangei& numDiv, bool vertical)
: m_minSize(minSize),
  m_numDiv(numDiv),
  m_forceVertical(true),
  m_vertical(vertical)
{
}

AreaDivisionStrategy::AreaDivisionStrategy(int minSize, const Rangei& numDiv)
: m_minSize(minSize),
  m_numDiv(numDiv),
  m_forceVertical(false),
  m_vertical(false)
{
}

const string_t& AreaDivisionStrategy::name() const
{
  static string_t name(U("AreaDivisionStrategy"));
  return name;
}

void AreaDivisionStrategy::execute(const Args& args) const
{
  const auto& data(args.getBuildData());
  for (const auto& areaIt : data.getAreas())
  {
    DivInfo info
    {
      args.getRibosome(),
      args.getJobData().getHallwayWidth(data.getBasis()),
      *areaIt.second,
      args.getPhenotype(),
      areaIt.first
    };

    auto subAreas(info.area.calculateSubAreas(nullptr, CellHelperNotClaimed()));
    divideSubAreas(info, 1, true, move(subAreas));
  }
}

void AreaDivisionStrategy::divideSubAreas(
  DivInfo& info,
  int depth,
  bool vertical,
  unique_ptr<Area::sub_areas> pSubAreas) const
{
  list<Area::sub_area> parents;
  for (const auto& subAreaIt : *pSubAreas)
  {
    if (depth == 1)
    {
      if (m_forceVertical)
      {
        vertical = m_vertical; 
      }
      else
      {
        vertical = info.geneIt.getBool();
      }
    }

    if (divideSubArea(info, vertical, subAreaIt))
      parents.push_back(subAreaIt);
  }

  CellHelperNotClaimed cellHelper;
  unique_ptr<Area::sub_areas> pChildSubAreas(new Area::sub_areas);
  for (const auto& parent : parents)
  {
    cellHelper.setID(parent.second);
    auto children(info.area.calculateSubAreas(nullptr, cellHelper));
    pChildSubAreas->splice(pChildSubAreas->end(), *children);
  }

  if (depth < 2)
    divideSubAreas(info, ++depth, !vertical, move(pChildSubAreas));
}
 
bool AreaDivisionStrategy::divideSubArea(
  DivInfo& info,
  bool vertical,
  const Area::sub_area& subArea) const
{
  int num = info.geneIt.getAmount(m_numDiv);

  if (num > 0)
  {
    const Rect2i& bounds(subArea.first);
    int size = (vertical ? bounds.w() + 1 : bounds.h() + 1);
    int off = vertical ? bounds.tl().x() : bounds.tl().y();

    list<int> slices(calculateSlices(
      info.geneIt,
      subArea.second,
      info.width,
      size,
      off,
      num));

    if (slices.size() > 0)
    {
      slice(info.area, info.phenotype, info.floor, slices, bounds, vertical, info.width);
      return true;
    }
  }

  return false;
}

namespace
{
  void fillList(list<int>& data, int count)
  {
    for (int index = 0; index < count; ++index)
      data.push_back(index + 1);
  }
}

list<int> AreaDivisionStrategy::calculateSlices(
  Ribosome& geneIt, 
  int /*id*/, 
  int width,
  int size,
  int off,
  int num) const
{
  float fCount(((size + width) / static_cast<float>(m_minSize + width)) - 1);
  int count(static_cast<int>(fCount));
  if (num > count)
    num = count;

  list<int> slices;
  if (count > 0)
  {
    list<int> options;
    fillList(options, count);

    for (int index(0); index < num; ++index)
    {
      int optIdx(geneIt.getAmount(Rangei(1, static_cast<int>(options.size()))));

      auto it(options.begin());
      advance(it, optIdx - 1);
      int opt(*it);
      options.erase(it);

      float step((size + width) / static_cast<float>(count + 1));
      float loc(step*opt + off - width / 2.0f);
      slices.push_back(static_cast<int>(loc));
    }
  }

  return slices;
}

void AreaDivisionStrategy::slice(Area& area, Phenotype& phenotype, int floor, const list<int>& slices, const Rect2i& bounds, bool vertical, int width) const
{
  for (auto& slice : slices)
  {
    int loc = slice - width / 2;
    Rect2i space;
    if (vertical)
      space = Rect2i(Point2i(loc, bounds.t()), Point2i(loc + width - 1, bounds.b()));
    else
      space = Rect2i(Point2i(bounds.l(), loc), Point2i(bounds.r(), loc + width - 1));
    phenotype.createSection(floor, space, Usage::tag(U("Hall")));
    area.claimSpace(space);
  }
}
