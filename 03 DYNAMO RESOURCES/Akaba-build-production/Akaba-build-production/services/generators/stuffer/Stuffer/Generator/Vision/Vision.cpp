#include <stdafx.h>
#include <Vision.h>
#include <Floor.h>
#include <Phenotype.h>
#include <SpaceLayout.h>

Vision::Vision(const JobRequest& jobRequest, const GridBasis& basis)
: basis(basis),
  jobData(new JobData(jobRequest)),
  plan(new Plan(nullptr)),
  genome(new Genome())
{
}

void Vision::setRootStrategy(unique_ptr<const Strategy> strategy)
{
  plan.reset(new Plan(move(strategy)));
}

void Vision::setGenome(unique_ptr<Genome> genome_)
{
  genome = move(genome_);
}

unique_ptr<Design> Vision::execute()
{
  if (!buildData)
    buildData.reset(new BuildData(basis));
  else
    buildData->reset();

  shared_ptr<Phenotype> rootPhenotype(new Phenotype(basis));

  shared_ptr<const Phenotype> phenotype(plan->createPhenotype(*jobData, *buildData, *genome, rootPhenotype));
  return phenotype->convert();
}

const Plan& Vision::getPlan() const
{
  return *plan;
}
