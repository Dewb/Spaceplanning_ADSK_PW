#pragma once

class Analyzer;
class Strategy;
class Args;

class Optimizer
{
public:
  virtual ~Optimizer() = default;

  virtual void optimize(const Analyzer& analyzer, const Strategy& strategy, const Args& args) const = 0;
};
