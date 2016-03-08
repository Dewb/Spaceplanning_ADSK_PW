#pragma once

#include <Analyzer.h>

class UsageAnalyzer : public Analyzer
{
public:
  virtual float analyze(
    const JobData& jobData,
    const BuildData& buildData,
    const Phenotype& phenotype) const;
};
