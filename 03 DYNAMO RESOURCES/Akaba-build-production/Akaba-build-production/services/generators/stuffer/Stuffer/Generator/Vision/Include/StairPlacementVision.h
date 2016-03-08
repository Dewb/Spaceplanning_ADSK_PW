#pragma once

#include <XYVision.h>

class StairPlacementVision : public XYVision
{
public:
  StairPlacementVision(const JobRequest& jobRequest);

protected:
  void createPlan();
};
