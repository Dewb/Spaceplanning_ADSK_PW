#include <stdafx.h>
#include <LevelConnectionStrategy.h>
#include <Args.h>
#include <BuildData.h>
#include <Area.h>
#include <CellHelperConnectedUp.h>
#include <Shaft.h>
#include <Phenotype.h>
#include <Floor.h>

LevelConnectionStrategy::LevelConnectionStrategy(const Rect2i& stairRect)
: stairRect(stairRect)
{
}

const string_t& LevelConnectionStrategy::name() const
{
  static string_t name(U("LevelConnectionStrategy"));
  return name;
}

void LevelConnectionStrategy::execute(const Args& args) const
{
  BuildData& buildData(args.getBuildData());
  if (buildData.getAreas().size() == 1)
    return;

  map<int, unique_ptr<Area::sub_areas>> connections;
  const auto& areas(buildData.getAreas());
  for (auto areaIt = areas.begin();;++areaIt)
  {
    // No need to check the top floor
    auto nextAreaIt(areaIt);
    if (++nextAreaIt == areas.end())
      break;

    auto subAreas(areaIt->second->calculateSubAreas(nullptr, CellHelperConnectedUp()));
    if (subAreas->size() > 0)
      connections[areaIt->first] = move(subAreas);
  }

  list<Shaft> shafts;
  for (const auto& connection : connections)
    processLevelConnection(shafts, connection.first, *connection.second);

  // For now just try to use a shaft that spans all floors
  Rangei levelRange(buildData.getLevelRange());
  vector<Shaft> potential;
  for (const auto& shaft : shafts)
    if (shaft.getBottomLevel() == levelRange.l() && shaft.getTopLevel() == levelRange.h())
      potential.push_back(shaft);

  if (potential.empty())
    return;

  auto amount(args.getRibosome().getAmount(Rangei(0, static_cast<int>(potential.size()) - 1)));
  const auto& shaft(potential[amount]);

  Point2i size(stairRect.size());
  if (args.getRibosome().getBool())
    size = Point2i(size.y(), size.x());

  Rect2i required(size);
  auto finalRect(args.getRibosome().insetRect(shaft.getRect(), required));
  for (auto levelNum = shaft.getBottomLevel(); levelNum <= shaft.getTopLevel(); ++levelNum)
  {
    args.getPhenotype().createSection(levelNum, finalRect, Usage::tag(U("Stairs")));
    args.getBuildData().setClaimed(levelNum, finalRect);
  }
}

void LevelConnectionStrategy::processLevelConnection(list<Shaft>& shafts, int level, const Area::sub_areas& areas) const
{
  for (const auto& area : areas)
    shafts.push_back(Shaft(level, area.first));
}
