#include <stdafx.h>
#include <CreateSpaceStrategy.h>
#include <Args.h>
#include <BuildData.h>
#include <Phenotype.h>

CreateSpaceStrategy::CreateSpaceStrategy(int floor, const Rect2i& rect, Usage::name_tag tag)
: floor(floor),
  rect(rect),
  tag(tag)
{
}

const string_t& CreateSpaceStrategy::name() const
{
  static string_t name(U("CreateSpaceStrategy"));
  return name;
}

void CreateSpaceStrategy::execute(const Args& args) const
{
  auto& phenotype(args.getPhenotype());
  phenotype.createFloor(floor);
  phenotype.createSection(floor, rect, tag);
  args.getBuildData().setClaimed(floor, rect);
}
