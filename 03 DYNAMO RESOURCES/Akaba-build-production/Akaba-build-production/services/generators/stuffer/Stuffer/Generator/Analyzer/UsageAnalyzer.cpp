#include <stdafx.h>
#include <UsageAnalyzer.h>
#include <JobData.h>
#include <JobRequest.h>
#include <Phenotype.h>

namespace
{
  float calculateFitness(const SpaceRequest& requested, const Usage::value_type& delivered)
  {
    float fitness(0.0f);

    float usageFitness(delivered.first / requested.minimumArea);
    fitness += usageFitness;

    static bool useCount(false);
    if (useCount)
    {
      float countFitness(delivered.second / static_cast<float>(requested.minimumCount));
      fitness += countFitness;
      fitness /= 2.0f;
    }

    return fitness;
  }
}

float UsageAnalyzer::analyze(const JobData& jobData, const BuildData& /*buildData*/, const Phenotype& phenotype) const
{
  auto actualUsage(phenotype.getUsage());

  float fitness(0.0f);
  float count(0.0f);
  for (const auto& request : jobData.getRequest().requirements.spaces)
  {
    if (request.minimumArea == 0 && request.minimumCount == 0)
      continue;

    auto usage(actualUsage.find(Usage::tag(request.usage)));
    if (!usage)
      continue;

    fitness += calculateFitness(request, *usage);
    count += 1;
  }

  return fitness / count;
}
