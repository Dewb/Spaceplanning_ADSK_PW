#pragma once

#include <XYVision.h>

class TrapelloVision : public XYVision
{
public:
  TrapelloVision(const JobRequest& jobRequest);

protected:
  void createPlan();
};
