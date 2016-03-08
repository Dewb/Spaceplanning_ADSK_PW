#pragma once

#include <XYVision.h>

class StackedVision : public XYVision
{
public:
  StackedVision(const JobRequest& jobRequest);

protected:
  void createPlan();
};
