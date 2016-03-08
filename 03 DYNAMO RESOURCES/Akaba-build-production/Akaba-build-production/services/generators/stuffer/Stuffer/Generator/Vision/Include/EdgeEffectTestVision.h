#pragma once

#include <XYVision.h>

class EdgeEffectTestVision : public XYVision
{
public:
  EdgeEffectTestVision(const JobRequest& jobRequest);

protected:
  void createPlan();
  void createGenome();
};
