#pragma once

#include <XYVision.h>

class DominoVision : public XYVision
{
public:
  DominoVision(const JobRequest& jobRequest);

protected:
  void createPlan();
};
