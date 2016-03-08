#include <stdafx.h>
#include <BuildingCoreVision.h>
#include <JobRequest.h>
#include <ListStrategy.h>
#include <CreateStairsStrategy.h>
#include <CreateSpaceStrategy.h>

BuildingCoreVision::BuildingCoreVision(const JobRequest& jobRequest)
: XYVision(jobRequest)
{
  createPlan();
}

void BuildingCoreVision::createPlan()
{
  Rangei levels(0, 5);

  unique_ptr<ListStrategy> list(new ListStrategy());
  list->addStrategy(make_unique<CreateStairsStrategy>(levels, Point2i(0, 0)));

  for (auto level : levels)
  {
    Rect2f sizeH(Point2f(11.0f, 4.0f));
    list->addStrategy(make_unique<CreateSpaceStrategy>(level, basis.toGrid(sizeH + Point2f(-4.0f, -4.0f)), Usage::tag(U("Hall"))));
    list->addStrategy(make_unique<CreateSpaceStrategy>(level, basis.toGrid(sizeH + Point2f(-4.0f, 5.0f)), Usage::tag(U("Hall"))));
    Rect2f sizeV(Point2f(4.0f, 5.0f));
    list->addStrategy(make_unique<CreateSpaceStrategy>(level, basis.toGrid(sizeV + Point2f(-4.0f, 0.0f)), Usage::tag(U("Hall"))));
    list->addStrategy(make_unique<CreateSpaceStrategy>(level, basis.toGrid(sizeV + Point2f(3.0f, 0.0f)), Usage::tag(U("Hall"))));
  }

  setRootStrategy(Strategy::const_ref(list));
}
