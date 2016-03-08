#pragma once

#include <XYVision.h>

class BuildingCoreVision : public XYVision
{
public:
  BuildingCoreVision(const JobRequest& jobRequest);

protected:
  void createPlan();
};
