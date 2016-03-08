#pragma once

#include <Strategy.h>

class Optimizer;
#include <Analyzer.h>

class OptimizingStrategy : public Strategy
{
public:
  OptimizingStrategy(
    unique_ptr<Optimizer> optimizer,
    unique_ptr<Analyzer> analyzer,
    unique_ptr<Strategy> strategy);

  void execute(const Args& args) const;

protected:
  const string_t& name() const;
  unique_ptr<Optimizer> optimizer;
  unique_ptr<Analyzer> analyzer;
  unique_ptr<Strategy> strategy;
};
