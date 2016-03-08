#pragma once

class JobRequest;
class GridBasis;
class Usage;

class JobData
{
public:
  JobData(const JobRequest& request);

  const JobRequest& getRequest() const;
  const Usage& getUsageRequest();

  int getHallwayWidth(const GridBasis& basis) const;
  Rect2i getStairRect(const GridBasis& basis) const;

private:
  const JobRequest& request;
  unique_ptr<Usage> usageRequest;

  float getHallwayWidth() const;
  Rect2f getStairRect() const;
};
