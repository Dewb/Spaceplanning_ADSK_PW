#include <stdafx.h>
#include <ShellAndCoreVision.h>
#include <ListStrategy.h>
#include <ShellStrategy.h>
#include <ExistingDesignStrategy.h>
#include <EdgeSpacesStrategy.h>
#include <RingHallwayStrategy.h>
#include <SpaceDivisionStrategy.h>

ShellAndCoreVision::ShellAndCoreVision(const JobRequest& jobRequest)
: XYVision(jobRequest)
{
  createPlan();
}

void ShellAndCoreVision::createPlan()
{
  unique_ptr<ListStrategy> list(new ListStrategy());
  list->addStrategy(make_unique<ShellStrategy>());
  list->addStrategy(make_unique<ExistingDesignStrategy>());
  list->addStrategy(make_unique<EdgeSpacesStrategy>(Rangei(5, 5), 0));
//  list->addStrategy(make_unique<RingHallwayStrategy>(jobData->getHallwayWidth(basis)));
//  list->addStrategy(make_unique<SpaceDivisionStrategy>());

  setRootStrategy(Strategy::const_ref(list));
}
