#include <stdafx.h>
#include <TrapelloVision.h>
#include <JobRequest.h>
#include <ListStrategy.h>
#include <ShellStrategy.h>
#include <LevelConnectionStrategy.h>
#include <OutdoorConnectionStrategy.h>
#include <EdgeSpacesStrategy.h>
#include <RingHallwayStrategy.h>
#include <SpaceDivisionStrategy.h>
#include <AreaDivisionStrategy.h>
#include <SetUsageStrategy.h>
#include <OptimizingStrategy.h>
#include <HillClimbingOptimizer.h>
#include <UsageAnalyzer.h>

TrapelloVision::TrapelloVision(const JobRequest& jobRequest)
: XYVision(jobRequest)
{
  createPlan();
}

void TrapelloVision::createPlan()
{
  unique_ptr<ListStrategy> list(new ListStrategy());
  list->addStrategy(make_unique<ShellStrategy>());
  list->addStrategy(make_unique<LevelConnectionStrategy>(jobData->getStairRect(basis)));
  list->addStrategy(make_unique<OutdoorConnectionStrategy>(Rangei(10, 20)));
  list->addStrategy(make_unique<EdgeSpacesStrategy>(Rangei(5, 15), 2));
  list->addStrategy(make_unique<RingHallwayStrategy>(jobData->getHallwayWidth(basis)));
  list->addStrategy(make_unique<AreaDivisionStrategy>(10, Rangei(1, 2)));
  list->addStrategy(make_unique<SpaceDivisionStrategy>());

  list->addStrategy(
    make_unique<OptimizingStrategy>(
      
      // NOTE: g++ only supports up to three make_unique arguments
      //make_unique<HillClimbingOptimizer>(0.8f, 1000, 0.10f, 0.20f),
      unique_ptr<HillClimbingOptimizer>(new HillClimbingOptimizer(0.8f, 1000, 0.10f, 0.20f)),

      make_unique<UsageAnalyzer>(),
      make_unique<SetUsageStrategy>()));

  setRootStrategy(Strategy::const_ref(list));
}
