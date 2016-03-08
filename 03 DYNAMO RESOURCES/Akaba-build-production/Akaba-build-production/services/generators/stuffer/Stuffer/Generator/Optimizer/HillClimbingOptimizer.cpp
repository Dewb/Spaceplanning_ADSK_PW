#include <stdafx.h>
#include <HillClimbingOptimizer.h>
#include <Analyzer.h>
#include <Args.h>
#include <JobData.h>
#include <BuildData.h>
#include <Strategy.h>
#include <Phenotype.h>
#include <Floor.h>
#include <Section.h>

HillClimbingOptimizer::HillClimbingOptimizer(float target, int maxIterations, float percent, float window)
: target(target),
  maxIterations(maxIterations),
  percent(percent),
  window(window)
{
}

void HillClimbingOptimizer::optimize(const Analyzer& analyzer, const Strategy& strategy, const Args& args) const
{
  float fitness(0.0f);
  unique_ptr<Genome> bestGenome(new Genome());
  unique_ptr<Genome> currentGenome(bestGenome->clone());

  int count(0);
  while (fitness < target && ++count < maxIterations)
  {
    Genome& genome(args.getGenome());

    Args workingArgs(
      args.getJobData(),
      genome,
      make_unique<Ribosome>(args.getRibosome()),
      args.getBuildData().createSnapshot(),
      args.getPhenotype().createSnapshot());

    strategy.execute(workingArgs);

    float newFitness(analyzer.analyze(args.getJobData(), args.getBuildData(), args.getPhenotype()));
    if (newFitness > fitness)
    {
      bestGenome = move(currentGenome);
      fitness = newFitness;
    }

    currentGenome = bestGenome->clone();
    currentGenome->mutate(percent, window);
    genome.discard(args.getRibosome());
    genome.append(*currentGenome);

    args.getBuildData().discardSnapshot();
  }

  strategy.execute(args);
}
