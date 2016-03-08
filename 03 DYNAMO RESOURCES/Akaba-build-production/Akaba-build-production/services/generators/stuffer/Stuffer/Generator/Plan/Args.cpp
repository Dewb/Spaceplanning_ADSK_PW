#include <stdafx.h>
#include <Args.h>
#include <BuildData.h>

Args::Args(
  JobData& jobData,
  Genome& genome,
  unique_ptr<Ribosome> ribosome,
  BuildData& buildData,
  Phenotype& phenotype)
: m_jobData(jobData),
  m_genome(genome),
  m_ribosome(move(ribosome)),
  m_buildData(buildData),
  m_phenotype(phenotype)
{
}

Args::~Args() = default;

JobData& Args::getJobData() const
{
  return m_jobData;
}

Genome& Args::getGenome() const
{
  return m_genome;
}

Ribosome& Args::getRibosome() const
{
  return *m_ribosome;
}

BuildData& Args::getBuildData() const
{
  return m_buildData;
}

Phenotype& Args::getPhenotype() const
{
  return m_phenotype;
}
