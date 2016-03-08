#include <stdafx.h>
#include <Plan.h>
#include <Strategy.h>
#include <Args.h>
#include <Ribosome.h>

Plan::Plan(unique_ptr<const Strategy> strategy)
: m_pRootStrategy(move(strategy))
{
}

shared_ptr<const Phenotype> Plan::createPhenotype(
  JobData& jobData,
  BuildData& buildData,
  Genome& genome,
  shared_ptr<Phenotype> rootPhenotype)
{
  m_pCurrentPhenotype = rootPhenotype;

  if (m_pRootStrategy)
  {
    m_buildData = &buildData;
    m_strategyArgs.reset(
      new Args(
        jobData,
        genome,
        make_unique<Ribosome>(genome),
        *m_buildData,
        *m_pCurrentPhenotype));

    ucout << U("Running root (") << m_pRootStrategy->name() << U(") ... ") << endl;
    if (m_pRootStrategy)
      m_pRootStrategy->execute(*m_strategyArgs);
    ucout << U("complete.") << endl;
  }

  return m_pCurrentPhenotype;
}

const BuildData& Plan::getBuildData() const
{
  return m_strategyArgs->getBuildData();
}

const Phenotype& Plan::getPhenotype() const
{
  return *m_pCurrentPhenotype;
}
