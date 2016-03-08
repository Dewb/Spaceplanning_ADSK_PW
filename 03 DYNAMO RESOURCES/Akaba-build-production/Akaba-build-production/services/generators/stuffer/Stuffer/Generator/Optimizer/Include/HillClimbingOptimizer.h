#pragma once

#include <Optimizer.h>

class HillClimbingOptimizer : public Optimizer
{
public:
  HillClimbingOptimizer(float target, int maxIterations, float percent, float window);

  void optimize(const Analyzer& analyzer, const Strategy& strategy, const Args& args) const;

private:
  float target;
  int maxIterations;
  float percent;
  float window;
};
