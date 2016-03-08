#include <stdafx.h>
#include <JobData.h>
#include <JobRequest.h>
#include <Usage.h>

JobData::JobData(const JobRequest& request)
: request(request)
{
}

const JobRequest& JobData::getRequest() const
{
  return request;
}

const Usage& JobData::getUsageRequest()
{
  if (!usageRequest)
  {
    usageRequest.reset(new Usage());
    for (const auto& spaceRequest : request.requirements.spaces)
      usageRequest->init(
        Usage::tag(spaceRequest.usage), 
        static_cast<float>(spaceRequest.minimumArea), 
        spaceRequest.minimumCount);
  }

  return *usageRequest;
}

int JobData::getHallwayWidth(const GridBasis& basis) const
{
  return basis.toGrid(request.settings.hallwayWidth);
}

Rect2i JobData::getStairRect(const GridBasis& basis) const
{
  return basis.toGrid(getStairRect());
}

float JobData::getHallwayWidth() const
{
  return request.settings.hallwayWidth;
}

Rect2f JobData::getStairRect() const
{
  return Rect2f(Point2f(3.0, 5.0));
}
