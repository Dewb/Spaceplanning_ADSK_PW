#include <stdafx.h>
#include <CreateStairsStrategy.h>
#include <Args.h>
#include <JobData.h>
#include <BuildData.h>
#include <Phenotype.h>
#include <Floor.h>

CreateStairsStrategy::CreateStairsStrategy()
: CreateStairsStrategy(0, Point2i(0, 0))
{
}

CreateStairsStrategy::CreateStairsStrategy(const Rangei& floors, const Point2i& offset)
: floors(floors),
  offset(offset)
{
}

const string_t& CreateStairsStrategy::name() const
{
  static string_t name(U("CreateStairsStrategy"));
  return name;
}

void CreateStairsStrategy::execute(const Args& args) const
{
  auto& buildData(args.getBuildData());
  Rect2i rect(args.getJobData().getStairRect(buildData.getBasis()));
  rect += offset;

  auto& phenotype(args.getPhenotype());
  phenotype.createFloor(floors);
  phenotype.createSection(floors, rect, Usage::tag(U("Stairs")));
  buildData.setClaimed(floors, rect);
}
