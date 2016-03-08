#pragma once

class JobData;
class Args;
class Strategy;
class BuildData;
class Genome;
class Phenotype;

class Plan
{
public:
  Plan(unique_ptr<const Strategy> strategy);

  shared_ptr<const Phenotype> createPhenotype(
    JobData& jobData,
    BuildData& buildData,
    Genome& geneotype,
    shared_ptr<Phenotype> rootPhenotype);

  const BuildData& getBuildData() const;
  const Phenotype& getPhenotype() const;

private:
  unique_ptr<const Strategy> m_pRootStrategy;

  BuildData* m_buildData;
  shared_ptr<Phenotype> m_pCurrentPhenotype;
  unique_ptr<const Args> m_strategyArgs;
};
