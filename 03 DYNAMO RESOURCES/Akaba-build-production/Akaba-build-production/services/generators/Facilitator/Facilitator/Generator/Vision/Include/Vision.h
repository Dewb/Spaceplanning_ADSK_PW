#pragma once

#include <JobRequest.h>

class Design;

class Vision
{
public:
  Vision::Vision(const JobRequest& jobRequest)
  : requestData(jobRequest)
  {
  }

  virtual ~Vision() = default;

  const JobRequest& Vision::request() const
  {
    return requestData;
  }

  virtual unique_ptr<Design> execute(std::function<void(unique_ptr<Design>)> callback) = 0;

protected:
  JobRequest requestData;
};
