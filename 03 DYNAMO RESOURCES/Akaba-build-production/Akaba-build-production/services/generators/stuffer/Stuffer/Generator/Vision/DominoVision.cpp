#include <stdafx.h>
#include <DominoVision.h>
#include <LoopStrategy.h>
#include <ListStrategy.h>
#include <ChoiceStrategy.h>
#include <DominoBacktrackStrategy.h>
#include <DominoAddSpaceStrategy.h>

DominoVision::DominoVision(const JobRequest& jobRequest)
: XYVision(jobRequest)
{
  createPlan();
}

void DominoVision::createPlan()
{
  bool requireCirculation(true);
  float nothing(0.0f);
  float backtrack(0.0f);
  float addSpace(1.0f - backtrack - nothing);
  unique_ptr<ChoiceStrategy> choice(new ChoiceStrategy());
  choice->addStrategy(nothing, nullptr);
  choice->addStrategy(backtrack, make_unique<DominoBacktrackStrategy>(requireCirculation));
  choice->addStrategy(addSpace, make_unique<DominoAddSpaceStrategy>(requireCirculation));

  unique_ptr<LoopStrategy> loop(make_unique<LoopStrategy>(Strategy::const_ref(choice), 300));
  setRootStrategy(Strategy::const_ref(loop));
}
