#pragma once

class JobData;
class BuildData;
class Phenotype;

class Analyzer
{
public:
  virtual ~Analyzer() = default;

  virtual float analyze(
    const JobData& jobData,
    const BuildData& buildData,
    const Phenotype& phenotype) const = 0;
};
