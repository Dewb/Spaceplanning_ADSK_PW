#include <stdafx.h>
#include <OptimizingStrategy.h>
#include <Optimizer.h>
#include <Analyzer.h>

const string_t& OptimizingStrategy::name() const
{
  static string_t name(U("OptimizingStrategy"));
  return name;
}

OptimizingStrategy::OptimizingStrategy(
  unique_ptr<Optimizer> optimizer, 
  unique_ptr<Analyzer> analyzer,
  unique_ptr<Strategy> strategy)
: optimizer(move(optimizer)),
  analyzer(move(analyzer)),
  strategy(move(strategy))
{
}

void OptimizingStrategy::execute(const Args& args) const
{
  ucout << endl << U("    Optimizing: ") << strategy->name() << U(" ... ");
  optimizer->optimize(*analyzer, *strategy, args);
}
