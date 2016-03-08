#include <stdafx.h>
#include <StairPlacementVision.h>
#include <JobRequest.h>
#include <ListStrategy.h>
#include <CreateStairsStrategy.h>
#include <CreateSpaceStrategy.h>

StairPlacementVision::StairPlacementVision(const JobRequest& jobRequest)
: XYVision(jobRequest)
{
  createPlan();
}

void StairPlacementVision::createPlan()
{
  unique_ptr<ListStrategy> list(new ListStrategy());
  //list->addStrategy(make_unique<CreateStairsStrategy>(0, Point2i(10, 10), true));
  list->addStrategy(make_unique<CreateStairsStrategy>(Rangei(0, 10), Point2i(0, 0)));
  list->addStrategy(make_unique<CreateSpaceStrategy>(0, Rect2i(Point2i(0, 10), Point2i(16, 16)), Usage::tag(U("Unknown"))));
  list->addStrategy(make_unique<CreateSpaceStrategy>(5, Rect2i(Point2i(0, 10), Point2i(16, 16)), Usage::tag(U("Unknown"))));


  setRootStrategy(Strategy::const_ref(list));
}
