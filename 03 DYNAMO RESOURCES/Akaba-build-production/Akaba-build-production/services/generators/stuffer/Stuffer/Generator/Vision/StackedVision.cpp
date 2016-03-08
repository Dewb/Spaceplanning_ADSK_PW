#include <stdafx.h>
#include <StackedVision.h>
#include <JobRequest.h>
#include <ListStrategy.h>
#include <CreateStairsStrategy.h>
#include <CreateSpaceStrategy.h>

StackedVision::StackedVision(const JobRequest& jobRequest)
: XYVision(jobRequest)
{
  createPlan();
}

void StackedVision::createPlan()
{
  unique_ptr<ListStrategy> list(new ListStrategy());
  //list->addStrategy(make_unique<CreateStairsStrategy>(0, Point2i(10, 10), true));
  list->addStrategy(make_unique<CreateStairsStrategy>(Rangei(0, 20), Point2i(0, 0)));

  Rect2i size(Point2i(0, 0), Point2i(5, 5));
  Point2i offset(0, 10);
  list->addStrategy(make_unique<CreateSpaceStrategy>(0, size + offset, Usage::tag(U("Hall"))));
  list->addStrategy(make_unique<CreateSpaceStrategy>(1, size + offset, Usage::tag(U("lobby"))));
  list->addStrategy(make_unique<CreateSpaceStrategy>(2, size + offset, Usage::tag(U("conference"))));
  list->addStrategy(make_unique<CreateSpaceStrategy>(3, size + offset, Usage::tag(U("restroom"))));
  list->addStrategy(make_unique<CreateSpaceStrategy>(4, size + offset, Usage::tag(U("office"))));


  setRootStrategy(Strategy::const_ref(list));
}
