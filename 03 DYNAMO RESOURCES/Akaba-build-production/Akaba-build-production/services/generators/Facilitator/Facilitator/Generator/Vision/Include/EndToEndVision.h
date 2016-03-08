#pragma once

#include <Vision.h>

class EndToEndVision : public Vision
{
public:
  EndToEndVision(const JobRequest& jobRequest);

  unique_ptr<Design> execute(std::function<void(unique_ptr<Design>)> callback);
};
