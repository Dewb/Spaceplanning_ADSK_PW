#pragma once

#include <Ribosome.h>

class JobData;
class BuildData;
class Phenotype;

class Args
{
public:
  Args(
    JobData& jobData,
    Genome& genome,
    unique_ptr<Ribosome> ribosome,
    BuildData& buildData,
    Phenotype& phenotype);
  ~Args();

  Args& operator=(const Args& other) = delete;

  JobData& getJobData() const;
  Genome& getGenome() const;
  Ribosome& getRibosome() const;
  BuildData& getBuildData() const;
  Phenotype& getPhenotype() const;

private:
  JobData& m_jobData;
  Genome& m_genome;
  unique_ptr<Ribosome> m_ribosome;
  BuildData& m_buildData;
  Phenotype& m_phenotype;
};
