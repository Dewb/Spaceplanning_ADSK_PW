#pragma once

#include <Vision.h>

class BuildingCoreVision : public Vision
{
public:
  BuildingCoreVision(const JobRequest& jobRequest);

  unique_ptr<Design> execute(std::function<void(unique_ptr<Design>)> callback);
};
