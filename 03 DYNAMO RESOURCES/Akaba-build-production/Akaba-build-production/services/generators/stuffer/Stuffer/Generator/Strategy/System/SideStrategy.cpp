#include <stdafx.h>
#include <SideStrategy.h>
#include <Args.h>
#include <Phenotype.h>
#include <BuildData.h>
#include <Floor.h>
#include <EdgeVisitor.h>
#include <CellScanner.h>
#include <CellHelper.h>

SideStrategy::SideStrategy()
{
  m_sides = { 0, 1, 2, 3 };
}

void SideStrategy::setSides(const list<int>& sides)
{
  m_sides = sides;
}

void SideStrategy::processSides(const Args& args) const
{
  const BuildData& data = args.getBuildData();
  for (const auto& areaIt : data.getAreas())
  {
    list<int> sides(m_sides);
    sides = args.getRibosome().getOrder(sides);

    for (auto side : sides)
    {
      unique_ptr<EdgeVisitor> visitor(getVisitor(args, nullptr));
      if (!visitor)
        continue;

      CellScanner processor(*areaIt.second, side);
      processor.process(*visitor);
      for (const auto& space : visitor->getSpaces())
      {
        args.getPhenotype().createSection(areaIt.first, space, getUsage());
        args.getBuildData().setClaimed(areaIt.first, space);
      }
    }
  }
}

unique_ptr<EdgeVisitor> SideStrategy::getVisitor(const Args&, unique_ptr<CellHelper>) const
{
  return make_unique<EdgeVisitor>(std::numeric_limits<int>::max(), false, nullptr);
}
