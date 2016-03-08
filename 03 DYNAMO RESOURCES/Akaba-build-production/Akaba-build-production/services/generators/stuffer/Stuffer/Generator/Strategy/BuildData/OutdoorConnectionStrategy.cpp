#include <stdafx.h>
#include <OutdoorConnectionStrategy.h>
#include <Args.h>
#include <Phenotype.h>
#include <BuildData.h>
#include <Floor.h>
#include <Area.h>
#include <EdgeVisitor.h>
#include <CellScanner.h>
#include <CellHelperNotClaimed.h>

OutdoorConnectionStrategy::OutdoorConnectionStrategy(Rangei range)
{
  setRange(range);
}

const string_t& OutdoorConnectionStrategy::name() const
{
  static string_t name(U("OutdoorConnectionStrategy"));
  return name;
}

void OutdoorConnectionStrategy::execute(const Args& args) const
{
  BuildData& data(args.getBuildData());
  auto area(data.getLevelArea(0));
  if (!area)
    return;

  auto orientation(args.getRibosome().getBool());

  Point2i desired(m_range.l(), m_range.h());
  auto amount(orientation ? desired.x() : desired.y());
  auto other(orientation ? desired.y() : desired.x());

  list<int> sides(m_sides);
  sides = args.getRibosome().getOrder(sides);

  auto complete(false);
  for (auto side : sides)
  {
    if (complete)
      break;

    unique_ptr<EdgeVisitor> visitor(getVisitor(args, nullptr));
    if (!visitor)
      continue;

    visitor->setAmount(amount);
    CellScanner processor(*area, side);
    processor.process(*visitor);

    vector<Rect2i> potential;
    for (const auto& space : visitor->getSpaces())
    {
      if (space.w() == amount - 1)
      {
        if (space.h() < other)
          continue;

        auto offset(args.getRibosome().getAmount(Rangei(0, space.h() + 1 - other)));

        Rect2i offsetSpace(
          Point2i(space.l(), space.t() + offset),
          Point2i(space.r(), space.t() + offset + other - 1));
        potential.push_back(offsetSpace);
      }
      else
      {
        if (space.h() != amount - 1)
          continue;

        if (space.w() < other)
          continue;

        auto offset(args.getRibosome().getAmount(Rangei(0, space.w() + 1 - other)));

        Rect2i offsetSpace(
          Point2i(space.l() + offset, space.t()),
          Point2i(space.l() + offset + other - 1, space.b()));
        potential.push_back(offsetSpace);
      }

      if (!potential.empty())
      {
        auto select(args.getRibosome().getAmount(Rangei(0, static_cast<int>(potential.size()) - 1)));

        const Rect2i& rect(potential[select]);
        args.getPhenotype().createSection(0, rect, getUsage());
        args.getBuildData().setClaimed(0, rect);
        complete = true;
        break;
      }
    }
  }
}

Usage::name_tag OutdoorConnectionStrategy::getUsage() const
{
  return Usage::tag(U("lobby"));
}

unique_ptr<EdgeVisitor> OutdoorConnectionStrategy::getVisitor(const Args&, unique_ptr<CellHelper> helper) const
{
  return make_unique<EdgeVisitor>(0, m_atEdgeOnly, move(helper));
}
