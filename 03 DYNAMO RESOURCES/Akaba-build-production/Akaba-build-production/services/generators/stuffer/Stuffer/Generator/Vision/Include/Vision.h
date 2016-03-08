#pragma once

#include <JobData.h>
#include <Usage.h>
#include <BuildData.h>
#include <Plan.h>
#include <Args.h>
#include <Genome.h>
#include <Strategy.h>

class JobRequest;
class Design;

class Vision
{
public:
  Vision(const JobRequest& jobRequest, const GridBasis& axis);
  virtual ~Vision() = default;

  unique_ptr<Design> execute();

  const Plan& getPlan() const;

protected:
  GridBasis basis;
  unique_ptr<JobData> jobData;
  unique_ptr<BuildData> buildData;
  unique_ptr<Plan> plan;
  unique_ptr<Genome> genome;

  void setRootStrategy(unique_ptr<const Strategy> strategy);
  void setGenome(unique_ptr<Genome> genome);
};
