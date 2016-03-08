#pragma once

#include <XYVision.h>

class ShellAndCoreVision : public XYVision
{
public:
  ShellAndCoreVision(const JobRequest& jobRequest);

protected:
  void createPlan();
};
